using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public interface IPerfilRepository
    {
        void AddPerfil(string Nikname);
        void UpdatePerfil(string Nikname);
    }
}
