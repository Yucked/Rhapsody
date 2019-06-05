using CSCore;
using System.IO;

namespace Frostbyte.Handlers.Streams
{
    public sealed class AudioStream : Stream
    {
        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
        public bool IsFinished { get; private set; }

        private int EndTime;
        private IWaveSource OldWave;
        private ISampleSource Source;
        private EqualizerSampleSource Equalizer;
        private AudioPcm16 AudioPcm;
        private readonly Stream _stream;

        public AudioStream(Stream stream)
        {
            _stream = stream;

            //OldWave = new FfmpegDecoder(stream).ChangeSampleRate(48000);
            Source = OldWave.ToSampleSource().ToStereo();
            Equalizer = new EqualizerSampleSource(Source);
            AudioPcm = new AudioPcm16(Equalizer);

            CanRead = true;
            CanSeek = true;
            CanWrite = true;
            IsFinished = false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsFinished)
                return 0;

            if ((OldWave.Length != 0 && OldWave.Position == OldWave.Length) || (EndTime != -1 && AudioPcm.Position >= EndTime))
            {
                IsFinished = true;
                return 0;
            }

            var read = AudioPcm.Read(buffer, offset, count);
            if (read == 0)
            {
                IsFinished = true;
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return origin switch
            {
                SeekOrigin.Begin    => Position = offset,
                SeekOrigin.Current  => Position += offset,
                SeekOrigin.End      => Position = Length - Position
            };
        }

        public override void Close()
        {
            IsFinished = true;
            base.Close();
        }

        public override void Flush() { }

        public override void SetLength(long value) { }

        public override void Write(byte[] buffer, int offset, int count) { }
    }
}