FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY project/TimesheetPayroll/ ./
RUN dotnet publish TimesheetPayroll.Web/TimesheetPayroll.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_RUNNING_IN_CONTAINER=true
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet TimesheetPayroll.Web.dll"]
