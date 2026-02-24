FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish LogHub.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 9696
ENV ASPNETCORE_URLS=http://+:9696

RUN mkdir -p /app/logs
VOLUME ["/app/logs"]

ENTRYPOINT ["dotnet", "LogHub.Server.dll"]
