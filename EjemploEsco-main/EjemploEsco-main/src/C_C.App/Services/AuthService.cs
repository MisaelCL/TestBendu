using System;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;
using FluentValidation;

namespace C_C.App.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPerfilRepository _perfilRepository;
    private readonly IValidator<UserRegistrationRequest> _registrationValidator;
    private readonly InputSanitizer _sanitizer;

    public AuthService(IUserRepository userRepository, IPerfilRepository perfilRepository, InputSanitizer sanitizer)
    {
        _userRepository = userRepository;
        _perfilRepository = perfilRepository;
        _sanitizer = sanitizer;
        _registrationValidator = new UserRegistrationValidator();
    }

    public async Task<UserModel> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var sanitized = request with
        {
            Email = _sanitizer.Sanitize(request.Email).ToLowerInvariant(),
            DisplayName = _sanitizer.Sanitize(request.DisplayName)
        };
        await _registrationValidator.ValidateAndThrowAsync(sanitized, cancellationToken);

        var existing = await _userRepository.GetByEmailAsync(sanitized.Email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("El correo ya se encuentra registrado");
        }

        var user = new UserModel
        {
            Email = sanitized.Email,
            DisplayName = sanitized.DisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(sanitized.Password),
            DateOfBirth = sanitized.DateOfBirth,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var perfil = new PerfilModel
        {
            UserId = user.Id,
            Bio = sanitized.Bio ?? string.Empty,
            Ciudad = sanitized.City ?? string.Empty,
            FotoUrl = sanitized.PhotoUrl ?? string.Empty,
            Intereses = sanitized.Interests ?? string.Empty
        };

        await _perfilRepository.AddAsync(perfil, cancellationToken);

        await _perfilRepository.SavePreferenciasAsync(new PreferenciasModel
        {
            UserId = user.Id,
            EdadMinima = sanitized.MinAge,
            EdadMaxima = sanitized.MaxAge,
            GeneroBuscado = sanitized.TargetGender,
            DistanciaMaximaKm = sanitized.MaxDistanceKm
        }, cancellationToken);

        return user;
    }

    public async Task<UserModel> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var sanitizedEmail = _sanitizer.Sanitize(email).ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(sanitizedEmail, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Credenciales inválidas");
        }

        if (user.IsBlocked)
        {
            throw new InvalidOperationException("La cuenta se encuentra bloqueada");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new InvalidOperationException("Credenciales inválidas");
        }

        return user;
    }
}

public record UserRegistrationRequest(
    string Email,
    string Password,
    string DisplayName,
    DateOnly DateOfBirth,
    string? Bio,
    string? City,
    string? PhotoUrl,
    string? Interests,
    int MinAge,
    int MaxAge,
    string TargetGender,
    double MaxDistanceKm);

public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserRegistrationValidator()
    {
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
        RuleFor(r => r.Password).NotEmpty().MinimumLength(8);
        RuleFor(r => r.DisplayName).NotEmpty().MaximumLength(128);
        RuleFor(r => r.DateOfBirth).Must(BeAdult).WithMessage("Debes ser mayor de 18 años");
        RuleFor(r => r.MinAge).InclusiveBetween(18, 120);
        RuleFor(r => r.MaxAge).InclusiveBetween(18, 120).GreaterThanOrEqualTo(r => r.MinAge);
        RuleFor(r => r.TargetGender).NotEmpty();
        RuleFor(r => r.MaxDistanceKm).GreaterThan(0).LessThanOrEqualTo(500);
    }

    private static bool BeAdult(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return today.AddYears(-18) >= date;
    }
}
