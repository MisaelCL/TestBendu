¡de lujo! Aquí tienes el **PROMPT MAESTRO** listo para pegar en tu generador (Codex). Está adaptado exactamente al **esquema de BD actual** que acordamos (sin enums/catálogos; `Preferencia_Carrera` e `Intereses` como texto). Incluye objetivos, arquitectura, contratos esperados, VMs a generar, reglas de calidad y criterios de aceptación.

---

# 🧠 Contexto del proyecto

* App: **WPF .NET Framework 4.7.2** (MVVM).
* Acceso a datos: **ADO.NET puro** (SqlClient).
* Configuración: **App.config** con `<connectionStrings>`; nombre: `"DefaultConnection"`.
* Patrón de dominio y repos: **Repository por Aggregate Root**.

  * **Cuenta (AR)** → hijo: **Alumno**.
  * **Perfil (AR)** → hijo: **Preferencias**.
  * **Match (AR)** → hijos: **Chat** y **Mensaje**.
* Transacciones: operaciones compuestas (registro, chat inicial, etc.) deben ser **atómicas**.

# 🗄️ Esquema de BD (SQL Server, ya creado)

> No crear/alterar tablas: **usar tal cual**.

**dbo.Cuenta**
`ID_Cuenta (PK, int identity 1001)`, `Email (nvarchar(200), UNIQUE)`, `Hash_Contrasena (nvarchar(200))`, `Estado_Cuenta (tinyint, default 1)`, `Fecha_Registro (datetime2(3), default SYSUTCDATETIME())`.

**dbo.Alumno** (hijo 1:1 de Cuenta)
`Matricula (PK, int)`, `ID_Cuenta (int, UNIQUE, FK → Cuenta ON DELETE CASCADE)`, `Nombre`, `Apaterno`, `Amaterno` (nvarchar(50) c/u), `F_Nac (date)`, `Genero (char(1) 'M'/'F')`, `Correo (nvarchar(50), UNIQUE)`, `Carrera (nvarchar(50))`.

**dbo.Perfil** (AR 1:1 con Cuenta)
`ID_Perfil (PK, int identity 2001)`, `ID_Cuenta (int, UNIQUE, FK → Cuenta ON DELETE CASCADE)`, `Nikname (nvarchar(50), UNIQUE)`, `Biografia (nvarchar(MAX))`, `Foto_Perfil (varbinary(MAX) o trata como ruta si cambias el tipo)`, `Fecha_Creacion (datetime2(3), default SYSUTCDATETIME())`.

**dbo.Preferencias** (hijo 1:1 de Perfil)
`ID_Preferencias (PK, int identity 3001)`, `ID_Perfil (int, UNIQUE, FK → Perfil ON DELETE CASCADE)`,
`Preferencia_Genero (tinyint {0=Todos,1=M,2=F,3=Otro})`, `Edad_Minima (int)`, `Edad_Maxima (int)`,
`Preferencia_Carrera (nvarchar(50))`, `Intereses (nvarchar(MAX))`.

**dbo.Match** (AR)
`ID_Match (PK, int identity 4001)`, `Perfil_Emisor (FK Perfil)`, `Perfil_Receptor (FK Perfil)`,
`Estado (char(10) IN ('pendiente','aceptado','rechazado','roto'))`, `Fecha_Match (datetime2(3) default UTC)`,
`UQ(Perfil_Emisor, Perfil_Receptor)`, `CHECK(Perfil_Emisor <> Perfil_Receptor)`.

**dbo.Chat** (hijo 1:1 de Match)
`ID_Chat (PK, int identity 100001)`, `ID_Match (FK Match, UNIQUE, ON DELETE CASCADE)`,
`Fecha_Creacion (UTC default)`, `LastMessageAtUtc (datetime2(3) NULL)`, `LastMessageId (bigint NULL)`.

**dbo.Mensaje** (hijo de Chat)
`ID_Mensaje (PK, bigint identity)`, `ID_Chat (FK Chat, ON DELETE CASCADE)`, `Remitente (FK Perfil)`,
`Contenido (nvarchar(MAX))`, `Fecha_Envio (UTC default)`,
`Confirmacion_Lectura (bit default 0)`, `IsEdited (bit default 0)`, `EditedAtUtc (datetime2(3) NULL)`, `IsDeleted (bit default 0)`.
Índices: `IX_Mensaje_Chat_Fecha (ID_Chat, Fecha_Envio DESC)`, `IX_Mensaje_Chat_Id (ID_Chat, ID_Mensaje DESC)`.

