using System;
using System.Collections.Generic;
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

        public Perfil GetById(int idPerfil)
        {
            return WithConnection(connection =>
            {
                var fechaColumn = ResolvePerfilFechaColumn(connection, null);
                var selectColumns = BuildPerfilSelectColumns(fechaColumn);
                var sql = $"SELECT {selectColumns} FROM dbo.Perfil WHERE ID_Perfil = @Id";
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

        public Perfil GetByCuentaId(int idCuenta)
        {
            return WithConnection(connection =>
            {
                var fechaColumn = ResolvePerfilFechaColumn(connection, null);
                var selectColumns = BuildPerfilSelectColumns(fechaColumn);
                var sql = $"SELECT {selectColumns} FROM dbo.Perfil WHERE ID_Cuenta = @Cuenta";
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

        public Preferencias GetPreferenciasByPerfil(int idPerfil)
        {
            return WithConnection(connection =>
            {
                var hasCarreraColumn = ColumnExists(connection, null, "Preferencias", "Preferencia_Carrera");
                var hasInteresesColumn = ColumnExists(connection, null, "Preferencias", "Intereses");

                var selectColumns = new List<string>
                {
                    "ID_Preferencias",
                    "ID_Perfil",
                    "Preferencia_Genero",
                    "Edad_Minima",
                    "Edad_Maxima",
                    hasCarreraColumn
                        ? "Preferencia_Carrera"
                        : "CAST('' AS nvarchar(50)) AS Preferencia_Carrera",
                    hasInteresesColumn
                        ? "Intereses"
                        : "CAST('' AS nvarchar(max)) AS Intereses"
                };

                var sql = $"SELECT {string.Join(", ", selectColumns)} FROM dbo.Preferencias WHERE ID_Perfil = @Perfil";
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
                var fechaColumn = ResolvePerfilFechaColumn(connection, null);
                var assignments = new List<string>
                {
                    "Nikname = @Nikname",
                    "Biografia = @Biografia",
                    "Foto_Perfil = @Foto"
                };

                if (!string.IsNullOrEmpty(fechaColumn))
                {
                    assignments.Add($"{WrapColumn(fechaColumn)} = @Fecha");
                }

                var sql = $"UPDATE dbo.Perfil SET {string.Join(", ", assignments)} WHERE ID_Perfil = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
                AddParameter(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
                AddParameter(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);
                AddParameter(command, "@Id", perfil.IdPerfil, SqlDbType.Int);

                if (!string.IsNullOrEmpty(fechaColumn))
                {
                    AddParameter(command, "@Fecha", perfil.FechaCreacion, SqlDbType.DateTime2);
                }

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

        public int CreatePerfil(SqlConnection connection, SqlTransaction tx, Perfil perfil)
        {
            var fechaColumn = ResolvePerfilFechaColumn(connection, tx);
            var insertColumns = new List<string>
            {
                "ID_Cuenta",
                "Nikname",
                "Biografia",
                "Foto_Perfil"
            };

            var insertValues = new List<string>
            {
                "@Cuenta",
                "@Nikname",
                "@Biografia",
                "@Foto"
            };

            if (!string.IsNullOrEmpty(fechaColumn))
            {
                insertColumns.Add(WrapColumn(fechaColumn));
                insertValues.Add("@Fecha");
            }

            var sql = $@"INSERT INTO dbo.Perfil ({string.Join(", ", insertColumns)})
OUTPUT INSERTED.ID_Perfil
VALUES ({string.Join(", ", insertValues)});";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Cuenta", perfil.IdCuenta, SqlDbType.Int);
            AddParameter(command, "@Nikname", perfil.Nikname ?? string.Empty, SqlDbType.NVarChar, 50);
            AddParameter(command, "@Biografia", perfil.Biografia ?? string.Empty, SqlDbType.NVarChar, -1);
            AddParameter(command, "@Foto", perfil.FotoPerfil, SqlDbType.VarBinary);

            if (!string.IsNullOrEmpty(fechaColumn))
            {
                AddParameter(command, "@Fecha", perfil.FechaCreacion, SqlDbType.DateTime2);
            }

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        public int UpsertPreferencias(SqlConnection connection, SqlTransaction tx, Preferencias prefs)
        {
            var hasCarreraColumn = ColumnExists(connection, tx, "Preferencias", "Preferencia_Carrera");
            var hasInteresesColumn = ColumnExists(connection, tx, "Preferencias", "Intereses");

            var updateAssignments = new List<string>
            {
                "Preferencia_Genero = @Genero",
                "Edad_Minima = @MinEdad",
                "Edad_Maxima = @MaxEdad"
            };

            var insertColumns = new List<string>
            {
                "ID_Perfil",
                "Preferencia_Genero",
                "Edad_Minima",
                "Edad_Maxima"
            };

            var insertValues = new List<string>
            {
                "@Perfil",
                "@Genero",
                "@MinEdad",
                "@MaxEdad"
            };

            if (hasCarreraColumn)
            {
                updateAssignments.Add("Preferencia_Carrera = @Carrera");
                insertColumns.Add("Preferencia_Carrera");
                insertValues.Add("@Carrera");
            }

            if (hasInteresesColumn)
            {
                updateAssignments.Add("Intereses = @Intereses");
                insertColumns.Add("Intereses");
                insertValues.Add("@Intereses");
            }

            var sql = $@"MERGE dbo.Preferencias AS Target
USING (SELECT @Perfil AS ID_Perfil) AS Source
ON Target.ID_Perfil = Source.ID_Perfil
WHEN MATCHED THEN
    UPDATE SET {string.Join(", ", updateAssignments)}
WHEN NOT MATCHED THEN
    INSERT ({string.Join(", ", insertColumns)})
    VALUES ({string.Join(", ", insertValues)})
OUTPUT inserted.ID_Preferencias;";

            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Perfil", prefs.IdPerfil, SqlDbType.Int);
            AddParameter(command, "@Genero", prefs.PreferenciaGenero, SqlDbType.TinyInt);
            AddParameter(command, "@MinEdad", prefs.EdadMinima, SqlDbType.Int);
            AddParameter(command, "@MaxEdad", prefs.EdadMaxima, SqlDbType.Int);

            if (hasCarreraColumn)
            {
                AddParameter(command, "@Carrera", prefs.PreferenciaCarrera ?? string.Empty, SqlDbType.NVarChar, 50);
            }

            if (hasInteresesColumn)
            {
                AddParameter(command, "@Intereses", prefs.Intereses ?? string.Empty, SqlDbType.NVarChar, -1);
            }

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
            var fechaIndex = reader.GetOrdinal("FechaCreacion");

            return new Perfil
            {
                IdPerfil = reader.IsDBNull(idPerfilIndex) ? 0 : reader.GetInt32(idPerfilIndex),
                IdCuenta = reader.IsDBNull(idCuentaIndex) ? 0 : reader.GetInt32(idCuentaIndex),
                Nikname = reader.IsDBNull(niknameIndex) ? string.Empty : reader.GetString(niknameIndex),
                Biografia = reader.IsDBNull(biografiaIndex) ? string.Empty : reader.GetString(biografiaIndex),
                FotoPerfil = reader.IsDBNull(fotoPerfilIndex) ? null : (byte[])reader[fotoPerfilIndex],
                FechaCreacion = reader.IsDBNull(fechaIndex) ? DateTime.MinValue : reader.GetDateTime(fechaIndex)
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

        private string ResolvePerfilFechaColumn(SqlConnection connection, SqlTransaction tx)
        {
            if (ColumnExists(connection, tx, "Perfil", "Fecha_Creacion"))
            {
                return "Fecha_Creacion";
            }

            if (ColumnExists(connection, tx, "Perfil", "FechaCreacion"))
            {
                return "FechaCreacion";
            }

            return null;
        }

        private static string BuildPerfilSelectColumns(string fechaColumn)
        {
            var columns = new List<string>
            {
                "ID_Perfil",
                "ID_Cuenta",
                "Nikname",
                "Biografia",
                "Foto_Perfil"
            };

            columns.Add(!string.IsNullOrEmpty(fechaColumn)
                ? $"{WrapColumn(fechaColumn)} AS FechaCreacion"
                : "CAST('1900-01-01T00:00:00' AS datetime2(0)) AS FechaCreacion");

            return string.Join(", ", columns);
        }

    }
}
