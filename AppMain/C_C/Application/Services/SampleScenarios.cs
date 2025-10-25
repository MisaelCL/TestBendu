using System;
using System.Threading;
using System.Threading.Tasks;
using C_C.Domain;

namespace C_C.Application.Services;

public static class SampleScenarios
{
    public static async Task<int> RegistrarAlumnoDemoAsync(IRegisterAlumnoService registerService, CancellationToken ct = default)
    {
        var request = new RegisterAlumnoRequest(
            Email: $"demo_{Guid.NewGuid():N}@example.com",
            Password: "DemoPassword!123",
            Matricula: Random.Shared.Next(100000, 999999),
            Nombre: "Demo",
            Apaterno: "Usuario",
            Amaterno: "Prueba",
            FechaNacimiento: DateTime.UtcNow.AddYears(-20),
            Genero: 'M',
            CorreoInstitucional: $"demo{Guid.NewGuid():N}@alumno.universidad.mx",
            Carrera: "Ingeniería",
            Nikname: null,
            Biografia: "Cuenta generada para pruebas manuales.",
            FotoPerfil: null,
            PreferenciaGenero: 0,
            EdadMinima: 18,
            EdadMaxima: 30,
            PreferenciaCarrera: "Ingeniería",
            Intereses: "Cine, música"
        );

        return await registerService.RegisterAsync(request, ct).ConfigureAwait(false);
    }

    public static async Task<(int MatchId, int ChatId, long? MensajeId)> CrearMatchConMensajeDemoAsync(
        IMatchService matchService,
        int perfilEmisor,
        int perfilReceptor,
        string mensajeInicial,
        CancellationToken ct = default)
    {
        return await matchService.CreateMatchWithFirstMessageAsync(
            perfilEmisor,
            perfilReceptor,
            estado: "pendiente",
            contenidoInicial: mensajeInicial,
            remitenteInicial: perfilEmisor,
            ct).ConfigureAwait(false);
    }

    public static async Task<long> EnviarMensajeDemoAsync(IMatchService matchService, int idMatch, int remitente, string contenido, CancellationToken ct = default)
    {
        return await matchService.SendMessageAsync(idMatch, remitente, contenido, confirmarLectura: false, ct).ConfigureAwait(false);
    }
}
