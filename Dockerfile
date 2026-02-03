# Sử dụng SDK .NET 8.0 để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj và restore (đường dẫn dựa trên ảnh a6abbe)
COPY ["SportBookingSystem.csproj", "./"]
RUN dotnet restore "SportBookingSystem.csproj"

# Copy toàn bộ code và build
COPY . .
RUN dotnet build "SportBookingSystem.csproj" -c Release -o /app/build

# Publish ứng dụng
FROM build AS publish
RUN dotnet publish "SportBookingSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime để chạy web
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render yêu cầu cổng 8080 hoặc cấu hình biến môi trường
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Chạy file .dll chính của dự án
ENTRYPOINT ["dotnet", "SportBookingSystem.dll"]