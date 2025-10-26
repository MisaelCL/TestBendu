using System.Data.SqlClient;

namespace C_C_Final.Model
{
    public interface ICuentaRepository
    {
        Cuenta? GetById(int idCuenta);
        Cuenta? GetByEmail(string email);
        bool ExistsByEmail(string email);

        int CreateCuenta(string email, string passwordHash, byte estadoCuenta);
        int CreateAlumno(Alumno alumno);
        bool UpdatePassword(int idCuenta, string newPasswordHash);
        bool DeleteCuenta(int idCuenta);

        int CreateCuenta(SqlConnection cn, SqlTransaction tx, string email, string passwordHash, byte estadoCuenta);
        int CreateAlumno(SqlConnection cn, SqlTransaction tx, Alumno alumno);
    }
}
