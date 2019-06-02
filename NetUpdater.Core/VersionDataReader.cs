using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class VersionDataReader : IDisposable
    {
        #region Private Fields

        private readonly StreamReader reader;

        #endregion Private Fields

        #region Public Constructors

        public VersionDataReader(Stream stream)
        {
            reader = new StreamReader(stream);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            reader.Dispose();
        }

        public async Task<VersionData> ReadAsync()
        {
            var version = Version.Parse(await reader.ReadLineAsync());
            var channel = await reader.ReadLineAsync();
            var manifestUri = new Uri(await reader.ReadLineAsync(), UriKind.Relative);

            return new VersionData(version, channel, manifestUri);
        }

        #endregion Public Methods
    }
}