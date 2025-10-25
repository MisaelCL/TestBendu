using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace C_C.Repositories;

public sealed class MensajeRepository : RepositoryBase, IMensajeRepository
{
    private readonly ILogger<MensajeRepository> _logger;

    public MensajeRepository(IConnectionFactory connectionFactory, ILogger<MensajeRepository> logger)
        : base(connectionFactory)
    {
        _logger = logger;
    }

    public Task<long> EnviarAsync(Mensaje msg, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            await using var transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);
            try
            {
                var id = await EnviarAsync(msg, connection, transaction, ct).ConfigureAwait(false);
                await transaction.CommitAsync(ct).ConfigureAwait(false);
                return id;
            }
            catch
            {
                await transaction.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }, ct);
    }

    public async Task<long> EnviarAsync(Mensaje msg, SqlConnection connection, SqlTransaction? transaction, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido, Fecha_Envio, IsDeleted, IsEdited, Confirmacion_Lectura)
VALUES (@Chat, @Remitente, @Contenido, @FechaEnvio, @IsDeleted, @IsEdited, @Confirmado);
SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add(P("@Chat", msg.ID_Chat));
        command.Parameters.Add(P("@Remitente", msg.Remitente));
        command.Parameters.Add(P("@Contenido", msg.Contenido));
        command.Parameters.Add(P("@FechaEnvio", msg.Fecha_Envio));
        command.Parameters.Add(P("@IsDeleted", msg.IsDeleted));
        command.Parameters.Add(P("@IsEdited", msg.IsEdited));
        command.Parameters.Add(P("@Confirmado", msg.Confirmacion_Lectura));

        var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
        var id = Convert.ToInt64(result, CultureInfo.InvariantCulture);
        _logger.LogInformation("Mensaje {MensajeId} enviado al chat {ChatId}", id, msg.ID_Chat);
        return id;
    }

    public Task<int> MarcarLeidoAsync(long ID_Mensaje, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = "UPDATE dbo.Mensaje SET Confirmacion_Lectura = 1 WHERE ID_Mensaje = @Mensaje";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Mensaje", ID_Mensaje));
            var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("Mensaje {MensajeId} marcado como leído. Filas: {Rows}", ID_Mensaje, rows);
            return rows;
        }, ct);
    }

    public Task<int> EditarContenidoAsync(long ID_Mensaje, string nuevo, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = "UPDATE dbo.Mensaje SET Contenido = @Contenido, IsEdited = 1 WHERE ID_Mensaje = @Mensaje AND IsDeleted = 0";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Contenido", nuevo));
            command.Parameters.Add(P("@Mensaje", ID_Mensaje));
            var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("Mensaje {MensajeId} editado. Filas: {Rows}", ID_Mensaje, rows);
            return rows;
        }, ct);
    }

    public Task<int> BorradoLogicoAsync(long ID_Mensaje, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = "UPDATE dbo.Mensaje SET IsDeleted = 1 WHERE ID_Mensaje = @Mensaje";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Mensaje", ID_Mensaje));
            var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("Mensaje {MensajeId} marcado como eliminado. Filas: {Rows}", ID_Mensaje, rows);
            return rows;
        }, ct);
    }

    public Task<IReadOnlyList<Mensaje>> ObtenerPaginaAsync(int ID_Chat, int top, DateTime? anchorFecha, long? anchorId, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            var sql = new StringBuilder();
            sql.Append(@"SELECT TOP(@Top) ID_Mensaje, ID_Chat, Remitente, Contenido, Fecha_Envio, IsDeleted, IsEdited, Confirmacion_Lectura
FROM dbo.Mensaje
WHERE ID_Chat = @Chat");
            if (anchorFecha.HasValue && anchorId.HasValue)
            {
                sql.Append(" AND ((Fecha_Envio < @AnchorFecha) OR (Fecha_Envio = @AnchorFecha AND ID_Mensaje < @AnchorId))");
            }
            sql.Append(" ORDER BY Fecha_Envio DESC, ID_Mensaje DESC");

            await using var command = new SqlCommand(sql.ToString(), connection);
            command.Parameters.Add(P("@Top", top));
            command.Parameters.Add(P("@Chat", ID_Chat));
            if (anchorFecha.HasValue && anchorId.HasValue)
            {
                command.Parameters.Add(P("@AnchorFecha", anchorFecha.Value));
                command.Parameters.Add(P("@AnchorId", anchorId.Value));
            }

            var mensajes = new List<Mensaje>();
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                mensajes.Add(new Mensaje
                {
                    ID_Mensaje = reader.GetInt64(0),
                    ID_Chat = reader.GetInt32(1),
                    Remitente = reader.GetInt32(2),
                    Contenido = reader.GetString(3),
                    Fecha_Envio = reader.GetDateTime(4),
                    IsDeleted = reader.GetBoolean(5),
                    IsEdited = reader.GetBoolean(6),
                    Confirmacion_Lectura = reader.GetBoolean(7)
                });
            }

            return mensajes;
        }, ct);
    }

    public Task<int> ContarNoLeidosAsync(int ID_Chat, int ID_Perfil, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"SELECT COUNT(*) FROM dbo.Mensaje WHERE ID_Chat = @Chat AND Remitente <> @Perfil AND Confirmacion_Lectura = 0 AND IsDeleted = 0";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Chat", ID_Chat));
            command.Parameters.Add(P("@Perfil", ID_Perfil));
            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return Convert.ToInt32(result, CultureInfo.InvariantCulture);
        }, ct);
    }
}
