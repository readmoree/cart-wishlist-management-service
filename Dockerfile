FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY out/ .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CartWishlistService.dll"]
