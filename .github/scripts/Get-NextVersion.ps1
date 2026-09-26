<#
.SYNOPSIS
    Works out the next release version from git history.

.DESCRIPTION
    Git tags of the form vMAJOR.MINOR.PATCH are the source of truth for released versions.
    The next version is the latest such tag bumped according to the commits made since it:

      major  a commit subject with a "!" before the colon (feat!: / fix(api)!: ...) or a
             body containing "BREAKING CHANGE"
      minor  a commit subject starting with feat: or feat(scope):
      patch  anything else

    With no release tag yet, the version is <Version> from the .csproj and no bump is applied.
    Pass -Bump major|minor|patch to override the detected bump.

    When run under GitHub Actions the results are appended to $GITHUB_OUTPUT as
    version, tag, previous_tag, bump and release (true/false). They are also printed.
    release=false means there is nothing to release: no commits since the last tag.

.EXAMPLE
    pwsh .github/scripts/Get-NextVersion.ps1
    pwsh .github/scripts/Get-NextVersion.ps1 -Bump minor
#>
[CmdletBinding()]
param(
    [ValidateSet('auto', 'major', 'minor', 'patch')]
    [string]$Bump = 'auto',

    # Project file whose <Version> seeds the very first release.
    [string]$ProjectFile = (Join-Path $PSScriptRoot '..\..\CRV.CleanArchitecture.Templates.csproj')
)

$ErrorActionPreference = 'Stop'

function Write-Output-Pair([string]$Name, [string]$Value) {
    Write-Host ("{0,-13} {1}" -f "$($Name):", $Value)
    if ($env:GITHUB_OUTPUT) {
        Add-Content -Path $env:GITHUB_OUTPUT -Value "$Name=$Value"
    }
}

# --- previous release ---------------------------------------------------------------
# Highest release tag reachable from HEAD. The pattern keeps unrelated tags (a spike, a
# rollback marker) from becoming the baseline; --merged keeps a tag on an unmerged
# branch out of it; version sort picks the highest, not the nearest, so a hotfix tag
# cut from an older commit cannot roll the version backwards. 'git tag --list' prints
# nothing and exits 0 when there are no tags, so no stderr handling is needed.
$previousTag = git tag --list 'v[0-9]*.[0-9]*.[0-9]*' --merged HEAD --sort=-v:refname |
    Select-Object -First 1
if (-not $previousTag) { $previousTag = '' }

if ($previousTag) {
    if ($previousTag -notmatch '^v(\d+)\.(\d+)\.(\d+)$') {
        throw "Latest release tag '$previousTag' is not vMAJOR.MINOR.PATCH."
    }
    $major, $minor, $patch = [int]$Matches[1], [int]$Matches[2], [int]$Matches[3]
    $range = "$previousTag..HEAD"
}
else {
    [xml]$project = Get-Content -Path $ProjectFile
    $seed = ($project.Project.PropertyGroup | ForEach-Object { $_.Version } | Where-Object { $_ } | Select-Object -First 1)
    if (-not $seed -or $seed -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
        throw "No release tag exists and <Version> in $ProjectFile is not MAJOR.MINOR.PATCH (found '$seed')."
    }
    $major, $minor, $patch = [int]$Matches[1], [int]$Matches[2], [int]$Matches[3]
    $range = 'HEAD'
}

# --- commits since then -------------------------------------------------------------
# A visible delimiter line rather than %x00/%x01: control characters do not survive the
# native-command pipeline reliably in Windows PowerShell 5.1, which is what a developer
# running this locally on Windows gets. The first line of each block is the subject,
# the rest is the body.
$delimiter = '@@COMMIT@@'
$raw = @(git log $range --format="%s%n%b%n$delimiter")
$commits = @()
$block = New-Object System.Collections.Generic.List[string]
foreach ($line in $raw) {
    if ($line -eq $delimiter) {
        if ($block.Count -gt 0) {
            $commits += [pscustomobject]@{
                Subject = $block[0].Trim()
                Body    = (($block | Select-Object -Skip 1) -join "`n").Trim()
            }
        }
        $block.Clear()
        continue
    }
    $block.Add($line)
}

if ($previousTag -and $commits.Count -eq 0) {
    Write-Host "No commits since $previousTag - nothing to release."
    Write-Output-Pair 'release' 'false'
    Write-Output-Pair 'previous_tag' $previousTag
    return
}

# --- decide the bump ----------------------------------------------------------------
$detected = 'patch'
foreach ($c in $commits) {
    # -cmatch: the footer token is upper-case by specification, and a case-insensitive
    # match would let the words "breaking change" in ordinary prose force a major bump.
    if ($c.Subject -match '^[a-zA-Z]+(\([^)]*\))?!:' -or $c.Body -cmatch '(?m)^BREAKING[ -]CHANGE:') {
        $detected = 'major'
        break
    }
    if ($c.Subject -match '^feat(\([^)]*\))?:') {
        $detected = 'minor'
    }
}

$effective = if ($Bump -eq 'auto') { $detected } else { $Bump }

if (-not $previousTag) {
    # First release ships the seed version as-is; there is nothing to bump from.
    $effective = 'initial'
}
else {
    switch ($effective) {
        'major' { $major++; $minor = 0; $patch = 0 }
        'minor' { $minor++; $patch = 0 }
        'patch' { $patch++ }
    }
}

$version = "$major.$minor.$patch"

Write-Host "Commits considered: $($commits.Count) ($range)"
Write-Output-Pair 'release' 'true'
Write-Output-Pair 'previous_tag' $previousTag
Write-Output-Pair 'bump' $effective
Write-Output-Pair 'version' $version
Write-Output-Pair 'tag' "v$version"
