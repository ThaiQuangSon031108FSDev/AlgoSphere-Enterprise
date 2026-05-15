using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Tournaments.Commands.JoinTournament;

public record JoinTournamentCommand(int TournamentId, int UserId) : IRequest<JoinResult>;

public record JoinResult(bool Success, string Message);

public class JoinTournamentCommandHandler : IRequestHandler<JoinTournamentCommand, JoinResult>
{
    private readonly IAlgoSphereDbContext _context;

    public JoinTournamentCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<JoinResult> Handle(JoinTournamentCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament == null)
            return new JoinResult(false, "Giải đấu không tồn tại.");

        if (tournament.Status == "Completed")
            return new JoinResult(false, "Giải đấu đã kết thúc.");

        var alreadyJoined = tournament.Participants.Any(p => p.UserId == request.UserId);
        if (alreadyJoined)
            return new JoinResult(false, "Bạn đã tham gia giải đấu này rồi.");

        _context.TournamentParticipants.Add(new TournamentParticipant
        {
            TournamentId = request.TournamentId,
            UserId = request.UserId
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new JoinResult(true, "Tham gia giải đấu thành công!");
    }
}
