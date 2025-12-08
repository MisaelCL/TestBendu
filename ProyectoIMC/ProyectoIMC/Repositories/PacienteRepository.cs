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

        public async Task<IReadOnlyList<Paciente>> ListarTodosAsync()
        {
            var lista = await _db.ObtenerPacientesAsync();
            return lista;
        }

        public Task<Paciente?> ObtenerPorIdAsync(int idPaciente)
        {
            return _db.ObtenerPacientePorIdAsync(idPaciente);
        }

        public Task<int> GuardarAsync(Paciente paciente)
        {
            return _db.GuardarPacienteAsync(paciente);
        }

        public async Task<bool> EliminarAsync(int idPaciente)
        {
            var paciente = await _db.ObtenerPacientePorIdAsync(idPaciente);
            if (paciente == null) return false;

            var result = await _db.EliminarPacienteAsync(paciente);
            return result > 0;
        }
    }
}