---

# 🎯 Objetivos de generación (qué debe producir Codex)

1. **Capa Infrastructure (Repos ADO.NET)**

   * Interfaces y **implementaciones** para:

     * `ICuentaRepository` (AR Cuenta, gestiona Alumno).
     * `IPerfilRepository` (AR Perfil, gestiona Preferencias).
     * `IMatchRepository` (AR Match, gestiona Chat y Mensaje).
   * `RepositoryBase` con lectura de connection string desde `App.config`.
   * (Opcional) `UnitOfWork` simple (`SqlConnection` + `SqlTransaction`).

2. **Capa Application/ViewModels (MVVM)**

   * `RegistroViewModel` (crear Cuenta+Alumno+Perfil+Preferencias en **una transacción**).
   * `PerfilViewModel` (ver/editar `Nikname`, `Biografia`, `Foto_Perfil`*; *si Foto son bytes, abstraer; si es ruta, usar string).
   * `PreferenciasViewModel` (editar género objetivo, rangos edad, carrera, intereses texto libre).
   * `InboxViewModel` (lista de chats por usuario actual, ordenado por `LastMessageAtUtc DESC`, paginado).
   * `ChatViewModel` (cargar chat por `ID_Match` o `ID_Chat`, listar mensajes con paginación, enviar mensaje, marcar lectura).
   * VMs con `INotifyPropertyChanged`, `ICommand` (RelayCommand), **async/await** y `CancellationToken`.

3. **Servicios de aplicación orquestadores**

   * `RegisterAlumnoService`: usa repos para registrar **Cuenta + Alumno + Perfil + Preferencias** en una **misma transacción**.
   * `MatchService`: crear match, `EnsureChatForMatch`, mandar primer mensaje (opcional), todo atómico.

4. **Infraestructura común**

   * `SqlConnectionFactory` o patrón similar para inyectar connection string.
   * Manejo de **errores**: excepciones específicas (NotFound/Conflict/Validation) o `Result<T>`.

---

# 📦 Estructura sugerida de namespaces/proyectos

* `C_C.Domain` (POCOs si se requieren DTOs de dominio).
* `C_C.Application` (Interfaces repos, DTOs de request/response, servicios de aplicación).
* `C_C.Infrastructure` (ADO.NET: RepositoryBase, Repos concretos, UnitOfWork, ConnectionFactory).
* `C_C.Presentation` (WPF: ViewModels, Commands, Validations).

---

# 🧾 Contratos (Interfaces) a generar (firmas exactas)

## ICuentaRepository (AR Cuenta; gestiona Alumno)

```
public interface ICuentaRepository
{
    // Lectura
    Task<Cuenta?> GetByIdAsync(int idCuenta, CancellationToken ct = default);
    Task<Cuenta?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    // Comando compuesto (Cuenta + Alumno + Perfil + Preferencias) – puede ir en servicio, pero repo debe exponer primitivas necesarias
    Task<int> CreateCuentaAsync(string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default);
    Task<int> CreateAlumnoAsync(Alumno alumno, CancellationToken ct = default); // alumno incluye ID_Cuenta
    Task<bool> UpdatePasswordAsync(int idCuenta, string newPasswordHash, CancellationToken ct = default);
    Task<bool> DeleteCuentaAsync(int idCuenta, CancellationToken ct = default); // FK ON DELETE CASCADE elimina Alumno/Perfil/Preferencias

    // Acceso “raw” para orquestación transaccional
    Task<Cuenta?> GetByIdAsync(SqlConnection cn, SqlTransaction? tx, int idCuenta, CancellationToken ct = default);
    Task<int> CreateCuentaAsync(SqlConnection cn, SqlTransaction? tx, string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default);
    Task<int> CreateAlumnoAsync(SqlConnection cn, SqlTransaction? tx, Alumno alumno, CancellationToken ct = default);
    Task<bool> DeleteCuentaAsync(SqlConnection cn, SqlTransaction? tx, int idCuenta, CancellationToken ct = default);
}
```

