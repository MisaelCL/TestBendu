using System;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    public sealed class PerfilRepository : RepositoryBase, IPerfilRepository
    {
        public PerfilRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Perfil? GetById(int idPerfil)
        {
            return WithConnection(connection =>
            {
                const string sql = "SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion FROM dbo.Perfil WHERE ID_Perfil = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idPerfil, SqlDbType.Int);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return MapPerfil(reader);
            });
        }

        public Perfil? GetByCuentaId(int idCuenta)
        {
            return WithConnection(connection =>
            {
                const string sql = "SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion FROM dbo.Perfil WHERE ID_Cuenta = @Cuenta";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Cuenta", idCuenta, SqlDbType.Int);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return MapPerfil(reader);
            });
        }

        public Preferencias? GetPreferenciasByPerfil(int idPerfil)
        {
            return WithConnection(connection =>
            {
                const string sql = "SELECT ID_Preferencias, ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses FROM dbo.Preferencias WHERE ID_Perfil = @Perfil";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Perfil", idPerfil, SqlDbType.Int);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return MapPreferencias(reader);
            });
        }

        public int CreatePerfil(Perfil perfil)
        {
            return WithConnection(connection => CreatePerfil(connection, null, perfil));
        }

        public bool UpdatePerfil(Perfil perfil)
        {
            return WithConnection(connection =>
            {
                const string sql = @"UPDATE dbo.Perfil
SET Nikname = @Nikname,
    Biografia = @Biografia,
    Foto_Perfil = @Foto,
    Fecha_Creacion = @Fecha
WHERE ID_Perfil = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
                AddParameter(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
                AddParameter(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);
                AddParameter(command, "@Fecha", perfil.FechaCreacion, SqlDbType.DateTime2);
                AddParameter(command, "@Id", perfil.IdPerfil, SqlDbType.Int);

                var rows = command.ExecuteNonQuery();
                return rows > 0;
            });
        }

        public int UpsertPreferencias(Preferencias prefs)
        {
            return WithConnection(connection => UpsertPreferencias(connection, null, prefs));
        }

        public bool DeletePerfil(int idPerfil)
        {
            return WithConnection(connection =>
            {
                const string sql = "DELETE FROM dbo.Perfil WHERE ID_Perfil = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idPerfil, SqlDbType.Int);

                var rows = command.ExecuteNonQuery();
                return rows > 0;
            });
        }

        public int CreatePerfil(SqlConnection connection, SqlTransaction? tx, Perfil perfil)
        {
            const string sql = @"INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion)
OUTPUT INSERTED.ID_Perfil
VALUES (@Cuenta, @Nikname, @Biografia, @Foto, @Fecha);";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Cuenta", perfil.IdCuenta, SqlDbType.Int);
            AddParameter(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);
            AddParameter(command, "@Fecha", perfil.FechaCreacion, SqlDbType.DateTime2);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public int UpsertPreferencias(SqlConnection connection, SqlTransaction? tx, Preferencias prefs)
        {
            const string sql = @"MERGE dbo.Preferencias AS Target
USING (SELECT @Perfil AS ID_Perfil) AS Source
ON Target.ID_Perfil = Source.ID_Perfil
WHEN MATCHED THEN
    UPDATE SET Preferencia_Genero = @Genero,
               Edad_Minima = @MinEdad,
               Edad_Maxima = @MaxEdad,
               Preferencia_Carrera = @Carrera,
               Intereses = @Intereses
WHEN NOT MATCHED THEN
    INSERT (ID_Perfil, Preferencia_Genero, Edad_Minima, Edad_Maxima, Preferencia_Carrera, Intereses)
    VALUES (@Perfil, @Genero, @MinEdad, @MaxEdad, @Carrera, @Intereses)
OUTPUT inserted.ID_Preferencias;";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Perfil", prefs.IdPerfil, SqlDbType.Int);
            AddParameter(command, "@Genero", prefs.PreferenciaGenero, SqlDbType.TinyInt);
            AddParameter(command, "@MinEdad", prefs.EdadMinima, SqlDbType.Int);
            AddParameter(command, "@MaxEdad", prefs.EdadMaxima, SqlDbType.Int);
            AddParameter(command, "@Carrera", prefs.PreferenciaCarrera ?? string.Empty, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Intereses", prefs.Intereses ?? string.Empty, SqlDbType.NVarChar, -1);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        private static Perfil MapPerfil(SqlDataReader reader)
        {
            return new Perfil
            {
                IdPerfil = reader.GetInt32(0),
                IdCuenta = reader.GetInt32(1),
                Nikname = reader.GetString(2),
                Biografia = reader.GetString(3),
                FotoPerfil = reader.IsDBNull(4) ? null : (byte[])reader[4],
                FechaCreacion = reader.GetDateTime(5)
            };
        }

        private static Preferencias MapPreferencias(SqlDataReader reader)
        {
            return new Preferencias
            {
                IdPreferencias = reader.GetInt32(0),
                IdPerfil = reader.GetInt32(1),
                PreferenciaGenero = reader.GetByte(2),
                EdadMinima = reader.GetInt32(3),
                EdadMaxima = reader.GetInt32(4),
                PreferenciaCarrera = reader.GetString(5),
                Intereses = reader.GetString(6)
            };
        }
    }
}
