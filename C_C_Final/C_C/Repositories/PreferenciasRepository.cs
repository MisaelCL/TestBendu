using System;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    /// <summary>
    /// Implementa las operaciones de persistencia relacionadas con las preferencias de un perfil.
    /// </summary>
    public sealed class PreferenciasRepository : RepositoryBase, IPreferenciasRepository
    {
        public PreferenciasRepository(string connectionString = null) : base(connectionString)
        {
        }

        /// <inheritdoc />
        public Preferencias ObtenerPorPerfilId(int idPerfil)
        {
            using var connection = AbrirConexion();
            return ObtenerPorPerfilId(connection, null, idPerfil);
        }

        /// <inheritdoc />
        public Preferencias ObtenerPorCuentaId(int idCuenta)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT pref.ID_Preferencias, pref.ID_Perfil, pref.Preferencia_Genero, pref.Edad_Minima,
       pref.Edad_Maxima, pref.Preferencia_Carrera, pref.Intereses
FROM dbo.Preferencias pref
INNER JOIN dbo.Perfil p ON p.ID_Perfil = pref.ID_Perfil
WHERE p.ID_Cuenta = @Cuenta";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Cuenta", idCuenta, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        /// <inheritdoc />
        public int CrearPreferencias(Preferencias preferencias)
        {
            using var connection = AbrirConexion();
            return CrearPreferencias(connection, null, preferencias);
        }

        /// <inheritdoc />
        public int CrearPreferencias(SqlConnection connection, SqlTransaction transaction, Preferencias preferencias)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (preferencias is null)
            {
                throw new ArgumentNullException(nameof(preferencias));
            }

            const string sql = @"INSERT INTO dbo.Preferencias (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses)
OUTPUT INSERTED.ID_Preferencias
VALUES (@Perfil, @Genero, @EdadMin, @EdadMax, @Carrera, @Intereses);";

            using var command = CrearComando(connection, sql, CommandType.Text, transaction);
            AgregarParametro(command, "@Perfil", preferencias.IdPerfil, SqlDbType.Int);
            AgregarParametro(command, "@Genero", preferencias.PreferenciaGenero, SqlDbType.TinyInt);
            AgregarParametro(command, "@EdadMin", preferencias.EdadMinima, SqlDbType.Int);
            AgregarParametro(command, "@EdadMax", preferencias.EdadMaxima, SqlDbType.Int);
            AgregarParametro(command, "@Carrera", preferencias.PreferenciaCarrera ?? string.Empty, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Intereses", preferencias.Intereses ?? string.Empty, SqlDbType.NVarChar, -1);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        /// <inheritdoc />
        public bool ActualizarPreferencias(Preferencias preferencias)
        {
            using var connection = AbrirConexion();
            return ActualizarPreferencias(connection, null, preferencias);
        }

        /// <inheritdoc />
        public bool ActualizarPreferencias(SqlConnection connection, SqlTransaction transaction, Preferencias preferencias)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (preferencias is null)
            {
                throw new ArgumentNullException(nameof(preferencias));
            }

            const string sql = @"UPDATE dbo.Preferencias SET
    Preferencia_Genero = @Genero,
    Edad_Minima = @EdadMin,
    Edad_Maxima = @EdadMax,
    Preferencia_Carrera = @Carrera,
    Intereses = @Intereses
WHERE ID_Perfil = @Perfil";

            using var command = CrearComando(connection, sql, CommandType.Text, transaction);
            AgregarParametro(command, "@Genero", preferencias.PreferenciaGenero, SqlDbType.TinyInt);
            AgregarParametro(command, "@EdadMin", preferencias.EdadMinima, SqlDbType.Int);
            AgregarParametro(command, "@EdadMax", preferencias.EdadMaxima, SqlDbType.Int);
            AgregarParametro(command, "@Carrera", preferencias.PreferenciaCarrera ?? string.Empty, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Intereses", preferencias.Intereses ?? string.Empty, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Perfil", preferencias.IdPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public bool EliminarPorPerfil(int idPerfil)
        {
            using var connection = AbrirConexion();
            const string sql = "DELETE FROM dbo.Preferencias WHERE ID_Perfil = @Perfil";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Perfil", idPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        private Preferencias ObtenerPorPerfilId(SqlConnection connection, SqlTransaction transaction, int idPerfil)
        {
            const string sql = @"SELECT ID_Preferencias, ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses
FROM dbo.Preferencias
WHERE ID_Perfil = @Perfil";
            using var command = CrearComando(connection, sql, CommandType.Text, transaction);
            AgregarParametro(command, "@Perfil", idPerfil, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        private static Preferencias Mapear(SqlDataReader reader)
        {
            var idPreferenciasOrdinal = reader.GetOrdinal("ID_Preferencias");
            var idPerfilOrdinal = reader.GetOrdinal("ID_Perfil");
            var generoOrdinal = reader.GetOrdinal("Preferencia_Genero");
            var edadMinOrdinal = reader.GetOrdinal("Edad_Minima");
            var edadMaxOrdinal = reader.GetOrdinal("Edad_Maxima");
            var carreraOrdinal = reader.GetOrdinal("Preferencia_Carrera");
            var interesesOrdinal = reader.GetOrdinal("Intereses");

            return new Preferencias
            {
                IdPreferencias = reader.IsDBNull(idPreferenciasOrdinal) ? 0 : reader.GetInt32(idPreferenciasOrdinal),
                IdPerfil = reader.IsDBNull(idPerfilOrdinal) ? 0 : reader.GetInt32(idPerfilOrdinal),
                PreferenciaGenero = reader.IsDBNull(generoOrdinal) ? (byte)0 : reader.GetByte(generoOrdinal),
                EdadMinima = reader.IsDBNull(edadMinOrdinal) ? 0 : reader.GetInt32(edadMinOrdinal),
                EdadMaxima = reader.IsDBNull(edadMaxOrdinal) ? 0 : reader.GetInt32(edadMaxOrdinal),
                PreferenciaCarrera = reader.IsDBNull(carreraOrdinal) ? string.Empty : reader.GetString(carreraOrdinal),
                Intereses = reader.IsDBNull(interesesOrdinal) ? string.Empty : reader.GetString(interesesOrdinal)
            };
        }
    }
}
