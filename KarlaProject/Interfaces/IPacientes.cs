namespace NetMAUI_Clase6_Crud_SQLLite.Interfaces;

public interface IPacientes
{
    Task<List<Paciente>> GetAll();
    Task<Paciente?> GetById(int id);
    Task<int> InsertPaciente(Paciente paciente);
    Task<int> DeletePaciente(Paciente paciente);
    Task<int> UpdatePaciente(Paciente paciente);
}