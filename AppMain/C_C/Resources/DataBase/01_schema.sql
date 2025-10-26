IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo');
END;

CREATE TABLE dbo.Alumno
(
    ID_Alumno INT IDENTITY(1,1) PRIMARY KEY,
    Matricula NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido_Paterno NVARCHAR(100) NOT NULL,
    Apellido_Materno NVARCHAR(100) NULL,
    Fecha_Nacimiento DATE NOT NULL,
    Genero NVARCHAR(50) NOT NULL,
    Carrera NVARCHAR(150) NOT NULL,
    Fecha_Registro DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Cuenta
(
    ID_Cuenta INT IDENTITY(1,1) PRIMARY KEY,
    ID_Alumno INT NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Fecha_Registro DATETIME2(0) NOT NULL,
    Ultimo_Acceso DATETIME2(0) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Cuenta_Alumno FOREIGN KEY (ID_Alumno) REFERENCES dbo.Alumno(ID_Alumno)
);

CREATE TABLE dbo.Perfil
(
    ID_Perfil INT IDENTITY(1,1) PRIMARY KEY,
    ID_Cuenta INT NOT NULL,
    Nickname NVARCHAR(100) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido_Paterno NVARCHAR(100) NOT NULL,
    Apellido_Materno NVARCHAR(100) NULL,
    Genero NVARCHAR(50) NOT NULL,
    Fecha_Nacimiento DATE NOT NULL,
    Carrera NVARCHAR(150) NOT NULL,
    Biografia NVARCHAR(500) NULL,
    Foto_Principal NVARCHAR(500) NULL,
    Fecha_Creacion DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    Fecha_Actualizacion DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Perfil_Cuenta FOREIGN KEY (ID_Cuenta) REFERENCES dbo.Cuenta(ID_Cuenta)
);

CREATE TABLE dbo.Match
(
    ID_Match INT IDENTITY(1,1) PRIMARY KEY,
    Perfil_Emisor INT NOT NULL,
    Perfil_Receptor INT NOT NULL,
    Estado NVARCHAR(20) NOT NULL,
    Fecha_Creacion DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    Fecha_Respuesta DATETIME2(0) NULL,
    CONSTRAINT FK_Match_Perfil_Emisor FOREIGN KEY (Perfil_Emisor) REFERENCES dbo.Perfil(ID_Perfil),
    CONSTRAINT FK_Match_Perfil_Receptor FOREIGN KEY (Perfil_Receptor) REFERENCES dbo.Perfil(ID_Perfil)
);

CREATE TABLE dbo.Chat
(
    ID_Chat INT IDENTITY(1,1) PRIMARY KEY,
    ID_Match INT NOT NULL,
    Fecha_Creacion DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    LastMessageAtUtc DATETIME2(0) NULL,
    LastMessageId BIGINT NULL,
    CONSTRAINT FK_Chat_Match FOREIGN KEY (ID_Match) REFERENCES dbo.Match(ID_Match)
);

CREATE TABLE dbo.Mensaje
(
    ID_Mensaje BIGINT IDENTITY(1,1) PRIMARY KEY,
    ID_Chat INT NOT NULL,
    Remitente INT NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,
    Fecha_Envio DATETIME2(0) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    IsEdited BIT NOT NULL DEFAULT 0,
    Confirmacion_Lectura BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Mensaje_Chat FOREIGN KEY (ID_Chat) REFERENCES dbo.Chat(ID_Chat),
    CONSTRAINT FK_Mensaje_Perfil FOREIGN KEY (Remitente) REFERENCES dbo.Perfil(ID_Perfil)
);
