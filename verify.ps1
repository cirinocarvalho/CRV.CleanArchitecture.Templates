<#
.SYNOPSIS
    Installs the template from source and verifies that each supported flag combination
    generates a solution that restores, builds, and passes its tests.

.DESCRIPTION
    Run this after changing anything under templates/content. It is the only check that
    exercises the template engine's conditional processing — building the template source
    directly does not, because the #if symbols are undefined there.

.EXAMPLE
    pwsh templates/verify.ps1
    pwsh templates/verify.ps1 -SkipTests
#>
[CmdletBinding()]
param(
    # Build only; skip 'dotnet test' for each generated solution.
    [switch]$SkipTests,

    # Directory the test projects are generated into. Recreated on every run.
    [string]$WorkDir = (Join-Path $env:TEMP 'crv-template-verify'),

    # An existing node_modules to junction into each generated src/ui so the UI can be
    # type-checked and linted without a network install. Skipped when absent.
    #
    # It deliberately lives in .ui-deps at the repo root rather than in the template's
    # own src/ui: the template engine enumerates every file under the source directory
    # before applying 'exclude', so a node_modules there turns each 'dotnet new' from
    # ~2s into ~90s. Bootstrap it once (see docs/authoring.md) — and never move an
    # existing one into place, because pnpm links packages with absolute-path junctions
    # on Windows, which a move silently breaks.
    [string]$UiNodeModules = (Join-Path $PSScriptRoot '.ui-deps\node_modules')
)

# 'Continue', not 'Stop': dotnet writes progress and test failures to stderr, which
# PowerShell surfaces as error records. Success is decided by $LASTEXITCODE below.
$ErrorActionPreference = 'Continue'

$templateRoot = Join-Path $PSScriptRoot 'content\StarterApp'

# Each case is a flag combination worth proving. Keep the matrix small but spanning:
# every symbol is exercised in both states across the set.
$cases = @(
    @{ Name = 'default';      Args = @() }
    @{ Name = 'no-ui';        Args = @('--ui', 'false') }
    @{ Name = 'no-auth';      Args = @('--auth', 'none') }
    @{ Name = 'entra';        Args = @('--auth', 'entra') }
    @{ Name = 'entra-no-ui';  Args = @('--auth', 'entra', '--ui', 'false') }
    # Proves the Docker files generate and that adding them leaves the CLI build intact.
    # It cannot prove the image builds (that needs a Docker daemon), nor that the .dcproj
    # loads — the CLI host never emits the .sln that carries it. Both need a manual check.
    @{ Name = 'docker';       Args = @('--docker', 'true') }
    @{ Name = 'infra';        Args = @('--infra', 'true') }
    @{ Name = 'minimal';      Args = @('--auth', 'none', '--ui', 'false', '--tests', 'false', '--samples', 'false') }
)

function Remove-WorkDir([string]$Path) {
    if (-not (Test-Path $Path)) { return }

    # Unlink the node_modules junctions before deleting anything. A recursive delete can
    # follow a directory junction into its target, and these point at the shared
    # .ui-deps\node_modules — losing that would mean a full reinstall. Deleting the link
    # itself (recursive:$false) never touches the target.
    foreach ($case in Get-ChildItem $Path -Directory -ErrorAction SilentlyContinue) {
        $link = Join-Path $case.FullName 'src\ui\node_modules'
        $item = Get-Item $link -Force -ErrorAction SilentlyContinue
        if ($item -and ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            [IO.Directory]::Delete($item.FullName, $false)
        }
    }

    # cmd's 'rd' rather than Remove-Item: pnpm nests paths past the 260-character limit
    # that PowerShell 5.1 still trips over, which leaves the directory half-deleted and
    # the next run generating on top of stale output.
    & cmd /c rd /s /q "$Path" 2>&1 | Out-Null

    if (Test-Path $Path) { throw "Could not clear work directory: $Path" }
}

Remove-WorkDir $WorkDir
New-Item -ItemType Directory -Force $WorkDir | Out-Null

Write-Host "Installing template from $templateRoot" -ForegroundColor Cyan
dotnet new install $templateRoot --force | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Template install failed.' }

$results = @()

