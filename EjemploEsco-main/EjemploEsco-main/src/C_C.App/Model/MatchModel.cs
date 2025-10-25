using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace C_C.App.Model;

public class MatchModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserIdA { get; set; }

    public Guid UserIdB { get; set; }

    public bool IsMutual { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ChatId { get; set; }

    public ChatModel? Chat { get; set; }

    public UserModel? UserA { get; set; }

    public UserModel? UserB { get; set; }

    [NotMapped]
    public (Guid, Guid) NormalizedPair => UserIdA.CompareTo(UserIdB) < 0 ? (UserIdA, UserIdB) : (UserIdB, UserIdA);
}
