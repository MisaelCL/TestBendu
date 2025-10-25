INSERT INTO dbo.Alumno (Matricula, Nombre, Apellido_Paterno, Apellido_Materno, Fecha_Nacimiento, Genero, Carrera)
VALUES
('A0001', 'María', 'García', 'López', '1999-05-10', 'Femenino', 'Ingeniería en Sistemas'),
('A0002', 'Carlos', 'Ramírez', 'Torres', '1998-02-18', 'Masculino', 'Diseño Digital');

INSERT INTO dbo.Cuenta (ID_Alumno, Email, PasswordHash, Fecha_Registro, Ultimo_Acceso, IsActive)
VALUES
(1, 'maria@example.com', '$2a$12$yjmSijNwa9uAd7kGouVpMe2hjc6uZXhwK40Oc0BkoWblcFN9uCGqu', SYSUTCDATETIME(), SYSUTCDATETIME(), 1),
(2, 'carlos@example.com', '$2a$12$yjmSijNwa9uAd7kGouVpMe2hjc6uZXhwK40Oc0BkoWblcFN9uCGqu', SYSUTCDATETIME(), SYSUTCDATETIME(), 1);

INSERT INTO dbo.Perfil (ID_Cuenta, Nickname, Nombre, Apellido_Paterno, Apellido_Materno, Genero, Fecha_Nacimiento, Carrera, Biografia)
VALUES
(1, 'mariag', 'María', 'García', 'López', 'Femenino', '1999-05-10', 'Ingeniería en Sistemas', 'Amante de la tecnología y el café.'),
(2, 'carlitos', 'Carlos', 'Ramírez', 'Torres', 'Masculino', '1998-02-18', 'Diseño Digital', 'Buscando nuevas aventuras.');

INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado)
VALUES (1, 2, 'Aceptado');

INSERT INTO dbo.Chat (ID_Match)
VALUES (1);

INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido, Fecha_Envio)
VALUES
(1, 1, N'Hola Carlos, ¿cómo estás?', SYSUTCDATETIME()),
(1, 2, N'Hola María, todo bien. ¿Y tú?', SYSUTCDATETIME());
