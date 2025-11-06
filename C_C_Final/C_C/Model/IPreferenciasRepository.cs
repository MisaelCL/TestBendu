using System.Data.SqlClient;

namespace C_C_Final.Model
{
    /// <summary>
    /// Define las operaciones de acceso a datos para las preferencias de los perfiles.
    /// </summary>
    public interface IPreferenciasRepository
    {
        Preferencias ObtenerPorPerfilId(int idPerfil);
        Preferencias ObtenerPorCuentaId(int idCuenta);
        int CrearPreferencias(Preferencias preferencias);
        int CrearPreferencias(SqlConnection connection, SqlTransaction transaction, Preferencias preferencias);
        bool ActualizarPreferencias(Preferencias preferencias);
        bool ActualizarPreferencias(SqlConnection connection, SqlTransaction transaction, Preferencias preferencias);
        bool EliminarPorPerfil(int idPerfil);
    }
}
