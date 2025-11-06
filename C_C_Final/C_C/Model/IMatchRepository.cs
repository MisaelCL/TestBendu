using System.Collections.Generic;
using System.Data.SqlClient;

namespace C_C_Final.Model
{
    /// <summary>
    /// Declara las operaciones de acceso a datos para los emparejamientos, chats y mensajes.
    /// </summary>
    public interface IMatchRepository
    {
        Match ObtenerPorId(int idMatch);
        Match ObtenerPorPerfiles(int idPerfilA, int idPerfilB);
        bool Existe(int idPerfilA, int idPerfilB);
        IReadOnlyList<Match> ListarPorPerfil(int idPerfil, int page, int pageSize);

        int CrearMatch(int idPerfilEmisor, int idPerfilReceptor, string estado);
        bool ActualizarEstado(int idMatch, string nuevoEstado);
        bool EliminarMatch(int idMatch);

        int AsegurarChatParaMatch(int idMatch);
        Chat ObtenerChatPorMatchId(int idMatch);

        long AgregarMensaje(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura);
        IReadOnlyList<Mensaje> ListarMensajes(int idChat, int page, int pageSize);

        int CrearMatch(SqlConnection cn, SqlTransaction tx, int idPerfilEmisor, int idPerfilReceptor, string estado);
        int AsegurarChatParaMatch(SqlConnection cn, SqlTransaction tx, int idMatch);
        long AgregarMensaje(SqlConnection cn, SqlTransaction tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura);

        void EliminarMatchesPorPerfil(int idPerfil, SqlConnection connection, SqlTransaction transaction);
        void ActualizarParticipantes(int idMatch, int nuevoEmisor, int nuevoReceptor);
    }
}