## IPerfilRepository (AR Perfil; gestiona Preferencias)

```
public interface IPerfilRepository
{
    // Lectura
    Task<Perfil?> GetByIdAsync(int idPerfil, CancellationToken ct = default);
    Task<Perfil?> GetByCuentaIdAsync(int idCuenta, CancellationToken ct = default);
    Task<Preferencias?> GetPreferenciasByPerfilAsync(int idPerfil, CancellationToken ct = default);

    // Comandos
    Task<int> CreatePerfilAsync(Perfil perfil, CancellationToken ct = default); // requiere ID_Cuenta existente
    Task<bool> UpdatePerfilAsync(Perfil perfil, CancellationToken ct = default); // nickname, bio, foto
    Task<int> UpsertPreferenciasAsync(Preferencias prefs, CancellationToken ct = default); // 1:1 por UQ(ID_Perfil)

    Task<bool> DeletePerfilAsync(int idPerfil, CancellationToken ct = default);

    // Overloads con conexión/transacción
    Task<int> CreatePerfilAsync(SqlConnection cn, SqlTransaction? tx, Perfil perfil, CancellationToken ct = default);
    Task<int> UpsertPreferenciasAsync(SqlConnection cn, SqlTransaction? tx, Preferencias prefs, CancellationToken ct = default);
}
```

## IMatchRepository (AR Match; gestiona Chat y Mensaje)

```
public interface IMatchRepository
{
    // Lectura
    Task<Match?> GetByIdAsync(int idMatch, CancellationToken ct = default);
    Task<bool> ExistsAsync(int idPerfilA, int idPerfilB, CancellationToken ct = default);
    Task<IReadOnlyList<Match>> ListByPerfilAsync(int idPerfil, int page, int pageSize, CancellationToken ct = default);

    // Comandos
    Task<int> CreateMatchAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default);
    Task<bool> UpdateEstadoAsync(int idMatch, string nuevoEstado, CancellationToken ct = default);
    Task<bool> DeleteMatchAsync(int idMatch, CancellationToken ct = default);

    // Chat (1:1 por UQ(ID_Match))
    Task<int> EnsureChatForMatchAsync(int idMatch, CancellationToken ct = default);
    Task<Chat?> GetChatByMatchIdAsync(int idMatch, CancellationToken ct = default);

    // Mensajes
    Task<long> AddMensajeAsync(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default);
    Task<IReadOnlyList<Mensaje>> ListMensajesAsync(int idChat, int page, int pageSize, CancellationToken ct = default);

    // Overloads con conexión/transacción para orquestación
    Task<int> CreateMatchAsync(SqlConnection cn, SqlTransaction? tx, int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default);
    Task<int> EnsureChatForMatchAsync(SqlConnection cn, SqlTransaction? tx, int idMatch, CancellationToken ct = default);
    Task<long> AddMensajeAsync(SqlConnection cn, SqlTransaction? tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default);
}
```

> Nota: **Autenticación** NO va en repos. El servicio de auth comparará el password con `Hash_Contrasena` leído desde `Cuenta`.

---

# 🧩 RepositoryBase, UnitOfWork y Connection

* `RepositoryBase` debe:

  * Leer `DefaultConnection` de `App.config` (`ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString`).
  * Exponer helpers `WithConnectionAsync` / `WithConnectionAsync<T>` para abrir/usar/cerrar.
  * Crear comandos parametrizados, `CommandTimeout` configurable.
* `UnitOfWork` (opcional recomendado):

  * `BeginAsync()` → abre `SqlConnection` y comienza `SqlTransaction`.
  * `CommitAsync()` / `RollbackAsync()`.
  * Todos los repos deben tener **overloads** que acepten `(SqlConnection cn, SqlTransaction? tx)`.

---

# 🧭 ViewModels a generar (capas de presentación)

