<#
.SYNOPSIS
    Compila la solucion WardrobeFlow y corre la suite de tests unitarios.

.DESCRIPTION
    Red de seguridad para ejecutar ANTES de hacer commit/push: si el build falla
    o algun test no pasa, el script termina con codigo de salida 1 (no-cero), de
    modo que se note el problema en vez de subir 'main' roto.

    Descubre MSBuild y vstest.console.exe con vswhere (no hardcodea la version de
    Visual Studio). Usa MSBuild de VS -- NO 'dotnet build' -- porque el proyecto es
    .NET Framework 4.7.2 con csproj clasico.

    NOTA: mensajes en ASCII a proposito. Windows PowerShell 5.1 lee los .ps1 sin BOM
    como ANSI; evitar acentos garantiza que la salida no se vea con mojibake.

.PARAMETER Configuration
    Debug (por defecto) o Release.

.PARAMETER SkipTests
    Solo compila, sin correr los tests.

.EXAMPLE
    .\build-and-test.ps1
    .\build-and-test.ps1 -Configuration Release
    .\build-and-test.ps1 -SkipTests
#>
#Requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$solution  = Join-Path $scriptDir "IngSoftware-Bolivar,Morana.slnx"

function Write-Section($texto) {
    Write-Host ""
    Write-Host ("=" * 60) -ForegroundColor DarkCyan
    Write-Host "  $texto" -ForegroundColor Cyan
    Write-Host ("=" * 60) -ForegroundColor DarkCyan
}

function Find-Tool {
    # Devuelve [pscustomobject]@{ MSBuild=...; VsTest=... } o lanza si no encuentra MSBuild.
    $msbuild = $null

    # Pedimos a vswhere la ruta de MSBuild de un producto con la carga .NET desktop.
    # NO usamos 'installationPath': con -latest puede devolver SSMS u otro shell de VS;
    # la raiz de VS se deriva del propio MSBuild resuelto (mas confiable).
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $msbuild = & $vswhere -latest -prerelease -products * `
            -requires Microsoft.Component.MSBuild `
            -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    }

    # Fallback: instalacion conocida de VS18 si vswhere no resolvio MSBuild.
    if ([string]::IsNullOrEmpty($msbuild) -or -not (Test-Path $msbuild)) {
        $fallback = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
        if (Test-Path $fallback) { $msbuild = $fallback }
    }

    if ([string]::IsNullOrEmpty($msbuild) -or -not (Test-Path $msbuild)) {
        throw "No se encontro MSBuild. Instala Visual Studio (carga de trabajo .NET desktop) o ajusta la ruta de fallback en el script."
    }

    # Raiz de VS derivada de la ruta de MSBuild:
    #   <VsRoot>\MSBuild\Current\Bin\MSBuild.exe  -> 4 niveles arriba.
    $vsPath = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $msbuild)))

    # vstest.console.exe vive en una de estas dos rutas segun la version de VS.
    $vstest = $null
    $candidatos = @(
        (Join-Path $vsPath "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"),
        (Join-Path $vsPath "Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe")
    )
    $vstest = $candidatos | Where-Object { Test-Path $_ } | Select-Object -First 1

    return [pscustomobject]@{ MSBuild = $msbuild; VsTest = $vstest }
}

# -- 0) Validaciones ----------------------------------------------------------
if (-not (Test-Path $solution)) {
    Write-Host "No se encontro la solucion: $solution" -ForegroundColor Red
    exit 1
}

$tools = Find-Tool
Write-Host "MSBuild: $($tools.MSBuild)" -ForegroundColor DarkGray
if ($tools.VsTest) { Write-Host "VSTest : $($tools.VsTest)" -ForegroundColor DarkGray }

# -- 1) Build -----------------------------------------------------------------
Write-Section "Compilando ($Configuration)"
& $tools.MSBuild $solution -t:Build -p:Configuration=$Configuration -v:minimal -nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "BUILD FALLIDO (exit $LASTEXITCODE). No se corren los tests." -ForegroundColor Red
    exit 1
}
Write-Host "Build OK." -ForegroundColor Green

# -- 2) Tests -----------------------------------------------------------------
if ($SkipTests) {
    Write-Host ""
    Write-Host "Tests omitidos (-SkipTests)." -ForegroundColor Yellow
    exit 0
}

$testDll = Join-Path $scriptDir "Tests\bin\$Configuration\Tests.dll"
if (-not (Test-Path $testDll)) {
    Write-Host "No se encontro el ensamblado de tests: $testDll" -ForegroundColor Red
    exit 1
}
if (-not $tools.VsTest) {
    Write-Host "No se encontro vstest.console.exe; el build paso pero no se pudieron correr los tests." -ForegroundColor Yellow
    exit 1
}

Write-Section "Ejecutando tests"
& $tools.VsTest $testDll
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "HAY TESTS QUE FALLAN (exit $LASTEXITCODE)." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "TODO OK: build + tests pasaron. Listo para commit/push." -ForegroundColor Green
exit 0
