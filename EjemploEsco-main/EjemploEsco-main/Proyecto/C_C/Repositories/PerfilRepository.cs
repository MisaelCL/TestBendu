using System;
using System.Collections.Generic;
using System.Linq;
using C_C.Model;

namespace C_C.Repositories
{
    public class PerfilRepository : RepositoryBase, IPerfilRepository
    {
        private static readonly List<PerfilModel> Perfiles = new List<PerfilModel>();

        public PerfilModel GetByUserId(Guid userId)
        {
            return Perfiles.FirstOrDefault(perfil => perfil.UserId == userId);
        }

        public IEnumerable<PerfilModel> GetAll()
        {
            return Perfiles;
        }

        public void Save(PerfilModel perfil)
        {
            var existing = Perfiles.FirstOrDefault(p => p.UserId == perfil.UserId);
            if (existing == null)
            {
                perfil.Id = perfil.Id == Guid.Empty ? Guid.NewGuid() : perfil.Id;
                Perfiles.Add(perfil);
                return;
            }

            existing.Nombre = perfil.Nombre;
            existing.Biografia = perfil.Biografia;
            existing.Carrera = perfil.Carrera;
            existing.FechaNacimiento = perfil.FechaNacimiento;
            existing.FotoPrincipal = perfil.FotoPrincipal;
            existing.Intereses = perfil.Intereses;
        }
    }
}
