using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class ClassRepository : IClassRepository
{
    private readonly AppDbContext _context;

    public ClassRepository(AppDbContext context) => _context = context;

    public async Task<CharacterClass?> GetByIdAsync(int id) =>
        await _context.Set<CharacterClass>().FindAsync(id);

    public Task<CharacterClass?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A non-empty value is required.", nameof(name));
        return _context.Set<CharacterClass>().FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<IReadOnlyList<CharacterClass>> GetAllAsync() =>
        await _context.Set<CharacterClass>().AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    public async Task<CharacterClass> AddAsync(CharacterClass characterClass)
    {
        ArgumentNullException.ThrowIfNull(characterClass);
        _context.Set<CharacterClass>().Add(characterClass);
        await _context.SaveChangesAsync();
        return characterClass;
    }

    public async Task<CharacterClass> UpdateAsync(CharacterClass characterClass)
    {
        ArgumentNullException.ThrowIfNull(characterClass);
        _context.Set<CharacterClass>().Update(characterClass);
        await _context.SaveChangesAsync();
        return characterClass;
    }
}
