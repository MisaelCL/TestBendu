CREATE OR ALTER TRIGGER TR_Mensaje_AfterInsert
ON dbo.Mensaje
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH ordered AS (
        SELECT i.ID_Chat,
               i.ID_Mensaje,
               i.Fecha_Envio,
               ROW_NUMBER() OVER (PARTITION BY i.ID_Chat ORDER BY i.Fecha_Envio DESC, i.ID_Mensaje DESC) AS rn
        FROM inserted i
    )
    UPDATE c
    SET c.LastMessageAtUtc = o.Fecha_Envio,
        c.LastMessageId    = o.ID_Mensaje
    FROM dbo.Chat c
    JOIN ordered o ON o.ID_Chat = c.ID_Chat AND o.rn = 1;
END;
