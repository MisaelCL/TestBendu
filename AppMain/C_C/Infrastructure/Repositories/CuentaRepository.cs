using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using C_C_Final.Application.Repositories;
using C_C_Final.Domain.Models;
using C_C_Final.Infrastructure.Data;

namespace C_C_Final.Infrastructure.Repositories
{
    public sealed class CuentaRepository : RepositoryBase, ICuentaRepository
    {
        public CuentaRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Task<Cuenta?> GetByIdAsync(int idCuenta, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idCuenta, SqlDbType.Int);

                using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    return null;
                }

                return MapCuenta(reader);
            }, ct);
        }

        public Task<Cuenta?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro FROM dbo.Cuenta WHERE Email = @Email";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Email", email, SqlDbType.NVarChar, 260);

                using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    return null;
                }

                return MapCuenta(reader);
            }, ct);
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Cuenta WHERE Email = @Email) THEN 1 ELSE 0 END";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Email", email, SqlDbType.NVarChar, 260);

                var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return Convert.ToInt32(result) == 1;
            }, ct);
        }

        public Task<int> CreateCuentaAsync(string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default)
        {
            return WithConnectionAsync(connection => CreateCuentaAsync(connection, null, email, passwordHash, estadoCuenta, ct), ct);
        }

        public Task<int> CreateAlumnoAsync(Alumno alumno, CancellationToken ct = default)
        {
            return WithConnectionAsync(connection => CreateAlumnoAsync(connection, null, alumno, ct), ct);
        }

        public Task<bool> UpdatePasswordAsync(int idCuenta, string newPasswordHash, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "UPDATE dbo.Cuenta SET Hash_Contrasena = @Hash WHERE ID_Cuenta = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Hash", newPasswordHash, SqlDbType.NVarChar, -1);
                AddParameter(command, "@Id", idCuenta, SqlDbType.Int);

                var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return rows > 0;
            }, ct);
        }

        public Task<bool> DeleteCuentaAsync(int idCuenta, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "DELETE FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idCuenta, SqlDbType.Int);

                var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return rows > 0;
            }, ct);
        }

        public async Task<int> CreateCuentaAsync(SqlConnection connection, SqlTransaction? tx, string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro)
OUTPUT INSERTED.ID_Cuenta
VALUES (@Email, @Hash, @Estado, @Fecha);";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Email", email, SqlDbType.NVarChar, 260);
            AddParameter(command, "@Hash", passwordHash, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Estado", estadoCuenta, SqlDbType.TinyInt);
            AddParameter(command, "@Fecha", DateTime.UtcNow, SqlDbType.DateTime2);

            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return Convert.ToInt32(result);
        }

        public async Task<int> CreateAlumnoAsync(SqlConnection connection, SqlTransaction? tx, Alumno alumno, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO dbo.Alumno (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Correo, Carrera)
VALUES (@Matricula, @Cuenta, @Nombre, @Apaterno, @Amaterno, @Nacimiento, @Genero, @Correo, @Carrera);";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Matricula", alumno.Matricula, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Cuenta", alumno.IdCuenta, SqlDbType.Int);
            AddParameter(command, "@Nombre", alumno.Nombre, SqlDbType.NVarChar, 100);
            AddParameter(command, "@Apaterno", alumno.ApellidoPaterno, SqlDbType.NVarChar, 100);
            AddParameter(command, "@Amaterno", alumno.ApellidoMaterno, SqlDbType.NVarChar, 100);
            AddParameter(command, "@Nacimiento", alumno.FechaNacimiento, SqlDbType.Date);
            AddParameter(command, "@Genero", alumno.Genero, SqlDbType.Char, 1);
            AddParameter(command, "@Correo", alumno.Correo, SqlDbType.NVarChar, 260);
            AddParameter(command, "@Carrera", alumno.Carrera, SqlDbType.NVarChar, 100);

            await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return alumno.IdCuenta;
        }

        private static Cuenta MapCuenta(SqlDataReader reader)
        {
            return new Cuenta
            {
                IdCuenta = reader.GetInt32(0),
                Email = reader.GetString(1),
                HashContrasena = reader.GetString(2),
                EstadoCuenta = reader.GetByte(3),
                FechaRegistro = reader.GetDateTime(4)
            };
        }
    }
}
