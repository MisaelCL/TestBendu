using System;
using System.Collections.Generic;

namespace C_C.App.Model;

public class ChatModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MatchId { get; set; }

    public Guid UserIdA { get; set; }

    public Guid UserIdB { get; set; }

    public MatchModel? Match { get; set; }

    public ICollection<MensajeModel> Mensajes { get; set; } = new List<MensajeModel>();
}
