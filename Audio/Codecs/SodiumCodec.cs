using Frostbyte.Handlers;
using System;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio.Codecs
{
    public sealed class SodiumCodec
    {
        public static int NonceSize
            => (int)SecretBoxNonceSize();

        public static int MacSize
            => (int)SecretBoxMacSize();

        public static int KeySize
            => (int)SecretBoxKeySize();

        public ReadOnlyMemory<byte> Key { get; }

        public SodiumCodec(ReadOnlyMemory<byte> key)
        {
            if (key.Length != KeySize)
            {
                LogHandler<SodiumCodec>.Log.Error($"{key.Length} isn't the same as Sodium's {KeySize} bytes.");
                return;
            }
        }

        [DllImport("sodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int SecretBoxEasy(byte* buffer, byte* message, ulong messageLength, byte* nonce, byte* key);

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxNonceSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxMacSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr SecretBoxKeySize();

        private static unsafe int Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
        {
            var status = 0;
            fixed (byte* sourcePtr = &source.GetPinnableReference())
            fixed (byte* targetPtr = &target.GetPinnableReference())
            fixed (byte* keyPtr = &key.GetPinnableReference())
            fixed (byte* noncePtr = &nonce.GetPinnableReference())
                status = SecretBoxEasy(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);

            return status;
        }

        public void Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
        {
            if (nonce.Length != NonceSize)
            {
                LogHandler<SodiumCodec>.Log.Error($"Nonce length didn't match {NonceSize} bytes.");
                return;
            }

            if (target.Length != MacSize + source.Length)
            {
                LogHandler<SodiumCodec>.Log.Error($"Buffer length wasn't the same as {nameof(MacSize)} + {nameof(source.Length)}.");
                return;
            }

            var result = 0;
            if ((result = Encrypt(source, target, Key.Span, nonce)) != 0)
            {
                LogHandler<SodiumCodec>.Log.Error($"Failed to encrypt buffer -> {result}.");
                return;
            }
        }

        public void GenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
        {
            if (rtpHeader.Length != RTPCodec.HeaderSize)
            {
                LogHandler<SodiumCodec>.Log.Error($"RTP header length {rtpHeader.Length} wasn't the same as default {RTPCodec.HeaderSize}.");
                return;
            }

            if (target.Length != NonceSize)
            {
                LogHandler<SodiumCodec>.Log.Error($"Buffer length didn't match {NonceSize} bytes.");
                return;
            }

            rtpHeader.CopyTo(target);
            AudioHelper.ZeroFill(target.Slice(rtpHeader.Length));
        }
    }
}