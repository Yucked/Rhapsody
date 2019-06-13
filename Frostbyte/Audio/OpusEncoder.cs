using Frostbyte.Entities;
using Frostbyte.Entities.Discord;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class OpusEncoder
    {
        [DllImport("libsodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
        static extern int CreateSecretBox(byte[] buffer, byte[] message, long messageLength, byte[] nonce, byte[] key);

        public static Task SendOpusAsync(ref bool isReady, UdpClient client, VoiceReadyPayload vrp, SessionDescriptionPayload sdp, Func<OpusPayload> func)
        {
            var payload = func.Invoke();
            var bytes = new byte[payload.Length];
            Buffer.BlockCopy(payload.Bytes, payload.Offset, bytes, 0, payload.Length);


            var ready = isReady;
            try
            {
                if (!isReady)
                {
                    SpinWait.SpinUntil(() => ready);
                }

                var rtp = RtpEncode(payload.Sequence, payload.Timestamp, vrp.SSRC.TryCast<uint>());
                var sodium = SodiumEncode(bytes, rtp, sdp.SecretKey);

                var buffer = new byte[12 + sodium.Length];
                Buffer.BlockCopy(rtp, 0, buffer, 0, 12);
                Buffer.BlockCopy(sodium, 0, buffer, 12, sodium.Length);
                return client.SendAsync(buffer);
            }
            catch (Exception ex)
            {
                LogHandler<OpusEncoder>.Log.Error(ex);
                return Task.CompletedTask;
            }
        }

        public static byte[] SodiumEncode(byte[] input, byte[] rtp, byte[] secretKey)
        {
            if (secretKey == null || secretKey.Length != 32)
                throw new ArgumentException("Invalid key.");

            if (rtp == null || rtp.Length != 24)
                throw new ArgumentException("Invalid nonce.");

            var buff = new byte[16 + input.Length];
            var secret = CreateSecretBox(buff, input, input.Length, rtp, secretKey);

            if (secret != 0)
                throw new CryptographicException("Error encrypting data.");

            return buff;
        }

        public static byte[] RtpEncode(ushort sequence, uint timestamp, uint ssrc)
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