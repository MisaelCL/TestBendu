using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoIMC.Data;
using ProyectoIMC.Model;

namespace ProyectoIMC.Repositories
{
    public sealed class PacienteRepository : IPacienteRepository
    {
        private readonly AppDatabase _db;

        public PacienteRepository(AppDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<IReadOnlyList<Paciente>> ListarTodosAsync() => _db.ObtenerPacientesAsync();

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
