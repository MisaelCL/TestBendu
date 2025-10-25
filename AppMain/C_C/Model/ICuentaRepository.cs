using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public interface ICuentaRepository
    {
        bool AutenticateCuenta(int Matricula, string HashContraseña);
        void AddCuenta(int Matricula, string contraseña);
        void DeleteCuenta(int Matricula);
        void EditContraseña(int Matricula, string nuevaContraseña);
    }
}