* **RegistroViewModel**

  * Inputs: `Email`, `Password`, datos Alumno (`Matricula`, `Nombre`, `Apaterno`, `Amaterno`, `F_Nac`, `Genero`, `Correo`, `Carrera`).
  * Al confirmar: crea `Cuenta`, luego `Alumno`, luego `Perfil` (con `Nikname` derivado si no se da) y `Preferencias` con defaults.
  * Todo **en una transacción** (servicio `RegisterAlumnoService` usa repos con overloads cn/tx).
  * Validaciones: email requerido/formato; `Matricula` única; `Edad` calculada ≥ 18.

* **PerfilViewModel**

  * Carga por `ID_Cuenta` o `ID_Perfil`.
  * Edita `Nikname`, `Biografia`, `Foto_Perfil` (si bytes, exponer como `byte[]` con convertidor; si path, string).
  * Guardar cambios async con feedback de UI.

* **PreferenciasViewModel**

  * Carga y edita: `Preferencia_Genero (0..3)`, `Edad_Minima`, `Edad_Maxima`, `Preferencia_Carrera (texto controlado por UI)`, `Intereses (texto libre)`.
  * Guardar con `UpsertPreferenciasAsync`.

* **InboxViewModel**

  * Lista de chats por perfil/cuenta actual, ordenado por `Chat.LastMessageAtUtc DESC`.
  * Cada item muestra último mensaje (usar `LastMessageId` si está seteado; si no, consulta top 1 por índice).
  * Paginación.

* **ChatViewModel**

  * Abre chat por `ID_Match` (usa `EnsureChatForMatch`) o por `ID_Chat`.
  * Carga mensajes paginados por `ID_Chat` (usar índices).
  * `SendMessageAsync(contenido)`: inserta en `Mensaje`, actualiza `Chat.LastMessageAtUtc` / `LastMessageId` (en repo).
  * Marcar lectura: (simple) no persistir “visto” por ahora; solo `Confirmacion_Lectura` del propio mensaje si lo deseas.

---

# ✅ Criterios de aceptación (QA)

* **Compila** en .NET Framework 4.7.2, sin paquetes externos obligatorios.
* Todos los métodos de repos y servicios son **async** y aceptan `CancellationToken`.
* **SQL parametrizado** siempre (nada de concatenar strings).
* Consultas usan **índices** existentes (mensajes paginados por `(ID_Chat, Fecha_Envio DESC)` o `(ID_Chat, ID_Mensaje DESC)`).
* Registro crea **Cuenta + Alumno + Perfil + Preferencias** en **una sola transacción**; si falla algo, nada queda a medias.
* Eliminación de **Cuenta** borra en cascada **Alumno/Perfil/Preferencias**; eliminación de **Match** borra **Chat y Mensajes**.
* ViewModels implementan `INotifyPropertyChanged` y exponen `ICommand` (RelayCommand) sin lógica de datos en la View.

---

# 🔧 Configuración (App.config)

* Agregar:

```
<configuration>
  <connectionStrings>
    <add name="DefaultConnection"
         connectionString="Server=SERVIDOR\\INSTANCIA;Initial Catalog=C_CBD;Integrated Security=True;TrustServerCertificate=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

---

# 📝 Entrega solicitada de Codex

1. Archivos `.cs` para:

   * `Infrastructure`: `RepositoryBase.cs`, `SqlConnectionFactory.cs`, `UnitOfWork.cs`, `CuentaRepository.cs`, `PerfilRepository.cs`, `MatchRepository.cs`.
   * `Application`: Interfaces arriba definidas + DTOs de request/response simples si los usas.
   * `Presentation`: `RegistroViewModel.cs`, `PerfilViewModel.cs`, `PreferenciasViewModel.cs`, `InboxViewModel.cs`, `ChatViewModel.cs`, `RelayCommand.cs`, `BaseViewModel.cs`.
2. Código **probado** con llamadas mínimas (ej. consola o tests simples) que demuestren:

   * Registro completo transaccional.
   * Crear match + asegurar chat + enviar mensaje + listar mensajes.
3. Documentación breve en comentarios sobre **qué tabla** toca cada método.

---

> Si decides tratar `Foto_Perfil` como **ruta** (string) en vez de bytes, cambia en `Perfil` el tipo a `NVARCHAR(260)` y ajusta `PerfilRepository`/`PerfilViewModel` en consecuencia.
