ALTER TABLE dbo.Match WITH CHECK
ADD CONSTRAINT CK_Match_Orden CHECK (Perfil_Emisor < Perfil_Receptor);

ALTER TABLE dbo.Match WITH CHECK
ADD CONSTRAINT UQ_Match_Pares UNIQUE (Perfil_Emisor, Perfil_Receptor);

ALTER TABLE dbo.Chat WITH CHECK
ADD CONSTRAINT UQ_Chat_Match UNIQUE (ID_Match);

ALTER TABLE dbo.Mensaje
ADD CONSTRAINT DF_Mensaje_FechaEnvio DEFAULT SYSUTCDATETIME() FOR Fecha_Envio;
