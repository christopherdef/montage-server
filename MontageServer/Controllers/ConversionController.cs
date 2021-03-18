using FFMpegCore;
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
        public static MemoryStream ConvertToWav(Stream inputStream)
        {
            GlobalFFOptions.Configure(options => options.BinaryFolder = Environment.GetEnvironmentVariable("FFMPEG_PATH"));
            var outputStream = new MemoryStream();

            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToPipe(new StreamPipeSink(outputStream),
                              options => options.WithAudioCodec("ac3")
                                                .ForceFormat("wav"))
                .ProcessAsynchronously().Wait();

            return outputStream;
        }
    }
}
