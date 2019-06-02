using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class VersionDataWriter : IDisposable
    {
        #region Private Fields

        private readonly StreamWriter writer;

        #endregion Private Fields

        #region Public Constructors

        public VersionDataWriter(Stream stream)
        {
            writer = new StreamWriter(stream);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            writer.Dispose();
        }

        public Task FlushAsync() => writer.FlushAsync();

        public async Task WriteAsync(VersionData versionData)
        {
            await writer.WriteLineAsync(versionData.Version.ToString());
            await writer.WriteLineAsync(versionData.Channel);
            await writer.WriteLineAsync(versionData.ManifestPath.ToString());
        }

        #endregion Public Methods
    }
}