using Frostbyte.Extensions;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed partial class GuildHandler
    {
        [DllImport("libsodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
        static extern int CreateSecretBox(byte[] buffer, byte[] message, long messageLength, byte[] nonce, byte[] key);

        public Task SendOpusAsync(byte[] buffer, int offset, int length, uint timestamp, ushort sequence)
        {
            var opus = new byte[length];
            Buffer.BlockCopy(buffer, offset, opus, 0, length);
            return SendOpusAsync(opus, timestamp, sequence);
        }

        public Task SendOpusAsync(byte[] opus, uint timestamp, ushort sequence)
        {
            try
            {
                if (!IsReady)
                {
                    SpinWait.SpinUntil(() => IsReady);
                }
                var nonce = RtpEncode(sequence, timestamp, VoiceReadyPayload.SSRC.TryCast<uint>());
                var sodium = SodiumEncode(opus, nonce, SessionDescriptionPayload.SecretKey);

                var buffer = new byte[12 + sodium.Length];
                Buffer.BlockCopy(nonce, 0, buffer, 0, 12);
                Buffer.BlockCopy(sodium, 0, buffer, 12, sodium.Length);
                return SendUdpPacketAsync(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send Opus Exception: " + e);
                return Task.CompletedTask;
            }
        }

        public byte[] SodiumEncode(byte[] input, byte[] nonce, byte[] secretKey)
        {
            if (secretKey == null || secretKey.Length != 32)
                throw new ArgumentException("Invalid key.");

            if (nonce == null || nonce.Length != 24)
                throw new ArgumentException("Invalid nonce.");

            var buff = new byte[16 + input.Length];
            var err = CreateSecretBox(buff, input, input.Length, nonce, secretKey);

            if (err != 0)
                throw new CryptographicException("Error encrypting data.");

            return buff;
        }

        public byte[] RtpEncode(ushort sequence, uint timestamp, uint ssrc)
        {
            var header = new byte[24];

            header[0] = 0x80;
            header[1] = 0x78;

            var flip = BitConverter.IsLittleEndian;
            var seqnb = BitConverter.GetBytes(sequence);
            var tmspb = BitConverter.GetBytes(timestamp);
            var ssrcb = BitConverter.GetBytes(ssrc);

            if (flip)
            {
                Array.Reverse(seqnb);
                Array.Reverse(tmspb);
                Array.Reverse(ssrcb);
            }

            Array.Copy(seqnb, 0, header, 2, seqnb.Length);
            Array.Copy(tmspb, 0, header, 4, tmspb.Length);
            Array.Copy(ssrcb, 0, header, 8, ssrcb.Length);

            return header;
        }
    }
}
