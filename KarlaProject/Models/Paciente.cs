using System.ComponentModel.DataAnnotations;
using SQLite;

namespace NetMAUI_Clase6_Crud_SQLLite.Models;

[Table("pacientes")]
public class Paciente : BaseModels
{
    [SQLite.MaxLength(30)]          
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [SQLite.MaxLength(30)]          
    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "La edad debe estar entre 1 y 120 años")]
    public int Edad { get; set; } = 18;

    [Range(1, 500, ErrorMessage = "El peso debe ser mayor a cero")]
    public double PesoKg { get; set; } = 70;

    [Range(50, 260, ErrorMessage = "La estatura debe estar entre 50 y 260 cm")]
    public double EstaturaCm { get; set; } = 170;

    public Sexo Sexo { get; set; } = Sexo.Masculino;

    public NivelActividad NivelActividad { get; set; } = NivelActividad.Sedentario;

    public double Imc { get; set; } = 0;
    public string EstadoImc { get; set; } = string.Empty;

    public double PorcentajeGrasa { get; set; } = 0;
    public string ClasificacionGrasa { get; set; } = string.Empty;

    public double PesoIdeal { get; set; } = 0;

    public double Bmr { get; set; } = 0;

    public double Tdee { get; set; } = 0;

    public void RecalcularMetricas()
    {
        var estaturaMetros = EstaturaCm / 100d;
        Imc = Math.Round(PesoKg / Math.Pow(estaturaMetros, 2), 2);
        EstadoImc = ClasificarImc(Imc);

        var sexoFactor = Sexo == Sexo.Masculino ? 1 : 0;
        PorcentajeGrasa = Math.Round(1.2 * Imc + 0.23 * Edad - 10.8 * sexoFactor - 5.4, 2);
        ClasificacionGrasa = ClasificarGrasa(PorcentajeGrasa, Sexo);

        PesoIdeal = Math.Round(CalcularPesoIdeal(EstaturaCm, Sexo), 2);

        Bmr = Math.Round(CalcularBmr(EstaturaCm, PesoKg, Edad, Sexo), 2);
        Tdee = Math.Round(Bmr * ObtenerFactorActividad(NivelActividad), 2);
    }

    private static string ClasificarImc(double imc)
    {
        return imc switch
        {
            < 18.5 => "Bajo peso",
            < 25.0 => "Peso normal",
            < 30.0 => "Sobrepeso / Pre-obesidad",
            < 35.0 => "Obesidad clase I",
            < 40.0 => "Obesidad clase II",
            _ => "Obesidad clase III"
        };
    }

    private static string ClasificarGrasa(double porcentaje, Sexo sexo)
    {
        if (sexo == Sexo.Femenino)
        {
            if (porcentaje <= 13) return "Grasa esencial";
            if (porcentaje <= 20) return "Atletas";
            if (porcentaje <= 24) return "Fitness";
            if (porcentaje <= 31) return "Aceptable";
            return "Obesidad";
        }

        if (porcentaje <= 5) return "Grasa esencial";
        if (porcentaje <= 13) return "Atletas";
        if (porcentaje <= 17) return "Fitness";
        if (porcentaje <= 24) return "Aceptable";
        return "Obesidad";
    }

    private static double CalcularPesoIdeal(double estaturaCm, Sexo sexo)
    {
        if (sexo == Sexo.Masculino)
            return estaturaCm - 100 - ((estaturaCm - 150) / 4);

        return estaturaCm - 100 - ((estaturaCm - 150) / 2.5);
    }

    private static double CalcularBmr(double estaturaCm, double pesoKg, int edad, Sexo sexo)
    {
        if (sexo == Sexo.Masculino)
            return (estaturaCm * 6.25) + (pesoKg * 9.99) - (edad * 4.92) + 5;

        return (estaturaCm * 6.25) + (pesoKg * 9.99) - (edad * 4.92) - 161;
    }

    private static double ObtenerFactorActividad(NivelActividad nivel)
    {
        return nivel switch
        {
            NivelActividad.Sedentario => 1.2,
            NivelActividad.Ligera => 1.375,
            NivelActividad.Moderada => 1.55,
            NivelActividad.MuyActiva => 1.725,
            NivelActividad.ExtremadamenteActiva => 1.9,
            _ => 1.2
        };
    }
}

public enum Sexo
{
    Masculino = 0,
    Femenino = 1
}

public enum NivelActividad
{
    Sedentario = 0,
    Ligera = 1,
    Moderada = 2,
    MuyActiva = 3,
    ExtremadamenteActiva = 4
}

public abstract class BaseModels
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}