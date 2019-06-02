using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class ManifestWriter : IDisposable
    {
        #region Private Fields

        private readonly StreamWriter writer;

        #endregion Private Fields

        #region Public Constructors

        public ManifestWriter(Stream stream)
        {
            writer = new StreamWriter(stream);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            writer.Dispose();
        }

        public async Task WriteAsync(Manifest manifest)
        {
            await writer.WriteLineAsync(manifest.Version.ToString());
            await writer.WriteLineAsync(manifest.Channel);
            await WriteHashes(manifest.Hashes);
        }

        private async Task WriteHashes(IEnumerable<(string, Uri)> hashes)
        {
            foreach (var hash in hashes)
            {
                await writer.WriteLineAsync($"{hash.Item1}:{hash.Item2}");
            }
        }

        #endregion Public Methods
    }
}