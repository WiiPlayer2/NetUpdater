using System;
using System.Collections.Generic;
using System.Text;

namespace NetUpdater.Core
{
    public class VersionData
    {
        #region Public Constructors

        public VersionData(Version version, string channel, Uri manifestPath)
        {
            Version = version;
            Channel = channel;
            ManifestPath = manifestPath;
        }

        #endregion Public Constructors

        #region Public Properties

        public string Channel { get; }

        public Uri ManifestPath { get; }

        public Version Version { get; }

        #endregion Public Properties
    }
}