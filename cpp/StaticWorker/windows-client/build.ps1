<#
.SYNOPSIS
Builds the StaticWorker client for Windows with MSVC.

.DESCRIPTION
Builds the sources in ..\client into a native Windows executable that sends tasks to the
Linux cppstatic worker. The steps are:

1. Fetch ArmoniK.Extensions.Cpp at SDK_WINDOWS_REF from cpp\versions.env.
2. Create the conda environment of the SDK (gRPC, protobuf, abseil, nlohmann_json, zlib,
   prebuilt with MSVC by conda-forge) with micromamba, unless -DepsPrefix is given.
3. Build and install ArmoniK.Api (client) and the SDK (Client + Common) with the SDK's
   tools\windows\build.ps1.
4. Build the client and copy it to -OutDir with appsettings.json and the DLLs it needs.

Requirements: Visual Studio 2022 (C++ workload), CMake >= 3.22, git, and micromamba
(or an existing environment passed with -DepsPrefix).

.EXAMPLE
.\build.ps1
cd dist
$env:GrpcClient__Endpoint = "http://<control-plane-ip>:5001"
.\ArmoniK.Samples.Cpp.StaticWorker.Client.exe
#>
[CmdletBinding()]
param(
    # Build directory for the SDK checkout, the dependencies and the builds. Keep it short, MSVC
    # and the gRPC headers hit the 260 character path limit in deep directories.
    # Default: <this folder>\build
    [string]$WorkDir,
    # Folder receiving the executable, appsettings.json and the DLLs. Default: <this folder>\dist
    [string]$OutDir,
    # ArmoniK.Extensions.Cpp commit or tag. Default: SDK_WINDOWS_REF from cpp\versions.env
    [string]$SdkRef,
    # Existing ArmoniK.Extensions.Cpp checkout. Default: fetch $SdkRef into $WorkDir
    [string]$SdkSource,
    # Existing environment created from the SDK's tools\windows\environment.yml.
    # Default: create one with micromamba in $WorkDir\env
    [string]$DepsPrefix
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
if (-not $SdkRef) { $SdkRef = Get-VersionsEnv 'SDK_WINDOWS_REF' }
New-Item -ItemType Directory -Force -Path $WorkDir, $OutDir | Out-Null
$WorkDir = (Resolve-Path $WorkDir).Path
$OutDir = (Resolve-Path $OutDir).Path
$Prefix = Join-Path $WorkDir 'install'

# 1. SDK sources. A fetch by commit works for tags and full SHAs, a shallow clone only for tags.
if (-not $SdkSource) {
    $SdkSource = Join-Path $WorkDir 'ArmoniK.Extensions.Cpp'
    if (-not (Test-Path (Join-Path $SdkSource '.git'))) {
        Invoke-Native git @('init', '--quiet', $SdkSource)
        Invoke-Native git @('-C', $SdkSource, 'remote', 'add', 'origin',
            'https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp.git')
    }
    Invoke-Native git @('-C', $SdkSource, 'fetch', '--quiet', '--depth', '1', 'origin', $SdkRef)
    Invoke-Native git @('-C', $SdkSource, 'checkout', '--quiet', '--force', 'FETCH_HEAD')
}
$SdkSource = (Resolve-Path $SdkSource).Path
$SdkScript = Join-Path $SdkSource 'tools\windows\build.ps1'
if (-not (Test-Path $SdkScript)) { throw "$SdkScript not found, $SdkRef predates the Windows SDK build" }

# 2. Dependencies
if (-not $DepsPrefix) {
    $DepsPrefix = Join-Path $WorkDir 'env'
    if (-not (Test-Path (Join-Path $DepsPrefix 'conda-meta'))) {
        Invoke-Native micromamba @('create', '--yes', '--prefix', $DepsPrefix,
            '--file', (Join-Path $SdkSource 'tools\windows\environment.yml'))
    }
}
$DepsPrefix = (Resolve-Path $DepsPrefix).Path
$DepsLibrary = Join-Path $DepsPrefix 'Library'
# protoc and grpc_cpp_plugin load the environment DLLs at build time
$env:PATH = "$DepsLibrary\bin;$env:PATH"

# 3. ArmoniK.Api and the SDK, both static libraries
& $SdkScript -DepsPrefix $DepsPrefix -Prefix $Prefix -WorkDir (Join-Path $WorkDir 'sdk')

# 4. Client
$ClientBuild = Join-Path $WorkDir 'client'
Invoke-Native cmake @('-S', $ClientSource, '-B', $ClientBuild, "-DCMAKE_PREFIX_PATH=$Prefix;$DepsLibrary")
Invoke-Native cmake @('--build', $ClientBuild, '--config', $Config, '--parallel')

# The ArmoniK libraries are linked in, gRPC and its dependencies are DLLs of the environment
Copy-Item (Join-Path $ClientBuild "$Config\$ClientName.exe") $OutDir -Force
Copy-Item (Join-Path $ClientSource 'appsettings.json') $OutDir -Force
Copy-Item (Join-Path $DepsLibrary 'bin\*.dll') $OutDir -Force

Write-Host "Done. Run $OutDir\$ClientName.exe from $OutDir, it reads .\appsettings.json."
