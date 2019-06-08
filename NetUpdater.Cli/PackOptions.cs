using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetUpdater.Cli
{
    [Verb("pack", HelpText = "Packs an application to provide an update package.")]
    internal class PackOptions
    {
        #region Public Properties

        [Option('a', "applicationPath", Required = true, HelpText = "Sets the application path.")]
        public Uri ApplicationPath { get; set; }

        [Option('c', "channel", Required = true, HelpText = "Sets the pack channel.")]
        public string Channel { get; set; }

        [Option('m', "manifest", Default = ".manifest", HelpText = "Sets the manifest file name.")]
        public string ManifestName { get; set; }

        [Option('o', "output", Default = "pack.zip", HelpText = "Sets the output path of the generated package.")]
        public string OutputPath { get; set; }

        [Option('v', "output-version", Required = true, HelpText = "Sets the pack version.")]
        public Version Version { get; set; }

        #endregion Public Properties
    }
}