using System.Collections.Generic;
using System.Data.SqlClient;

namespace C_C_Final.Model
{
    public interface IPerfilRepository
    {
        Perfil GetById(int idPerfil);
        Perfil GetByCuentaId(int idCuenta);
        Preferencias GetPreferenciasByPerfil(int idPerfil);
        IReadOnlyList<Perfil> ListAll();

        int CreatePerfil(Perfil perfil);
        bool UpdatePerfil(Perfil perfil);
        int UpsertPreferencias(Preferencias prefs);
        bool DeletePerfil(int idPerfil);

        int CreatePerfil(SqlConnection cn, SqlTransaction tx, Perfil perfil);
        int UpsertPreferencias(SqlConnection cn, SqlTransaction tx, Preferencias prefs);
    }
}
