using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using MontageServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MontageServer.Controllers
{
    public class AnalysisController : Controller
    {
        private static string PYTHON_PATH = @"..\Python38\python.exe";
        private static string SCRIPT_PATH = @"PyScripts\analyze_transcript.py";
        private static string BASE_DIR = AppDomain.CurrentDomain.BaseDirectory;

        // load our subscription
        // TODO: get this hard coded sensitive stuff outta here
        private static SpeechConfig SPEECH_CONFIG = SpeechConfig.FromSubscription("77f35c60bef24e58bce1a0c0b9f4be65", "eastus");


        // GET: Analysis
        public ActionResult Index()
        {
            return View();
        }

        public static Response AnalyzeTranscript(ref Response response, HttpPostedFile transcriptFile)
        {
            // save transcript as file with unique name
            string transFileFn = Path.GetTempFileName();
            transcriptFile.SaveAs(transFileFn);
            
            string scriptOutput = RunCmd(PYTHON_PATH, $"{SCRIPT_PATH} {response.ReqId} {transFileFn}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            response = JsonSerializer.Deserialize<Response>(scriptOutput, options);

            return response;
        }

        public static Response AnalyzeAudio(ref Response response, HttpPostedFile audioFile)
        {
            var audioFormat128 = AudioStreamFormat.GetWaveFormatPCM(8000, 16, 1);
            var audioFormat256 = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);

            // load bytestream -> audio stream
            // load audio config from audio stream
            // initialize speech recognizer
            using (var br = new BinaryReader(audioFile.InputStream))
            using (var audioInputStream = AudioInputStream.CreatePushStream(audioFormat128))
            using (var audioConfig = AudioConfig.FromStreamInput(audioInputStream))
            using (var recognizer = new SpeechRecognizer(SPEECH_CONFIG, audioConfig))
            {
                int nbytes = audioFile.ContentLength;
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
                response.Transcript = recognitionResult.Result.Text;

                // TODO: begin analysis on transcript

                return response;
            }
        }

        public Response AnalyzeTranscriptDummy(ref Response response, HttpPostedFile transcriptFile)
        {
            Random rng;
            var topics = new List<List<int>>();
            var individuals = new List<string>();
            var objects = new List<string>();
            var sentiments = new List<double>();
            using (var sr = new StreamReader(transcriptFile.InputStream))
            {
                string content = sr.ReadToEnd();

                // TODO: send this content to the analysis script
                // TODO: await response from analysis script

                /*
                 *  until ^^ that's complete, return some random garbage
                 */

                content = content.Replace('\r', ' ');
                content = content.Replace('\n', ' ');

                // keep returns consistent for the same string
                int chash = content.GetHashCode();
                rng = new Random(chash);

                var words = content.Split(' ');

                int N = words.Length;

                // add space for the topics
                int topic_count = rng.Next(2, 10);
                for (int j = 0; j < topic_count; j++)
                    topics.Append(new List<int>());

                // randomly create a response
                for (int i = 0; i < N; i++)
                {
                    string w = words[i];
                    if (w.Length == 0)
                        continue;

                    // "calculate" sentiment
                    double sentiment = (double)(w.Length) / (double)N;
                    sentiments.Add(sentiment);

                    // "assign" to a topic
                    int topic_idx = rng.Next(0, topic_count);
                    topics[topic_idx].Add(rng.Next(10, 100));

                    // randomly assign words as individuals, objects, or neither
                    switch (rng.Next(0, 10))
                    {
                        // individuals
                        case 0:
                            individuals.Add(w);
                            break;

                        // objects
                        case 1:
                            objects.Add(w);
                            break;

                        // neither
                        default:
                            break;
                    }
                }
            }

            // return the file name
            // TODO: return more than just the file name
            //return file != null ? "/uploads/" + file.FileName : null;
            response.Topics = topics;
            response.Individuals = individuals;
            response.Objects = objects;
            response.Sentiments = sentiments;
            response.Transcript = "";
            return response;
        }

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
                return r;
            }
        }
    }
}