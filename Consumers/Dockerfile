FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Restore Stage
WORKDIR /src
COPY ["Consumers.csproj", "Consumers/"]
RUN dotnet restore "Consumers/Consumers.csproj"

WORKDIR /src/Consumers
COPY . .

# build Stage
RUN dotnet build "Consumers.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "Consumers.csproj" -c Release -o /app/publish


# Publish Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=5121
EXPOSE 5121
COPY --from=build /src/Consumers/Certificates /app/Certificates
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Consumers.dll" ]