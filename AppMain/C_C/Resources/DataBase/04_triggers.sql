CREATE OR ALTER TRIGGER TR_Mensaje_AfterInsert
ON dbo.Mensaje
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH ult AS (
        SELECT i.ID_Chat, MAX(i.Fecha_Envio) AS MaxFecha
        FROM inserted i
        GROUP BY i.ID_Chat
    )
    UPDATE c
    SET c.LastMessageAtUtc = u.MaxFecha,
        c.LastMessageId    = m.ID_Mensaje
    FROM dbo.Chat c
    JOIN ult u ON u.ID_Chat = c.ID_Chat
    JOIN dbo.Mensaje m ON m.ID_Chat = u.ID_Chat AND m.Fecha_Envio = u.MaxFecha;
END;
