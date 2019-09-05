#tool "nuget:?package=xunit.runner.console&version=2.3.1"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument ("target", "Default");
var configuration = Argument ("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var rootDir = Directory ("./");
var sourceDir = Directory ("./src");
var buildDir = Directory ("./localBuild");
var solutionOutputDir = Directory(buildDir.Path + "/SQLBulkLoaderSolution");
var integrationTestOutputDir = Directory(buildDir.Path + "/IntegrationTests");
var sqlBulkLoaderOutputDir = Directory(buildDir.Path + "/SQLBulkLoaderUtility");


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
            .SetVerbosity(Verbosity.Minimal)
            .WithProperty("OutDir", MakeAbsolute(solutionOutputDir).FullPath);

        MSBuild (sourceDir.Path + "/SQLBulkLoader.sln", settings);          
    });

Task ("BuildIntegrationTests")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {   
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity(Verbosity.Minimal)
            .WithProperty("OutDir", MakeAbsolute(integrationTestOutputDir).FullPath);

        MSBuild (sourceDir.Path + "/IntegrationTests/IntegrationTests.csproj", settings);
    });

Task ("BuildSqlBulkLoader")
    .IsDependentOn ("Restore-NuGet-Packages")
    .Does (() => {
        var settings = new MSBuildSettings ()
            .SetConfiguration (configuration)
            .SetVerbosity(Verbosity.Minimal)
            .WithProperty("OutDir", MakeAbsolute(sqlBulkLoaderOutputDir).FullPath);

        MSBuild (sourceDir.Path + "/ivaldez.SqlBulkLoader/ivaldez.Sql.SqlBulkLoader.csproj", settings);
    });

Task ("Run-Unit-Tests")
    .IsDependentOn ("BuildIntegrationTests")
    .Does (() => {
        Information("Start Running Tests");
        XUnit2(integrationTestOutputDir.Path + "/*Tests.dll");   
    });

Task("BuildPackages")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("BuildSqlBulkLoader")
    .Does(() =>
{
    /*
    var nuGetPackSettings = new NuGetPackSettings
	{
		OutputDirectory = buildDir.Path,
		IncludeReferencedProjects = true,
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", "Release" }
		}
	};

    MSBuild(sourceDir.Path + "/ivaldez.SqlBulkLoader/ivaldez.Sql.SqlBulkLoader.csproj",
        new MSBuildSettings().SetConfiguration(configuration));
    NuGetPack(sourceDir.Path + "/ivaldez.SqlBulkLoader/ivaldez.Sql.SqlBulkLoader.csproj", 
        nuGetPackSettings);
 */
           var settings = new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = buildDir.Path,
     };

     DotNetCorePack(sourceDir.Path + "/ivaldez.SqlBulkLoader/ivaldez.Sql.SqlBulkLoader.csproj", settings);
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