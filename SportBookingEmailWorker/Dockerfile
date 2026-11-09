FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SportBookingEmailWorker.csproj", "./"]
RUN dotnet restore "SportBookingEmailWorker.csproj"
COPY . .
RUN dotnet build "SportBookingEmailWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportBookingEmailWorker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SportBookingEmailWorker.dll"]
