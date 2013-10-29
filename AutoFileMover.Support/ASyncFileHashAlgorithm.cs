using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Support
{
    public class AsyncFileHashAlgorithm
    {
        protected HashAlgorithm _hashAlgorithm;
        protected byte[] _hash = new byte[0];
        protected int _bufferSize = 1024 * 1024;

        public AsyncFileHashAlgorithm(HashAlgorithm hashAlgorithm)
        {
            this._hashAlgorithm = hashAlgorithm;
        }

        public async Task<byte[]> ComputeHash(string filePath, EventHandler<FileHashingProgressArgs> handler)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return await ComputeHash(fileStream, handler);
            }
        }

        public async Task<byte[]> ComputeHash(Stream stream, EventHandler<FileHashingProgressArgs> handler)
        {
            _hash = null;
            int bufferSize = _bufferSize;

            byte[] readAheadBuffer, buffer;
            int readAheadBytesRead, bytesRead;
            long size, totalBytesRead = 0;

            _hashAlgorithm.Initialize();

            size = stream.Length;
            readAheadBuffer = new byte[bufferSize];
            readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0, readAheadBuffer.Length);

            totalBytesRead += readAheadBytesRead;

            do
            {
                bytesRead = readAheadBytesRead;
                buffer = readAheadBuffer;

                readAheadBuffer = new byte[bufferSize];
                readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0, readAheadBuffer.Length);

                totalBytesRead += readAheadBytesRead;

                if (readAheadBytesRead == 0)
                    _hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                else
                    _hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);

                if (handler != null)
                {
                    handler(this, new FileHashingProgressArgs(totalBytesRead, size));
                }

            } while (readAheadBytesRead != 0);

            _hash = _hashAlgorithm.Hash;
            
            return _hash;
        }

        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        public byte[] Hash
        {
            get { return _hash; }
        }

        public override string ToString()
        {
            return ConvertHashToString(_hash);
        }

        private static string ConvertHashToString(byte[] hash)
        {
            string hex = "";
            foreach (byte b in hash)
                hex += b.ToString("x2");

            return hex;
        }
    }

    public class FileHashingProgressArgs : EventArgs
    {
        public long ProcessedSize { get; private set; }
        public long TotalSize { get; private set; }
        public int Percentage { get { return (int)((((double)ProcessedSize) / ((double)TotalSize)) * 100.0); } }

        public FileHashingProgressArgs(long processedSize, long totalSize)
        {
            ProcessedSize = processedSize;
            TotalSize = totalSize;
        }
    }
}
