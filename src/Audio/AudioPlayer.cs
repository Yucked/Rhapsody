using System.Threading.Tasks;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Payloads;

namespace Frostbyte.Audio
{
    public sealed class AudioPlayer
    {
        public PlayerState State { get; set; }

        public async Task PlayAsync(PlayPayload payload, AudioStream audioStream)
        {
            await AudioHelper.Pipe.ConvertAndWriteAsync(default, audioStream)
                .ConfigureAwait(false);

            State = PlayerState.Playing;
        }
    }
}