#tool "nuget:?package=xunit.runner.console&version=2.3.1"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument ("target", "Default");
var configuration = Argument ("configuration", "Release");
var version = Argument ("build_version", "1.0.0.0");
var releaseCandidate = Argument ("release_candidate", "0");

Information("target: {0}", target);
Information("configuration: {0}", configuration);
Information("build_version: {0}", version);
Information("release_candidate: {0}", releaseCandidate);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var rootDir = Directory ("./");
var sourceDir = Directory ("./src");
var buildDir = Directory ("./localBuild");
var solutionOutputDir = Directory (buildDir.Path + "/SQLBulkLoaderSolution");
var IntegrationSqlServerTestsOutputDir = Directory (buildDir.Path + "/IntegrationSqlServerTests");
var IntegrationPostgreSqlOutputDir = Directory (buildDir.Path + "/IntegrationPostgreSqlTests");
var IntegrationSharedOutputDir = Directory (buildDir.Path + "/IntegrationSharedTests");
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

Task ("BuildIntegrationSqlServerTests")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity (Verbosity.Minimal)
            .WithProperty ("OutDir", MakeAbsolute (IntegrationSqlServerTestsOutputDir).FullPath)
            .WithProperty ("Version", version)
            .WithProperty ("AssemblyVersion", version)
            .WithProperty ("FileVersion", version);

        MSBuild (sourceDir.Path + "/IntegrationSqlServerTests/IntegrationSqlServerTests.csproj", settings);
    });

Task ("BuildIntegrationSharedTests")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity (Verbosity.Minimal)
            .WithProperty ("OutDir", MakeAbsolute (IntegrationSharedOutputDir).FullPath)
            .WithProperty ("Version", version)
            .WithProperty ("AssemblyVersion", version)
            .WithProperty ("FileVersion", version);

        MSBuild (sourceDir.Path + "/IntegrationShared/IntegrationCompatibilityTests.csproj", settings);
    });

Task ("BuildIntegrationPostgreSqlTests")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity (Verbosity.Minimal)
            .WithProperty ("OutDir", MakeAbsolute (IntegrationPostgreSqlOutputDir).FullPath)
            .WithProperty ("Version", version)
            .WithProperty ("AssemblyVersion", version)
            .WithProperty ("FileVersion", version);

        MSBuild (sourceDir.Path + "/IntegrationPostgreSqlTests/IntegrationPostgreSqlTests.csproj", settings);
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
    .IsDependentOn ("BuildIntegrationSqlServerTests")
    .IsDependentOn ("BuildIntegrationPostgreSqlTests")
    .IsDependentOn ("BuildIntegrationSharedTests") 
    .Does (() => {
        Information ("Start Running Tests");
        XUnit2 (IntegrationSqlServerTestsOutputDir.Path + "/*Tests.dll");

        XUnit2 (IntegrationPostgreSqlOutputDir.Path + "/*Tests.dll");

        XUnit2 (IntegrationSharedOutputDir.Path + "/ivaldez.Sql.IntegrationShared.dll");
    });

Task ("BuildSqlServerPackages")
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

        var package_version = version;
        if (releaseCandidate != "0"){
            package_version = package_version + "-rc" + releaseCandidate;
        }

        XmlPoke(projectPath, "/Project/PropertyGroup/Version", package_version);
        XmlPoke(projectPath, "/Project/PropertyGroup/AssemblyVersion", version);

        DotNetCorePack (projectPath, settings);
    });

Task ("BuildPostgrePackages")
    .IsDependentOn ("Restore-NuGet-Packages")
    .IsDependentOn ("BuildSqlBulkLoader")
    .Does (() => {
        var settings = new DotNetCorePackSettings {
            Configuration = "Release",
            OutputDirectory = buildDir.Path,
            IncludeSource = true,
            IncludeSymbols = true
        };
        var projectPath = sourceDir.Path + "/ivaldez.SqlBulkLoader.PostgreSql/ivaldez.SqlBulkLoader.PostgreSql.csproj";
      
        var package_version = version;
        if (releaseCandidate != "0"){
            package_version = package_version + "-rc" + releaseCandidate;
        }

        XmlPoke(projectPath, "/Project/PropertyGroup/Version", package_version);
        XmlPoke(projectPath, "/Project/PropertyGroup/AssemblyVersion", version);

        DotNetCorePack (projectPath, settings);
    });

Task ("BuildPackages")
    .IsDependentOn ("BuildSqlServerPackages")
    .IsDependentOn ("BuildPostgrePackages");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task ("Default")
    .IsDependentOn ("BuildSolution")
    .IsDependentOn ("BuildIntegrationSqlServerTests")
    .IsDependentOn ("BuildIntegrationPostgreSqlTests")
    .IsDependentOn ("BuildIntegrationSharedTests") 
    .IsDependentOn ("BuildSqlBulkLoader")    
    .IsDependentOn ("Run-Unit-Tests")
    .IsDependentOn ("BuildPackages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget (target);