using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    internal interface IPreferenciasRepository
    {
        public abstract void AddPreferencias(int IdPerfil, char PreferenciaGenero, int EdadMinima, int EdadMaxima);
        public abstract void UpdatePreferencias(int IdPerfil, char PreferenciaGenero, int EdadMinima, int EdadMaxima);

    }
}
