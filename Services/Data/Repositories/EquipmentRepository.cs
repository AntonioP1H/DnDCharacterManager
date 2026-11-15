using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Equipment;

namespace DnDCharacterManager.Services.Data.Repositories;

public sealed class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext _context;

    public EquipmentRepository(AppDbContext context) => _context = context;

    public Task<Weapon?> GetWeaponByIdAsync(int id)
        => _context.Weapons.Include(w => w.WeaponProperties)
            .AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IReadOnlyList<Weapon>> GetAllWeaponsAsync()
        => await _context.Weapons.Include(w => w.WeaponProperties)
            .AsNoTracking().OrderBy(w => w.Name).ToListAsync();

    public async Task<IReadOnlyList<Weapon>> GetWeaponsByCategoryAsync(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        // Properties are stored as comma-separated text, not a relational category column.
        // Load first, then filter in memory; StringComparison overloads are not SQL-translatable.
        var weapons = await GetAllWeaponsAsync();
        if (category.Equals("Melee", StringComparison.OrdinalIgnoreCase))
            return weapons.Where(w => w.IsMelee).ToList();
        if (category.Equals("Ranged", StringComparison.OrdinalIgnoreCase))
            return weapons.Where(w => w.IsRanged).ToList();
        return weapons.Where(w => w.PropertyList.Any(p =>
            p.Equals(category, StringComparison.OrdinalIgnoreCase))).ToList();
    }

    public Task<Armor?> GetArmorByIdAsync(int id)
        => _context.Armors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IReadOnlyList<Armor>> GetAllArmorAsync()
        => await _context.Armors.AsNoTracking().OrderBy(a => a.Category)
            .ThenBy(a => a.Name).ToListAsync();

    public async Task<IReadOnlyList<Armor>> GetArmorByCategoryAsync(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        return await _context.Armors.AsNoTracking()
            .Where(a => a.Category == category)
            .OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<EquipmentItem>> GetAllItemsAsync()
        => await _context.EquipmentItems.AsNoTracking()
            .OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();

    public Task SeedCoreEquipmentAsync()
    {
        // Intentionally does not insert hard-coded game content. The rulebook importer
        // should populate this catalogue from the user's selected source PDFs.
        return Task.CompletedTask;
    }
}
