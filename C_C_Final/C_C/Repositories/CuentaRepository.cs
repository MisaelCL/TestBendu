using System;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    /// <summary>
    /// Implementa las operaciones de persistencia relacionadas con cuentas y alumnos.
    /// </summary>
    public sealed class CuentaRepository : RepositoryBase, ICuentaRepository
    {
        public CuentaRepository(string connectionString = null) : base(connectionString)
        {
        }

        /// <inheritdoc />
        public Cuenta ObtenerPorId(int idCuenta)
        {
            using var connection = AbrirConexion();
            const string sql = "SELECT ID_Cuenta, Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta, Fecha_Registro FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idCuenta, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearCuenta(reader);
        }

        /// <inheritdoc />
        public Cuenta ObtenerPorCorreo(string email)
        {
            using var connection = AbrirConexion();
            const string sql = "SELECT ID_Cuenta, Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta, Fecha_Registro FROM dbo.Cuenta WHERE Email = @Email";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Email", email, SqlDbType.NVarChar, 260);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearCuenta(reader);
        }

        /// <inheritdoc />
        public bool ExistePorCorreo(string email)
        {
            using var connection = AbrirConexion();
            const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Cuenta WHERE Email = @Email) THEN 1 ELSE 0 END";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Email", email, SqlDbType.NVarChar, 260);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result) == 1;
        }

        /// <inheritdoc />
        public void CrearCuenta(string email, string passwordHash, string passwordSalt, byte estadoCuenta)
        {
            using var connection = AbrirConexion();
            CrearCuenta(connection, null, email, passwordHash, passwordSalt, estadoCuenta);
        }

        /// <inheritdoc />
        public void CrearAlumno(Alumno alumno)
        {
            using var connection = AbrirConexion();
            CrearAlumno(connection, null, alumno);
        }

        /// <inheritdoc />
        public bool ActualizarContrasena(int idCuenta, string newPasswordHash, string newPasswordSalt)
        {
            using var connection = AbrirConexion();
            const string sql = "UPDATE dbo.Cuenta SET Hash_Contrasena = @Hash, Salt_Contrasena = @Salt WHERE ID_Cuenta = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Hash", newPasswordHash, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Salt", newPasswordSalt, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Id", idCuenta, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public bool EliminarCuenta(int idCuenta)
        {
            using var connection = AbrirConexion();
            const string sql = "DELETE FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idCuenta, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public int CrearCuenta(SqlConnection connection, SqlTransaction tx, string email, string passwordHash, string passwordSalt, byte estadoCuenta)
        {
            const string sql = @"INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Salt_Contrasena, Estado_Cuenta, Fecha_Registro)
OUTPUT INSERTED.ID_Cuenta
VALUES (@Email, @Hash, @Salt, @Estado, @Fecha);";
            using var command = CrearComando(connection, sql, CommandType.Text, tx);
            AgregarParametro(command, "@Email", email, SqlDbType.NVarChar, 260);
            AgregarParametro(command, "@Hash", passwordHash, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Salt", passwordSalt, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Estado", estadoCuenta, SqlDbType.TinyInt);
            AgregarParametro(command, "@Fecha", DateTime.UtcNow, SqlDbType.DateTime2);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        /// <inheritdoc />
        public int CrearAlumno(SqlConnection connection, SqlTransaction tx, Alumno alumno)
        {
            const string sql = @"INSERT INTO dbo.Alumno (Matricula, ID_Cuenta, Nombre, Apaterno, Amaterno, F_Nac, Genero, Correo, Carrera)
VALUES (@Matricula, @Cuenta, @Nombre, @Apaterno, @Amaterno, @Nacimiento, @Genero, @Correo, @Carrera);";
            using var command = CrearComando(connection, sql, CommandType.Text, tx);
            AgregarParametro(command, "@Matricula", alumno.Matricula, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Cuenta", alumno.IdCuenta, SqlDbType.Int);
            AgregarParametro(command, "@Nombre", alumno.Nombre, SqlDbType.NVarChar, 100);
            AgregarParametro(command, "@Apaterno", alumno.ApellidoPaterno, SqlDbType.NVarChar, 100);
            AgregarParametro(command, "@Amaterno", alumno.ApellidoMaterno, SqlDbType.NVarChar, 100);
            AgregarParametro(command, "@Nacimiento", alumno.FechaNacimiento, SqlDbType.Date);
            AgregarParametro(command, "@Genero", alumno.Genero, SqlDbType.Char, 1);
            AgregarParametro(command, "@Correo", alumno.Correo, SqlDbType.NVarChar, 260);
            AgregarParametro(command, "@Carrera", alumno.Carrera, SqlDbType.NVarChar, 100);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected;
        }

        /// <summary>
        /// Construye una entidad de cuenta a partir de un lector de datos.
        /// </summary>
        /// <param name="reader">Lector posicionado en el registro deseado.</param>
        /// <returns>Instancia de <see cref="Cuenta"/>.</returns>
        private static Cuenta MapearCuenta(SqlDataReader reader)
        {
            return new Cuenta
            {
                IdCuenta = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                Email = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                HashContrasena = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                SaltContrasena = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                EstadoCuenta = reader.IsDBNull(4) ? (byte)0 : reader.GetByte(4),
                FechaRegistro = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5)
            };
        }
    }
}
