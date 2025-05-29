FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
EXPOSE 80

COPY *.sln ./
COPY Contacts.Api/*.csproj ./Contacts.Api/
COPY BL/*.csproj ./BL/
COPY DAL/*.csproj ./DAL/
RUN dotnet restore

COPY . .
WORKDIR /src/Contacts.Api
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Contacts.Api.dll"]