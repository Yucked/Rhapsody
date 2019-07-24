using System.Threading.Tasks;
using Frostbyte.Entities.Payloads;

namespace Frostbyte.Audio
{
    public sealed class AudioPlayer
    {
        public async Task PlayAsync(PlayPayload payload, AudioStream audioStream)
        {
            await AudioHelper.Pipe.ConvertAndWriteAsync(default, audioStream)
                .ConfigureAwait(false);
            await Task.Delay(0);
        }
    }
}