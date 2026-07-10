FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["Todos-Api.csproj", "./"]
RUN dotnet restore "./Todos-Api.csproj"

# copy everything else and publish
COPY . .
RUN dotnet publish "./Todos-Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Todos-Api.dll"]
