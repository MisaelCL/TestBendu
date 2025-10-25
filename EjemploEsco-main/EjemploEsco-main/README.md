# C_C - Aplicación de Citas (WPF .NET 8)

## Requisitos previos
- SDK .NET 8 (en Windows para ejecutar WPF)
- SQLite (opcional, se crea el archivo `c_c.db` automáticamente)

## Estructura del proyecto
```
C_C.sln
└── src/
    └── C_C.App/
        ├── App.xaml / App.xaml.cs
        ├── MainWindow.xaml / MainWindow.xaml.cs
        ├── Model/
        ├── Repositories/
        ├── Services/
        ├── ViewModel/
        ├── Views/
        └── Migrations/
```

### Arquitectura (ASCII)
```
+-------------------+
|       Views        |  XAML (WPF) + code-behind mínimo
+---------+---------+
          |
          v
+-------------------+
|    ViewModels      |  MVVM, comandos, validaciones UI
+---------+---------+
          |
          v
+-------------------+
|     Services       |  Reglas de negocio (Auth, Match, Chat)
+---------+---------+
          |
          v
+-------------------+
|   Repositories     |  EF Core, acceso a datos
+---------+---------+
          |
          v
+-------------------+
|      Models        |  Entidades dominio
+-------------------+
```

## Comandos útiles
```bash
# Restaurar paquetes
 dotnet restore C_C.sln

# Compilar (requiere Windows)
 dotnet build C_C.sln

# Ejecutar la app (requiere Windows)
 dotnet run --project src/C_C.App/C_C.App.csproj

# Crear nueva migración (ejecutar con dotnet-ef disponible)
 dotnet ef migrations add <Nombre> --project src/C_C.App/C_C.App.csproj
 dotnet ef database update --project src/C_C.App/C_C.App.csproj
```

## Características clave
- Arquitectura MVVM siguiendo patrones del proyecto de referencia.
- Persistencia con EF Core (SQLite) y migración inicial incluida.
- Servicios para autenticación, descubrimiento, matches, chat y moderación.
- Hash de contraseñas con BCrypt, sanitización de entradas y límite anti-spam.
- UI inicial con vistas: Login, Registro, Descubrir, Matches, ChatList, Chat, Perfil, Preferencias y Ajustes.
- Logging estructurado con Serilog (consola + archivo).
- Inyección de dependencias con `Microsoft.Extensions.DependencyInjection`.

## Consideraciones
- La ejecución de WPF solo es compatible con Windows; en otros sistemas operativos se puede revisar y compilar el código pero no ejecutar la UI.
- Las vistas proporcionan una interfaz base que puede estilizarse y conectarse con navegación adicional según necesidades de producto.
