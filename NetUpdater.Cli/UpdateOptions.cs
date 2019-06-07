using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetUpdater.Cli
{
    [Verb("update", HelpText = "Updates an application packed with NetUpdater.")]
    internal class UpdateOptions
    {
        #region Public Properties

        [Option('a', "applicationPath", Required = true, HelpText = "Sets the application path.")]
        public Uri ApplicationPath { get; set; }

        [Option('m', "manifestName", Required = true, HelpText = "Sets the manifest name of the application.")]
        public string ManifestName { get; set; }

        [Option('p', "pid", HelpText = "Sets the pid of the process to wait for exit before updating.")]
        public int? Pid { get; set; }

        [Option('u', "updateUrl", Required = true, HelpText = "Sets the update url to download contents from.")]
        public Uri UpdateUrl { get; set; }

        #endregion Public Properties
    }
}