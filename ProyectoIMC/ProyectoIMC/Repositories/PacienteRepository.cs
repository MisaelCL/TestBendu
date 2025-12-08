using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoIMC.Data;
using ProyectoIMC.Model;

namespace ProyectoIMC.Repositories
{
    public sealed class PacienteRepository(AppDatabase db) : IPacienteRepository
    {
        private readonly AppDatabase _db = db ?? throw new ArgumentNullException(nameof(db));

        // Devuelve todos los pacientes tal cual están en la base para poblar las listas.
        public async Task<IReadOnlyList<Paciente>> ListarTodosAsync()
        {
            var lista = await _db.ObtenerPacientesAsync();
            return lista;
        }

        // Busca un paciente específico por Id.
        public Task<Paciente?> ObtenerPorIdAsync(int idPaciente)
        {
            return _db.ObtenerPacientePorIdAsync(idPaciente);
        }

        // Inserta o actualiza según corresponda y devuelve el Id final.
        public Task<int> GuardarAsync(Paciente paciente)
        {
            return _db.GuardarPacienteAsync(paciente);
        }

        // Intenta eliminar un paciente, devolviendo true sólo cuando el borrado se completa.
        public async Task<bool> EliminarAsync(int idPaciente)
        {
            var paciente = await _db.ObtenerPacientePorIdAsync(idPaciente);
            if (paciente == null) return false;

            var result = await _db.EliminarPacienteAsync(paciente);
            return result > 0;
        }
    }
}
