using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core;

namespace AutoFileMover.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config
            {
                DestinationPath = @"D:\Temp\AFMTEST\Output",
                FileMoveRetries = 3,
                IncludeSubdirectories = false,
                SourcePaths = new[] { @"D:\Temp\AFMTEST\Input" },
                SourceRegex = new[] { @"^(\b.*\b(?=[.])).*(?<Season>(?:(?<=s)[1-9][0-9]|(?<=s0)[1-9])).*\.(?:avi|mkv|mp4)$" }
            };

            using (var engine = new Engine())
            {
                engine.Config = config;

                engine.Starting += (s, e) => System.Console.WriteLine("Engine Starting");
                engine.Started += (s, e) => System.Console.WriteLine("Engine Started");
                engine.Stopping += (s, e) => System.Console.WriteLine("Engine Stopping");
                engine.Stopped += (s, e) => System.Console.WriteLine("Engine Stopped");

                engine.Error += (s, e) => System.Console.WriteLine("Engine Error: {0}", e.GetException().Message);

                engine.FileDetected += (s, e) => System.Console.WriteLine("Engine FileDetected: {0}", e.OldFilePath);
                engine.FileMoveStarted += (s, e) => System.Console.WriteLine("Engine FileMoveStarted: {0} -> {1}", e.OldFilePath, e.FilePath);
                engine.FileMoveProgress += (s, e) => System.Console.WriteLine("Engine FileMoveProgress: {0} -> {1} - {2}%", e.OldFilePath, e.FilePath, e.Percentage);
                engine.FileMoveError += (s, e) => System.Console.WriteLine("Engine FileMoveError: {0} -> {1} - {2}", e.OldFilePath, e.FilePath, e.Exception.Message);
                engine.FileMoveCompleted += (s, e) => System.Console.WriteLine("Engine FileMoveCompleted: {0} -> {1}", e.OldFilePath, e.FilePath);

                //start the engine
                engine.Start();

                // Wait for the user to quit the program.
                System.Console.WriteLine("Press \'q\' to quit.");
                while (System.Console.Read() != 'q') ;
            }
        }
    }
}
