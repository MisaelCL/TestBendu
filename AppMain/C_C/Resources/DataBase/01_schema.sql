/* =========================================================
   0) Crear BD si no existe y cambiar contexto
   ========================================================= */
IF DB_ID(N'C_CBD') IS NULL
BEGIN
  DECLARE @sql_create_db NVARCHAR(MAX) = N'CREATE DATABASE C_CBD;';
  EXEC(@sql_create_db);
END;

IF (DB_NAME() <> N'C_CBD')
BEGIN
  DECLARE @sql_use NVARCHAR(MAX) = N'USE C_CBD;';
  EXEC(@sql_use);
END;

/* =========================================================
   1) CUENTA (AR)  — PK por ID_Cuenta; Email único
   ========================================================= */
IF OBJECT_ID('dbo.Cuenta','U') IS NULL
BEGIN
  CREATE TABLE dbo.Cuenta(
    ID_Cuenta       INT IDENTITY(1001,1) NOT NULL PRIMARY KEY,
    Email           NVARCHAR(200) NOT NULL UNIQUE,
    Hash_Contrasena NVARCHAR(200) NOT NULL,
    Estado_Cuenta   TINYINT NOT NULL CONSTRAINT DF_Cuenta_Estado DEFAULT(1),
    Fecha_Registro  DATETIME2(3) NOT NULL CONSTRAINT DF_Cuenta_Fecha DEFAULT (SYSUTCDATETIME())
  );
END;

/* =========================================================
   2) ALUMNO (hijo de Cuenta; 1:1 por UNIQUE(ID_Cuenta))
      Nota: sustituimos Edad por F_Nac para evitar desincronización
   ========================================================= */
IF OBJECT_ID('dbo.Alumno','U') IS NULL
BEGIN
  CREATE TABLE dbo.Alumno(
    Matricula   INT           NOT NULL PRIMARY KEY,
    ID_Cuenta   INT           NOT NULL UNIQUE,
    Nombre      NVARCHAR(50)  NOT NULL,
    Apaterno    NVARCHAR(50)  NOT NULL,
    Amaterno    NVARCHAR(50)  NOT NULL,
    F_Nac       DATE          NOT NULL,
    Genero      CHAR(1)       NOT NULL CHECK (Genero IN ('M','F')),
    Correo      NVARCHAR(50)  NULL UNIQUE,
    Carrera     NVARCHAR(50)  NOT NULL,
    CONSTRAINT FK_Alumno_Cuenta
      FOREIGN KEY (ID_Cuenta) REFERENCES dbo.Cuenta(ID_Cuenta)
      ON DELETE CASCADE
  );
END;

/* =========================================================
   3) PERFIL (AR)  — 1:1 con Cuenta (UNIQUE(ID_Cuenta))
   ========================================================= */
IF OBJECT_ID('dbo.Perfil','U') IS NULL
BEGIN
  CREATE TABLE dbo.Perfil(
    ID_Perfil      INT IDENTITY(2001,1) PRIMARY KEY NOT NULL,
    ID_Cuenta      INT NOT NULL UNIQUE,
    Nikname        NVARCHAR(50) NOT NULL UNIQUE,
    Biografia      NVARCHAR(500) NULL,
    Foto_Perfil    NVARCHAR(260) NULL,
    Fecha_Creacion DATETIME2(3) NOT NULL CONSTRAINT DF_Perfil_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Perfil_Cuenta
      FOREIGN KEY (ID_Cuenta) REFERENCES dbo.Cuenta(ID_Cuenta)
      ON DELETE CASCADE
  );
END;

/* =========================================================
   4) PREFERENCIAS (hija de Perfil; 1:1 por UNIQUE(ID_Perfil))
   ========================================================= */
