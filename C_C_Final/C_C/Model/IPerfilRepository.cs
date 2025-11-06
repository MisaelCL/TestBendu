using System.Collections.Generic;
using System.Data.SqlClient;

namespace C_C_Final.Model
{
    /// <summary>
    /// Define las operaciones de acceso a datos para los perfiles y sus preferencias.
    /// </summary>
    public interface IPerfilRepository
    {
        Perfil ObtenerPorId(int idPerfil);
        Perfil ObtenerPorCuentaId(int idCuenta);
        Preferencias ObtenerPreferenciasPorPerfil(int idPerfil);
        IReadOnlyList<Perfil> ListarTodos();

        int CrearPerfil(Perfil perfil);
        bool ActualizarPerfil(Perfil perfil);
        int InsertarOActualizarPreferencias(Preferencias prefs);
        bool EliminarPerfil(int idPerfil);

        int CrearPerfil(SqlConnection cn, SqlTransaction tx, Perfil perfil);
        int InsertarOActualizarPreferencias(SqlConnection cn, SqlTransaction tx, Preferencias prefs);

        Perfil ObtenerSiguientePerfilPara(int idPerfilActual);
    }
}
