using System;
using Frostbyte.Entities.Packets;
using Frostbyte.Enums;

namespace Frostbyte.Handlers
{
    public sealed class GuildHandler
    {
        public void HandleOperation(PlayerPacket packet)
        {
            switch (packet.Operation)
            {
                case Operation.VoiceUpdate:
                    break;
                case Operation.Play:
                    break;
                case Operation.Pause:
                    break;
                case Operation.Stop:
                    break;
                case Operation.Skip:
                    break;
                case Operation.Seek:
                    break;
                case Operation.Volume:
                    break;
                case Operation.Destroy:
                    break;
                case Operation.Equalizer:
                    break;
                case Operation.Statistics:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}