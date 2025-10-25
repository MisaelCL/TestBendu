using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    internal interface IMesagesRepository
    {
        public abstract void AddMessage(int IdPerfilEmisor, int IdPerfilReceptor, string Mensaje);

    }
}
