using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoFileMover.Core
{
    public class Engine : IEngine
    {
        private IEnumerable<FileSystemWatcher> _watchers;
        private IEnumerable<Regex> _regexList;
        
        public void Start()
        {
            Stop();

            OnStarting();

            //build the regex list
            _regexList = Config.SourceRegex.Select(sr => new Regex(sr, RegexOptions.Compiled | RegexOptions.IgnoreCase));

            _watchers = Config.SourcePaths.Select(sp =>
            {
                var fsw = new FileSystemWatcher(sp);

                fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.Security | NotifyFilters.Size;

                fsw.IncludeSubdirectories = Config.IncludeSubdirectories;

                fsw.Created += fsw_Changed;
                fsw.Renamed += fsw_Renamed;
                fsw.Error += fsw_Error;
                fsw.Changed += fsw_Changed;

                fsw.EnableRaisingEvents = true;

                return fsw;
            }).ToList();

            OnStarted();

        }

        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            ProcessFile(e.FullPath);
        }

        void fsw_Error(object sender, ErrorEventArgs e)
        {
            OnError(e.GetException());
        }

        void fsw_Renamed(object sender, RenamedEventArgs e)
        {
            ProcessFile(e.FullPath);
        }

        private void ProcessFile(string filePath)
        {
            Task.Factory.StartNew(() =>
            {
                var fileInfo = new FileInfo(filePath);

                OnFileDetected(null, filePath, fileInfo.Length);

                var fileName = Path.GetFileName(filePath);

                foreach (var regex in _regexList)
                {
                    var match = regex.Match(fileName);

                    if (match.Success)
                    {
                        var outputPath = Config.DestinationPath;

                        foreach (string groupName in regex.GetGroupNames())
                        {
                            string prefix = string.Empty;
                            int index;
                            if (int.TryParse(groupName, out index))
                            {
                                if (index == 0)
                                    continue;
                            }
                            else
                            {
                                prefix = groupName + " ";
                            }

                            outputPath = Path.Combine(outputPath, string.Format("{0}{1}", prefix, match.Groups[groupName].Value.Replace(".", " ")));
                        }

                        //ensure the directory has been created
                        Directory.CreateDirectory(outputPath);

                        outputPath = Path.Combine(outputPath, fileName);

                        OnFileMoveStarted(outputPath, filePath, fileInfo.Length);

                        for (int i = 0; i < Config.FileMoveRetries; i++)
                        {
                            try
                            {
                                XCopy.Copy(filePath, outputPath, true, true, (o, pce) =>
                                {
                                    OnFileMoveProgress(outputPath, filePath, fileInfo.Length, pce.ProgressPercentage);
                                });
                                break;
                            }
                            catch (Exception ex)
                            {
                                OnFileMoveError(outputPath, filePath, fileInfo.Length, ex);
                                Thread.Sleep(1000 * (i + 1));
                            }
                        }

                        for (int i = 0; i < Config.FileMoveRetries && File.Exists(filePath); i++)
                        {
                            try
                            {
                                File.Delete(filePath);
                                break;
                            }
                            catch (Exception ex)
                            {
                                OnFileMoveError(outputPath, filePath, fileInfo.Length, ex);
                                Thread.Sleep(1000 * (i + 1));
                            }
                        }

                        OnFileMoveCompleted(outputPath, filePath, fileInfo.Length);

                        break;
                    }
                }
            });
        }

        public void Stop()
        {
            OnStopping();

            if (_watchers != null)
            {
                foreach (var fsw in _watchers)
                {
                    fsw.EnableRaisingEvents = false;
                }

                foreach (var fsw in _watchers)
                {
                    fsw.Created -= fsw_Changed;
                    fsw.Renamed -= fsw_Renamed;
                    fsw.Error -= fsw_Error;
                    fsw.Changed -= fsw_Changed;

                    fsw.Dispose();
                }
            }

            OnStopped();
        }

        public IConfig Config { get; set; }

        public event EventHandler<FileEventArgs> FileDetected;

        private void OnFileDetected(string filePath, string oldFilePath, long fileSize)
        {
            var handler = FileDetected;
            if (handler != null)
            {
                handler(this, new FileEventArgs(filePath, oldFilePath, fileSize));
            }
        }

        public event EventHandler<FileEventArgs> FileMoveStarted;

        private void OnFileMoveStarted(string filePath, string oldFilePath, long fileSize)
        {
            var handler = FileMoveStarted;
            if (handler != null)
            {
                handler(this, new FileEventArgs(filePath, oldFilePath, fileSize));
            }
        }

        public event EventHandler<FileMoveEventArgs> FileMoveProgress;

        private void OnFileMoveProgress(string filePath, string oldFilePath, long fileSize, int percentage)
        {
            var handler = FileMoveProgress;
            if (handler != null)
            {
                handler(this, new FileMoveEventArgs(filePath, oldFilePath, fileSize, percentage));
            }
        }
        
        public event EventHandler<FileEventArgs> FileMoveCompleted;

        private void OnFileMoveCompleted(string filePath, string oldFilePath, long fileSize)
        {
            var handler = FileMoveCompleted;
            if (handler != null)
            {
                handler(this, new FileEventArgs(filePath, oldFilePath, fileSize));
            }
        }

        public event EventHandler<FileErrorEventArgs> FileMoveError;

        private void OnFileMoveError(string filePath, string oldFilePath, long fileSize, Exception ex)
        {
            var handler = FileMoveError;
            if (handler != null)
            {
                handler(this, new FileErrorEventArgs(filePath, oldFilePath, fileSize, ex));
            }
        }

        public event EventHandler Starting;

        private void OnStarting()
        {
            var handler = Starting;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler Started;

        private void OnStarted()
        {
            var handler = Started;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler Stopping;

        private void OnStopping()
        {
            var handler = Stopping;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler Stopped;

        private void OnStopped()
        {
            var handler = Stopped;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public event EventHandler<ErrorEventArgs> Error;

        private void OnError(Exception ex)
        {
            var handler = Error;
            if (handler != null)
            {
                handler(this, new ErrorEventArgs(ex));
            }
        }
    }
}
