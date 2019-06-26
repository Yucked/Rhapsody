using Frostbyte.Entities.Enums;
using Frostbyte.Handlers;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio
{
    public sealed class Libsodium
    {
        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int SecretBoxEasy(byte* output, byte* input, long inputLength, byte[] nonce, byte[] secret);

        public static int Encrypt(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            unsafe
            {
                fixed (byte* inPtr = input)
                fixed (byte* outPtr = output)
                {
                    var error = SecretBoxEasy(outPtr + outputOffset, inPtr + inputOffset, inputLength, nonce, secret);
                    if (error != 0)
                    {
                        LogHandler<Libsodium>.Log.RawLog(LogLevel.Critical, "Libsodium threw an error.", default);
                        return 0;
                    }

                    return inputLength + 16;
                }
            }
        }
    }
}