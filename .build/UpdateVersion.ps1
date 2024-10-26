<#
.SYNOPSIS
    Script to updating project version.
.DESCRIPTION
    Script will update the display version of the application.
.PARAMETER version
    The build version.
.EXAMPLE
    UpdateVersion.ps1 "1.2.3.4"
#>

[cmdletbinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$version
)

# The display version of the application. This should be at most a three part version number such as 1.0.0.
# Split the version string by '.' and remove the last segment
$applicationDisplayVersion = ($version -split '\.')[0..2] -join '.'

# Update project files
$projectFiles = Get-ChildItem -Path $PSScriptRoot/../*.csproj -Recurse -Force

foreach ($file in $projectFiles) {
    Write-Host "Found project file:" $file.FullName

    $xml = [xml](Get-Content $file)
    [bool]$updated = $false

    $xml.GetElementsByTagName("ApplicationDisplayVersion") | ForEach-Object{
        Write-Host "Updating ApplicationDisplayVersion to:" $applicationDisplayVersion
        $_."#text" = $applicationDisplayVersion

        $updated = $true
    }

    $xml.GetElementsByTagName("FileVersion") | ForEach-Object{
        Write-Host "Updating FileVersion to:" $version
        $_."#text" = $version

        $updated = $true
    }

    if ($updated) {
        Write-Host "Project file saved"
        $xml.Save($file.FullName)
    } else {
        Write-Host "Neither the 'ApplicationDisplayVersion' nor the 'FileVersion' property was found in the project file '$($file.FullName)'."
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