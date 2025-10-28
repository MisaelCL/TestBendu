using System;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    public sealed class CuentaRepository : RepositoryBase, ICuentaRepository
    {
        public CuentaRepository(string connectionString = null) : base(connectionString)
        {
        }

        public Cuenta GetById(int idCuenta)
        {
            using var connection = OpenConnection();
            const string sql = "SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Id", idCuenta, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapCuenta(reader);
        }

        public Cuenta GetByEmail(string email)
        {
            using var connection = OpenConnection();
            const string sql = "SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro FROM dbo.Cuenta WHERE Email = @Email";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Email", email, SqlDbType.NVarChar, 260);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapCuenta(reader);
        }

        public bool ExistsByEmail(string email)
        {
            using var connection = OpenConnection();
            const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Cuenta WHERE Email = @Email) THEN 1 ELSE 0 END";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Email", email, SqlDbType.NVarChar, 260);

            var result = command.ExecuteScalar();
            return SafeToInt32(result) == 1;
        }

        public int CreateCuenta(string email, string passwordHash, byte estadoCuenta)
        {
            using var connection = OpenConnection();
            return CreateCuenta(connection, null, email, passwordHash, estadoCuenta);
        }

        public int CreateAlumno(Alumno alumno)
        {
            using var connection = OpenConnection();
            return CreateAlumno(connection, null, alumno);
        }

        public bool UpdatePassword(int idCuenta, string newPasswordHash)
        {
            using var connection = OpenConnection();
            const string sql = "UPDATE dbo.Cuenta SET Hash_Contrasena = @Hash WHERE ID_Cuenta = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Hash", newPasswordHash, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Id", idCuenta, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public bool DeleteCuenta(int idCuenta)
        {
            using var connection = OpenConnection();
            const string sql = "DELETE FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Id", idCuenta, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public int CreateCuenta(SqlConnection connection, SqlTransaction tx, string email, string passwordHash, byte estadoCuenta)
        {
            const string sql = @"INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro)
OUTPUT INSERTED.ID_Cuenta
VALUES (@Email, @Hash, @Estado, @Fecha);";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Email", email, SqlDbType.NVarChar, 260);
            AddParameter(command, "@Hash", passwordHash, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Estado", estadoCuenta, SqlDbType.TinyInt);
            AddParameter(command, "@Fecha", DateTime.UtcNow, SqlDbType.DateTime2);

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        public int CreateAlumno(SqlConnection connection, SqlTransaction tx, Alumno alumno)
        {
            const string sql = @"INSERT INTO dbo.Alumno (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Correo, Carrera)
VALUES (@Matricula, @Cuenta, @Nombre, @Apaterno, @Amaterno, @Nacimiento, @Genero, @Correo, @Carrera);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
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

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        private static Cuenta MapCuenta(SqlDataReader reader)
        {
            return new Cuenta
            {
                IdCuenta = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                Email = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                HashContrasena = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                EstadoCuenta = reader.IsDBNull(3) ? (byte)0 : reader.GetByte(3),
                FechaRegistro = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4)
            };
        }
    }
}
