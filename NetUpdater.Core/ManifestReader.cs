using MicroElements.Functional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class ManifestReader : IDisposable
    {
        #region Private Fields

        private readonly StreamReader reader;

        #endregion Private Fields

        #region Public Constructors

        public ManifestReader(Stream stream)
        {
            reader = new StreamReader(stream);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            reader.Dispose();
        }

        public async Task<Result<Manifest, Exception>> ReadAsync()
        {
            var versionResult = await ReadVersion();
            var channelResult = await ReadChannel();
            var hashesResult = await ReadHashes();

            return versionResult.Bind(version =>
                channelResult.Bind(channel =>
                    hashesResult.Map(hashes =>
                        new Manifest(channel, version, hashes))));
        }

        private Task<Result<string, Exception>> ReadChannel() => Helper.Try(async () =>
        {
            const string regexString = @"[a-z][a-z0-9]*";
            var line = await reader.ReadLineAsync();
            if (line.Matches(regexString))
                return line;
            else
                throw new ArgumentOutOfRangeException();
        });

        private Task<Result<(string, Uri), Exception>> ReadHash() => Helper.Try(async () =>
                {
                    var regex = new Regex(@"^(?<hash>[a-fA-F0-9]{128})\:(?<path>.*)$");
                    var line = await reader.ReadLineAsync();
                    var match = regex.Match(line);

                    if (!match.Success)
                        throw new ArgumentOutOfRangeException();

                    var hash = match.Groups["hash"].Value;
                    var path = match.Groups["path"].Value;

                    if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
                        throw new ArgumentOutOfRangeException();

                    return (hash, new Uri(path, UriKind.Relative));
                });

        private Task<Result<IEnumerable<(string, Uri)>, Exception>> ReadHashes() => Helper.Try<IEnumerable<(string, Uri)>>(async () =>
        {
            var list = new List<(string, Uri)>();
            while (!reader.EndOfStream)
            {
                list.Add((await ReadHash()).GetValueOrThrow());
            }
            return list;
        });

        private Task<Result<Version, Exception>> ReadVersion() => Helper.Try(async () => Version.Parse(await reader.ReadLineAsync()));

        #endregion Public Methods
    }
}