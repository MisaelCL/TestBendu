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

        public CuentaDeletionService(
            ICuentaRepository cuentaRepository,
            IPerfilRepository perfilRepository,
            IMatchRepository matchRepository)
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
            // Obtener el ID del Perfil antes de abrir la transacción
            var perfil = _perfilRepository.ObtenerPorCuentaId(idCuenta);
            if (perfil == null)
            {
                // No hay perfil, solo borrar la cuenta (es más simple)
                try
                {
                    return _cuentaRepository.EliminarCuenta(idCuenta);
                }
                catch (Exception) // Captura posible error de FK si algo más depende de la cuenta
                {
                    return false;
                }
            }

            // Si hay perfil, orquestamos una eliminación transaccional
            using (var connection = AbrirConexion())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Borrar dependencias que NO tienen CASCADE (Match y Mensaje)

                    // Borra Matches
                    _matchRepository.EliminarMatchesPorPerfil(perfil.IdPerfil, connection, transaction);

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

                    // 3. Si todo salió bien, confirma la transacción
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    // Algo salió mal, revierte todos los cambios
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}
