-- ------------------------------------------------------------
-- Esquema de base de datos para la aplicación de coincidencias
-- Requiere SQL Server 2017 o superior
-- Base de datos objetivo: C_CBD
-- ------------------------------------------------------------
USE [C_CBD];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Elimina tablas existentes respetando dependencias
IF OBJECT_ID('dbo.Mensaje', 'U') IS NOT NULL DROP TABLE dbo.Mensaje;
IF OBJECT_ID('dbo.Chat', 'U') IS NOT NULL DROP TABLE dbo.Chat;
IF OBJECT_ID('dbo.Match', 'U') IS NOT NULL DROP TABLE dbo.Match;
IF OBJECT_ID('dbo.Preferencias', 'U') IS NOT NULL DROP TABLE dbo.Preferencias;
IF OBJECT_ID('dbo.Perfil', 'U') IS NOT NULL DROP TABLE dbo.Perfil;
IF OBJECT_ID('dbo.Alumno', 'U') IS NOT NULL DROP TABLE dbo.Alumno;
IF OBJECT_ID('dbo.Cuenta', 'U') IS NOT NULL DROP TABLE dbo.Cuenta;
GO

-- Tabla principal de autenticación
CREATE TABLE dbo.Cuenta
(
    ID_Cuenta          INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Email              NVARCHAR(260)     NOT NULL,
    Hash_Contrasena    NVARCHAR(MAX)     NOT NULL,
    Salt_Contrasena    NVARCHAR(MAX)     NOT NULL,
    Estado_Cuenta      TINYINT           NOT NULL CONSTRAINT DF_Cuenta_Estado DEFAULT (1),
    Fecha_Registro     DATETIME2(0)      NOT NULL CONSTRAINT DF_Cuenta_Fecha DEFAULT (SYSUTCDATETIME())
);
GO

CREATE UNIQUE INDEX UX_Cuenta_Email ON dbo.Cuenta (Email);
GO

-- Datos personales asociados a la cuenta
CREATE TABLE dbo.Alumno
(
    Matricula      NVARCHAR(50) NOT NULL PRIMARY KEY,
    ID_Cuenta      INT           NOT NULL UNIQUE,
    Nombre         NVARCHAR(100) NOT NULL,
    Apaterno       NVARCHAR(100) NOT NULL,
    Amaterno       NVARCHAR(100) NOT NULL,
    F_Nac          DATE          NOT NULL,
    Genero         CHAR(1)       NOT NULL,
    Carrera        NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Alumno_Cuenta FOREIGN KEY (ID_Cuenta) REFERENCES dbo.Cuenta (ID_Cuenta) ON DELETE CASCADE
);
GO

-- Información pública del usuario
CREATE TABLE dbo.Perfil
(
    ID_Perfil    INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    ID_Cuenta    INT                NOT NULL UNIQUE,
    Nikname      NVARCHAR(50)       NOT NULL,
    Biografia    NVARCHAR(500)      NULL,
    Foto_Perfil  IMAGE              NULL,
    CONSTRAINT FK_Perfil_Cuenta FOREIGN KEY (ID_Cuenta) REFERENCES dbo.Cuenta (ID_Cuenta) ON DELETE CASCADE
);
GO

-- Preferencias de búsqueda ligadas al perfil
CREATE TABLE dbo.Preferencias
(
    ID_Preferencias     INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    ID_Perfil           INT                NOT NULL UNIQUE,
    Preferencia_Genero  TINYINT            NOT NULL,
    Edad_Minima         INT                NOT NULL,
    Edad_Maxima         INT                NOT NULL,
    Preferencia_Carrera NVARCHAR(50)       NOT NULL CONSTRAINT DF_Preferencias_Carrera DEFAULT (''),
    Intereses           NVARCHAR(MAX)      NOT NULL CONSTRAINT DF_Preferencias_Intereses DEFAULT (''),
    CONSTRAINT FK_Preferencias_Perfil FOREIGN KEY (ID_Perfil) REFERENCES dbo.Perfil (ID_Perfil) ON DELETE CASCADE
);
GO

-- Relación entre perfiles (likes, matches, etc.)
CREATE TABLE dbo.Match
(
    ID_Match        INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Perfil_Emisor   INT                NOT NULL,
    Perfil_Receptor INT                NOT NULL,
    Estado          NVARCHAR(10)       NOT NULL,
    Fecha_Match     DATETIME2(0)       NOT NULL CONSTRAINT DF_Match_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Match_Perfil_Emisor FOREIGN KEY (Perfil_Emisor) REFERENCES dbo.Perfil (ID_Perfil) ON DELETE CASCADE,
    CONSTRAINT FK_Match_Perfil_Receptor FOREIGN KEY (Perfil_Receptor) REFERENCES dbo.Perfil (ID_Perfil) ON DELETE CASCADE,
    CONSTRAINT CK_Match_Perfiles_Distintos CHECK (Perfil_Emisor <> Perfil_Receptor)
);
GO

CREATE INDEX IX_Match_Emisor ON dbo.Match (Perfil_Emisor, Fecha_Match DESC);
CREATE INDEX IX_Match_Receptor ON dbo.Match (Perfil_Receptor, Fecha_Match DESC);
GO

-- Cabecera del chat asociada a un match aceptado
CREATE TABLE dbo.Chat
(
    ID_Chat          INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    ID_Match         INT                NOT NULL UNIQUE,
    Fecha_Creacion   DATETIME2(0)       NOT NULL CONSTRAINT DF_Chat_Fecha DEFAULT (SYSUTCDATETIME()),
    LastMessageAtUtc DATETIME2(0)       NULL,
    LastMessageId    BIGINT             NULL,
    CONSTRAINT FK_Chat_Match FOREIGN KEY (ID_Match) REFERENCES dbo.Match (ID_Match) ON DELETE CASCADE
);
GO

-- Mensajes individuales dentro de cada chat
CREATE TABLE dbo.Mensaje
(
    ID_Mensaje          BIGINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    ID_Chat             INT                   NOT NULL,
    Remitente           INT                   NOT NULL,
    Contenido           NVARCHAR(MAX)         NOT NULL,
    Fecha_Envio         DATETIME2(0)          NOT NULL CONSTRAINT DF_Mensaje_Fecha DEFAULT (SYSUTCDATETIME()),
    Confirmacion_Lectura BIT                  NOT NULL CONSTRAINT DF_Mensaje_Confirmacion DEFAULT (0),
    IsEdited            BIT                   NOT NULL CONSTRAINT DF_Mensaje_Editado DEFAULT (0),
    EditedAtUtc         DATETIME2(0)          NULL,
    IsDeleted           BIT                   NOT NULL CONSTRAINT DF_Mensaje_Borrado DEFAULT (0),
    CONSTRAINT FK_Mensaje_Chat FOREIGN KEY (ID_Chat) REFERENCES dbo.Chat (ID_Chat) ON DELETE CASCADE,
    CONSTRAINT FK_Mensaje_Perfil FOREIGN KEY (Remitente) REFERENCES dbo.Perfil (ID_Perfil)
);
GO

CREATE INDEX IX_Mensaje_ChatFecha ON dbo.Mensaje (ID_Chat, Fecha_Envio DESC);
GO
