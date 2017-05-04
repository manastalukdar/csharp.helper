# Script modified from: https://github.com/Wheelies/MarkdownLog/blob/master/NuGet/pack.ps1

$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'

Get-ChildItem -Path $root -Include *.nupkg -File -Recurse | foreach { $_.Delete()}
$version = [System.Reflection.Assembly]::LoadFile("$root\csharp.Helper\bin\$Env:CONFIGURATION\csharp.Helper.dll").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)
$yearStr = get-date -Format yyyy
If($Env:APPVEYOR_REPO_BRANCH -eq 'develop') {
	$versionStr = $versionStr + "-beta"
}

Write-Host "Setting .nuspec version tag to $versionStr"
Write-Host "Setting .nuspec year tag to $yearStr"

$content = (Get-Content $root\nuget\csharp.Helper.nuspec) 
$content = $content -replace '\$version\$',$versionStr
$content = $content -replace '\$year\$',$yearStr

$content | Out-File $root\nuget\csharp.Helper.compiled.nuspec

Copy-Item $root\nuget\csharp.Helper.compiled.nuspec $root\csharp.Helper\csharp.Helper.nuspec

Set-Location -path $root\csharp.Helper -PassThru

& $root\nuget\NuGet.exe pack $root\csharp.Helper\csharp.Helper.csproj -Symbols -IncludeReferencedProjects -Prop Platform=AnyCPU`;Configuration=$Env:CONFIGURATION