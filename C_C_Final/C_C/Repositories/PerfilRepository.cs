using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    /// <summary>
    /// Gestiona las operaciones de datos relacionadas con los perfiles e intereses de los alumnos.
    /// </summary>
    public sealed class PerfilRepository : RepositoryBase, IPerfilRepository
    {
        public PerfilRepository(string connectionString = null) : base(connectionString)
        {
        }

        /// <inheritdoc />
        public Perfil ObtenerPorId(int idPerfil)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil FROM dbo.Perfil WHERE ID_Perfil = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idPerfil, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearPerfil(reader);
        }

        /// <inheritdoc />
        public Perfil ObtenerPorCuentaId(int idCuenta)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil FROM dbo.Perfil WHERE ID_Cuenta = @Cuenta";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Cuenta", idCuenta, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearPerfil(reader);
        }

        /// <inheritdoc />
        public Preferencias ObtenerPreferenciasPorPerfil(int idPerfil)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Preferencias, ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses
FROM dbo.Preferencias
WHERE ID_Perfil = @Perfil";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Perfil", idPerfil, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearPreferencias(reader);
        }

        /// <inheritdoc />
        public IReadOnlyList<Perfil> ListarTodos()
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil FROM dbo.Perfil ORDER BY ID_Perfil DESC";
            using var command = CrearComando(connection, sql);

            var perfiles = new List<Perfil>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                perfiles.Add(MapearPerfil(reader));
            }

            return perfiles;
        }

        /// <inheritdoc />
        public Perfil ObtenerSiguientePerfilPara(int idPerfilActual)
        {
            using var connection = AbrirConexion();
            const string sql = @"
SELECT TOP 1 p.ID_Perfil, p.ID_Cuenta, p.Nikname, p.Biografia, p.Foto_Perfil
FROM dbo.Perfil AS p
WHERE p.ID_Perfil <> @PerfilActual
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.Match AS m
        WHERE (m.Perfil_Emisor = @PerfilActual AND m.Perfil_Receptor = p.ID_Perfil)
           OR (m.Perfil_Receptor = @PerfilActual AND m.Perfil_Emisor = p.ID_Perfil)
    )
ORDER BY NEWID();";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@PerfilActual", idPerfilActual, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearPerfil(reader);
        }

        /// <inheritdoc />
        public int CrearPerfil(Perfil perfil)
        {
            using var connection = AbrirConexion();
            return CrearPerfil(connection, null, perfil);
        }

        /// <inheritdoc />
        public bool ActualizarPerfil(Perfil perfil)
        {
            using var connection = AbrirConexion();
            const string sql = @"UPDATE dbo.Perfil SET Nikname = @Nikname, Biografia = @Biografia, Foto_Perfil = @Foto WHERE ID_Perfil = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);
            AgregarParametro(command, "@Id", perfil.IdPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public int InsertarOActualizarPreferencias(Preferencias prefs)
        {
            using var connection = AbrirConexion();
            return InsertarOActualizarPreferencias(connection, null, prefs);
        }

        /// <inheritdoc />
        public bool EliminarPerfil(int idPerfil)
        {
            using var connection = AbrirConexion();
            const string sql = "DELETE FROM dbo.Perfil WHERE ID_Perfil = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public int CrearPerfil(SqlConnection connection, SqlTransaction tx, Perfil perfil)
        {
            const string sql = @"INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil) OUTPUT INSERTED.ID_Perfil VALUES (@Cuenta, @Nikname, @Biografia, @Foto);";
            using var command = CrearComando(connection, sql, CommandType.Text, tx);
            AgregarParametro(command, "@Cuenta", perfil.IdCuenta, SqlDbType.Int);
            AgregarParametro(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
            AgregarParametro(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        /// <inheritdoc />
        public int InsertarOActualizarPreferencias(SqlConnection connection, SqlTransaction tx, Preferencias prefs)
        {
            const string sql = @"MERGE dbo.Preferencias WITH (HOLDLOCK) AS Target USING (VALUES (@Perfil)) AS Source(ID_Perfil)
ON Target.ID_Perfil = Source.ID_Perfil WHEN MATCHED THEN UPDATE SET Preferencia_Genero = @Genero, Edad_Minima = @MinEdad, Edad_Maxima = @MaxEdad, Preferencia_Carrera = @Carrera, Intereses = @Intereses WHEN NOT MATCHED BY TARGET THEN INSERT (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses) VALUES (Source.ID_Perfil, @Genero, @MinEdad, @MaxEdad, @Carrera, @Intereses) OUTPUT inserted.ID_Preferencias;";
            using var command = CrearComando(connection, sql, CommandType.Text, tx);
            AgregarParametro(command, "@Perfil", prefs.IdPerfil, SqlDbType.Int);
            AgregarParametro(command, "@Genero", prefs.PreferenciaGenero, SqlDbType.TinyInt);
            AgregarParametro(command, "@MinEdad", prefs.EdadMinima, SqlDbType.Int);
            AgregarParametro(command, "@MaxEdad", prefs.EdadMaxima, SqlDbType.Int);
            AgregarParametro(command, "@Carrera", prefs.PreferenciaCarrera ?? string.Empty, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Intereses", prefs.Intereses ?? string.Empty, SqlDbType.NVarChar, -1);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        /// <summary>
        /// Convierte un registro de datos en un objeto de perfil.
        /// </summary>
        /// <param name="reader">Lector con los datos del perfil.</param>
        /// <returns>Perfil construido.</returns>
        private static Perfil MapearPerfil(SqlDataReader reader)
        {
            var idPerfilIndex = reader.GetOrdinal("ID_Perfil");
            var idCuentaIndex = reader.GetOrdinal("ID_Cuenta");
            var niknameIndex = reader.GetOrdinal("Nikname");
            var biografiaIndex = reader.GetOrdinal("Biografia");
            var fotoPerfilIndex = reader.GetOrdinal("Foto_Perfil");

            return new Perfil
            {
                IdPerfil = reader.IsDBNull(idPerfilIndex) ? 0 : reader.GetInt32(idPerfilIndex),
                IdCuenta = reader.IsDBNull(idCuentaIndex) ? 0 : reader.GetInt32(idCuentaIndex),
                Nikname = reader.IsDBNull(niknameIndex) ? string.Empty : reader.GetString(niknameIndex),
                Biografia = reader.IsDBNull(biografiaIndex) ? string.Empty : reader.GetString(biografiaIndex),
                FotoPerfil = reader.IsDBNull(fotoPerfilIndex) ? null : (byte[])reader[fotoPerfilIndex],
            };
        }

        /// <summary>
        /// Convierte un registro de datos en un objeto de preferencias.
        /// </summary>
        /// <param name="reader">Lector con los datos de preferencias.</param>
        /// <returns>Preferencias construidas.</returns>
        private static Preferencias MapearPreferencias(SqlDataReader reader)
        {
            return new Preferencias
            {
                IdPreferencias = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                IdPerfil = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                PreferenciaGenero = reader.IsDBNull(2) ? (byte)0 : reader.GetByte(2),
                EdadMinima = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                EdadMaxima = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                PreferenciaCarrera = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Intereses = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            };
        }
    }
}
