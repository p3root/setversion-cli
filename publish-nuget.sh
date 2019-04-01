#!/bin/sh

buildDir=$1
for line in $(find $buildDir  -name "*.nupkg"); do

       echo "publish $line..."
       apiKey=$2

	dotnet nuget push $line -k $apiKey
done