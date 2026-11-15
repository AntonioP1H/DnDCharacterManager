using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Enemies;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Models.Combat;
using System.IO;
namespace DnDCharacterManager.Services.Data;

/// <summary>Entity Framework Core context backed by a local SQLite database.</summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Character> Characters => Set<Character>();
    public DbSet<AbilityScore> AbilityScores => Set<AbilityScore>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CharacterEquipment> CharacterEquipment => Set<CharacterEquipment>();
    public DbSet<CharacterWeapon> CharacterWeapons => Set<CharacterWeapon>();
    public DbSet<CharacterArmor> CharacterArmor => Set<CharacterArmor>();

    public DbSet<Enemy> Enemies => Set<Enemy>();
    public DbSet<EnemyAttack> EnemyAttacks => Set<EnemyAttack>();
    public DbSet<EnemySpecialAbility> EnemySpecialAbilities => Set<EnemySpecialAbility>();

    public DbSet<Weapon> Weapons => Set<Weapon>();
    public DbSet<Armor> Armors => Set<Armor>();
    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();
    public DbSet<WeaponProperty> WeaponProperties => Set<WeaponProperty>();
    public DbSet<DamageType> DamageTypes => Set<DamageType>();
    public DbSet<Condition> Conditions => Set<Condition>();

    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<CombatParticipant> CombatParticipants => Set<CombatParticipant>();
    public DbSet<CombatLogEntry> CombatLog => Set<CombatLogEntry>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var appFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DnDCharacterManager");
            Directory.CreateDirectory(appFolder);
            var databasePath = Path.Combine(appFolder, "data.db");
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }

#if DEBUG
        optionsBuilder.EnableDetailedErrors();
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(10);
                property.SetScale(2);
            }
        }

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.PlayerName).HasMaxLength(100);
            entity.Property(c => c.Class).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Race).HasMaxLength(50);
            entity.Property(c => c.Background).HasMaxLength(100);
            entity.Property(c => c.Alignment).HasMaxLength(30);
            entity.Property(c => c.PortraitPath).HasMaxLength(500);

            entity.HasMany(c => c.AbilityScores).WithOne()
                .HasForeignKey(a => a.CharacterId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.Skills).WithOne()
                .HasForeignKey(s => s.CharacterId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.Equipment).WithOne(e => e.Character)
                .HasForeignKey(e => e.CharacterId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.Weapons).WithOne(w => w.Character)
                .HasForeignKey(w => w.CharacterId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.Armor).WithOne(a => a.Character)
                .HasForeignKey(a => a.CharacterId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CharacterEquipment>()
            .HasOne(e => e.Item).WithMany().HasForeignKey(e => e.EquipmentItemId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CharacterWeapon>()
            .HasOne(w => w.Weapon).WithMany().HasForeignKey(w => w.WeaponId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CharacterArmor>()
            .HasOne(a => a.Armor).WithMany().HasForeignKey(a => a.ArmorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbilityScore>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.CharacterId, a.Ability }).IsUnique();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(s => new { s.CharacterId, s.Name }).IsUnique();
        });

        modelBuilder.Entity<Weapon>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
            entity.Property(w => w.DamageDice).IsRequired().HasMaxLength(20);
            entity.Property(w => w.DamageType).IsRequired().HasMaxLength(20);
            entity.Property(w => w.Properties).HasMaxLength(500);
            entity.HasMany(w => w.WeaponProperties).WithMany(p => p.Weapons);
        });

        modelBuilder.Entity<Armor>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.ArmorClassFormula).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Category).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<EquipmentItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Category).HasMaxLength(50);
        });

        modelBuilder.Entity<Enemy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatureType).HasMaxLength(50);
            entity.Property(e => e.ChallengeRating).HasMaxLength(20);
            entity.HasMany(e => e.Attacks).WithOne().HasForeignKey(a => a.EnemyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.SpecialAbilities).WithOne().HasForeignKey(a => a.EnemyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Participants).WithOne().HasForeignKey(p => p.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.CombatLog).WithOne().HasForeignKey(log => log.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CombatParticipant>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Type).HasConversion<string>();
            entity.HasIndex(p => new { p.EncounterId, p.Initiative });
        });

        SeedLookupData(modelBuilder);
    }

    private static void SeedLookupData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DamageType>().HasData(
            Enum.GetValues<DamageTypeEnum>()
                .Select(value => new DamageType { Id = (int)value, Name = value.ToString() }));

        modelBuilder.Entity<Condition>().HasData(
            Enum.GetValues<ConditionEnum>()
                .Select(value => new Condition { Id = (int)value, Name = value.ToString() }));

        modelBuilder.Entity<WeaponProperty>().HasData(
            Enum.GetValues<WeaponPropertyEnum>()
                .Select(value => new WeaponProperty { Id = (int)value, Name = value.ToString() }));
    }
}

/// <summary>Small app-specific factory, separate from EF Core's generic IDbContextFactory.</summary>
public interface IAppDbContextFactory
{
    AppDbContext CreateDbContext();
}

public sealed class AppDbContextFactory : IAppDbContextFactory
{
    public AppDbContext CreateDbContext() => new();
}
