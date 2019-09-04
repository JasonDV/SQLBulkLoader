Framework 4.7

properties {
    $owner = "name"
    $configuration = "Release"
    $target = "ReBuild"

    $rootFolder = $PSScriptRoot
    $sourceFolder = "$rootFolder\src"
    $packagesFolder = "$sourceFolder\packages"
    $buildFolder = "$rootFolder\localBuild"
    $buildFolderLibs = "$buildFolder\Libs"
    $buildFolderUnitTests = "$buildFolder\UnitTests"
    $buildFolderIntegrationTests = "$buildFolder\IntegrationTest"


    $solutionFile = ".\src\MergeQueryObject.sln"
    $unitTestsProject = "$sourceFolder\UnitTests\UnitTests.csproj"
    $integrationTestProject = "$sourceFolder\IntegrationTests\IntegrationTests.csproj"    

    $xunitRunner = "$sourceFolder\packages\xunit.runner.console.2.3.1\tools\net452\xunit.console.exe"
    $nuGetExec = "$sourceFolder\packages\.nuget\NuGet.exe"
}

task default -depends Startup, UpdateNuGetPackages, BuildSolution, `
    BuildUnitTests, BuiltIntegrationTests, `
    RunUnitTests, RunIntegrationTests

task Startup {
    write-host "Maintainer:  $owner"
    write-host "Starting build of application...."  
}

task UpdateNuGetPackages {
	Exec { 
		&$nuGetExec restore "$solutionFile" -PackagesDirectory $packagesFolder
	}
	
	$srcDirectories = Get-ChildItem -Path "$sourceFolder" | ?{ $_.PSIsContainer } `
		| Where-Object{ !($_.FullName).EndsWith("packages") } `
		| Where-Object{ !($_.FullName).EndsWith(".nuget") } `
		| Where-Object{ !($_.FullName).EndsWith(".git") }`
		| Where-Object{ !($_.FullName).EndsWith("AssemblyInfo") } 			
		
	Foreach ($k in $srcDirectories)
	{		
		$searchFolder = "$sourceFolder\$k"
		
		write-host "Search for package.config in: $searchFolder"
		
		$configFiles = Get-ChildItem -Path $searchFolder -Include "packages.config" 
		
		Foreach ($i in $configFiles)
		{
			if (($i -ne $null) -and (Test-Path -Path $i))
			{
				write-host "Restoring NuGet Packages: $i"
				Exec { 			
					&$nuGetExec restore "$i" -PackagesDirectory "$packagesFolder"
				}
			}
		}
	}
}

task CleanSolution  {
    If(test-path $buildFolder)
    {
        Remove-Item $buildFolder -recurse -Exclude "$buildFolder"
    }
    New-Item -ItemType Directory -Force -Path $buildFolder    

    If(test-path $buildFolderLibs)
    {
        Remove-Item $buildFolderLibs -recurse -Exclude "$buildFolderLibs"
    }
    New-Item -ItemType Directory -Force -Path $buildFolderLibs  

    Exec { 
        msbuild $solutionFile /target:Clean
    }
}

task BuildSolution -depends CleanSolution {
    Exec { 
        msbuild $solutionFile `
            /t:$target `
            /p:Configuration=$configuration
    }
}

task BuildUnitTests -depends BuildSolution {
    Exec { 
        msbuild $unitTestsProject `
            /t:$target `
            /p:Configuration=$configuration `
            /p:OutputPath=$buildFolderUnitTests
    }
}

task BuiltIntegrationTests -depends BuildSolution {
    Exec { 
        msbuild $integrationTestProject `
            /t:$target `
            /p:Configuration=$configuration `
            /p:OutputPath=$buildFolderIntegrationTests
    }
}

task RunUnitTests -depends BuildUnitTests {
    Exec {
        &$xunitRunner "$buildFolderIntegrationTests\IntegrationTests.dll"
    }
}

task RunIntegrationTests -depends BuiltIntegrationTests {
    Exec {
        &$xunitRunner "$buildFolderUnitTests\UnitTests.dll"
    }
}