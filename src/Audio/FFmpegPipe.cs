using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class FFmpegPipe
    {
        private readonly NamedPipeServerStream
            _pipe;

        public FFmpegPipe()
        {
            _pipe = new NamedPipeServerStream("ffpipe", PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                10000, 10000);

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                Arguments = @"-hide_banner -loglevel panic -ac 2 -f sl6le -ar 48000 -i \\.\pipe\ffpipe"
            });
        }

        public async Task<Stream> ConvertAsync(Stream stream)
        {
            if (!_pipe.IsConnected)
                await _pipe.WaitForConnectionAsync()
                    .ConfigureAwait(false);

            await stream.CopyToAsync(_pipe)
                .ConfigureAwait(false);
            await stream.FlushAsync()
                .ConfigureAwait(false);

            var outputStream = new MemoryStream();
            await _pipe.CopyToAsync(outputStream)
                .ConfigureAwait(false);

            return outputStream;
        }
    }
}