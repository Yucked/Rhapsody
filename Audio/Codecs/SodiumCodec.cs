using System;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio.Codecs
{
    public sealed class SodiumCodec
    {
        public static int NonceSize
            => (int)SecretBoxNonceSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxNonceSize();

        [DllImport("sodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int SecretBoxEasy(byte* buffer, byte* message, ulong messageLength, byte* nonce, byte* key);

        public static unsafe int Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
        {
            var status = 0;
            fixed (byte* sourcePtr = &source.GetPinnableReference())
            fixed (byte* targetPtr = &target.GetPinnableReference())
            fixed (byte* keyPtr = &key.GetPinnableReference())
            fixed (byte* noncePtr = &nonce.GetPinnableReference())
                status = SecretBoxEasy(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);

            return status;
        }

        public void GenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
        {
            if (rtpHeader.Length != RTPCodec.HeaderSize)
                throw new ArgumentException($"RTP header needs to have a length of exactly {RTPCodec.HeaderSize} bytes.", nameof(rtpHeader));

            if (target.Length != NonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {NonceSize} bytes.", nameof(target));

            rtpHeader.CopyTo(target);
            ZeroFill(target.Slice(rtpHeader.Length));
        }

        public static void ZeroFill(Span<byte> buff)
        {
            var zero = 0;
            var i = 0;
            for (; i < buff.Length / 4; i++)
                MemoryMarshal.Write(buff, ref zero);

            var remainder = buff.Length % 4;
            if (remainder == 0)
                return;

            for (; i < buff.Length; i++)
                buff[i] = 0;
        }
    }
}