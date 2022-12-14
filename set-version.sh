# Samples
xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" samples/Samples.Api/Samples.Api.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" samples/Samples.Api/Samples.Api.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" samples/Samples.Api/Samples.Api.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" samples/Samples.Cli/Samples.Cli.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" samples/Samples.Cli/Samples.Cli.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" samples/Samples.Cli/Samples.Cli.csproj

# src
xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Api/Nox.Api.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Api/Nox.Api.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Api/Nox.Api.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Api.OData/Nox.Api.OData.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Api.OData/Nox.Api.OData.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Api.OData/Nox.Api.OData.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Core/Nox.Core.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Core/Nox.Core.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Core/Nox.Core.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Data/Nox.Data.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Data/Nox.Data.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Data/Nox.Data.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Data.MySql/Nox.Data.MySql.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Data.MySql/Nox.Data.MySql.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Data.MySql/Nox.Data.MySql.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Data.Postgres/Nox.Data.Postgres.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Data.Postgres/Nox.Data.Postgres.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Data.Postgres/Nox.Data.Postgres.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Data.SqlServer/Nox.Data.SqlServer.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Data.SqlServer/Nox.Data.SqlServer.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Data.SqlServer/Nox.Data.SqlServer.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Entity.XtendedAttributes/Nox.Entity.XtendedAttributes.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Entity.XtendedAttributes/Nox.Entity.XtendedAttributes.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Entity.XtendedAttributes/Nox.Entity.XtendedAttributes.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Etl/Nox.Etl.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Etl/Nox.Etl.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Etl/Nox.Etl.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Generator/Nox.Generator.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Generator/Nox.Generator.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Generator/Nox.Generator.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Jobs/Nox.Jobs.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Jobs/Nox.Jobs.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Jobs/Nox.Jobs.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Lib/Nox.Lib.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Lib/Nox.Lib.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Lib/Nox.Lib.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Messaging/Nox.Messaging.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Messaging/Nox.Messaging.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Messaging/Nox.Messaging.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Messaging.AmazonSQS/Nox.Messaging.AmazonSQS.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Messaging.AmazonSQS/Nox.Messaging.AmazonSQS.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Messaging.AmazonSQS/Nox.Messaging.AmazonSQS.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Messaging.AzureServiceBus/Nox.Messaging.AzureServiceBus.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Messaging.AzureServiceBus/Nox.Messaging.AzureServiceBus.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Messaging.AzureServiceBus/Nox.Messaging.AzureServiceBus.csproj

xmlstarlet ed -L -u /Project/PropertyGroup/AssemblyVersion -v "$1.0" src/Nox.Messaging.RabbitMQ/Nox.Messaging.RabbitMQ.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/FileVersion -v "$1.0" src/Nox.Messaging.RabbitMQ/Nox.Messaging.RabbitMQ.csproj
xmlstarlet ed -L -u /Project/PropertyGroup/PackageVersion -v "$1" src/Nox.Messaging.RabbitMQ/Nox.Messaging.RabbitMQ.csproj
