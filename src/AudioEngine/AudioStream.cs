using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.AudioEngine
{
    public sealed class AudioStream
    {
        private readonly Stream baseStream,
            targetStream;

        public AudioStream(Stream stream)
        {
            baseStream = stream;
            targetStream = new MemoryStream();
        }

        public async Task WriteAsync()
        {
            
        }
    }
}