using System;
using System.IO;
using System.Security.Cryptography;

namespace Murmur
{
#if !NET35 && !NETSTANDARD1_4
    public class MurmurInputStream : Stream
    {
        static readonly byte[] DEFAULT_FINAL_TRANFORM = new byte[0];

        readonly Stream UnderlyingStream;
        readonly HashAlgorithm Algorithm;
        public MurmurInputStream(Stream underlyingStream, uint seed = 0, bool managed = true, AlgorithmType type = AlgorithmType.Murmur128, AlgorithmPreference preference = AlgorithmPreference.Auto)
        {
            UnderlyingStream = underlyingStream;
            Algorithm = type == AlgorithmType.Murmur32
                ? (HashAlgorithm)MurmurHash.Create32(seed, managed)
                : (HashAlgorithm)MurmurHash.Create128(seed, managed, preference);
        }

        public byte[] Hash { get { Algorithm.TransformFinalBlock(DEFAULT_FINAL_TRANFORM, 0, 0); return Algorithm.Hash; } }
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { return UnderlyingStream.Length; } }
        public override long Position { get { return UnderlyingStream.Position; } set { throw new NotSupportedException(); } }

        public override void Flush()
        {
            UnderlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = UnderlyingStream.Read(buffer, offset, count);
            Algorithm.TransformBlock(buffer, offset, result, null, 0);

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("This stream does not support seeking, it is forward-only.");
        }

        public override void SetLength(long value)
        {
            UnderlyingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream does not support writing, it is read-only.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Algorithm.Dispose();

            base.Dispose(disposing);
        }
    }
#endif
}
