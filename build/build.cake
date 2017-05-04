//////////////////////////////////////////////////////////////////////
// CREDITS
// Borrowed ideas from https://github.com/michael-wolfenden/Polly/blob/master/build.cake
//////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET TOOLS
//////////////////////////////////////////////////////////////////////

#Tool "xunit.runner.console"
#Tool "GitVersion.CommandLine"
#Tool "Brutal.Dev.StrongNameSigner"
#Tool "NuSpec.ReferenceGenerator"

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET LIBRARIES
//////////////////////////////////////////////////////////////////////

#addin "System.Text.Json"
using System.Text.Json;

//////////////////////////////////////////////////////////////////////
// PREPARATION - GLOBAL VARIABLES, ETC.
//////////////////////////////////////////////////////////////////////

var projectName = "csharp.Helper";
var solutions = GetFiles("./**/*.sln");

// Define directories.
var solutionPaths = solutions.Select(solution => solution.GetDirectory());
//var buildDir = Directory("./src/Example/bin") + Directory(configuration);
var buildDir = Directory("./build");
var artifactsDir = Directory("./artifacts");
var testResultsDir = artifactsDir + Directory("test-results");

// NuGet
var nuspecFilename = projectName + ".nuspec";
var nuspecSrcFile = srcDir + File(nuspecFilename);
var nuspecDestFile = buildDir + File(nuspecFilename);
var nupkgDestDir = artifactsDir + Directory("nuget-package");

var projectToNugetFolderMap = new Dictionary<string, string[]>() {
    { "charp.Helper", new [] {"charp.Helper"} }
};

// Gitversion
var gitVersionPath = ToolsExePath("GitVersion.exe");
Dictionary<string, object> gitVersionOutput;

// NuSpec.ReferenceGenerator
var refGenPath = ToolsExePath("RefGen.exe");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("");
    Information("           _                       _   _      _                 ");
    Information("   ___ ___| |__   __ _ _ __ _ __  | | | | ___| |_ __   ___ _ __ ");
    Information("  / __/ __| '_ \ / _` | '__| '_ \ | |_| |/ _ \ | '_ \ / _ \ '__|");
    Information(" | (__\__ \ | | | (_| | |  | |_) ||  _  |  __/ | |_) |  __/ |   ");
    Information("  \___|___/_| |_|\__,_|_|  | .__(_)_| |_|\___|_| .__/ \___|_|   ");
    Information("                           |_|                 |_|              ");
    Information("");
});

Teardown(() =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
// PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {
        buildDir,
        artifactsDir,
        testResultsDir,
        nupkgDestDir
  	});

    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
});

Task("__RestoreNugetPackages")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Restoring NuGet Packages for {0}", solution);
        NuGetRestore(solution);
    }
});

Task("__UpdateAssemblyVersionInformation")
    .Does(() =>
{
    var gitVersionSettings = new ProcessSettings()
        .SetRedirectStandardOutput(true);

    IEnumerable<string> outputLines;
    StartProcess(gitVersionPath, gitVersionSettings, out outputLines);

    var output = string.Join("\n", outputLines);
    gitVersionOutput = new JsonParser().Parse<Dictionary<string, object>>(output);

    Information("Updated GlobalAssemblyInfo");
    Information("AssemblyVersion -> {0}", gitVersionOutput["AssemblySemVer"]);
    Information("AssemblyFileVersion -> {0}", gitVersionOutput["MajorMinorPatch"]);
    Information("AssemblyInformationalVersion -> {0}", gitVersionOutput["InformationalVersion"]);
});

Task("__UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    var fullSemVer = gitVersionOutput["FullSemVer"].ToString();
    AppVeyor.UpdateBuildVersion(fullSemVer);
});

Task("__BuildSolutions")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);

        if(IsRunningOnWindows())
        {
            // Use MSBuild
            MSBuild(solution, settings =>
                settings
                    .SetConfiguration(configuration)
                    .WithProperty("TreatWarningsAsErrors", "true")
                    .UseToolVersion(MSBuildToolVersion.NET46)
                    .SetVerbosity(Verbosity.Minimal)
                    .SetNodeReuse(false));
        }
        else
        {
            // Use XBuild
            XBuild(solution);
        }
    }
});

Task("__RunTests")
    .Does(() =>
{
    XUnit2("./**/bin/" + configuration + "/*.Helper.Tests.dll", new XUnit2Settings {
        OutputDirectory = testResultsDir,
        XmlReportV1 = true
    });
});

Task("__CopyOutputToNugetFolder")
    .Does(() =>
{
    foreach(var project in projectToNugetFolderMap.Keys) {
        var sourceDir = srcDir + Directory(projectName + "." + project) + Directory("bin") + Directory(configuration);

        foreach(var targetFolder in projectToNugetFolderMap[project]) {
            var destDir = buildDir + Directory("lib") + Directory(targetFolder);

            Information("Copying {0} -> {1}.", sourceDir, destDir);
            CopyDirectory(sourceDir, destDir);
       }
    }

    CopyFile(nuspecSrcFile, nuspecDestFile);
});

//////////////////////////////////////////////////////////////////////
// BUILD TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/Example.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/Example.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./src/Example.sln");
    }
});

///////////////////////////////////////////////////////////////////////////////
// PRIMARY TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
//////////////////////////////////////////////////////////////////////

string ToolsExePath(string exeFileName) {
    var exePath = System.IO.Directory.GetFiles(@".\Tools", exeFileName, SearchOption.AllDirectories).FirstOrDefault();
    return exePath;
}