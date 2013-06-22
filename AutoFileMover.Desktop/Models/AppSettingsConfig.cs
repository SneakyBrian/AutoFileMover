﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop;

namespace AutoFileMover.Desktop.Models
{
    public class AppSettingsConfig : IConfig
    {
        public IEnumerable<string> SourcePaths
        {
            get { return Properties.Settings.Default.SourcePaths.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)); }
            set 
            { 
                Properties.Settings.Default.SourcePaths = value.Aggregate("", (a, b) => a + "," + b); 
                Properties.Settings.Default.Save(); 
            }
        }

        public IEnumerable<string> SourceRegex
        {
            get { return Properties.Settings.Default.SourceRegex.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)); }
            set 
            { 
                Properties.Settings.Default.SourceRegex = value.Aggregate("", (a, b) => a + "," + b); 
                Properties.Settings.Default.Save(); 
            }
        }

        public string DestinationPath
        {
            get { return Properties.Settings.Default.DestinationPath; }
            set 
            { 
                Properties.Settings.Default.DestinationPath = value; 
                Properties.Settings.Default.Save(); 
            }
        }

        public bool IncludeSubdirectories
        {
            get { return Properties.Settings.Default.IncludeSubdirectories; }
            set 
            { 
                Properties.Settings.Default.IncludeSubdirectories = value;
                Properties.Settings.Default.Save();
            }
        }

        public int FileMoveRetries
        {
            get { return Properties.Settings.Default.FileMoveRetries; }
            set 
            { 
                Properties.Settings.Default.FileMoveRetries = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
