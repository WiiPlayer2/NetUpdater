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
            HashMap = Hashes.ToDictionary(o => o.Item2, o => o.Item1);
        }

        #endregion Public Constructors

        #region Public Properties

        public string Channel { get; }

        public IEnumerable<(string, Uri)> Hashes { get; }

        public IReadOnlyDictionary<Uri, string> HashMap { get; }

        public Version Version { get; }

        #endregion Public Properties

        #region Public Methods

        public Diff CreateDiff(Manifest target)
        {
            var newFiles = target.HashMap.Keys.Except(HashMap.Keys);
            var oldFiles = HashMap.Keys.Except(target.HashMap.Keys);
            var updatedFiles = HashMap.Keys.Intersect(target.HashMap.Keys).Where(o => HashMap[o] != target.HashMap[o]);

            return new Diff(newFiles.Concat(updatedFiles), oldFiles);
        }

        #endregion Public Methods
    }
}