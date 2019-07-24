using System.Threading.Tasks;
using Frostbyte.Entities.Payloads;

namespace Frostbyte.Audio
{
    public sealed class AudioPlayer
    {
        public async Task PlayAsync(PlayPayload payload)
        {
            await AudioHelper.Pipe.ConvertAsync(default)
                .ConfigureAwait(false);
            await Task.Delay(0);
        }
    }
}