using System;
using System.Collections.Generic;
using C_C.Model;

namespace C_C.Repositories
{
    public interface IPerfilRepository
    {
        PerfilModel GetByUserId(Guid userId);

        void Save(PerfilModel perfil);

        IEnumerable<PerfilModel> GetAll();
    }
}
