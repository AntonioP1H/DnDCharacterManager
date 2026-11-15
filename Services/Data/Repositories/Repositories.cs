using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Enemies;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Models.Combat;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface ICharacterRepository
{
    Task<List<Character>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Character character, CancellationToken cancellationToken = default);
    Task UpdateAsync(Character character, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class CharacterRepository : ICharacterRepository
{
    private readonly AppDbContext _db;
    public CharacterRepository(AppDbContext db) => _db = db;

    public Task<List<Character>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Characters.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Characters
            .Include(c => c.AbilityScores)
            .Include(c => c.Skills)
            .Include(c => c.Equipment).ThenInclude(e => e.Item)
            .Include(c => c.Weapons).ThenInclude(w => w.Weapon)
            .Include(c => c.Armor).ThenInclude(a => a.Armor)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Character character, CancellationToken cancellationToken = default)
    {
        _db.Characters.Add(character);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Character character, CancellationToken cancellationToken = default)
    {
        _db.Characters.Update(character);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var character = await _db.Characters.FindAsync(new object[] { id }, cancellationToken);
        if (character is null) return;
        _db.Characters.Remove(character);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public interface IEnemyRepository
{
    Task<List<Enemy>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Enemy?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Enemy enemy, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class EnemyRepository : IEnemyRepository
{
    private readonly AppDbContext _db;
    public EnemyRepository(AppDbContext db) => _db = db;

    public Task<List<Enemy>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Enemies.AsNoTracking().OrderBy(e => e.Name).ToListAsync(cancellationToken);

    public Task<Enemy?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Enemies.Include(e => e.Attacks).Include(e => e.SpecialAbilities)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(Enemy enemy, CancellationToken cancellationToken = default)
    {
        _db.Enemies.Add(enemy);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var enemy = await _db.Enemies.FindAsync(new object[] { id }, cancellationToken);
        if (enemy is null) return;
        _db.Enemies.Remove(enemy);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public interface IEquipmentRepository
{
    Task<List<Weapon>> GetWeaponsAsync(CancellationToken cancellationToken = default);
    Task<List<Armor>> GetArmorsAsync(CancellationToken cancellationToken = default);
    Task<List<EquipmentItem>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task SeedCoreEquipmentAsync(CancellationToken cancellationToken = default);
}

public sealed class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext _db;
    public EquipmentRepository(AppDbContext db) => _db = db;

    public Task<List<Weapon>> GetWeaponsAsync(CancellationToken cancellationToken = default) =>
        _db.Weapons.AsNoTracking().OrderBy(w => w.Name).ToListAsync(cancellationToken);

    public Task<List<Armor>> GetArmorsAsync(CancellationToken cancellationToken = default) =>
        _db.Armors.AsNoTracking().OrderBy(a => a.Name).ToListAsync(cancellationToken);

    public Task<List<EquipmentItem>> GetItemsAsync(CancellationToken cancellationToken = default) =>
        _db.EquipmentItems.AsNoTracking().OrderBy(i => i.Name).ToListAsync(cancellationToken);

    public Task SeedCoreEquipmentAsync(CancellationToken cancellationToken = default)
    {
        // Intentionally does not invent official content. Import equipment from the user's
        // rulebooks before seeding any game catalogue entries.
        return Task.CompletedTask;
    }
}

public interface IEncounterRepository
{
    Task<List<Encounter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Encounter?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Encounter encounter, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class EncounterRepository : IEncounterRepository
{
    private readonly AppDbContext _db;
    public EncounterRepository(AppDbContext db) => _db = db;

    public Task<List<Encounter>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Encounters.AsNoTracking().OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);

    public Task<Encounter?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Encounters.Include(e => e.Participants).Include(e => e.CombatLog)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(Encounter encounter, CancellationToken cancellationToken = default)
    {
        _db.Encounters.Add(encounter);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var encounter = await _db.Encounters.FindAsync(new object[] { id }, cancellationToken);
        if (encounter is null) return;
        _db.Encounters.Remove(encounter);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
