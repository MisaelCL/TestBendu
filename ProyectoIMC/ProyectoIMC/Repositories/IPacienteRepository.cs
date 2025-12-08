using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoIMC.Model;

namespace ProyectoIMC.Repositories
{
    public interface IPacienteRepository
    {
        // Trae toda la lista de pacientes tal como está guardada.
        Task<IReadOnlyList<Paciente>> ListarTodosAsync();

        // Busca un paciente concreto por su Id único.
        Task<Paciente?> ObtenerPorIdAsync(int idPaciente);

        // Guarda un paciente nuevo o actualiza uno existente y devuelve el Id final.
        Task<int> GuardarAsync(Paciente paciente);

        // Elimina al paciente indicado; devuelve true si el borrado se completó.
        Task<bool> EliminarAsync(int idPaciente);
    }
}