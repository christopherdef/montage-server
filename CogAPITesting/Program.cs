using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
//using Azure.Storage.Blobs;
//using Azure.Storage.Blobs.Models;

namespace MontageSpeechToText
{
    class Program
    {
        async static Task FromFile(SpeechConfig speechConfig)
        {
            foreach (var file in Directory.EnumerateFiles(@"C:\Users\chris\source\repos\montage-server\CogAPITesting\", "*.wav"))
            {
                file.Replace("\\\\", "\\");

                using var audioConfig = AudioConfig.FromWavFileInput(@"" + file);
                using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

                var result = await recognizer.RecognizeOnceAsync();
                Console.WriteLine($"RECOGNIZED: Text={result.Text}");
            }
        }

        async static Task Main(string[] args)
        {
            var speechConfig = SpeechConfig.FromSubscription("77f35c60bef24e58bce1a0c0b9f4be65", "eastus");
            await FromFile(speechConfig);
        }
    }
}