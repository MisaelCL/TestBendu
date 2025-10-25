DECLARE @cuentas TABLE(ID_Cuenta INT, Email NVARCHAR(200));

INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Estado_Cuenta)
OUTPUT inserted.ID_Cuenta, inserted.Email INTO @cuentas
VALUES
('maria@example.com', '$2a$12$yjmSijNwa9uAd7kGouVpMe2hjc6uZXhwK40Oc0BkoWblcFN9uCGqu', 1),
('carlos@example.com', '$2a$12$yjmSijNwa9uAd7kGouVpMe2hjc6uZXhwK40Oc0BkoWblcFN9uCGqu', 1);

DECLARE @CuentaMaria INT = (SELECT ID_Cuenta FROM @cuentas WHERE Email = 'maria@example.com');
DECLARE @CuentaCarlos INT = (SELECT ID_Cuenta FROM @cuentas WHERE Email = 'carlos@example.com');

INSERT INTO dbo.Alumno (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Correo, Carrera)
VALUES
(2021001, @CuentaMaria, 'María', 'García', 'López', '1999-05-10', 'F', 'maria.uni@example.com', 'Ingeniería en Sistemas'),
(2021002, @CuentaCarlos, 'Carlos', 'Ramírez', 'Torres', '1998-02-18', 'M', 'carlos.uni@example.com', 'Diseño Digital');

DECLARE @PerfilMaria INT;
DECLARE @PerfilCarlos INT;

INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
VALUES (@CuentaMaria, 'mariag', N'Amante de la tecnología y el café.', NULL);
SET @PerfilMaria = SCOPE_IDENTITY();

INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
VALUES (@CuentaCarlos, 'carlitos', N'Buscando nuevas aventuras.', NULL);
SET @PerfilCarlos = SCOPE_IDENTITY();

INSERT INTO dbo.Preferencias (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, RadioKm)
VALUES
(@PerfilMaria, 0, 22, 32, 15),
(@PerfilCarlos, 1, 20, 30, 20);

DECLARE @PrefMaria INT = (SELECT ID_Preferencias FROM dbo.Preferencias WHERE ID_Perfil = @PerfilMaria);
DECLARE @PrefCarlos INT = (SELECT ID_Preferencias FROM dbo.Preferencias WHERE ID_Perfil = @PerfilCarlos);

INSERT INTO dbo.Preferencias_Carrera (ID_Preferencias, Nombre_Carrera)
VALUES
(@PrefMaria, N'Ingeniería en Sistemas'),
(@PrefCarlos, N'Diseño Digital');

INSERT INTO dbo.Intereses_Buscados (ID_Preferencias, Nombre_Interes)
VALUES
(@PrefMaria, N'Lectura'),
(@PrefCarlos, N'Música');

DECLARE @MatchId INT;

INSERT INTO dbo.[Match] (Perfil_Emisor, Perfil_Receptor, Estado)
VALUES (@PerfilMaria, @PerfilCarlos, N'aceptado');

SET @MatchId = SCOPE_IDENTITY();

DECLARE @ChatId INT;

INSERT INTO dbo.Chat (ID_Match)
VALUES (@MatchId);

SET @ChatId = SCOPE_IDENTITY();

INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido)
VALUES
(@ChatId, @PerfilMaria, N'Hola Carlos, ¿cómo estás?'),
(@ChatId, @PerfilCarlos, N'Hola María, todo bien. ¿Y tú?');
