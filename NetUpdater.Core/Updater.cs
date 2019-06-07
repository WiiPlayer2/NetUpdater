using MicroElements.Functional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class Updater
    {
        #region Private Fields

        private readonly string applicationFolder;

        private readonly Uri applicationUri;

        private readonly Option<string> channel;

        private readonly Option<string> manifestName;

        private readonly IUriLocator uriLocator;

        #endregion Private Fields

        #region Public Constructors

        public Updater(IUriLocator uriLocator, string applicationFolder)
            : this(uriLocator, applicationFolder, Option<string>.None, Option<string>.None) { }

        public Updater(IUriLocator uriLocator, string applicationFolder, Option<string> manifestName, Option<string> channel)
        {
            this.uriLocator = uriLocator;
            this.applicationFolder = applicationFolder;
            this.manifestName = manifestName;
            this.channel = channel;

            applicationUri = new Uri(Path.GetFullPath(applicationFolder) + "/", UriKind.Absolute);
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<Result<Option<VersionData>, Exception>> GetNewerVersion(Uri updateUri)
        {
            var localManifest = await GetLocalManifest();
            var versionDataResult = await GetRemoteVersionData(updateUri, GetChannel(localManifest));
            return versionDataResult.Map(versionData => versionData.Version > localManifest.Version ? versionData : Option<VersionData>.None);
        }

        public async Task Update(Uri updateUri)
        {
            var localManifest = await GetLocalManifest();
            var versionDataResult = (await GetNewerVersion(updateUri)).Flatten(() => new ArgumentException());
            var remoteManifestResult = await versionDataResult
                .BindAsync(async versionData => await GetRemoteManifest(new Uri(updateUri, versionData.ManifestPath)));
            await remoteManifestResult.Match(async remoteManifest =>
            {
                var manifestUri = new Uri(updateUri, versionDataResult.GetValueOrThrow().ManifestPath);
                var diff = localManifest.CreateDiff(remoteManifest);

                var updateTasks = diff.Updates.Select(o => UpdateFile(manifestUri, o));
                var deletionTasks = diff.Deletions.Select(o => Delete(o));

                await Task.WhenAll(updateTasks.Concat(deletionTasks));
            }, error => throw new Exception());
        }

        #endregion Public Methods

        #region Private Methods

        private Task Delete(Uri fileUri)
        {
            var localUri = new Uri(applicationUri, fileUri);
            File.Delete(localUri.LocalPath);
            return Task.CompletedTask;
        }

        private string GetChannel(Manifest localManifest) => channel.Match(o => o, localManifest.Channel);

        private async Task<Manifest> GetLocalManifest()
        {
            return await manifestName.Match(async name =>
            {
                using (var stream = File.OpenRead(Path.Combine(applicationFolder, name)))
                using (var reader = new ManifestReader(stream))
                    return (await reader.ReadAsync()).GetValueOrThrow();
            }, async () =>
            {
                return await new ManifestCreator(applicationFolder).CreateAsync(channel.GetValueOrThrow(), new Version(0, 0, 0, 1));
            });
        }

        private async Task<Result<Manifest, Exception>> GetRemoteManifest(Uri manifestUri)
        {
            var streamResult = await Helper.Try(async () => await uriLocator.Locate(manifestUri));
            var manifestResult = await streamResult.BindAsync(async stream =>
            {
                using (stream)
                using (var reader = new ManifestReader(stream))
                    return await reader.ReadAsync();
            });
            return manifestResult;
        }

        private async Task<Result<VersionData, Exception>> GetRemoteVersionData(Uri updateUri, string channel)
        {
            var versionUri = new Uri(updateUri, $"{channel}.version");
            var streamResult = await Helper.Try(async () => await uriLocator.Locate(versionUri));
            var versionDataResult = await streamResult.MapAsync(async stream =>
            {
                using (stream)
                using (var reader = new VersionDataReader(stream))
                    return await reader.ReadAsync();
            });
            return versionDataResult;
        }

        private async Task UpdateFile(Uri updateUri, Uri fileUri)
        {
            var remoteUri = new Uri(updateUri, fileUri);
            var localUri = new Uri(applicationUri, fileUri);

            var stream = await uriLocator.Locate(remoteUri);

            using (stream)
            using (var outStream = File.Create(localUri.LocalPath))
            {
                await stream.CopyToAsync(outStream);
                await stream.FlushAsync();
            }
        }

        #endregion Private Methods
    }
}