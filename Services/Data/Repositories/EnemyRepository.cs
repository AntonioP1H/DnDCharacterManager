using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Enemies;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class EnemyRepository : IEnemyRepository
{
    private readonly AppDbContext _context;

    public EnemyRepository(AppDbContext context) => _context = context;

    public Task<Enemy?> GetByIdAsync(int id)
        => _context.Enemies.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Enemy>> GetAllAsync()
        => await _context.Enemies.AsNoTracking().OrderBy(e => e.Name).ToListAsync();

    public async Task<IReadOnlyList<Enemy>> GetByChallengeRatingAsync(string challengeRating)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(challengeRating);
        return await _context.Enemies.AsNoTracking()
            .Where(e => e.ChallengeRating == challengeRating)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Enemy> AddAsync(Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        _context.Enemies.Add(enemy);
        await _context.SaveChangesAsync();
        return enemy;
    }

    public async Task<Enemy> UpdateAsync(Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        _context.Enemies.Update(enemy);
        await _context.SaveChangesAsync();
        return enemy;
    }

    public async Task DeleteAsync(int id)
    {
        Enemy? enemy = await _context.Enemies.FindAsync(id);
        if (enemy is null) return;
        _context.Enemies.Remove(enemy);
        await _context.SaveChangesAsync();
    }

    public Task<Enemy?> GetWithDetailsAsync(int id)
        => _context.Enemies.Include(e => e.Attacks)
            .Include(e => e.SpecialAbilities)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
}
