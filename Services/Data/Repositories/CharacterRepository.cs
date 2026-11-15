using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Characters;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class CharacterRepository : ICharacterRepository
{
    private readonly AppDbContext _context;

    public CharacterRepository(AppDbContext context) => _context = context;

    public Task<Character?> GetByIdAsync(int id)
        => _context.Characters.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Character>> GetAllAsync()
        => await _context.Characters.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    public async Task<Character> AddAsync(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);
        _context.Characters.Add(character);
        await _context.SaveChangesAsync();
        return character;
    }

    public async Task<Character> UpdateAsync(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);
        _context.Characters.Update(character);
        await _context.SaveChangesAsync();
        return character;
    }

    public async Task DeleteAsync(int id)
    {
        Character? character = await _context.Characters.FindAsync(id);
        if (character is null) return;
        _context.Characters.Remove(character);
        await _context.SaveChangesAsync();
    }

    public Task<Character?> GetWithDetailsAsync(int id)
        => _context.Characters
            .Include(c => c.AbilityScores)
            .Include(c => c.Skills)
            .Include(c => c.Equipment).ThenInclude(e => e.Item)
            .Include(c => c.Weapons).ThenInclude(w => w.Weapon)
            .Include(c => c.Armor).ThenInclude(a => a.Armor)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
}
