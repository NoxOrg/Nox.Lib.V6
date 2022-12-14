xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" samples/Samples.Api/Samples.Api.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" samples/Samples.Api/Samples.Api.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" samples/Samples.Api/Samples.Api.csproj


