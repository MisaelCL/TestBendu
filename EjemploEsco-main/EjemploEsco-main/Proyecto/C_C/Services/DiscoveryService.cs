using System;
using System.Collections.Generic;
using System.Linq;
using C_C.Model;
using C_C.Repositories;

namespace C_C.Services
{
    public class DiscoveryService
    {
        private readonly IPerfilRepository _perfilRepository;
        private readonly IUserRepository _userRepository;

        public DiscoveryService(IPerfilRepository perfilRepository, IUserRepository userRepository)
        {
            _perfilRepository = perfilRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<PerfilModel> ObtenerSugerencias(Guid usuarioId)
        {
            var perfilActual = _perfilRepository.GetByUserId(usuarioId);
            var perfiles = _perfilRepository.GetAll();

            return perfiles
                .Where(perfil => perfil.UserId != usuarioId)
                .OrderBy(perfil => DistanciaPorCarrera(perfilActual, perfil));
        }

        private static int DistanciaPorCarrera(PerfilModel origen, PerfilModel destino)
        {
            if (origen == null || destino == null)
            {
                return int.MaxValue;
            }

            return string.Equals(origen.Carrera, destino.Carrera, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        }
    }
}
