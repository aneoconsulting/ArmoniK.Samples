<#
.SYNOPSIS
Builds the StaticWorker client for Windows with MSVC.

.DESCRIPTION
Builds the sources in ..\client into a native Windows executable that sends tasks to the
Linux cppstatic worker. The steps are:

1. Fetch ArmoniK.Extensions.Cpp at SDK_VERSION from cpp\versions.env.
2. Get the dependencies (gRPC, protobuf, abseil, nlohmann_json, zlib), from one of:
   - a conda environment created from the SDK's tools\windows\environment.yml with
     micromamba (prebuilt MSVC binaries from conda-forge). This is the default.
   - an existing conda environment, passed with -DepsPrefix.
   - any CMake prefix holding MSVC builds of them (vcpkg, source builds), passed with
     -DepsCMakePrefix. For vcpkg, also pass -VcpkgRoot.
3. Build and install ArmoniK.Api (client) at API_VERSION and the SDK (Client + Common) with
   the SDK's tools\windows\build.ps1.
4. Build the client and copy it to -OutDir with appsettings.json and the DLLs it needs.

Requirements: Visual Studio 2022 (C++ workload), CMake >= 3.22, git, and micromamba
unless -DepsPrefix or -DepsCMakePrefix is given.

.EXAMPLE
.\build.ps1
cd dist
$env:GrpcClient__Endpoint = "http://<control-plane-ip>:5001"
.\ArmoniK.Samples.Cpp.StaticWorker.Client.exe

