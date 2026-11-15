using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class RaceRepository : IRaceRepository
{
    private readonly AppDbContext _context;

    public RaceRepository(AppDbContext context) => _context = context;

    public async Task<Race?> GetByIdAsync(int id) => await _context.Set<Race>().FindAsync(id);

    public Task<Race?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A non-empty value is required.", nameof(name));
        return _context.Set<Race>().FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IReadOnlyList<Race>> GetAllAsync() =>
        await _context.Set<Race>().AsNoTracking().OrderBy(r => r.Name).ToListAsync();

    public async Task<Race> AddAsync(Race race)
    {
        ArgumentNullException.ThrowIfNull(race);
        _context.Set<Race>().Add(race);
        await _context.SaveChangesAsync();
        return race;
    }

    public async Task<Race> UpdateAsync(Race race)
    {
        ArgumentNullException.ThrowIfNull(race);
        _context.Set<Race>().Update(race);
        await _context.SaveChangesAsync();
        return race;
    }
}
