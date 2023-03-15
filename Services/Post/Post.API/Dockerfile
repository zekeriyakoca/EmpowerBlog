#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Post/Post.API/Post.API.csproj", "Services/Post/Post.API/"]
COPY ["Common/EventBus/EventBus.csproj", "Common/EventBus/"]
COPY ["Services/Post/Post.Domain/Post.Domain.csproj", "Services/Post/Post.Domain/"]
COPY ["Services/Post/Post.Infrastructure/Post.Infrastructure.csproj", "Services/Post/Post.Infrastructure/"]
RUN dotnet restore "Services/Post/Post.API/Post.API.csproj"
COPY . .
WORKDIR "/src/Services/Post/Post.API"
RUN dotnet build "Post.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Post.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Post.API.dll"]