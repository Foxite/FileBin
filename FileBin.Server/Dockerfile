﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FileBin.Server/FileBin.Server.csproj", "FileBin.Server/"]
RUN dotnet restore "FileBin.Server/FileBin.Server.csproj"
COPY . .
WORKDIR "/src/FileBin.Server"
RUN dotnet build "FileBin.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileBin.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./FileBin.Server"]
