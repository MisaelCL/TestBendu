using System;
using System.Configuration;
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

        private readonly string _connectionString;
        private static string _cachedConnectionString;

        /// <summary>
        ///     Inicializa el repositorio resolviendo y normalizando la cadena de conexión. Permite opcionalmente
        ///     recibir una cadena externa (por ejemplo, durante pruebas unitarias) o reutilizar la configurada
        ///     en <c>App.config</c>.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión alternativa; si es nula se toma la configurada.</param>
        protected RepositoryBase(string connectionString = null)
        {
            _connectionString = ResolverCadenaConexion(connectionString);
        }

        /// <summary>
        /// Obtiene una cadena de conexión normalizada a partir de la configuración o del parámetro proporcionado.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión opcional proporcionada externamente.</param>
        /// <returns>Cadena de conexión normalizada lista para ser utilizada.</returns>
        internal static string ResolverCadenaConexion(string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return NormalizarCadenaConexion(connectionString, "proporcionada");
            }

            if (!string.IsNullOrEmpty(_cachedConnectionString))
            {
                return _cachedConnectionString;
            }

            var configured = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            _cachedConnectionString = NormalizarCadenaConexion(configured, "DefaultConnection");
            return _cachedConnectionString;
        }

        /// <summary>
        /// Crea un comando SQL configurado con los parámetros indicados.
        /// </summary>
        /// <param name="connection">Conexión SQL sobre la que se ejecutará el comando.</param>
        /// <param name="sql">Consulta o procedimiento a ejecutar.</param>
        /// <param name="commandType">Tipo de comando.</param>
        /// <param name="transaction">Transacción opcional asociada.</param>
        /// <returns>Instancia de <see cref="SqlCommand"/> lista para su ejecución.</returns>
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
        /// <param name="command">Comando al que se añadirá el parámetro.</param>
        /// <param name="name">Nombre del parámetro.</param>
        /// <param name="value">Valor del parámetro.</param>
        /// <param name="type">Tipo de dato opcional.</param>
        /// <param name="size">Tamaño opcional.</param>
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
        /// <param name="result">Valor a convertir.</param>
        /// <returns>Entero obtenido o cero si es nulo.</returns>
        protected static int ConvertirSeguroAInt32(object result)
        {
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// Convierte de forma segura un resultado a <see cref="long"/> evitando excepciones por valores nulos.
        /// </summary>
        /// <param name="result">Valor a convertir.</param>
        /// <returns>Entero largo obtenido o cero si es nulo.</returns>
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

        /// <summary>
        /// Valida y normaliza una cadena de conexión, garantizando que cuente con los datos mínimos requeridos.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión a validar.</param>
        /// <param name="sourceName">Nombre descriptivo de la fuente de la cadena.</param>
        /// <returns>Cadena de conexión validada.</returns>
        private static string NormalizarCadenaConexion(string connectionString, string sourceName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"No se encontró la cadena de conexión '{sourceName}'.");
            }

            SqlConnectionStringBuilder builder;
            try
            {
                builder = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"La cadena de conexión '{sourceName}' no es válida.", ex);
            }

            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                throw new InvalidOperationException($"La cadena de conexión '{sourceName}' debe especificar la base de datos mediante 'Initial Catalog'.");
            }

            return builder.ConnectionString;
        }
    }
}
