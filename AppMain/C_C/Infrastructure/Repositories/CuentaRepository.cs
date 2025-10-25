using C_C.Application.Repositories;
using C_C.Domain;
using C_C.Infrastructure.Common;
using Microsoft.Data.SqlClient;

namespace C_C.Infrastructure.Repositories;

public sealed class CuentaRepository : RepositoryBase, ICuentaRepository
{
    public CuentaRepository(SqlConnectionFactory? connectionFactory = null)
        : base(connectionFactory)
    {
    }

    public Task<Cuenta?> GetByIdAsync(int idCuenta, CancellationToken ct = default)
        => WithConnectionAsync(cn => GetByIdAsync(cn, null, idCuenta, ct), ct);

    public Task<Cuenta?> GetByEmailAsync(string email, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand(@"SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro
                                                   FROM dbo.Cuenta WHERE Email = @Email", cn);
            cmd.Parameters.AddWithValue("@Email", email);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return null;
            }

            return MapCuenta(reader);
        }, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand("SELECT 1 FROM dbo.Cuenta WHERE Email = @Email", cn);
            cmd.Parameters.AddWithValue("@Email", email);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is not null;
        }, ct);

    public Task<int> CreateCuentaAsync(string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default)
        => WithConnectionAsync(cn => CreateCuentaAsync(cn, null, email, passwordHash, estadoCuenta, ct), ct);

    public Task<int> CreateAlumnoAsync(Alumno alumno, CancellationToken ct = default)
        => WithConnectionAsync(cn => CreateAlumnoAsync(cn, null, alumno, ct), ct);

    public Task<bool> UpdatePasswordAsync(int idCuenta, string newPasswordHash, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand("UPDATE dbo.Cuenta SET Hash_Contrasena = @Hash WHERE ID_Cuenta = @Id", cn);
            cmd.Parameters.AddWithValue("@Hash", newPasswordHash);
            cmd.Parameters.AddWithValue("@Id", idCuenta);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return rows > 0;
        }, ct);

    public Task<bool> DeleteCuentaAsync(int idCuenta, CancellationToken ct = default)
        => WithConnectionAsync(cn => DeleteCuentaAsync(cn, null, idCuenta, ct), ct);

    public async Task<Cuenta?> GetByIdAsync(SqlConnection cn, SqlTransaction? tx, int idCuenta, CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand(@"SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro
                                               FROM dbo.Cuenta WHERE ID_Cuenta = @Id", cn, tx);
        cmd.Parameters.AddWithValue("@Id", idCuenta);
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapCuenta(reader);
    }

    public async Task<int> CreateCuentaAsync(SqlConnection cn, SqlTransaction? tx, string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand(@"INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Estado_Cuenta)
                                               OUTPUT INSERTED.ID_Cuenta
                                               VALUES (@Email, @Hash, @Estado)", cn, tx);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@Hash", passwordHash);
        cmd.Parameters.AddWithValue("@Estado", estadoCuenta);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    public async Task<int> CreateAlumnoAsync(SqlConnection cn, SqlTransaction? tx, Alumno alumno, CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand(@"INSERT INTO dbo.Alumno
                                               (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Correo, Carrera)
                                               VALUES (@Matricula, @IdCuenta, @Nombre, @Apaterno, @Amaterno, @FNac, @Genero, @Correo, @Carrera)", cn, tx);
        cmd.Parameters.AddWithValue("@Matricula", alumno.Matricula);
        cmd.Parameters.AddWithValue("@IdCuenta", alumno.ID_Cuenta);
        cmd.Parameters.AddWithValue("@Nombre", alumno.Nombre);
        cmd.Parameters.AddWithValue("@Apaterno", alumno.Apaterno);
        cmd.Parameters.AddWithValue("@Amaterno", alumno.Amaterno);
        cmd.Parameters.AddWithValue("@FNac", alumno.F_Nac);
        cmd.Parameters.AddWithValue("@Genero", alumno.Genero);
        cmd.Parameters.AddWithValue("@Correo", alumno.Correo);
        cmd.Parameters.AddWithValue("@Carrera", alumno.Carrera);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        return alumno.Matricula;
    }

    public async Task<bool> DeleteCuentaAsync(SqlConnection cn, SqlTransaction? tx, int idCuenta, CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand("DELETE FROM dbo.Cuenta WHERE ID_Cuenta = @Id", cn, tx);
        cmd.Parameters.AddWithValue("@Id", idCuenta);
        var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        return rows > 0;
    }

    private static Cuenta MapCuenta(SqlDataReader reader)
        => new()
        {
            ID_Cuenta = reader.GetInt32(0),
            Email = reader.GetString(1),
            Hash_Contrasena = reader.GetString(2),
            Estado_Cuenta = reader.GetByte(3),
            Fecha_Registro = reader.GetDateTime(4)
        };
}
