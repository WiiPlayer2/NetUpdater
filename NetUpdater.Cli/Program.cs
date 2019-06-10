using CommandLine;
using MicroElements.Functional;
using NetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NetUpdater.Cli
{
    internal class Program
    {
        #region Private Methods

        private static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<PackOptions, UpdateOptions>(args)
                .MapResult<PackOptions, UpdateOptions, Task>(RunPack, RunUpdate, RunError);
        }

        private static Task RunError(IEnumerable<Error> arg) => Task.CompletedTask;

        private static async Task RunPack(PackOptions options)
        {
            Console.WriteLine($"Packing \"{options.ApplicationPath}\"...");
            var packer = new Packer(options.ApplicationPath, options.ManifestName, options.Channel);
            await packer.Pack(options.Version, options.OutputPath);
            Console.WriteLine($"Packed to \"{options.OutputPath}\".");
        }

        private static async Task RunUpdate(UpdateOptions options)
        {
            if (options.Pid.HasValue)
            {
                Console.WriteLine($"Waiting for process {options.Pid} to exit...");
                Process.GetProcessById(options.Pid.Value).WaitForExit();
            }

            Console.WriteLine($"Running update in \"{options.ApplicationPath}\"...");
            var localManifestFile = Path.Combine(options.ApplicationPath, options.ManifestName);
            var manifestNameOption = File.Exists(localManifestFile) ? (Option<string>)new Some<string>(options.ManifestName) : OptionNone.Default;
            var channelOption = manifestNameOption.Match(_ => OptionNone.Default, () => (Option<string>)new Some<string>(options.Channel));

            var updater = new Updater(WebLocator.Instance, options.ApplicationPath, manifestNameOption, channelOption);
            var newManifest = await updater.Update(options.UpdateUrl);

            Console.WriteLine(newManifest.Match(
                manifest => $"Updated to {manifest.Channel}-{manifest.Version}.",
                () => "No update found."));
        }

        #endregion Private Methods
    }
}