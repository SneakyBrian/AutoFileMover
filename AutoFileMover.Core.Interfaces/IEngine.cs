using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Core.Interfaces
{
    public interface IEngine : IDisposable
    {
        void Start();
        void Stop();
        void Scan();

        IConfig Config { get; set; }

        event EventHandler<FileEventArgs> FileDetected;
        event EventHandler<FileEventArgs> FileMoveStarted;
        event EventHandler<FileHashEventArgs> FileHashProgress;
        event EventHandler<FileMoveEventArgs> FileMoveProgress;
        event EventHandler<FileEventArgs> FileMoveCompleted;
        event EventHandler<FileErrorEventArgs> FileMoveError;
        event EventHandler<FileRetryEventArgs> FileRetryWaiting;
        event EventHandler<FileTryEventArgs> FileRetrying;

        event EventHandler Starting;
        event EventHandler Started;
        event EventHandler Stopping;
        event EventHandler Stopped;
        event EventHandler<ErrorEventArgs> Error;
    }

    public class FileEventArgs : EventArgs
    {
        public string OldFilePath { get; private set; }
        public string FilePath { get; private set; }
        public long FileSize { get; private set; }

        public FileEventArgs(string filePath, string oldFilePath, long fileSize)
        {
            FilePath = filePath;
            OldFilePath = oldFilePath;
            FileSize = fileSize;
        }
    }

    public class FileTryEventArgs : FileEventArgs
    {
        public int Tries { get; private set; }

        public FileTryEventArgs(string filePath, string oldFilePath, long fileSize, int tries)
            : base(filePath, oldFilePath, fileSize)
        {
            Tries = tries;
        }

    }

    public class FileMoveEventArgs : FileTryEventArgs
    {
        public int Percentage { get; private set; }

        public FileMoveEventArgs(string filePath, string oldFilePath, long fileSize, int percentage, int tries)
            : base(filePath, oldFilePath, fileSize, tries)
        {
            Percentage = percentage;
        }
    }

    public class FileHashEventArgs : FileTryEventArgs
    {
        public int Percentage { get; private set; }
        public string Hash { get; private set; }
        public string HashFilePath { get; private set; }

        public FileHashEventArgs(string filePath, string oldFilePath, string hashFilePath, string hash, long fileSize, int percentage, int tries)
            : base(filePath, oldFilePath, fileSize, tries)
        {
            Percentage = percentage;
            Hash = hash;
            HashFilePath = hashFilePath;
        }
    }

    public class FileErrorEventArgs : FileTryEventArgs
    {
        public Exception Exception { get; private set; }

        public FileErrorEventArgs(string filePath, string oldFilePath, long fileSize, Exception ex, int tries)
            : base(filePath, oldFilePath, fileSize, tries)
        {
            Exception = ex;
        }
    }

    public class FileRetryEventArgs : FileTryEventArgs
    {
        public TimeSpan RetryDelay { get; private set; }

        public FileRetryEventArgs(string filePath, string oldFilePath, long fileSize, int tries, TimeSpan retryDelay)
            : base(filePath, oldFilePath, fileSize, tries)
        {
            RetryDelay = retryDelay;
        }
    }
}
