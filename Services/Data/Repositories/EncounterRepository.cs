using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Combat;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class EncounterRepository : IEncounterRepository
{
    private readonly AppDbContext _context;

    public EncounterRepository(AppDbContext context) => _context = context;

    public Task<Encounter?> GetByIdAsync(int id)
        => _context.Encounters.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Encounter>> GetAllAsync()
        => await _context.Encounters.AsNoTracking()
            .OrderByDescending(e => e.LastPlayedAt ?? e.CreatedAt).ToListAsync();

    public async Task<Encounter> AddAsync(Encounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        _context.Encounters.Add(encounter);
        await _context.SaveChangesAsync();
        return encounter;
    }

    public async Task<Encounter> UpdateAsync(Encounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        _context.Encounters.Update(encounter);
        await _context.SaveChangesAsync();
        return encounter;
    }

    public async Task DeleteAsync(int id)
    {
        Encounter? encounter = await _context.Encounters.FindAsync(id);
        if (encounter is null) return;
        _context.Encounters.Remove(encounter);
        await _context.SaveChangesAsync();
    }

    public Task<Encounter?> GetWithParticipantsAsync(int id)
        => _context.Encounters.Include(e => e.Participants)
            .Include(e => e.CombatLog)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
}
