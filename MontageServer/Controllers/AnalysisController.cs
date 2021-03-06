﻿using Microsoft.CognitiveServices.Speech;
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
using System.Resources;
using MontageServer.Properties;
using System.Threading;

namespace MontageServer.Controllers
{
    public class AnalysisController
    {
        private static string CONDA_PATH = Environment.GetEnvironmentVariable("CONDAPATH");
        private static string PYTHON_PATH = Environment.GetEnvironmentVariable("PYTHONPATH");
        private static string TRANSCRIPT_SCRIPT_PATH = Path.Combine(Resources.script_dir, Resources.analyze_transcript_pt);
        private static string AUDIO_SCRIPT_PATH = Path.Combine(Resources.script_dir, Resources.transcribe_audio_pt);
        //private static string BASE_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\");

        // load our subscription
        // TODO: get this hard coded sensitive stuff outta here
        private static SpeechConfig SPEECH_CONFIG = SpeechConfig.FromSubscription("77f35c60bef24e58bce1a0c0b9f4be65", "eastus");


        /// <summary>
        /// Fill the Transcript field within the provided AnalysisResult
        /// </summary>
        public static AnalysisResult AnalyzeAudio(ref AnalysisResult response, Stream audioStream)
        {
            var f = new FormFile(audioStream, 0, audioStream.Length, response.ClipId, response.FootagePath);
            return AnalyzeAudio(ref response, f);
        }

        /// <summary>
        /// Fill the Transcript field within the provided AnalysisResult
        /// </summary>
        public static AnalysisResult AnalyzeAudio(ref AnalysisResult response, IFormFile audioFile)
        {

#if (DEBUG)
            response.Transcript = Resources.sample_text;
#else
            TranscribeAudio(ref response, audioFile);
#endif
            if (response.Transcript.Length != 0)
            {
                string transFileFn = Path.GetTempFileName();
                using (StreamWriter sw = File.CreateText(transFileFn))
                    sw.Write(response.Transcript);
                
                FormFile transcriptFile = new FormFile(File.OpenRead(transFileFn), 0, response.Transcript.Length, audioFile.Name, transFileFn);
                AnalyzeTranscript(ref response, transcriptFile);
            }

            return response;
        }

        /// <summary>
        /// Run Python analysis script on the given transcriptFile
        /// </summary>
        public static AnalysisResult AnalyzeTranscript(ref AnalysisResult audioResponse, IFormFile transcriptFile)
        {
            // save transcript as file with unique name
            string transFileFn = Path.GetTempFileName();
            SaveFormFile(transcriptFile, transFileFn);

            // run analysis on saved transcript file
            string scriptOutput = RunCmd(PYTHON_PATH, $"{TRANSCRIPT_SCRIPT_PATH} {audioResponse.ClipId ?? "-1"} {transFileFn}");

            var pythonResponse = AnalysisResult.DeserializePythonResponse(scriptOutput);
            audioResponse.JoinRight(pythonResponse);
            return audioResponse;
        }

        /// <summary>
        /// Local transcription of the given audioFile with Sphinx
        /// </summary>
        public static AnalysisResult TranscribeAudioLocal(ref AnalysisResult audioResponse, IFormFile audioFile)
        {
            // save audio as file with unique name
            string audioFileFn = Path.GetTempFileName();
            SaveFormFile(audioFile, audioFileFn);

            // transcribe saved audio file
            string scriptOutput = RunCmd(PYTHON_PATH, $"{AUDIO_SCRIPT_PATH} {audioResponse.ClipId} {audioFileFn}");

            var pythonResponse = AnalysisResult.DeserializePythonResponse(scriptOutput);
            audioResponse.JoinRight(pythonResponse);

            return audioResponse;
        }

        /// <summary>
        /// Remote audio transcription of the given audioFile with CognitiveServices
        /// </summary>
        public static AnalysisResult TranscribeAudio(ref AnalysisResult audioResponse, IFormFile audioFile)
        {
            // needed for speaker diarization to resolve at the word level
            SPEECH_CONFIG.RequestWordLevelTimestamps();

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

                var transcript = ExecuteRecognizer(recognizer).Result;
                audioResponse.Transcript = transcript;
                return audioResponse;
            }
        }

