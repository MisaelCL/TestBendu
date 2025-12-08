using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoIMC.Model;

namespace ProyectoIMC.Repositories
{
    public interface IPacienteRepository
    {
        Task<IReadOnlyList<Paciente>> ListarTodosAsync();
        Task<Paciente?> ObtenerPorIdAsync(int idPaciente);
        Task<int> GuardarAsync(Paciente paciente);
        Task<bool> EliminarAsync(int idPaciente);
    }
}