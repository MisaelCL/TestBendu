using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    public sealed class PerfilRepository : RepositoryBase, IPerfilRepository
    {
        public PerfilRepository(string connectionString = null) : base(connectionString)
        {
        }

        public Perfil GetById(int idPerfil)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, FROM dbo.Perfil WHERE ID_Perfil = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Id", idPerfil, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapPerfil(reader);
        }

        public Perfil GetByCuentaId(int idCuenta)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil FROM dbo.Perfil WHERE ID_Cuenta = @Cuenta";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Cuenta", idCuenta, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapPerfil(reader);
        }

        public Preferencias GetPreferenciasByPerfil(int idPerfil)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Preferencias, ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses
FROM dbo.Preferencias
WHERE ID_Perfil = @Perfil";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Perfil", idPerfil, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapPreferencias(reader);
        }

        public IReadOnlyList<Perfil> ListAll()
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, FROM dbo.Perfil ORDER BY ID_Perfil DESC";
            using var command = CreateCommand(connection, sql);

            var perfiles = new List<Perfil>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                perfiles.Add(MapPerfil(reader));
            }

            return perfiles;
        }

        public int CreatePerfil(Perfil perfil)
        {
            using var connection = OpenConnection();
            return CreatePerfil(connection, null, perfil);
        }

        public bool UpdatePerfil(Perfil perfil)
        {
            using var connection = OpenConnection();
            const string sql = @"UPDATE dbo.Perfil SET Nikname = @Nikname, Biografia = @Biografia, Foto_Perfil = @Foto, WHERE ID_Perfil = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);
            AddParameter(command, "@Id", perfil.IdPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public int UpsertPreferencias(Preferencias prefs)
        {
            using var connection = OpenConnection();
            return UpsertPreferencias(connection, null, prefs);
        }

        public bool DeletePerfil(int idPerfil)
        {
            using var connection = OpenConnection();
            const string sql = "DELETE FROM dbo.Perfil WHERE ID_Perfil = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Id", idPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public int CreatePerfil(SqlConnection connection, SqlTransaction tx, Perfil perfil)
        {
            const string sql = @"INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil) OUTPUT INSERTED.ID_Perfil VALUES (@Cuenta, @Nikname, @Biografia, @Foto);";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Cuenta", perfil.IdCuenta, SqlDbType.Int);
            AddParameter(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        public int UpsertPreferencias(SqlConnection connection, SqlTransaction tx, Preferencias prefs)
        {
            const string sql = @"MERGE dbo.Preferencias WITH (HOLDLOCK) AS Target USING (VALUES (@Perfil)) AS Source(ID_Perfil) ON Target.ID_Perfil = Source.ID_Perfil WHEN MATCHED THEN UPDATE SET Preferencia_Genero = @Genero, Edad_Minima = @MinEdad, Edad_Maxima = @MaxEdad, Preferencia_Carrera = @Carrera, Intereses = @Intereses WHEN NOT MATCHED BY TARGET THEN INSERT (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses) VALUES (Source.ID_Perfil, @Genero, @MinEdad, @MaxEdad, @Carrera, @Intereses) OUTPUT inserted.ID_Preferencias;"
;
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Perfil", prefs.IdPerfil, SqlDbType.Int);
            AddParameter(command, "@Genero", prefs.PreferenciaGenero, SqlDbType.TinyInt);
            AddParameter(command, "@MinEdad", prefs.EdadMinima, SqlDbType.Int);
            AddParameter(command, "@MaxEdad", prefs.EdadMaxima, SqlDbType.Int);
            AddParameter(command, "@Carrera", prefs.PreferenciaCarrera ?? string.Empty, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Intereses", prefs.Intereses ?? string.Empty, SqlDbType.NVarChar, -1);

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        private static Perfil MapPerfil(SqlDataReader reader)
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

        private static Preferencias MapPreferencias(SqlDataReader reader)
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
