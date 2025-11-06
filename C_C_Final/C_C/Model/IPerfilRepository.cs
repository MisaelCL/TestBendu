using System.Collections.Generic;

namespace C_C_Final.Model
{
    public interface IPerfilRepository
    {
        Perfil ObtenerPorId(int idPerfil);
        Perfil ObtenerPorCuentaId(int idCuenta);
        IReadOnlyList<Perfil> ObtenerPorIds(IEnumerable<int> idsPerfiles);
        int CrearPerfil(Perfil perfil);
        bool ActualizarPerfil(Perfil perfil);
        bool EliminarPerfil(int idPerfil);

        Perfil ObtenerSiguientePerfilPara(int idPerfilActual);
    }
}
