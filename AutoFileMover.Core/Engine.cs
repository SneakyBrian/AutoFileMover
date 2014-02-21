using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Support;

namespace AutoFileMover.Core
{
    public class Engine : IEngine
    {
        private IEnumerable<FileSystemWatcher> _watchers;
        private IEnumerable<Regex> _regexList;

        private TaskFactory _taskFactory;
        
        public void Start()
        {
            Stop();

            OnStarting();

            _taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(Config.ConcurrentOperations));

            //build the regex list
            _regexList = Config.SourceRegex.Select(sr => new Regex(sr, RegexOptions.Compiled | RegexOptions.IgnoreCase));

            _watchers = Config.SourcePaths.Where(sp => Directory.Exists(sp)).Select(sp =>
            {
                var fsw = new FileSystemWatcher(sp);

                fsw.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName;

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
            _taskFactory.StartNew(() => AsyncHelpers.RunSync(() => ProcessFileAsync(filePath)));
        }

        private async Task ProcessFileAsync(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            var fileName = Path.GetFileName(filePath);

            foreach (var regex in _regexList)
            {
                var match = regex.Match(fileName);

                if (match.Success)
                {
                    OnFileDetected(null, filePath, fileInfo.Length); 
                    
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

                    //Phase 1 - Copy file from source to destination

                    int tries = 0;
                    for (; tries < Config.FileMoveRetries; tries++)
                    {
                        try
                        {
                            if (!File.Exists(outputPath))
                            {
                                await AsyncFileCopier.CopyFile(filePath, outputPath, percentage =>
                                {
                                    OnFileMoveProgress(outputPath, filePath, fileInfo.Length, percentage, tries);
                                }); 
                            }

                            if (Config.VerifyFiles)
                            {
                                var hasher = new AsyncFileHashAlgorithm(SHA1.Create());

                                long inputSize = 0;

                                await hasher.ComputeHash(filePath, (o, fhp) =>
                                {
                                    OnFileHashProgress(outputPath, filePath, filePath, "", inputSize = fhp.TotalSize, fhp.Percentage, tries);
                                });

                                string inputHash = hasher.ToString();

                                OnFileHashProgress(outputPath, filePath, filePath, inputHash, inputSize, 100, tries);

                                long outputSize = 0;

                                await hasher.ComputeHash(outputPath, (o, fhp) =>
                                {
                                    OnFileHashProgress(outputPath, filePath, outputPath, "", outputSize = fhp.TotalSize, fhp.Percentage, tries);
                                });

                                string outputHash = hasher.ToString();

                                OnFileHashProgress(outputPath, filePath, outputPath, outputHash, outputSize, 100, tries);

                                if (inputHash != outputHash)
                                {
                                    //make sure it's not readonly and just nuke it
                                    File.SetAttributes(outputPath, FileAttributes.Normal);
                                    File.Delete(outputPath);

                                    throw new ApplicationException(string.Format("Source File '{0}' hash '{1}' does not match Destination File '{2}' hash '{3}'", 
                                        filePath, inputHash, outputPath, outputHash));
                                }
                            }

                            break;
                        }
                        catch (Exception ex)
                        {
                            OnFileMoveError(outputPath, filePath, fileInfo.Length, ex, tries);
                        }

                        await Task.Delay(Config.TimeBetweenRetries);
                    }

                    //Phase 2 = delete source file

                    for (; tries < Config.FileMoveRetries; tries++)
                    {
                        try
                        {
                            File.SetAttributes(filePath, FileAttributes.Normal);
                            File.Delete(filePath);

                            break;
                        }
                        catch (Exception ex)
                        {
                            OnFileMoveError(outputPath, filePath, fileInfo.Length, ex, tries);
                        }

                        await Task.Delay(Config.TimeBetweenRetries);
                    }    

                    OnFileMoveCompleted(outputPath, filePath, fileInfo.Length);

                    break;
                }
            }
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

        public void Scan()
        {
            Config.SourcePaths
                .SelectMany(p => Directory.GetFiles(p, "*.*", Config.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                .ToList()
                .ForEach(ProcessFile);
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

        private void OnFileMoveProgress(string filePath, string oldFilePath, long fileSize, int percentage, int tries)
        {
            var handler = FileMoveProgress;
            if (handler != null)
            {
                handler(this, new FileMoveEventArgs(filePath, oldFilePath, fileSize, percentage, tries));
            }
        }

        public event EventHandler<FileHashEventArgs> FileHashProgress;

        private void OnFileHashProgress(string filePath, string oldFilePath, string hashFilePath, string hash, long fileSize, int percentage, int tries)
        {
            var handler = FileHashProgress;
            if (handler != null)
            {
                handler(this, new FileHashEventArgs(filePath, oldFilePath, hashFilePath, hash, fileSize, percentage, tries));
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

        private void OnFileMoveError(string filePath, string oldFilePath, long fileSize, Exception ex, int tries)
        {
            var handler = FileMoveError;
            if (handler != null)
            {
                handler(this, new FileErrorEventArgs(filePath, oldFilePath, fileSize, ex, tries));
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
