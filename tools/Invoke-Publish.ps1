$BuildConfiguration = 'Release'
$root = (Split-Path $PSScriptRoot)
$Sln = [System.IO.Path]::Combine($root, 'SuperMaxxii.Apps.sln')

$destination = [System.IO.Path]::Combine($root, 'release')

dotnet restore  /property:Configuration=$BuildConfiguration --no-cache

dotnet publish $Sln -p:PublishSingleFile=true --self-contained -r win-x64 -f net5.0 -c $BuildConfiguration -o $destination /p:LangVersion=latest 
