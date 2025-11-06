INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta, Fecha_Registro)
VALUES
('maria@example.com', 'Ffd1+1e6k+KC5v6MEZHos3xVXQfKXVvJ0l5tXmtk4ic=', 'w7z+zafTzGJ4i8k1QxL9YQ==', 1, SYSUTCDATETIME()),
('carlos@example.com', 'Ffd1+1e6k+KC5v6MEZHos3xVXQfKXVvJ0l5tXmtk4ic=', 'w7z+zafTzGJ4i8k1QxL9YQ==', 1, SYSUTCDATETIME());

INSERT INTO dbo.Alumno (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Correo, Carrera)
VALUES
('A0001', 1001, 'María', 'García', 'López', '1999-05-10', 'F', 'maria@example.com', 'Ingeniería en Sistemas'),
('A0002', 1002, 'Carlos', 'Ramírez', 'Torres', '1998-02-18', 'M', 'carlos@example.com', 'Diseño Digital');

INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, FotoPerfil, FechaCreacion)
VALUES
(1001, 'mariag', 'Amante de la tecnología y el café.', NULL, SYSUTCDATETIME()),
(1002, 'carlitos', 'Buscando nuevas aventuras.', NULL, SYSUTCDATETIME());

INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado)
VALUES (1, 2, 'Aceptado');

INSERT INTO dbo.Chat (ID_Match)
VALUES (1);

INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido, Fecha_Envio)
VALUES
(1, 1, N'Hola Carlos, ¿cómo estás?', SYSUTCDATETIME()),
(1, 2, N'Hola María, todo bien. ¿Y tú?', SYSUTCDATETIME());
