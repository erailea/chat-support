##with most slim .net 8 image build that .net 8 app
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY ChatSupport/. ./ChatSupport/
WORKDIR /app/ChatSupport
RUN dotnet restore
RUN dotnet publish -c Release -o out -v d

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/ChatSupport/out ./
ENTRYPOINT ["dotnet", "ChatSupport.dll"]
