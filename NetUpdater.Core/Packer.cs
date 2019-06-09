using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class Packer
    {
        #region Private Fields

        private readonly DirectoryInfo applicationDirectory;

        private readonly string channel;

        private readonly GenerateFolderNameDelegate generateFolderName;

        private readonly string manifestName;

        #endregion Private Fields

        #region Public Delegates

        public delegate string GenerateFolderNameDelegate(string channel, Version version);

        #endregion Public Delegates

        #region Public Constructors

        public Packer(string applicationPath, string manifestName, string channel, GenerateFolderNameDelegate generateFolderName = null)
        {
            applicationDirectory = new DirectoryInfo(applicationPath);
            this.manifestName = manifestName;
            this.channel = channel;
            this.generateFolderName = generateFolderName ?? GenerateDefaultFolderName;
        }

        #endregion Public Constructors

        #region Private Methods

        private string GenerateDefaultFolderName(string channel, Version version) => $"{channel}_{version}";

        #endregion Private Methods

        #region Public Methods

        public async Task Pack(Version version, string outputPath)
        {
            var manifest = await CreateManifest(version);
            var manifestPath = await WriteManifest(manifest);
            await Pack(version, manifest, manifestPath, outputPath);
        }

        private Task<Manifest> CreateManifest(Version version) => new ManifestCreator(applicationDirectory.FullName).CreateAsync(channel, version);

        private async Task<string> GenerateVersionData(Manifest manifest, Uri manifestEntryUri)
        {
            var versionData = new VersionData(manifest.Version, manifest.Channel, manifestEntryUri);

            var path = Path.GetTempFileName();
            using (var stream = File.OpenWrite(path))
            using (var writer = new VersionDataWriter(stream))
            {
                await writer.WriteAsync(versionData);
                await writer.FlushAsync();
            }
            return path;
        }

        private async Task Pack(Version version, Manifest manifest, string manifestPath, string outputPath)
        {
            var baseFileUri = new Uri($"{applicationDirectory.FullName}/");
            var baseEntryUri = new Uri(generateFolderName(channel, version), UriKind.Relative);
            var tmpPath = Path.GetTempFileName();
            var zip = ZipFile.Create(tmpPath);

            zip.BeginUpdate();
            foreach (var fileUri in manifest.HashMap.Keys)
            {
                var pathUri = new Uri(baseFileUri, fileUri);
                var entryUri = new Uri($"{baseEntryUri}/{fileUri}", UriKind.Relative);

                zip.Add(pathUri.LocalPath, entryUri.ToString());
            }

            var manifestUri = new Uri($"{baseEntryUri}/{manifestName}", UriKind.Relative);
            zip.Add(manifestPath, manifestUri.ToString());

            var versionDataPath = await GenerateVersionData(manifest, manifestUri);
            zip.Add(versionDataPath, $"{manifest.Channel}.version");

            zip.CommitUpdate();
            zip.Close();

            File.Copy(tmpPath, outputPath, true);
        }

        private async Task<string> WriteManifest(Manifest manifest)
        {
            var manifestPath = Path.GetTempFileName();
            using (var stream = File.OpenWrite(manifestPath))
            using (var writer = new ManifestWriter(stream))
            {
                await writer.WriteAsync(manifest);
                await writer.FlushAsync();
            }
            return manifestPath;
        }

        #endregion Public Methods
    }
}