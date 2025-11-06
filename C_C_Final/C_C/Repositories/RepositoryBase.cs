using System;
using System.Data;
using System.Data.SqlClient;

namespace C_C_Final.Repositories
{
    /// <summary>
    /// Proporciona utilidades comunes para los repositorios que interactúan con la base de datos SQL Server.
    /// </summary>
    public abstract class RepositoryBase
    {
        protected const int DefaultCommandTimeout = 30;

        private static readonly string _connectionString =
            @"Data Source=LABENDUPC\BENDUOLOSERVER;Initial Catalog=C_CBD;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        protected RepositoryBase()
        {
        }

        /// <summary>
        /// Crea un comando SQL configurado con los parámetros indicados.
        /// </summary>
        protected SqlCommand CrearComando(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, SqlTransaction transaction = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = commandType;
            command.CommandTimeout = DefaultCommandTimeout;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            return command;
        }

        /// <summary>
        /// Agrega un parámetro a un comando SQL con las características especificadas.
        /// </summary>
        protected static void AgregarParametro(SqlCommand command, string name, object value, SqlDbType? type = null, int? size = null)
        {
            var parameter = command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            if (type.HasValue)
            {
                parameter.SqlDbType = type.Value;
            }
            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }
        }

        /// <summary>
        /// Convierte de forma segura un resultado a <see cref="int"/> evitando excepciones por valores nulos.
        /// </summary>
        protected static int ConvertirSeguroAInt32(object result)
        {
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// Convierte de forma segura un resultado a <see cref="long"/> evitando excepciones por valores nulos.
        /// </summary>
        protected static long ConvertirSeguroAInt64(object result)
        {
            return result == null || result == DBNull.Value ? 0L : Convert.ToInt64(result);
        }

        /// <summary>
        /// Abre y devuelve una conexión SQL utilizando la cadena configurada.
        /// </summary>
        /// <returns>Instancia abierta de <see cref="SqlConnection"/>.</returns>
        protected SqlConnection AbrirConexion()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

    }
}