# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BoboMeet.McpServer.csproj", "./"]
RUN dotnet restore "BoboMeet.McpServer.csproj"

COPY . .
RUN dotnet publish "BoboMeet.McpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 运行时阶段
FROM mcr.microsoft.com/dotnet/aspnet:10.0.9 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BoboMeet.McpServer.dll"]
