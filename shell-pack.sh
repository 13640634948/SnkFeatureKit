key=$1
version=$2
directory="NugetOutput"
source="https://api.nuget.org/v3/index.json"

nuget pack SnkFeatureKit.Asynchronous/SnkFeatureKit.Asynchronous.csproj -Version $version -OutputDirectory $directory/$version
nuget pack SnkFeatureKit.ContentDelivery/SnkFeatureKit.ContentDelivery.csproj -Version $version -OutputDirectory $directory/$version
nuget pack SnkFeatureKit.Logging/SnkFeatureKit.Logging.csproj -Version $version -OutputDirectory $directory/$version
nuget pack SnkFeatureKit.Patcher/SnkFeatureKit.Patcher.csproj -Version $version -OutputDirectory $directory/$version

#nuget push $directory/$version/SnkFeatureKit.Asynchronous.$version.nupkg $key -Source $source
#nuget push $directory/$version/SnkFeatureKit.ContentDelivery.$version.nupkg $key -Source $source
#nuget push $directory/$version/SnkFeatureKit.Logging.$version.nupkg $key -Source $source
#nuget push $directory/$version/SnkFeatureKit.Patcher.$version.nupkg $key -Source $source