using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Core
{
    public interface IEngine : IDisposable
    {
        void Start();
        void Stop();
        void Scan();

        IConfig Config { get; set; }

        event EventHandler<FileEventArgs> FileDetected;
        event EventHandler<FileEventArgs> FileMoveStarted;
        event EventHandler<FileMoveEventArgs> FileMoveProgress;
        event EventHandler<FileEventArgs> FileMoveCompleted;
        event EventHandler<FileErrorEventArgs> FileMoveError;

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

    public class FileMoveEventArgs : FileEventArgs
    {
        public int Percentage { get; private set; }

        public FileMoveEventArgs(string filePath, string oldFilePath, long fileSize, int percentage)
            : base(filePath, oldFilePath, fileSize)
        {
            Percentage = percentage;
        }
    }

    public class FileErrorEventArgs : FileEventArgs
    {
        public Exception Exception { get; private set; }

        public FileErrorEventArgs(string filePath, string oldFilePath, long fileSize, Exception ex)
            : base(filePath, oldFilePath, fileSize)
        {
            Exception = ex;
        }
    }
}