        /// <summary>
        /// Performs asynchronous speech recognition for long audio clips
        /// Requires a SpeechRecognizer which has been preloaded with data
        /// </summary>
        private static async Task<string> ExecuteRecognizer(SpeechRecognizer recognizer)
        {
            object noMatchLock = new object();
            int consecutiveNoMatchCount = 0;

            object transcriptLock = new object();
            StringBuilder transcriptBuilder = new StringBuilder();

            var stopRecognition = new TaskCompletionSource<int>();

            recognizer.Recognizing += (s, e) =>
            {
                if (e.Result.Reason.HasFlag(ResultReason.RecognizedSpeech))
                {
                    consecutiveNoMatchCount = 0;

                    lock (transcriptLock)
                    {
                        transcriptBuilder.Append(e.Result.Text);
                        transcriptBuilder.Append(' ');
                        transcriptBuilder.Append('\n');
                    }
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    lock (noMatchLock)
                    {
                        consecutiveNoMatchCount++;
                        if (consecutiveNoMatchCount > 4)
                            recognizer.StopContinuousRecognitionAsync();

                    }
                }
            };

            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    consecutiveNoMatchCount = 0;
                    lock (transcriptLock)
                    {
                        transcriptBuilder.Append(e.Result.Text);
                        transcriptBuilder.Append(' ');
                        transcriptBuilder.Append('\n');
                    }
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    lock (noMatchLock)
                    {
                        consecutiveNoMatchCount++;
                        if (consecutiveNoMatchCount > 4)
                            recognizer.StopContinuousRecognitionAsync();

                    }
                }
            };

            recognizer.SessionStopped += (s, e) =>
            {
                stopRecognition.TrySetResult(0);
            };

            recognizer.Canceled += (s, e) =>
            {
                if (e.Reason == CancellationReason.Error)
                {
                    lock (transcriptLock)
                    {
                        transcriptBuilder.Append($"\nSPEECH RECOGNITION ERROR: {e.ErrorCode}\nDETAILS: {e.ErrorDetails}");
                    }
                }

                stopRecognition.TrySetResult(0);
            };

            await recognizer.StartContinuousRecognitionAsync();
            Task.WaitAny(new[] { stopRecognition.Task });
            await recognizer.StopContinuousRecognitionAsync();
            return transcriptBuilder.ToString();

        }

        /// <summary>
        /// Runs the given cmd with the specified args as a separate shell process
        /// Awaits the process finishing before returning, *can lock*
        /// </summary>
        private static string RunCmd(string cmd, string args)
        {
            if (cmd == null)
                throw new ArgumentNullException($"Command cannot be null or empty! \ncmd: {cmd}\nargs: {args}");

            // start new shell
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };
            process.Start();

            // send commands into shell
            using (var sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    // activate Anaconda, if available
                    sw.WriteLine(CONDA_PATH ?? "");
                    sw.WriteLine($"{cmd} {args}");
                }
            }

            // read output
            using (StreamReader reader = process.StandardOutput)
            using (StreamReader errReader = process.StandardError)
            {
                string r = reader.ReadToEnd().Trim();
                string err = errReader.ReadToEnd();

                if (err.Length > 0)
                    r += "\n" + err;

                process.Kill();
                string rTrimmed = r.Substring(r.IndexOf(args) + args.Length).Trim();

                // try to get number of characters in response
                if (!int.TryParse(rTrimmed.TakeWhile((c) => c != '\n').ToArray(), out int responseLength))
                        throw new FormatException($"Unable to parse response length from response:\n{r}");

                return string.Concat(rTrimmed.SkipWhile((c) => c != '\n')
                                             .Take(responseLength));
            }
        }


        /// <summary>
        /// Given an IFormFile, save to disk at the specified path
        /// </summary>
        private static void SaveFormFile(IFormFile formFile, string pt)
        {
            using (StreamWriter sw = File.CreateText(pt))
            using (StreamReader sr = new StreamReader(formFile.OpenReadStream()))
                sw.Write(sr.ReadToEnd());
        }


    }
}