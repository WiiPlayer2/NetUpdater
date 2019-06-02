using GlobExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class ManifestCreator
    {
        #region Private Fields

        private readonly Glob glob;
        private readonly DirectoryInfo rootFolder;

        #endregion Private Fields

        #region Public Constructors

        public ManifestCreator(string rootFolder)
            : this(rootFolder, null) { }

        public ManifestCreator(string rootFolder, Glob glob)
        {
            this.rootFolder = new DirectoryInfo(rootFolder);
            this.glob = glob ?? new Glob("*");
        }

        #endregion Public Constructors

        #region Public Methods

        public Task<Manifest> CreateAsync(string channel, Version version)
        {
            var rootUri = new Uri(rootFolder.FullName + "/", UriKind.Absolute);
            var hashes = rootFolder.EnumerateFiles("*", SearchOption.AllDirectories)
                .AsParallel()
                .Select(file => (Info: file, RelativeUri: rootUri.MakeRelativeUri(new Uri(file.FullName, UriKind.Absolute))))
                .Where(o => glob.IsMatch(o.RelativeUri.ToString()))
                .Select(o => (Hash: o.Info.ComputeHash(), o.RelativeUri));

            return Task.Run(() => new Manifest(channel, version, hashes));
        }

        #endregion Public Methods
    }
}