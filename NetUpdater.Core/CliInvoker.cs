using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class CliInvoker
    {
        #region Private Fields

        private readonly FileInfo cliFile;

        private readonly string manifestName;

        #endregion Private Fields

        #region Public Constructors

        public CliInvoker(string cliPath, string manifestName)
        {
            cliFile = new FileInfo(cliPath);
            this.manifestName = manifestName;
        }

        #endregion Public Constructors

        #region Public Methods

        public Task Update(Uri updateUri)
        {
            var applicationAssemblyPath = Assembly.GetEntryAssembly().Location;
            var applicationPath = Path.GetDirectoryName(applicationAssemblyPath);
            var currentProcess = Process.GetCurrentProcess();

            return Invoke(false, "update", "-a", applicationPath, "-m", manifestName, "-u", updateUri.ToString(), "-p", currentProcess.Id.ToString());
        }

        #endregion Public Methods

        #region Private Methods

        private async Task Invoke(bool waitForExit, params string[] args)
        {
            var tmpCliFile = await Prepare();
            var totalArgs = new[] { tmpCliFile.FullName }.Concat(args);
            var arguments = string.Join(" ", totalArgs.Select(o => $"\"{o.Replace("\"", "\"\"")}\""));
            var process = Process.Start("dotnet", arguments);

            if (waitForExit)
                await Task.Run(() => process.WaitForExit());
        }

        private async Task<FileInfo> Prepare()
        {
            var netUpdaterFiles = cliFile.Directory.EnumerateFiles("NetUpdater.*");

            var tmpDirectoryPath = Path.GetTempFileName();
            File.Delete(tmpDirectoryPath);

            var copyTasks = netUpdaterFiles.Select(file => Task.Run(() => File.Copy(file.FullName, Path.Combine(tmpDirectoryPath, file.Name))));
            await Task.WhenAll(copyTasks);
            return new FileInfo(Path.Combine(tmpDirectoryPath, cliFile.Name));
        }

        #endregion Private Methods
    }
}