IF OBJECT_ID('dbo.Preferencias','U') IS NULL
BEGIN
  CREATE TABLE dbo.Preferencias(
    ID_Preferencias INT IDENTITY(3001,1) PRIMARY KEY NOT NULL,
    ID_Perfil       INT NOT NULL UNIQUE,
    Preferencia_Genero TINYINT NOT NULL CONSTRAINT DF_Pref_Genero DEFAULT(0),
    Edad_Minima     INT NOT NULL CONSTRAINT DF_Pref_EdMin DEFAULT(18),
    Edad_Maxima     INT NOT NULL CONSTRAINT DF_Pref_EdMax DEFAULT(35),
    RadioKm         INT NOT NULL CONSTRAINT DF_Pref_Radio DEFAULT(10),
    CONSTRAINT FK_Preferencias_Perfil
      FOREIGN KEY (ID_Perfil) REFERENCES dbo.Perfil(ID_Perfil)
      ON DELETE CASCADE,
    CONSTRAINT CK_Pref_Genero CHECK (Preferencia_Genero IN (0,1,2,3)),
    CONSTRAINT CK_Pref_Edades CHECK (Edad_Minima >= 18 AND Edad_Maxima <= 99 AND Edad_Minima <= Edad_Maxima),
    CONSTRAINT CK_Pref_Radio  CHECK (RadioKm BETWEEN 1 AND 1000)
  );
END;

/* =========================================================
   5) INTERESES (catálogo) — opcional pero recomendado
   ========================================================= */
IF OBJECT_ID('dbo.Interes','U') IS NULL
BEGIN
  CREATE TABLE dbo.Interes(
    ID_Interes INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Nombre     NVARCHAR(100) NOT NULL UNIQUE
  );
END;

/* =========================================================
   6) INTERESES_BUSCADOS (M:N Preferencias<->Interes)
      — si prefieres sin catálogo, deja Nombre_Interes y quita FK a Interes
   ========================================================= */
IF OBJECT_ID('dbo.Intereses_Buscados','U') IS NULL
BEGIN
  CREATE TABLE dbo.Intereses_Buscados(
    ID_InteresesBuscados INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    ID_Preferencias      INT NOT NULL,
    ID_Interes           INT NULL,
    Nombre_Interes       NVARCHAR(50) NULL,
    CONSTRAINT FK_InteresesBuscados_Preferencias
      FOREIGN KEY (ID_Preferencias) REFERENCES dbo.Preferencias(ID_Preferencias)
      ON DELETE CASCADE,
    CONSTRAINT FK_InteresesBuscados_Interes
      FOREIGN KEY (ID_Interes) REFERENCES dbo.Interes(ID_Interes),
    CONSTRAINT CK_Intereses_Def CHECK (
      (ID_Interes IS NOT NULL AND Nombre_Interes IS NULL) OR
      (ID_Interes IS NULL AND Nombre_Interes IS NOT NULL)
    ),
    CONSTRAINT UQ_InteresesPref UNIQUE (ID_Preferencias, ID_Interes, Nombre_Interes)
  );
END;

/* =========================================================
   7) PREFERENCIAS_CARRERA (detalle por Preferencias)
   ========================================================= */
IF OBJECT_ID('dbo.Preferencias_Carrera','U') IS NULL
BEGIN
  CREATE TABLE dbo.Preferencias_Carrera(
    ID_PreferenciasCarrera INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    ID_Preferencias        INT NOT NULL,
    Nombre_Carrera         NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_PreferenciasCarrera_Preferencias
      FOREIGN KEY (ID_Preferencias) REFERENCES dbo.Preferencias(ID_Preferencias)
      ON DELETE CASCADE
  );

  CREATE INDEX IX_PrefCarrera_Pref ON dbo.Preferencias_Carrera(ID_Preferencias);
END;

/* =========================================================
   8) MATCH (AR) — evita duplicados con UNIQUE(Perfil_Emisor, Perfil_Receptor)
   ========================================================= */
