﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18047
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoFileMover.Desktop.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Temp\\AFMTEST\\Input")]
        public string SourcePaths {
            get {
                return ((string)(this["SourcePaths"]));
            }
            set {
                this["SourcePaths"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("^(\\b.*\\b(?=[.])).*(?<Season>(?:(?<=s)[1-9][0-9]|(?<=s0)[1-9])).*\\.(?:avi|mkv|mp4)" +
            "$")]
        public string SourceRegex {
            get {
                return ((string)(this["SourceRegex"]));
            }
            set {
                this["SourceRegex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Temp\\AFMTEST\\Output")]
        public string DestinationPath {
            get {
                return ((string)(this["DestinationPath"]));
            }
            set {
                this["DestinationPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IncludeSubdirectories {
            get {
                return ((bool)(this["IncludeSubdirectories"]));
            }
            set {
                this["IncludeSubdirectories"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int FileMoveRetries {
            get {
                return ((int)(this["FileMoveRetries"]));
            }
            set {
                this["FileMoveRetries"] = value;
            }
        }
    }
}
