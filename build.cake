#addin nuget:?package=Cake.Git&version=0.19.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0
using IOPath = System.IO.Path;

var appVersion = new Version(0, 1);
var runtimes = new []
{
    string.Empty,
    "win-x64",
    // "win-x86",
    // "win-arm",
    // "win-arm64",
    // "linux-x64",
    // "linux-arm",
    // "osx-x64",
};

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");
var nugetSource = EnvironmentVariable("NUGET_SOURCE");
var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");
var skipClean = HasArgument("SkipClean");
var skipPack = HasArgument("SkipPack");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

var branch = GitBranchCurrent(".").FriendlyName;
var buildFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine("_build", configuration));
var testResultsPath = System.IO.Path.Combine(buildFolder, "TestResults.xml");

Setup(ctx =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");

    if(Jenkins.IsRunningOnJenkins)
    {
        branch = Jenkins.Environment.Repository.BranchName;
    }

    var now = DateTimeOffset.UtcNow;
    var epochStart = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);
    var timeSinceEpochStart = now - epochStart;
    var daysSinceEpochStart = (int)timeSinceEpochStart.TotalDays;
    var minutesSinceMidnight = (int)timeSinceEpochStart.Subtract(TimeSpan.FromDays(daysSinceEpochStart)).TotalMinutes;
    appVersion = new Version(appVersion.Major, appVersion.Minor, daysSinceEpochStart, minutesSinceMidnight);

    Information($"Building version {appVersion}-{branch}...");

});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

// Core
Task("Clean-Core")
.WithCriteria(!skipClean)
.Does(() => {
    if(!DirectoryExists(buildFolder))
        return;
    
    var files = GetFiles($"_build/{configuration}/*.nupkg");
    DeleteFiles(files);
});

Task("Pack-Core")
.WithCriteria(!skipPack)
.IsDependentOn("Clean-Core")
.Does(() => {
    DotNetCorePack("./NetUpdater.Core/NetUpdater.Core.csproj", new DotNetCorePackSettings
    {
        OutputDirectory = buildFolder,
        VersionSuffix = branch,
    });
});

Task("Publish-Core")
.IsDependentOn("Pack-Core")
.Does(() => {
    var nugetPackageFile = GetFiles($"./_build/{configuration}/*.nupkg").FirstOrDefault();
    if(nugetPackageFile != null)
        NuGetPush(nugetPackageFile, new NuGetPushSettings
        {
            ApiKey = nugetApiKey,
            Source = nugetSource,
        });
});

// Cli
Task("Clean-Cli")
.WithCriteria(!skipClean)
.Does(() => {
    if(DirectoryExists($"./_build/{configuration}/cli"))
        DeleteDirectory($"./_build/{configuration}/cli", new DeleteDirectorySettings
        {
            Recursive = true,
        });
    
    var files = GetFiles($"./_build/{configuration}/cli-*.zip");
    DeleteFiles(files);
});

Task("Build-Cli")
.IsDependentOn("Clean-Cli")
.DoesForEach(runtimes, runtime => {
    var runtimeName = runtime;
    if(string.IsNullOrEmpty(runtime))
        runtimeName = "any";
    
    var outputPath = System.IO.Path.Combine(buildFolder, "cli", runtimeName);
    DotNetCorePublish("./NetUpdater.Cli/NetUpdater.Cli.csproj", new DotNetCorePublishSettings
    {
        Configuration = configuration,
        VersionSuffix = branch,
        OutputDirectory = outputPath,
        Runtime = runtime,
        Verbosity = DotNetCoreVerbosity.Quiet,
    });
});

Task("Pack-Cli")
.WithCriteria(!skipPack)
.IsDependentOn("Build-Cli")
.DoesForEach(() => GetDirectories($"./_build/{configuration}/cli/*"), directoryPath => {
    var zipFile = $"./_build/{configuration}/cli_{directoryPath.GetDirectoryName()}-{appVersion}-{configuration}.zip";
    Information($"Zipping {directoryPath}...");
    Zip(directoryPath, zipFile);
});

// Test
Task("Clean-Test")
.WithCriteria(!skipClean)
.Does(() => {
    var files = GetFiles($"./_build/{configuration}/**/*TestResults.xml");
    DeleteFiles(files);
});

Task("Test")
.IsDependentOn("Clean-Test")
.Does(() => {
    DotNetCoreTest("./NetUpdater.Test/NetUpdater.Test.csproj", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        Logger = $"trx;LogFileName={testResultsPath}",
    });
});

// Clean
Task("Clean")
.WithCriteria(!skipClean)
.Does(() => {
    if(DirectoryExists("./_build"))
        CleanDirectory("./_build");
});

// Pack
Task("Pack")
.WithCriteria(!skipPack)
.IsDependentOn("Pack-Core")
.IsDependentOn("Pack-Cli");

//Publish
Task("Publish")
.IsDependentOn("Publish-Core");

RunTarget(target);
