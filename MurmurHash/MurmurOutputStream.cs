using System;
using System.IO;
using System.Security.Cryptography;

namespace Murmur
{
    /// <summary>
    /// Exposes the murmur algorithm as a pass through stream that computes the hash incrementally.
    /// </summary>
    public class MurmurOutputStream : Stream
    {
        readonly Stream UnderlyingStream;
        readonly HashAlgorithm Algorithm;
        public MurmurOutputStream(Stream underlyingStream, uint seed = 0, bool managed = true, AlgorithmType type = AlgorithmType.Murmur128, AlgorithmPreference preference = AlgorithmPreference.Auto)
        {
            UnderlyingStream = underlyingStream;
            Algorithm = type == AlgorithmType.Murmur32
                ? (HashAlgorithm)MurmurHash.Create32(seed, managed)
                : (HashAlgorithm)MurmurHash.Create128(seed, managed, preference);
        }

        public byte[] Hash { get { return Algorithm.Hash; } }
        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { return UnderlyingStream.Length; } }
        public override long Position { get { return UnderlyingStream.Position; } set { throw new NotSupportedException(); } }

        public override void Flush()
        {
            UnderlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream does not support reading.");
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
            Algorithm.ComputeHash(buffer, offset, count);
            UnderlyingStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Algorithm.Dispose();

            base.Dispose(disposing);
        }
    }
}
