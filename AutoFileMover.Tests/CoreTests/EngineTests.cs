using System;
using System.IO;
using AutoFileMover.Core;
using AutoFileMover.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoFileMover.Tests.CoreTests
{
    [TestClass]
    public class EngineTests
    {
        [TestMethod]
        public void EngineIntegrationTest()
        {
            var testFolder = Path.Combine(Path.GetTempPath(), "EngineIntegrationTest", Guid.NewGuid().ToString());

            var config = new TestConfig
            {
                DestinationPath = Path.Combine(testFolder, "Output"),
                FileMoveRetries = 3,
                IncludeSubdirectories = false,
                SourcePaths = new[] { Path.Combine(testFolder, "Input") },
                SourceRegex = new[] { "" }
            };


        }
    }
}
