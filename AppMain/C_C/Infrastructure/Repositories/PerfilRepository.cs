using C_C.Application.Repositories;
using C_C.Domain;
using C_C.Infrastructure.Common;
using Microsoft.Data.SqlClient;

namespace C_C.Infrastructure.Repositories;

public sealed class PerfilRepository : RepositoryBase, IPerfilRepository
{
    public PerfilRepository(SqlConnectionFactory? connectionFactory = null)
        : base(connectionFactory)
    {
    }

    public Task<Perfil?> GetByIdAsync(int idPerfil, CancellationToken ct = default)
        => WithConnectionAsync(async cn => await GetByIdInternalAsync(cn, null, idPerfil, ct).ConfigureAwait(false), ct);

    public Task<Perfil?> GetByCuentaIdAsync(int idCuenta, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand(@"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion
                                                   FROM dbo.Perfil WHERE ID_Cuenta = @IdCuenta", cn);
            cmd.Parameters.AddWithValue("@IdCuenta", idCuenta);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return null;
            }

            return MapPerfil(reader);
        }, ct);

    public Task<Preferencias?> GetPreferenciasByPerfilAsync(int idPerfil, CancellationToken ct = default)
        => WithConnectionAsync(cn => GetPreferenciasInternalAsync(cn, null, idPerfil, ct), ct);

    public Task<int> CreatePerfilAsync(Perfil perfil, CancellationToken ct = default)
        => WithConnectionAsync(cn => CreatePerfilAsync(cn, null, perfil, ct), ct);

    public Task<bool> UpdatePerfilAsync(Perfil perfil, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand(@"UPDATE dbo.Perfil
                                                   SET Nikname = @Nikname,
                                                       Biografia = @Biografia,
                                                       Foto_Perfil = @Foto
                                                   WHERE ID_Perfil = @Id", cn);
            cmd.Parameters.AddWithValue("@Nikname", perfil.Nikname);
            cmd.Parameters.AddWithValue("@Biografia", (object?)perfil.Biografia ?? DBNull.Value);
            var fotoParam = cmd.Parameters.Add("@Foto", System.Data.SqlDbType.VarBinary, -1);
            fotoParam.Value = (object?)perfil.Foto_Perfil ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@Id", perfil.ID_Perfil);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return rows > 0;
        }, ct);

    public Task<int> UpsertPreferenciasAsync(Preferencias prefs, CancellationToken ct = default)
        => WithConnectionAsync(cn => UpsertPreferenciasAsync(cn, null, prefs, ct), ct);

    public Task<bool> DeletePerfilAsync(int idPerfil, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand("DELETE FROM dbo.Perfil WHERE ID_Perfil = @Id", cn);
            cmd.Parameters.AddWithValue("@Id", idPerfil);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return rows > 0;
        }, ct);

    public Task<int> CreatePerfilAsync(SqlConnection cn, SqlTransaction? tx, Perfil perfil, CancellationToken ct = default)
        => CreatePerfilAsyncInternal(cn, tx, perfil, ct);

    public Task<int> UpsertPreferenciasAsync(SqlConnection cn, SqlTransaction? tx, Preferencias prefs, CancellationToken ct = default)
        => UpsertPreferenciasAsyncInternal(cn, tx, prefs, ct);

    private async Task<Perfil?> GetByIdInternalAsync(SqlConnection cn, SqlTransaction? tx, int idPerfil, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(@"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion
                                               FROM dbo.Perfil WHERE ID_Perfil = @Id", cn, tx);
        cmd.Parameters.AddWithValue("@Id", idPerfil);
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapPerfil(reader);
    }

    private async Task<Preferencias?> GetPreferenciasInternalAsync(SqlConnection cn, SqlTransaction? tx, int idPerfil, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(@"SELECT ID_Preferencias, ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses
                                               FROM dbo.Preferencias WHERE ID_Perfil = @IdPerfil", cn, tx);
        cmd.Parameters.AddWithValue("@IdPerfil", idPerfil);
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapPreferencias(reader);
    }

    private async Task<int> CreatePerfilAsyncInternal(SqlConnection cn, SqlTransaction? tx, Perfil perfil, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(@"INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
                                               OUTPUT INSERTED.ID_Perfil
                                               VALUES (@IdCuenta, @Nikname, @Biografia, @Foto)", cn, tx);
        cmd.Parameters.AddWithValue("@IdCuenta", perfil.ID_Cuenta);
        cmd.Parameters.AddWithValue("@Nikname", perfil.Nikname);
        cmd.Parameters.AddWithValue("@Biografia", (object?)perfil.Biografia ?? DBNull.Value);
        var fotoParam = cmd.Parameters.Add("@Foto", System.Data.SqlDbType.VarBinary, -1);
        fotoParam.Value = (object?)perfil.Foto_Perfil ?? DBNull.Value;
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    private async Task<int> UpsertPreferenciasAsyncInternal(SqlConnection cn, SqlTransaction? tx, Preferencias prefs, CancellationToken ct)
    {
        await using var updateCmd = new SqlCommand(@"UPDATE dbo.Preferencias
                                                       SET Preferencia_Genero = @Genero,
                                                           Edad_Minima = @EdadMin,
                                                           Edad_Maxima = @EdadMax,
                                                           Preferencia_Carrera = @Carrera,
                                                           Intereses = @Intereses
                                                     WHERE ID_Perfil = @IdPerfil", cn, tx);
        updateCmd.Parameters.AddWithValue("@Genero", prefs.Preferencia_Genero);
        updateCmd.Parameters.AddWithValue("@EdadMin", prefs.Edad_Minima);
        updateCmd.Parameters.AddWithValue("@EdadMax", prefs.Edad_Maxima);
        updateCmd.Parameters.AddWithValue("@Carrera", prefs.Preferencia_Carrera);
        updateCmd.Parameters.AddWithValue("@Intereses", (object?)prefs.Intereses ?? DBNull.Value);
        updateCmd.Parameters.AddWithValue("@IdPerfil", prefs.ID_Perfil);
        var rows = await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows > 0)
        {
            await using var selectCmd = new SqlCommand("SELECT ID_Preferencias FROM dbo.Preferencias WHERE ID_Perfil = @IdPerfil", cn, tx);
            selectCmd.Parameters.AddWithValue("@IdPerfil", prefs.ID_Perfil);
            var existing = await selectCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return Convert.ToInt32(existing);
        }

        await using var insertCmd = new SqlCommand(@"INSERT INTO dbo.Preferencias
                                                     (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses)
                                                     OUTPUT INSERTED.ID_Preferencias
                                                     VALUES (@IdPerfil, @Genero, @EdadMin, @EdadMax, @Carrera, @Intereses)", cn, tx);
        insertCmd.Parameters.AddWithValue("@IdPerfil", prefs.ID_Perfil);
        insertCmd.Parameters.AddWithValue("@Genero", prefs.Preferencia_Genero);
        insertCmd.Parameters.AddWithValue("@EdadMin", prefs.Edad_Minima);
        insertCmd.Parameters.AddWithValue("@EdadMax", prefs.Edad_Maxima);
        insertCmd.Parameters.AddWithValue("@Carrera", prefs.Preferencia_Carrera);
        insertCmd.Parameters.AddWithValue("@Intereses", (object?)prefs.Intereses ?? DBNull.Value);
        var inserted = await insertCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return Convert.ToInt32(inserted);
    }

    private static Perfil MapPerfil(SqlDataReader reader)
        => new()
        {
            ID_Perfil = reader.GetInt32(0),
            ID_Cuenta = reader.GetInt32(1),
            Nikname = reader.GetString(2),
            Biografia = reader.IsDBNull(3) ? null : reader.GetString(3),
            Foto_Perfil = reader.IsDBNull(4) ? null : (byte[])reader[4],
            Fecha_Creacion = reader.GetDateTime(5)
        };

    private static Preferencias MapPreferencias(SqlDataReader reader)
        => new()
        {
            ID_Preferencias = reader.GetInt32(0),
            ID_Perfil = reader.GetInt32(1),
            Preferencia_Genero = reader.GetByte(2),
            Edad_Minima = reader.GetInt32(3),
            Edad_Maxima = reader.GetInt32(4),
            Preferencia_Carrera = reader.GetString(5),
            Intereses = reader.IsDBNull(6) ? null : reader.GetString(6)
        };
}
