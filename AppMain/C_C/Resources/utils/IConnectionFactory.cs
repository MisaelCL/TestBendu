using Microsoft.Data.SqlClient;

namespace C_C.Resources.utils;

public interface IConnectionFactory
{
    SqlConnection CreateConnection();
}
