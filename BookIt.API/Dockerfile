FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src
EXPOSE 7065

COPY *.sln ./
COPY BookIt.API/*.csproj ./BookIt.API/
COPY BookIt.BLL/*.csproj ./BookIt.BLL/
COPY BookIt.DAL/*.csproj ./BookIt.DAL/

RUN dotnet restore

COPY . ./

RUN dotnet publish BookIt.API/BookIt.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "BookIt.API.dll"]