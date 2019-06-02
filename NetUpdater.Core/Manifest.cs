using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetUpdater.Core
{
    public class Manifest
    {
        #region Public Constructors

        public Manifest(string channel, Version version, IEnumerable<(string, Uri)> hashes)
        {
            Channel = channel;
            Version = version;
            Hashes = hashes.ToList();
        }

        #endregion Public Constructors

        #region Public Properties

        public string Channel { get; }

        public IEnumerable<(string, Uri)> Hashes { get; }

        public Version Version { get; }

        #endregion Public Properties
    }
}