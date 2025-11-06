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

        public Perfil ObtenerPorId(int idPerfil)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT p.ID_Perfil, p.ID_Cuenta, p.Nikname, p.Biografia, p.Foto_Perfil AS FotoPerfil,
c.Fecha_Registro AS FechaCreacion
FROM dbo.Perfil p
INNER JOIN dbo.Cuenta c ON c.ID_Cuenta = p.ID_Cuenta
WHERE p.ID_Perfil = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idPerfil, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearPerfil(reader) : null;
        }

        public Perfil ObtenerPorCuentaId(int idCuenta)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT p.ID_Perfil, p.ID_Cuenta, p.Nikname, p.Biografia, p.Foto_Perfil AS FotoPerfil,
c.Fecha_Registro AS FechaCreacion
FROM dbo.Perfil p
INNER JOIN dbo.Cuenta c ON c.ID_Cuenta = p.ID_Cuenta
WHERE p.ID_Cuenta = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idCuenta, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearPerfil(reader) : null;
        }

        public IReadOnlyList<Perfil> ObtenerPorIds(IEnumerable<int> idsPerfiles)
        {
            var list = new List<Perfil>();
            if (idsPerfiles == null) return list;

            var idList = new List<int>(idsPerfiles);
            if (idList.Count == 0) return list;

            using var connection = AbrirConexion();
            var sql = $@"SELECT p.ID_Perfil, p.ID_Cuenta, p.Nikname, p.Biografia, p.Foto_Perfil AS FotoPerfil,
c.Fecha_Registro AS FechaCreacion
FROM dbo.Perfil p
INNER JOIN dbo.Cuenta c ON c.ID_Cuenta = p.ID_Cuenta
WHERE p.ID_Perfil IN ({string.Join(",", idList)})";
            using var command = CrearComando(connection, sql);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapearPerfil(reader));
            }
            return list;
        }

        public int CrearPerfil(Perfil perfil)
        {
            using var connection = AbrirConexion();
            return CrearPerfil(connection, null, perfil);
        }

        public int CrearPerfil(SqlConnection connection, SqlTransaction transaction, Perfil perfil)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (perfil is null)
            {
                throw new ArgumentNullException(nameof(perfil));
            }

            const string sql = @"INSERT INTO dbo.Perfil (ID_Cuenta, Nikname, Biografia, Foto_Perfil)
OUTPUT INSERTED.ID_Perfil
VALUES (@Cuenta, @Nik, @Bio, @Foto);";
            using var command = CrearComando(connection, sql, CommandType.Text, transaction);
            AgregarParametro(command, "@Cuenta", perfil.IdCuenta, SqlDbType.Int);
            AgregarParametro(command, "@Nik", perfil.Nikname, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Bio", perfil.Biografia, SqlDbType.NVarChar, 500);
            AgregarParametro(command, "@Foto", perfil.FotoPerfil, SqlDbType.Image);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        public bool ActualizarPerfil(Perfil perfil)
        {
            using var connection = AbrirConexion();
            const string sql = @"UPDATE dbo.Perfil SET
Nikname = @Nik,
Biografia = @Bio,
Foto_Perfil = @Foto
WHERE ID_Perfil = @Id AND ID_Cuenta = @CuentaId";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Nik", perfil.Nikname, SqlDbType.NVarChar, 50);
            AgregarParametro(command, "@Bio", perfil.Biografia, SqlDbType.NVarChar, 500);
            AgregarParametro(command, "@Foto", perfil.FotoPerfil, SqlDbType.Image);
            AgregarParametro(command, "@Id", perfil.IdPerfil, SqlDbType.Int);
            AgregarParametro(command, "@CuentaId", perfil.IdCuenta, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public bool EliminarPerfil(int idPerfil)
        {
            using var connection = AbrirConexion();
            const string sql = "DELETE FROM dbo.Perfil WHERE ID_Perfil = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idPerfil, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        // --- IMPLEMENTACIÓN DEL MÉTODO AÑADIDO ---
        public Perfil ObtenerSiguientePerfilPara(int idPerfilActual)
        {
            using var connection = AbrirConexion();
            // Esta consulta busca un perfil aleatorio
            // 1. Que no sea el usuario actual.
            // 2. Que no exista ya en la tabla Match (en ninguna dirección).
            // 3. Ordena aleatoriamente (NEWID()) y toma el primero.
            const string sql = @"
SELECT TOP 1 p.ID_Perfil, p.ID_Cuenta, p.Nikname, p.Biografia, p.Foto_Perfil AS FotoPerfil,
       c.Fecha_Registro AS FechaCreacion
FROM dbo.Perfil p
INNER JOIN dbo.Cuenta c ON c.ID_Cuenta = p.ID_Cuenta
WHERE p.ID_Perfil != @IdPerfilActual
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Match m
      WHERE (m.Perfil_Emisor = @IdPerfilActual AND m.Perfil_Receptor = p.ID_Perfil)
         OR (m.Perfil_Emisor = p.ID_Perfil AND m.Perfil_Receptor = @IdPerfilActual)
  )
ORDER BY NEWID();";
            
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@IdPerfilActual", idPerfilActual, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearPerfil(reader) : null;
        }


        private static Perfil MapearPerfil(SqlDataReader reader)
        {
            var idPerfilIndex = reader.GetOrdinal("ID_Perfil");
            var idCuentaIndex = reader.GetOrdinal("ID_Cuenta");
            var niknameIndex = reader.GetOrdinal("Nikname");
            var biografiaIndex = reader.GetOrdinal("Biografia");
            var fotoIndex = reader.GetOrdinal("FotoPerfil");
            var fechaIndex = reader.GetOrdinal("FechaCreacion");

            return new Perfil
            {
                IdPerfil = reader.IsDBNull(idPerfilIndex) ? 0 : reader.GetInt32(idPerfilIndex),
                IdCuenta = reader.IsDBNull(idCuentaIndex) ? 0 : reader.GetInt32(idCuentaIndex),
                Nikname = reader.IsDBNull(niknameIndex) ? string.Empty : reader.GetString(niknameIndex),
                Biografia = reader.IsDBNull(biografiaIndex)
                    ? string.Empty
                    : Convert.ToString(reader.GetValue(biografiaIndex)) ?? string.Empty,
                FotoPerfil = reader.IsDBNull(fotoIndex) ? Array.Empty<byte>() : (byte[])reader.GetValue(fotoIndex),
                FechaCreacion = reader.IsDBNull(fechaIndex) ? DateTime.MinValue : reader.GetDateTime(fechaIndex)
            };
        }
    }
}
