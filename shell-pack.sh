version=$1
configuration=$2
key=$3


directory="NugetOutput"
source="https://api.nuget.org/v3/index.json"

proj_array[0]="SnkFeatureKit.Asynchronous"
proj_array[1]="SnkFeatureKit.ContentDelivery"
proj_array[2]="SnkFeatureKit.Logging"
proj_array[3]="SnkFeatureKit.Patcher"


for (( i = 0; i < ${#proj_array[@]}; i++ )); do
	proj_name=${proj_array[$i]}
	nuget pack $proj_name/$proj_name.csproj -Version $version -OutputDirectory $directory/$version -Properties Configuration=$configuration
done

if [ -n "$key" ]; then
	for (( i = 0; i < ${#proj_array[@]}; i++ )); do
		proj_name=${proj_array[$i]}
		nuget push $directory/$version/$proj_name.$version.nupkg $key -Source $source
	done
fi

