using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;

namespace C_C.App.Services;

public class MatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IChatRepository _chatRepository;

    public MatchService(IMatchRepository matchRepository, IChatRepository chatRepository)
    {
        _matchRepository = matchRepository;
        _chatRepository = chatRepository;
    }

    public async Task<IReadOnlyList<MatchModel>> GetMatchesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _matchRepository.GetMatchesForUserAsync(userId, cancellationToken);
    }

    public async Task<MatchModel> RegisterLikeAsync(Guid originUserId, Guid targetUserId, bool liked, CancellationToken cancellationToken = default)
    {
        if (originUserId == targetUserId)
        {
            throw new InvalidOperationException("No puedes evaluarte a ti mismo");
        }

        var existing = await _matchRepository.FindExistingAsync(originUserId, targetUserId, cancellationToken);
        if (existing is null)
        {
            var match = new MatchModel
            {
                UserIdA = originUserId,
                UserIdB = targetUserId,
                IsMutual = liked
            };

            if (liked)
            {
                match.IsMutual = false; // se establecerá cuando exista reciprocidad
            }

            return await _matchRepository.AddAsync(match, cancellationToken);
        }

        if (!liked)
        {
            return existing;
        }

        if (!existing.IsMutual)
        {
            existing.IsMutual = true;
            await _matchRepository.UpdateAsync(existing, cancellationToken);
            await EnsureChatAsync(existing, cancellationToken);
        }

        return existing;
    }

    private async Task EnsureChatAsync(MatchModel match, CancellationToken cancellationToken)
    {
        if (match.ChatId.HasValue)
        {
            return;
        }

        var chat = await _chatRepository.GetChatBetweenUsersAsync(match.UserIdA, match.UserIdB, cancellationToken);
        if (chat is not null)
        {
            match.ChatId = chat.Id;
            await _matchRepository.UpdateAsync(match, cancellationToken);
            return;
        }

        var newChat = new ChatModel
        {
            MatchId = match.Id,
            UserIdA = match.UserIdA,
            UserIdB = match.UserIdB
        };

        newChat = await _chatRepository.AddAsync(newChat, cancellationToken);
        match.ChatId = newChat.Id;
        await _matchRepository.UpdateAsync(match, cancellationToken);
    }
}