IF OBJECT_ID('dbo.[Match]','U') IS NULL
BEGIN
  CREATE TABLE dbo.[Match](
    ID_Match        INT IDENTITY(4001,1) PRIMARY KEY NOT NULL,
    Perfil_Emisor   INT NOT NULL,
    Perfil_Receptor INT NOT NULL,
    Estado          NVARCHAR(50) NOT NULL
                     CHECK (Estado IN (N'pendiente', N'aceptado', N'rechazado', N'roto')),
    Fecha_Match     DATETIME2(3) NOT NULL CONSTRAINT DF_Match_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Match_PerfilE FOREIGN KEY (Perfil_Emisor)   REFERENCES dbo.Perfil(ID_Perfil),
    CONSTRAINT FK_Match_PerfilR FOREIGN KEY (Perfil_Receptor) REFERENCES dbo.Perfil(ID_Perfil),
    CONSTRAINT CK_Match_Distintos CHECK (Perfil_Emisor <> Perfil_Receptor),
    CONSTRAINT UQ_Match_Par UNIQUE (Perfil_Emisor, Perfil_Receptor)
  );
END;

/* =========================================================
   9) CHAT (hijo de Match; 1:1 mediante UNIQUE(ID_Match))
      — Quitamos columna 'Mensajes' (texto va en dbo.Mensaje)
   ========================================================= */
IF OBJECT_ID('dbo.Chat','U') IS NULL
BEGIN
  CREATE TABLE dbo.Chat(
    ID_Chat        INT IDENTITY(100001,1) PRIMARY KEY NOT NULL,
    ID_Match       INT NOT NULL UNIQUE,
    Fecha_Creacion DATETIME2(3) NOT NULL CONSTRAINT DF_Chat_Created DEFAULT (SYSUTCDATETIME()),
    LastMessageAtUtc DATETIME2(3) NULL,
    LastMessageId    BIGINT NULL,
    CONSTRAINT FK_Chat_Match
      FOREIGN KEY (ID_Match) REFERENCES dbo.[Match](ID_Match)
      ON DELETE CASCADE
  );

  CREATE INDEX IX_Chat_IdMatch ON dbo.Chat(ID_Match);
END;

/* =========================================================
   10) MENSAJE (hijo de Chat) — FK correcta a Chat(ID_Chat)
   ========================================================= */
IF OBJECT_ID('dbo.Mensaje','U') IS NULL
BEGIN
  CREATE TABLE dbo.Mensaje(
    ID_Mensaje  BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    ID_Chat     INT    NOT NULL,
    Remitente   INT    NOT NULL,
    Contenido   NVARCHAR(MAX) NOT NULL,
    Fecha_Envio DATETIME2(3) NOT NULL CONSTRAINT DF_Msg_Created DEFAULT (SYSUTCDATETIME()),
    Confirmacion_Lectura BIT NOT NULL CONSTRAINT DF_Msg_Confirm DEFAULT(0),
    IsEdited    BIT NOT NULL CONSTRAINT DF_Msg_Edited DEFAULT(0),
    EditedAtUtc DATETIME2(3) NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Msg_Deleted DEFAULT(0),
    CONSTRAINT FK_Mensaje_Chat
      FOREIGN KEY (ID_Chat)   REFERENCES dbo.Chat(ID_Chat)
      ON DELETE CASCADE,
    CONSTRAINT FK_Mensaje_Perfil
      FOREIGN KEY (Remitente) REFERENCES dbo.Perfil(ID_Perfil)
  );

  CREATE INDEX IX_Mensaje_Chat_Fecha ON dbo.Mensaje(ID_Chat, Fecha_Envio DESC);
  CREATE INDEX IX_Mensaje_Chat_Id    ON dbo.Mensaje(ID_Chat, ID_Mensaje DESC);
END;

/* =========================================================
   11) Semillas mínimas (Interés) — opcional
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Interes)
BEGIN
  INSERT INTO dbo.Interes(Nombre) VALUES
    (N'Música'),(N'Deportes'),(N'Lectura'),(N'Viajar'),(N'Cine');
END;
