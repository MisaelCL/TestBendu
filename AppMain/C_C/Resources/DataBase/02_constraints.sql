ALTER TABLE dbo.Match WITH CHECK
ADD CONSTRAINT CK_Match_Orden CHECK (Perfil_Emisor < Perfil_Receptor);

ALTER TABLE dbo.Match WITH CHECK
ADD CONSTRAINT UQ_Match_Pares UNIQUE (Perfil_Emisor, Perfil_Receptor);

ALTER TABLE dbo.Chat WITH CHECK
ADD CONSTRAINT UQ_Chat_Match UNIQUE (ID_Match);

ALTER TABLE dbo.Perfil
ADD CONSTRAINT DF_Perfil_FechaActualizacion DEFAULT SYSUTCDATETIME() FOR Fecha_Actualizacion;

ALTER TABLE dbo.Cuenta
ADD CONSTRAINT DF_Cuenta_FechaRegistro DEFAULT SYSUTCDATETIME() FOR Fecha_Registro;

ALTER TABLE dbo.Cuenta
ADD CONSTRAINT DF_Cuenta_IsActive DEFAULT 1 FOR IsActive;

ALTER TABLE dbo.Mensaje
ADD CONSTRAINT DF_Mensaje_FechaEnvio DEFAULT SYSUTCDATETIME() FOR Fecha_Envio;
