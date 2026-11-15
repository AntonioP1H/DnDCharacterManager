using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class SpellRepository : ISpellRepository
{
    private readonly AppDbContext _context;

    public SpellRepository(AppDbContext context) => _context = context;

    public async Task<Spell?> GetByIdAsync(int id) => await _context.Set<Spell>().FindAsync(id);

    public Task<Spell?> GetByNameAndLevelAsync(string name, int level)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A non-empty value is required.", nameof(name));
        return _context.Set<Spell>().FirstOrDefaultAsync(s => s.Name == name && s.Level == level);
    }

    public async Task<IReadOnlyList<Spell>> GetAllAsync() =>
        await _context.Set<Spell>().AsNoTracking()
            .OrderBy(s => s.Level).ThenBy(s => s.Name).ToListAsync();

    public async Task<IReadOnlyList<Spell>> GetByLevelAsync(int level) =>
        await _context.Set<Spell>().AsNoTracking().Where(s => s.Level == level)
            .OrderBy(s => s.Name).ToListAsync();

    public async Task<IReadOnlyList<Spell>> GetByClassAsync(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("A non-empty value is required.", nameof(className));

        // Classes is a JSON array. Filter after materializing so "Art" cannot match "Artificer".
        var spells = await _context.Set<Spell>().AsNoTracking()
            .OrderBy(s => s.Level).ThenBy(s => s.Name).ToListAsync();

        return spells.Where(spell => DeserializeClasses(spell.Classes)
                .Contains(className, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<Spell>> GetBySchoolAsync(string school)
    {
        if (string.IsNullOrWhiteSpace(school))
            throw new ArgumentException("A non-empty value is required.", nameof(school));
        return await _context.Set<Spell>().AsNoTracking()
            .Where(s => s.School == school)
            .OrderBy(s => s.Level).ThenBy(s => s.Name).ToListAsync();
    }

    public async Task<Spell> AddAsync(Spell spell)
    {
        ArgumentNullException.ThrowIfNull(spell);
        _context.Set<Spell>().Add(spell);
        await _context.SaveChangesAsync();
        return spell;
    }

    public async Task<Spell> UpdateAsync(Spell spell)
    {
        ArgumentNullException.ThrowIfNull(spell);
        _context.Set<Spell>().Update(spell);
        await _context.SaveChangesAsync();
        return spell;
    }

    private static string[] DeserializeClasses(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
