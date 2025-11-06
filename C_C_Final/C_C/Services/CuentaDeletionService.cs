using C_C_Final.Model;
using C_C_Final.Repositories;
using System;
using System.Data;
using System.Data.SqlClient;

namespace C_C_Final.Services
{
    /// <summary>
    /// Orquesta la eliminación segura de una cuenta y todos sus datos dependientes.
    /// </summary>
    public sealed class CuentaDeletionService : RepositoryBase
    {
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly IMatchRepository _matchRepository;
        // NOTA: Falta un IMensajeRepository.

        public CuentaDeletionService(
            ICuentaRepository cuentaRepository,
            IPerfilRepository perfilRepository,
            IMatchRepository matchRepository,
            string connectionString = null)
            : base(connectionString)
        {
            _cuentaRepository = cuentaRepository;
            _perfilRepository = perfilRepository;
            _matchRepository = matchRepository;
        }

        /// <summary>
        /// Elimina una cuenta y todas sus dependencias (Perfil, Alumno, Matches, Mensajes)
        /// dentro de una transacción.
        /// </summary>
        /// <param name="idCuenta">El ID de la cuenta a eliminar.</param>
        /// <returns>True si la eliminación fue exitosa, false en caso contrario.</returns>
        public bool EliminarCuenta(int idCuenta)
        {
            var perfil = _perfilRepository.ObtenerPorCuentaId(idCuenta);
            if (perfil == null)
            {
                try
                {
                    return _cuentaRepository.EliminarCuenta(idCuenta);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            using (var connection = AbrirConexion())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    _matchRepository.EliminarMatchesPorPerfil(perfil.IdPerfil, connection, transaction);

                    // Falta implementar eliminación de mensajes. Ver nota en especificación.

                    const string sqlDeleteCuenta = "DELETE FROM dbo.Cuenta WHERE ID_Cuenta = @Id";
                    using (var cmd = CrearComando(connection, sqlDeleteCuenta, CommandType.Text, transaction))
                    {
                        AgregarParametro(cmd, "@Id", idCuenta, SqlDbType.Int);
                        var rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            throw new InvalidOperationException("La cuenta no se encontró o no se pudo eliminar.");
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}
