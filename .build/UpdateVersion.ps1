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

# Update project files
$projectFiles = Get-ChildItem -Path $PSScriptRoot/../*.csproj -Recurse -Force

foreach ($file in $projectFiles) {
    Write-Host "Found project file:" $file.FullName

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

# Update MAUI .appxmanifest File
$appxmanifestFiles = Get-ChildItem -Path $PSScriptRoot/../*.appxmanifest -Recurse -Force

foreach ($file in $appxmanifestFiles) {
    Write-Host "Found appxmanifest file:" $file.FullName

    $xml = [xml](Get-Content $file)
    [bool]$updated = $false

    $xml.GetElementsByTagName("Identity") | ForEach-Object{
        Write-Host "Updating Identity element..."

        $_."Version" = $version

        $updated = $true
    }

    if ($updated) {
        $xml.Save($file.FullName)
        Write-Host "'$($file.FullName)' file saved"
    } else {
        Write-Host "'Identity' element not found in '$($file.FullName)'"
    }
}