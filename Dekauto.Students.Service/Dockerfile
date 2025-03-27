FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7238

# Устанавливаем переменные окружения
COPY aspnetapp.pfx .
ENV ASPNETCORE_ENVIRONMENT=Development
ARG CERT_PATH
ARG CERT_PASSWORD
ARG ASPNETCORE_URLS
ARG CONN_STR

ENV ASPNETCORE_URLS=$ASPNETCORE_URLS
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=./${CERT_PATH}
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
ENV ConnectionStrings__Main=${CONN_STR}

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только файлы проекта для восстановления зависимостей
COPY ["Dekauto.Students.Service/Dekauto.Students.Service.csproj", "Dekauto.Students.Service/"]
RUN dotnet restore "Dekauto.Students.Service/Dekauto.Students.Service.csproj"

# Копируем остальные файлы
COPY . .
WORKDIR "/src/Dekauto.Students.Service"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dekauto.Students.Service.dll"]