using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Audio
{
    public sealed class AudioPlayer
    {
        public PlayerState State { get; set; }

        public async Task PlayAsync(Stream trackStream, AudioStream audioStream)
        {
            await AudioHelper.Pipe.ConvertAndWriteAsync(trackStream, audioStream)
                .ConfigureAwait(false);

            State = PlayerState.Playing;
        }
    }
}