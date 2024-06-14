<#
.SYNOPSIS
    Script to updating project version.
.DESCRIPTION
    Script will update version for all csharp projects.
.PARAMETER mode
    Specify a value for the version
.EXAMPLE
    UpdateVersion.ps1 "1.2.3.4"
#>

[cmdletbinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$version
)

$projectFiles = Get-ChildItem -Path $PSScriptRoot/../*.csproj -Recurse -Force

foreach ($file in $projectFiles) {
    Write-Host "Found project file:" $file.Name

    $xml = [xml](Get-Content $file)
    [bool]$updated = $false

    $xml.GetElementsByTagName("PackageVersion") | ForEach-Object{
        Write-Host "Updating PackageVersion to:" $version
        $_."#text" = $version

        $updated = $true
    }

    $xml.GetElementsByTagName("Version") | ForEach-Object{
        Write-Host "Updating Version to:" $version
        $_."#text" = $version
    }

    if ($updated) {
        Write-Host "Project file saved"
        $xml.Save($file.FullName)
    } else {
        Write-Host "'PackageVersion' property not found in the project file"
    }
}