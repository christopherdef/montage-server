using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MontageServer.Data;
using Microsoft.AspNetCore.Mvc;
using MontageServer.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MontageServer.Controllers
{
    public class AnalysisController
    {
        private static string PYTHON_PATH = @"..\Python38\python.exe";
        private static string TRANSCRIPT_SCRIPT_PATH = @"PyScripts\analyze_transcript.py";
        private static string AUDIO_SCRIPT_PATH = @"PyScripts\transcribe_audio.py";
        private static string BASE_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\");

        // load our subscription
        // TODO: get this hard coded sensitive stuff outta here
        private static SpeechConfig SPEECH_CONFIG = SpeechConfig.FromSubscription("77f35c60bef24e58bce1a0c0b9f4be65", "eastus");

        /// <summary>
        /// Given an IFormFile, save to disk at the specified path
        /// </summary>
        private static void SaveFormFile(IFormFile formFile, string pt)
        {
            using (StreamWriter sw = File.CreateText(pt))
            using (StreamReader sr = new StreamReader(formFile.OpenReadStream()))
                sw.Write(sr.ReadToEnd());
        }

        private static AudioResponse DeserializeResponse(string scriptOutput)
        {
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            // deserialize json AudioResponse from script
            return JsonConvert.DeserializeObject<AudioResponse>(scriptOutput, options);
        }

        public static AudioResponse AnalyzeAudio(ref AudioResponse response, IFormFile audioFile)
        {
            TranscribeAudio(ref response, audioFile);

            string transFileFn = Path.GetTempFileName();
            using (StreamWriter sw = File.CreateText(transFileFn))
                sw.Write(response.Transcript);

            FormFile transcriptFile = new FormFile(File.OpenRead(transFileFn), 0, response.Transcript.Length, audioFile.Name, transFileFn);
            AnalyzeTranscript(ref response, transcriptFile);
                
            return response;
        }

        /// <summary>
        /// Run Python analysis script on the given transcriptFile
        /// </summary>
        public static AudioResponse AnalyzeTranscript(ref AudioResponse audioResponse, IFormFile transcriptFile)
        {
            // save transcript as file with unique name
            string transFileFn = Path.GetTempFileName();
            SaveFormFile(transcriptFile, transFileFn);

            // run analysis on saved transcript file
            string scriptOutput = RunCmd(PYTHON_PATH, $"{TRANSCRIPT_SCRIPT_PATH} {audioResponse.ReqId} {transFileFn}");

            audioResponse = DeserializeResponse(scriptOutput);

            //audioResponse = JsonSerializer.Deserialize<AudioResponse>(scriptOutput, options);

            return audioResponse;
        }

        /// <summary>
        /// Local transcription of the given audioFile with Sphinx
        /// </summary>
        public static AudioResponse TranscribeAudioLocal(ref AudioResponse audioResponse, IFormFile audioFile)
        {
            // save audio as file with unique name
            string audioFileFn = Path.GetTempFileName();
            SaveFormFile(audioFile, audioFileFn);

            // transcribe saved audio file
            string scriptOutput = RunCmd(PYTHON_PATH, $"{AUDIO_SCRIPT_PATH} {audioResponse.ReqId} {audioFileFn}");

            audioResponse = DeserializeResponse(scriptOutput);

            return audioResponse;
        }

        /// <summary>
        /// Remote audio transcription of the given audioFile with CognitiveServices
        /// </summary>
        public static AudioResponse TranscribeAudio(ref AudioResponse audioResponse, IFormFile audioFile)
        {
            var audioFormat128 = AudioStreamFormat.GetWaveFormatPCM(8000, 16, 1);
            var audioFormat256 = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);

            // load bytestream -> audio stream
            // load audio config from audio stream
            // initialize speech recognizer
            using (var br = new BinaryReader(audioFile.OpenReadStream()))
            using (var audioInputStream = AudioInputStream.CreatePushStream(audioFormat128))
            using (var audioConfig = AudioConfig.FromStreamInput(audioInputStream))
            using (var recognizer = new SpeechRecognizer(SPEECH_CONFIG, audioConfig))
            {
                long nbytes = audioFile.Length;
                var buff = new List<byte>();

                // read through bytes of audio
                byte[] readBytes;
                do
                {
                    readBytes = br.ReadBytes(1024);
                    buff.AddRange(readBytes);
                    audioInputStream.Write(readBytes, readBytes.Length);

                } while (readBytes.Length > 0);

                // call 
                // TODO: start async recognition
                //var recognitionResult = recognizer.StartContinuousRecognitionAsync();
                var recognitionResult = recognizer.RecognizeOnceAsync();
                recognitionResult.Wait();
                audioResponse.Transcript = recognitionResult.Result.Text;

                return audioResponse;
            }
        }

        //public AudioResponse AnalyzeTranscriptDummy(ref AudioResponse audioResponse, IFormFile transcriptFile)
        //{
        //    Random rng;
        //    var topics = new List<List<int>>();
        //    var individuals = new List<string>();
        //    var objects = new List<string>();
        //    var sentiments = new List<double>();
        //    using (var sr = new StreamReader(transcriptFile.OpenReadStream()))
        //    {
        //        string content = sr.ReadToEnd();

        //        // TODO: send this content to the analysis script
        //        // TODO: await AudioResponse from analysis script

        //        /*
        //         *  until ^^ that's complete, return some random garbage
        //         */

        //        content = content.Replace('\r', ' ');
        //        content = content.Replace('\n', ' ');

        //        // keep returns consistent for the same string
        //        int chash = content.GetHashCode();
        //        rng = new Random(chash);

        //        var words = content.Split(' ');

        //        int N = words.Length;

        //        // add space for the topics
        //        int topic_count = rng.Next(2, 10);
        //        for (int j = 0; j < topic_count; j++)
        //            topics.Append(new List<int>());

        //        // randomly create a AudioResponse
        //        for (int i = 0; i < N; i++)
        //        {
        //            string w = words[i];
        //            if (w.Length == 0)
        //                continue;

        //            // "calculate" sentiment
        //            double sentiment = (double)(w.Length) / (double)N;
        //            sentiments.Add(sentiment);

        //            // "assign" to a topic
        //            int topic_idx = rng.Next(0, topic_count);
        //            topics[topic_idx].Add(rng.Next(10, 100));

        //            // randomly assign words as individuals, objects, or neither
        //            switch (rng.Next(0, 10))
        //            {
        //                // individuals
        //                case 0:
        //                    individuals.Add(w);
        //                    break;

        //                // objects
        //                case 1:
        //                    objects.Add(w);
        //                    break;

        //                // neither
        //                default:
        //                    break;
        //            }
        //        }
        //    }

        //    // return the file name
        //    // TODO: return more than just the file name
        //    //return file != null ? "/uploads/" + file.FileName : null;
        //    audioResponse.Topics = topics;
        //    audioResponse.Individuals = individuals;
        //    audioResponse.Objects = objects;
        //    audioResponse.Sentiments = sentiments;
        //    audioResponse.Transcript = "";
        //    return audioResponse;
        //}

        /// <summary>
        /// Runs the given cmd with the specified args as a separate shell process
        /// Awaits the process finishing before returning, *can lock*
        /// </summary>
        private static string RunCmd(string cmd, string args)
        {
            cmd = BASE_DIR + cmd;
            args = BASE_DIR + args;
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = cmd; //cmd is full path to python.exe
            start.Arguments = args; //args is path to .py file and any cmd line args
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            using (Process process = Process.Start(start))
            using (StreamReader reader = process.StandardOutput)
            using (StreamReader errReader = process.StandardError)
            {
                string r = reader.ReadToEnd().Trim();
                string err = errReader.ReadToEnd();

                if (err.Length > 0)
                    r += "\n" + err;    
                return r;
            }
        }
    }
}