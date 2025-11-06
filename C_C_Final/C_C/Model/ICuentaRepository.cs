using System.Data.SqlClient;

namespace C_C_Final.Model
{
    /// <summary>
    /// Define las operaciones de acceso a datos para las cuentas y alumnos.
    /// </summary>
    public interface ICuentaRepository
    {
        Cuenta ObtenerPorId(int idCuenta);
        Cuenta ObtenerPorCorreo(string email);
        bool ExistePorCorreo(string email);

        void CrearCuenta(string email, string passwordHash, string passwordSalt, byte estadoCuenta);
        void CrearAlumno(Alumno alumno);
        bool ActualizarContrasena(int idCuenta, string newPasswordHash);
        bool EliminarCuenta(int idCuenta);

        int CrearCuenta(SqlConnection cn, SqlTransaction tx, string email, string passwordHash, string passwordSalt, byte estadoCuenta);
        int CrearAlumno(SqlConnection cn, SqlTransaction tx, Alumno alumno);
    }
}
