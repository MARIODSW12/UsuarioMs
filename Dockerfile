FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
WORKDIR /src/Usuarios.API

RUN dotnet restore Usuarios.API.sln

RUN dotnet publish Usuarios.API.sln -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /out .

EXPOSE 80
ENTRYPOINT ["dotnet", "Usuarios.API.dll"]
