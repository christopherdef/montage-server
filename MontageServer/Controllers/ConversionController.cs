using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Controllers
{
    public class ConversionController
    {
        public static Stream ConvertToWavFiles(string inputPt, string mimeType)
        {
            if (mimeType.EndsWith("/wav"))
                return File.OpenRead(inputPt);

            var outputPt = Path.GetTempFileName()+".wav";

            // pcm_s16le or pcm_s32le
            var ffargs = FFMpegArguments
                .FromFileInput(inputPt)
                .OutputToFile(outputPt, true,
                              options => options.WithCustomArgument("-ar 8000 -c:a pcm_s16le")
                              .ForceFormat("wav")
                              );

            ffargs.ProcessAsynchronously().Wait();

            return File.OpenRead(outputPt);
        }
        public static Stream ConvertToWav(Stream inputStream, string mimeType)
        {
            if (mimeType.EndsWith("/wav"))
                return inputStream;
            
            var outputStream = new MemoryStream();
            var outputPipe = new StreamPipeSink(outputStream);
            outputPipe.Format = "wav";
            
            // pcm_s16le or pcm_s32le
            var ffargs = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToPipe(outputPipe,
                              options => options.WithCustomArgument("-ar 8000 -c:a pcm_s16le")
                              .ForceFormat("wav")
                              );

            ffargs.ProcessAsynchronously().Wait();


            return outputStream;
        }

        public static (Stream videoStream, Stream audioStream) SeparateAudioVideo(Stream inputStream, string mimeType)
        {
            // ffmpeg -y -i inputStream -map 0:a -c:a copy -f:a adts outputStream -y
            
            var tmpIn = Path.GetTempFileName();
            var tmpAud = Path.GetTempFileName();
            var tmpVid = Path.GetTempFileName();

            var vidFmt = mimeType.Substring(mimeType.IndexOf('/')+1);
            var audFmt = "adts";
            switch (vidFmt)
            {
                case "mp4":
                case "mov":
                    audFmt = "adts";
                    break;
                case "x-ms-wmv":
                    audFmt = "wav";
                    break;
                case "x-msvideo":
                    audFmt = "mp3";
                    break;
                case "webm":
                    audFmt = "opus";
                    break;
            }
            
            WriteStreamToDisk(inputStream, tmpIn);
            FFMpegArguments
                .FromFileInput(tmpIn)
                .OutputToFile(tmpAud, true, options => options
                    .WithCustomArgument($"-map 0:a -c:a copy -f:a {audFmt}")
                    )
                .ProcessSynchronously();
            
            var audioStream = File.OpenRead(tmpAud);
            //FFMpegArguments.FromFileInput(tmpAud)
            //    .OutputToFile(tmpIn, true, options => options.ForceFormat("wav")
            //    )
            //    .ProcessSynchronously();
            //var audioStream = File.OpenRead(tmpIn);
            // ffmpeg -i inputStream -map 0:a -c:a copy outputStream
            //var ffargs = FFMpegArguments
            //    .FromPipeInput(new StreamPipeSource(inputStream))
            //    .OutputToPipe(new StreamPipeSink(audioStream),
            //    options => options
            //                .WithCustomArgument("-map 0:a -c:a copy -f:a adts"));
            //                //.CopyChannel(Channel.Audio)
            //Console.WriteLine(ffargs.Arguments);
            //ffargs.ProcessSynchronously();


            //var videoStream = new MemoryStream();
            //var ffargs = FFMpegArguments
            //    .FromPipeInput(new StreamPipeSource(inputStream))
            //    .OutputToPipe(new StreamPipeSink(audioStream),
            //    options => options
            //         .DisableChannel(Channel.Audio));
            //ffargs.ProcessSynchronously();

            // ffmpeg -i outputStream wavStream
            return (inputStream, audioStream);
        }


        public static void WriteStreamToDisk(Stream stream, string outpt)
        {
            var buf = new byte[stream.Length];
            stream.Read(buf);
            var f = File.Create(outpt, buf.Length);
            f.Write(buf);
            f.Close();
        }
    }
}