foreach ($case in $cases) {
    $name = $case.Name
    $outDir = Join-Path $WorkDir $name
    $projName = 'Acme.Portal'
    Write-Host "`n=== $name ===" -ForegroundColor Cyan

    $status = 'ok'
    $detail = ''

    # --- generate ---
    $genArgs = @('new', 'crv-cleanarch', '-n', $projName, '-o', $outDir, '--no-restore') + $case.Args
    $genOut = & dotnet @genArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        $results += [pscustomobject]@{ Case = $name; Status = 'generate failed'; Detail = ($genOut | Out-String).Trim() }
        continue
    }

    $sln = Join-Path $outDir "$projName.slnx"

    # --- build ---
    $buildOut = & dotnet build $sln -v q --nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
        $errors = $buildOut | Select-String -Pattern 'error ' | Select-Object -First 5
        $results += [pscustomobject]@{ Case = $name; Status = 'build failed'; Detail = ($errors | Out-String).Trim() }
        continue
    }

    # --- test ---
    $hasTests = Test-Path (Join-Path $outDir 'src\api\Acme.Portal.UnitTests')
    if (-not $SkipTests -and $hasTests) {
        $testOut = & dotnet test $sln --no-build -v q --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            $fails = $testOut | Select-String -Pattern '\[FAIL\]' | Select-Object -First 5
            $status = 'tests failed'
            $detail = ($fails | Out-String).Trim()
        }
        else {
            $summary = $testOut | Select-String -Pattern 'Passed!' | ForEach-Object { $_.ToString().Trim() }
            $detail = ($summary -join '; ')
        }
    }
    elseif (-not $hasTests) {
        $detail = 'no test projects (expected for this case)'
    }

    # --- UI type-check + lint ---
    # Type errors are the usual symptom of a botched conditional: markup that survives
    # while the script block that fed it was stripped out.
    $genUi = Join-Path $outDir 'src\ui'
    if ((Test-Path $genUi) -and (Test-Path $UiNodeModules)) {
        $link = Join-Path $genUi 'node_modules'
        if (-not (Test-Path $link)) {
            New-Item -ItemType Junction -Path $link -Target (Resolve-Path $UiNodeModules) | Out-Null
        }

        Push-Location $genUi
        & '.\node_modules\.bin\vue-tsc.CMD' --noEmit 2>&1 | Out-Null
        $tscExit = $LASTEXITCODE
        & '.\node_modules\.bin\eslint.CMD' --ext .ts,.vue src 2>&1 | Out-Null
        $lintExit = $LASTEXITCODE
        Pop-Location

        if ($tscExit -ne 0 -or $lintExit -ne 0) {
            $status = 'ui checks failed'
            $detail = "vue-tsc exit $tscExit; eslint exit $lintExit"
        }
        else {
            $detail = ($detail, 'ui: typecheck + lint ok' | Where-Object { $_ }) -join '; '
        }
    }
    elseif (Test-Path $genUi) {
        # Loud, because a skipped UI check still lets the case report 'ok' — which is
        # how UI regressions reach main looking verified.
        Write-Host "  WARNING: UI checks SKIPPED - no node_modules at $UiNodeModules" -ForegroundColor Yellow
        $detail = ($detail, 'ui NOT checked (no node_modules)' | Where-Object { $_ }) -join '; '
    }

    # --- Terraform format check ---
    # Formatting only. 'terraform validate' needs 'init', and init resolves the Fargate
    # module from a placeholder registry source ('your-org') that does not resolve until
    # the generated project points it at a real module. The AWS resources in ui.tf are schema-checked by hand instead; see
    # docs/authoring.md.
    $genTf = Join-Path $outDir 'terraform'
    if ((Test-Path $genTf) -and (Get-Command terraform -ErrorAction SilentlyContinue)) {
        Push-Location $genTf
        & terraform fmt -check -recursive 2>&1 | Out-Null
        $fmtExit = $LASTEXITCODE
        Pop-Location

        if ($fmtExit -ne 0) {
            $status = 'terraform fmt failed'
            $detail = "terraform fmt -check exit $fmtExit"
        }
        else {
            $detail = ($detail, 'terraform: fmt ok' | Where-Object { $_ }) -join '; '
        }
    }
    elseif (Test-Path $genTf) {
        Write-Host "  WARNING: terraform checks SKIPPED - terraform CLI not on PATH" -ForegroundColor Yellow
        $detail = ($detail, 'terraform NOT checked' | Where-Object { $_ }) -join '; '
    }

    $results += [pscustomobject]@{ Case = $name; Status = $status; Detail = $detail }
}

Write-Host "`n================ SUMMARY ================" -ForegroundColor Cyan
$results | Format-Table -AutoSize -Wrap

$failed = $results | Where-Object { $_.Status -ne 'ok' }
if ($failed) {
    Write-Host "$($failed.Count) case(s) failed." -ForegroundColor Red
    exit 1
}

Write-Host 'All cases passed.' -ForegroundColor Green
