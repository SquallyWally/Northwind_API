#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Northwind.Mvc/Northwind.Mvc.csproj", "Northwind.Mvc/"]
COPY ["../Northwind.Common.DataContext.SqlServer/Northwind.Common.DataContext.SqlServer.csproj", "../Northwind.Common.DataContext.SqlServer/"]
COPY ["../Northwind.Common.EntityModels.SqlServer/Northwind.Common.EntityModels.SqlServer.csproj", "../Northwind.Common.EntityModels.SqlServer/"]
RUN dotnet restore "Northwind.Mvc/Northwind.Mvc.csproj"
COPY . .
WORKDIR "/src/Northwind.Mvc"
RUN dotnet build "Northwind.Mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Northwind.Mvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Northwind.Mvc.dll"]