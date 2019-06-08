using CommandLine;
using MicroElements.Functional;
using NetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var packer = new Packer(options.ApplicationPath.LocalPath, options.ManifestName, options.Channel);
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
            var updater = new Updater(WebLocator.Instance, options.ApplicationPath.LocalPath, options.ManifestName, OptionNone.Default);
            await updater.Update(options.UpdateUrl);
        }

        #endregion Private Methods
    }
}