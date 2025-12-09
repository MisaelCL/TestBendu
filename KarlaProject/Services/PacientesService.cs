using NetMAUI_Clase6_Crud_SQLLite.Helpers;
using NetMAUI_Clase6_Crud_SQLLite.Interfaces;
using NetMAUI_Clase6_Crud_SQLLite.Models;

namespace NetMAUI_Clase6_Crud_SQLLite.Services;

public class PacientesService : IPacientes
{
    private readonly SQLLiteHelper<Paciente> _db = new(); // usa tu helper genérico

    public Task<List<Paciente>> GetAll()
    {
        var lista = _db.GetAllData();
        return Task.FromResult(lista);
    }

    public Task<Paciente?> GetById(int id)
    {
        var pac = _db.Get(id);
        return Task.FromResult(pac);
    }

    public Task<int> InsertPaciente(Paciente paciente)
    {
        var id = _db.Add(paciente);
        return Task.FromResult(id);
    }

    public Task<int> DeletePaciente(Paciente paciente)
    {
        var result = _db.Delete(paciente);
        return Task.FromResult(result);
    }

    public Task<int> UpdatePaciente(Paciente paciente)
    {
        var result = _db.Update(paciente);
        return Task.FromResult(result);
    }
}