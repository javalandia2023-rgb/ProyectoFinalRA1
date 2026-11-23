# Fase 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos los archivos .csproj de todos los proyectos necesarios
COPY ["ProyectoFinal.API/ProyectoFinal.API.csproj", "ProyectoFinal.API/"]
COPY ["ProyectoFinal.Domain/ProyectoFinal.Domain.csproj", "ProyectoFinal.Domain/"]
COPY ["ProyectoFinal.Persistence/ProyectoFinal.Persistence.csproj", "ProyectoFinal.Persistence/"]

# Restauramos las dependencias
RUN dotnet restore "ProyectoFinal.API/ProyectoFinal.API.csproj"

# Copiamos el resto del código y compilamos
COPY . .
WORKDIR "/src/ProyectoFinal.API"
RUN dotnet build "ProyectoFinal.API.csproj" -c Release -o /app/build

# Fase 2: Publicación
FROM build AS publish
RUN dotnet publish "ProyectoFinal.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Fase 3: Imagen final (más ligera)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProyectoFinal.API.dll"]
