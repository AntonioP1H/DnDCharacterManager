using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class BackgroundRepository : IBackgroundRepository
{
    private readonly AppDbContext _context;

    public BackgroundRepository(AppDbContext context) => _context = context;

    public async Task<Background?> GetByIdAsync(int id) => await _context.Set<Background>().FindAsync(id);

    public Task<Background?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A non-empty value is required.", nameof(name));
        return _context.Set<Background>().FirstOrDefaultAsync(b => b.Name == name);
    }

    public async Task<IReadOnlyList<Background>> GetAllAsync() =>
        await _context.Set<Background>().AsNoTracking().OrderBy(b => b.Name).ToListAsync();

    public async Task<Background> AddAsync(Background background)
    {
        ArgumentNullException.ThrowIfNull(background);
        _context.Set<Background>().Add(background);
        await _context.SaveChangesAsync();
        return background;
    }

    public async Task<Background> UpdateAsync(Background background)
    {
        ArgumentNullException.ThrowIfNull(background);
        _context.Set<Background>().Update(background);
        await _context.SaveChangesAsync();
        return background;
    }
}
