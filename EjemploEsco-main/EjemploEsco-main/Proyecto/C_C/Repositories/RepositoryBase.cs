using System.Configuration;
using System.Data.SqlClient;

namespace C_C.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly string _connectionString;

        protected RepositoryBase()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CCDatabase"]?.ConnectionString
                ?? "Server=.;Database=CC;Integrated Security=True";
        }

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
