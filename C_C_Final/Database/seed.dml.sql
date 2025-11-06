-- ------------------------------------------------------------
-- Datos de ejemplo para la base C_CBD
-- Ejecutar después de crear el esquema
-- ------------------------------------------------------------
USE [C_CBD];
GO

SET NOCOUNT ON;
GO

DECLARE @CuentaAna INT,
        @CuentaLuis INT,
        @CuentaMaria INT;

INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta)
VALUES
    ('ana.martinez@alumnos.universidad.edu', 'HASH_ANA', 'SALT_ANA', 1);
SET @CuentaAna = SCOPE_IDENTITY();

INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta)
VALUES
    ('luis.gomez@alumnos.universidad.edu', 'HASH_LUIS', 'SALT_LUIS', 1);
SET @CuentaLuis = SCOPE_IDENTITY();

INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta)
VALUES
    ('maria.lopez@alumnos.universidad.edu', 'HASH_MARIA', 'SALT_MARIA', 1);
SET @CuentaMaria = SCOPE_IDENTITY();

INSERT INTO dbo.Alumno (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Carrera)
VALUES
    ('A001', @CuentaAna, 'Ana', 'Martínez', 'Ruiz', '2000-04-12', 'F', 'Ingeniería en Sistemas'),
    ('A002', @CuentaLuis, 'Luis', 'Gómez', 'Hernández', '1999-09-02', 'M', 'Diseño Gráfico'),
    ('A003', @CuentaMaria, 'María', 'López', 'Santos', '2001-01-25', 'F', 'Psicología');

DECLARE @PerfilAna INT,
        @PerfilLuis INT,
        @PerfilMaria INT;

INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
VALUES (@CuentaAna, 'AnaM', N'Amante del arte y la tecnología.', NULL);
SET @PerfilAna = SCOPE_IDENTITY();

INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
VALUES (@CuentaLuis, 'Luigi', N'Diseñador gráfico con pasión por la fotografía.', NULL);
SET @PerfilLuis = SCOPE_IDENTITY();

INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
VALUES (@CuentaMaria, 'MaryL', N'Psicología, música y voluntariado.', NULL);
SET @PerfilMaria = SCOPE_IDENTITY();

INSERT INTO dbo.Preferencias (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses)
VALUES
    (@PerfilAna, 0, 20, 30, '', N'Arte; Tecnología; Viajes'),
    (@PerfilLuis, 1, 19, 28, 'Arquitectura', N'Fotografía; Diseño; Cafés'),
    (@PerfilMaria, 2, 20, 32, '', N'Voluntariado; Música; Lectura');

DECLARE @MatchAnaLuis INT,
        @MatchLuisMaria INT;

INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado)
VALUES
    (@PerfilAna, @PerfilLuis, 'aceptado');
SET @MatchAnaLuis = SCOPE_IDENTITY();

INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado)
VALUES
    (@PerfilLuis, @PerfilMaria, 'pendiente');
SET @MatchLuisMaria = SCOPE_IDENTITY();

DECLARE @ChatAnaLuis INT;

INSERT INTO dbo.Chat (ID_Match)
VALUES (@MatchAnaLuis);
SET @ChatAnaLuis = SCOPE_IDENTITY();

INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido, Confirmacion_Lectura)
VALUES
    (@ChatAnaLuis, @PerfilAna, N'Hola Luis, ¿listo para la expo de arte digital?', 1),
    (@ChatAnaLuis, @PerfilLuis, N'¡Claro! Ya tengo los boletos para el viernes.', 0);
GO
