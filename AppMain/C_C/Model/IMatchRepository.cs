using System.Collections.Generic;
using System.Data.SqlClient;

namespace C_C_Final.Model
{
    public interface IMatchRepository
    {
        Match GetById(int idMatch);
        bool Exists(int idPerfilA, int idPerfilB);
        IReadOnlyList<Match> ListByPerfil(int idPerfil, int page, int pageSize);

        void CreateMatch(int idPerfilEmisor, int idPerfilReceptor, string estado);
        bool UpdateEstado(int idMatch, string nuevoEstado);
        bool DeleteMatch(int idMatch);

        int EnsureChatForMatch(int idMatch);
        Chat GetChatByMatchId(int idMatch);

        long AddMensaje(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura);
        IReadOnlyList<Mensaje> ListMensajes(int idChat, int page, int pageSize);

        void CreateMatch(SqlConnection cn, SqlTransaction tx, int idPerfilEmisor, int idPerfilReceptor, string estado);
        void  EnsureChatForMatch(SqlConnection cn, SqlTransaction tx, int idMatch);
        long AddMensaje(SqlConnection cn, SqlTransaction tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura);
    }
}
