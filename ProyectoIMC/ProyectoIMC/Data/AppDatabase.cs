using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ProyectoIMC.Model;
using SQLite;

namespace ProyectoIMC.Data
{
    public sealed class AppDatabase
    {
        private readonly SQLiteAsyncConnection _connection;

        public AppDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pacientes.db3");
            _connection = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

            _connection.CreateTableAsync<Paciente>().GetAwaiter().GetResult();
        }

        public Task<List<Paciente>> ObtenerPacientesAsync()
        {
            return _connection.Table<Paciente>()
                              .OrderBy(p => p.Apellido)
                              .ThenBy(p => p.Nombre)
                              .ToListAsync();
        }

        public Task<Paciente?> ObtenerPacientePorIdAsync(int id)
        {
            return _connection.Table<Paciente>()
                              .Where(p => p.IdPaciente == id)
                              .FirstOrDefaultAsync();
        }

        public async Task<int> GuardarPacienteAsync(Paciente paciente)
        {
            if (paciente == null) throw new ArgumentNullException(nameof(paciente));

            if (paciente.IdPaciente == 0)
            {
                await _connection.InsertAsync(paciente);
            }
            else
            {
                await _connection.UpdateAsync(paciente);
            }

            return paciente.IdPaciente;
        }

        public Task<int> EliminarPacienteAsync(Paciente paciente)
        {
            return _connection.DeleteAsync(paciente);
        }
    }
}
