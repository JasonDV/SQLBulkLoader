#tool "nuget:?package=xunit.runner.console&version=2.3.1"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument ("target", "Default");
var configuration = Argument ("configuration", "Release");
var version = Argument ("build_version", "1.0.0.0");

Information("target: {0}", target);
Information("configuration: {0}", configuration);
Information("build_version: {0}", version);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var rootDir = Directory ("./");
var sourceDir = Directory ("./src");
var buildDir = Directory ("./localBuild");
var solutionOutputDir = Directory (buildDir.Path + "/SQLBulkLoaderSolution");
var integrationTestOutputDir = Directory (buildDir.Path + "/IntegrationTests");
var sqlBulkLoaderOutputDir = Directory (buildDir.Path + "/SQLBulkLoaderUtility");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task ("Clean")
    .Does (() => {
        CleanDirectory (buildDir);
    });

Task ("Restore-NuGet-Packages")
    .IsDependentOn ("Clean")
    .Does (() => {
        NuGetRestore ("./src/SQLBulkLoader.sln");
    });

Task ("BuildSolution")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity (Verbosity.Minimal)
            .WithProperty ("OutDir", MakeAbsolute (solutionOutputDir).FullPath)
            .WithProperty ("Version", version)
            .WithProperty ("AssemblyVersion", version)
            .WithProperty ("FileVersion", version);

        MSBuild (sourceDir.Path + "/SQLBulkLoader.sln", settings);
    });

Task ("BuildIntegrationTests")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity (Verbosity.Minimal)
            .WithProperty ("OutDir", MakeAbsolute (integrationTestOutputDir).FullPath)
            .WithProperty ("Version", version)
            .WithProperty ("AssemblyVersion", version)
            .WithProperty ("FileVersion", version);

        MSBuild (sourceDir.Path + "/IntegrationTests/IntegrationTests.csproj", settings);
    });

Task ("BuildSqlBulkLoader")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity (Verbosity.Minimal)
            .WithProperty ("OutDir", MakeAbsolute (sqlBulkLoaderOutputDir).FullPath)
            .WithProperty ("Version", version)
            .WithProperty ("AssemblyVersion", version)
            .WithProperty ("FileVersion", version);

        MSBuild (sourceDir.Path + "/ivaldez.SqlBulkLoader/ivaldez.Sql.SqlBulkLoader.csproj", settings);
    });

Task ("Run-Unit-Tests")
    .IsDependentOn ("BuildIntegrationTests")
    .Does (() => {
        Information ("Start Running Tests");
        XUnit2 (integrationTestOutputDir.Path + "/*Tests.dll");
    });

Task ("BuildPackages")
    .IsDependentOn ("Restore-NuGet-Packages")
    .IsDependentOn ("BuildSqlBulkLoader")
    .Does (() => {
        var settings = new DotNetCorePackSettings {
            Configuration = "Release",
            OutputDirectory = buildDir.Path,
            IncludeSource = true,
            IncludeSymbols = true
        };
        var projectPath = sourceDir.Path + "/ivaldez.SqlBulkLoader/ivaldez.Sql.SqlBulkLoader.csproj";

        XmlPoke(projectPath, "/Project/PropertyGroup/Version", version);
        XmlPoke(projectPath, "/Project/PropertyGroup/AssemblyVersion", version);

        DotNetCorePack (projectPath, settings);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task ("Default")
    .IsDependentOn ("BuildSolution")
    .IsDependentOn ("BuildIntegrationTests")
    .IsDependentOn ("BuildSqlBulkLoader")
    .IsDependentOn ("BuildPackages")
    .IsDependentOn ("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget (target);