#tool "nuget:?package=xunit.runner.console"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./localBuild");

//////////////////////////////////////////////////////////////////////
// TASKS
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
    NuGetRestore("./src/SQLBulkLoader.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{ 
      MSBuild("./src/SQLBulkLoader.sln", settings => {
        settings.SetConfiguration(configuration);
        settings.WithProperty("OutDir", buildDir);
        });   

      Information("Build tests");
      DirectoryPath buildDir = MakeAbsolute(buildDir + Directory("IntegrationTests"));
      MSBuild("./src/IntegrationTests/IntegrationTests.csproj", settings => {          
        settings.WithProperty("OutDir", buildDir.FullPath);
        });   
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    //Information("Start Running Tests");
   // XUnit2("./artifacts/_tests/**/*.Tests.dll");   
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