.EXAMPLE
.\build.ps1 -DepsCMakePrefix C:\vcpkg\installed\x64-windows -VcpkgRoot C:\vcpkg
#>
[CmdletBinding()]
param(
    # Build directory for the SDK checkout, the dependencies and the builds. Keep it short, MSVC
    # and the gRPC headers hit the 260 character path limit in deep directories.
    # Default: <this folder>\build
    [string]$WorkDir,
    # Folder receiving the executable, appsettings.json and the DLLs. Default: <this folder>\dist
    [string]$OutDir,
    # ArmoniK.Extensions.Cpp tag. Default: SDK_VERSION from cpp\versions.env
    [string]$SdkVersion,
    # ArmoniK.Api tag. Default: API_VERSION from cpp\versions.env
    [string]$ApiVersion,
    # Existing ArmoniK.Extensions.Cpp checkout. Default: fetch $SdkVersion into $WorkDir
    [string]$SdkSource,
    # Existing environment created from the SDK's tools\windows\environment.yml.
    # Default: create one with micromamba in $WorkDir\env
    [string]$DepsPrefix,
    # CMake prefix holding the dependencies, used as is (e.g. <vcpkg>\installed\x64-windows).
    # Overrides -DepsPrefix. The DLLs in its bin\ directory, if any, are copied to -OutDir.
    [string]$DepsCMakePrefix,
    # vcpkg checkout. With -DepsCMakePrefix <installed>\<triplet>, configures the SDK and the
    # client with vcpkg's toolchain file
    [string]$VcpkgRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Works on Windows PowerShell 5.1, which ignores $PSNativeCommandUseErrorActionPreference
function Invoke-Native {
    param([string]$Exe, [string[]]$Arguments)
    Write-Host "> $Exe $($Arguments -join ' ')"
    & $Exe @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Exe exited with code $LASTEXITCODE" }
}

function Get-VersionsEnv([string]$Name) {
    $content = Get-Content (Join-Path $PSScriptRoot '..\..\versions.env') -Raw
    if ($content -notmatch "(?m)^$Name=(\S+)") { throw "$Name not found in cpp\versions.env" }
    return $Matches[1]
}

# conda-forge ships release (/MD) binaries, a Debug build would mix them with /MDd
$Config = 'Release'
$ClientName = 'ArmoniK.Samples.Cpp.StaticWorker.Client'
$ClientSource = (Resolve-Path (Join-Path $PSScriptRoot '..\client')).Path

if (-not $WorkDir) { $WorkDir = Join-Path $PSScriptRoot 'build' }
if (-not $OutDir) { $OutDir = Join-Path $PSScriptRoot 'dist' }
if (-not $SdkVersion) { $SdkVersion = Get-VersionsEnv 'SDK_VERSION' }
if (-not $ApiVersion) { $ApiVersion = Get-VersionsEnv 'API_VERSION' }
New-Item -ItemType Directory -Force -Path $WorkDir, $OutDir | Out-Null
$WorkDir = (Resolve-Path $WorkDir).Path
$OutDir = (Resolve-Path $OutDir).Path
$Prefix = Join-Path $WorkDir 'install'

# 1. SDK sources. Fetching into an existing checkout follows SDK_VERSION changes.
if (-not $SdkSource) {
    $SdkSource = Join-Path $WorkDir 'ArmoniK.Extensions.Cpp'
    if (-not (Test-Path (Join-Path $SdkSource '.git'))) {
        Invoke-Native git @('init', '--quiet', $SdkSource)
        Invoke-Native git @('-C', $SdkSource, 'remote', 'add', 'origin',
            'https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp.git')
    }
    Invoke-Native git @('-C', $SdkSource, 'fetch', '--quiet', '--depth', '1', 'origin', $SdkVersion)
    Invoke-Native git @('-C', $SdkSource, 'checkout', '--quiet', '--force', 'FETCH_HEAD')
}
$SdkSource = (Resolve-Path $SdkSource).Path
$SdkScript = Join-Path $SdkSource 'tools\windows\build.ps1'
if (-not (Test-Path $SdkScript)) { throw "$SdkScript not found, SDK $SdkVersion predates the Windows build (0.5.9)" }

# 2. Dependencies. $DepsCMake is the CMake prefix holding them, $DepsArgs selects them in the SDK script.
$DepsOptions = @()
if ($DepsCMakePrefix) {
    $DepsCMake = (Resolve-Path $DepsCMakePrefix).Path
    $DepsArgs = @{ DepsCMakePrefix = $DepsCMake }
    if ($VcpkgRoot) {
        $VcpkgRoot = (Resolve-Path $VcpkgRoot).Path
        $DepsArgs.VcpkgRoot = $VcpkgRoot
        # Same options as the SDK script, so the client finds the dependencies the same way
        $DepsOptions = @("-DCMAKE_TOOLCHAIN_FILE=$(Join-Path $VcpkgRoot 'scripts\buildsystems\vcpkg.cmake')",
            "-DVCPKG_INSTALLED_DIR=$(Split-Path $DepsCMake -Parent)",
            "-DVCPKG_TARGET_TRIPLET=$(Split-Path $DepsCMake -Leaf)",
            '-DVCPKG_MANIFEST_MODE=OFF')
    }
} else {
    if ($VcpkgRoot) { throw '-VcpkgRoot needs -DepsCMakePrefix <vcpkg installed dir>\<triplet>' }
    if (-not $DepsPrefix) {
        $DepsPrefix = Join-Path $WorkDir 'env'
        if (-not (Test-Path (Join-Path $DepsPrefix 'conda-meta'))) {
            Invoke-Native micromamba @('create', '--yes', '--prefix', $DepsPrefix,
                '--file', (Join-Path $SdkSource 'tools\windows\environment.yml'))
        }
    }
    $DepsPrefix = (Resolve-Path $DepsPrefix).Path
    # conda on Windows installs C++ libraries under <env>\Library
    $DepsCMake = Join-Path $DepsPrefix 'Library'
    $DepsArgs = @{ DepsPrefix = $DepsPrefix }
}
# protoc and grpc_cpp_plugin load the dependency DLLs at build time
$env:PATH = "$DepsCMake\bin;$env:PATH"

# 3. ArmoniK.Api and the SDK, both static libraries
& $SdkScript @DepsArgs -Prefix $Prefix -WorkDir (Join-Path $WorkDir 'sdk') -ApiVersion $ApiVersion -Version $SdkVersion

# 4. Client
$ClientBuild = Join-Path $WorkDir 'client'
Invoke-Native cmake (@('-S', $ClientSource, '-B', $ClientBuild, "-DCMAKE_PREFIX_PATH=$Prefix;$DepsCMake") + $DepsOptions)
Invoke-Native cmake @('--build', $ClientBuild, '--config', $Config, '--parallel')

# The ArmoniK libraries are linked in. gRPC and its dependencies are DLLs, except with static
# builds such as vcpkg's x64-windows-static-md triplet, which leave nothing to copy.
Copy-Item (Join-Path $ClientBuild "$Config\$ClientName.exe") $OutDir -Force
Copy-Item (Join-Path $ClientSource 'appsettings.json') $OutDir -Force
$DepsDlls = Join-Path $DepsCMake 'bin\*.dll'
if (Test-Path $DepsDlls) { Copy-Item $DepsDlls $OutDir -Force }

Write-Host "Done. Run $OutDir\$ClientName.exe from $OutDir, it reads .\appsettings.json."
