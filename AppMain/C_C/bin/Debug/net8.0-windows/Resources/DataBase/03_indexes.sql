CREATE INDEX IX_Mensaje_Chat_FechaDesc
ON dbo.Mensaje (ID_Chat, Fecha_Envio DESC)
INCLUDE (ID_Mensaje, Remitente, IsDeleted, IsEdited, Confirmacion_Lectura);

CREATE INDEX IX_Mensaje_Unread
ON dbo.Mensaje (ID_Chat, Confirmacion_Lectura)
INCLUDE (Fecha_Envio)
WHERE Confirmacion_Lectura = 0 AND IsDeleted = 0;

CREATE INDEX IX_Mensaje_Remitente_Fecha
ON dbo.Mensaje (Remitente, Fecha_Envio DESC)
INCLUDE (ID_Chat, ID_Mensaje, IsDeleted);
