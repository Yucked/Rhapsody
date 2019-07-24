using System;
using System.Runtime.InteropServices;
using Frostbyte.Factories;

namespace Frostbyte.Audio.Codecs
{
    public sealed class SodiumCodec
    {
        public static int NonceSize
            => (int) SecretBoxNonceSize();

        public static int MacSize
            => (int) SecretBoxMacSize();

        public static int KeySize
            => (int) SecretBoxKeySize();

        [DllImport("sodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SecretBoxEasy(byte* buffer, byte* message, ulong messageLength, byte* nonce,
            byte* key);

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxNonceSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxMacSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxKeySize();

        private static unsafe int Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key,
            ReadOnlySpan<byte> nonce)
        {
            int status;
            fixed (byte* sourcePtr = &source.GetPinnableReference())
            fixed (byte* targetPtr = &target.GetPinnableReference())
            fixed (byte* keyPtr = &key.GetPinnableReference())
            fixed (byte* noncePtr = &nonce.GetPinnableReference())
            {
                status = SecretBoxEasy(targetPtr, sourcePtr, (ulong) source.Length, noncePtr, keyPtr);
            }

            return status;
        }

        public static bool TryEncrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce,
            ReadOnlyMemory<byte> key)
        {
            if (nonce.Length != NonceSize)
            {
                LogFactory.Error<SodiumCodec>($"Nonce length didn't match {NonceSize} bytes.");
                return false;
            }

            if (target.Length != MacSize + source.Length)
            {
                LogFactory.Error<SodiumCodec>(
                    $"Buffer length wasn't the same as {nameof(MacSize)} + {nameof(source.Length)}.");
                return false;
            }

            if (key.Length != KeySize)
            {
                LogFactory.Error<SodiumCodec>($"{key.Length} isn't the same as Sodium's {KeySize} bytes.");
                return false;
            }

            int result;
            if ((result = Encrypt(source, target, key.Span, nonce)) == 0)
                return true;

            LogFactory.Error<SodiumCodec>($"Failed to encrypt buffer -> {result}.");
            return false;
        }

        public static bool TryGenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
        {
            if (rtpHeader.Length != RtpCodec.HEADER_SIZE)
            {
                LogFactory.Error<SodiumCodec>(
                    $"RTP header length {rtpHeader.Length} wasn't the same as default {RtpCodec.HEADER_SIZE}.");
                return false;
            }

            if (target.Length != NonceSize)
            {
                LogFactory.Error<SodiumCodec>($"Buffer length didn't match {NonceSize} bytes.");
                return false;
            }

            rtpHeader.CopyTo(target);
            AudioHelper.ZeroFill(target.Slice(rtpHeader.Length));

            return true;
        }
    }
}