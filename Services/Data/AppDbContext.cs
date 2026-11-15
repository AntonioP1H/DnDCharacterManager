using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Enemies;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Models.Combat;

namespace DnDCharacterManager.Services.Data;

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
            string path = GetDatabasePath();
            optionsBuilder.UseSqlite($"Data Source={path}");
#if DEBUG
            optionsBuilder.EnableDetailedErrors();
#endif
        }
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

        ConfigureCharacters(modelBuilder);
        ConfigureEquipment(modelBuilder);
        ConfigureEnemies(modelBuilder);
        ConfigureCombat(modelBuilder);
        SeedLookupData(modelBuilder);
    }

    private static void ConfigureCharacters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.ToTable("Characters");
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

        modelBuilder.Entity<AbilityScore>(entity =>
        {
            entity.ToTable("AbilityScores");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Ability).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(a => new { a.CharacterId, a.Ability }).IsUnique();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Ability).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(s => new { s.CharacterId, s.Name }).IsUnique();
        });

        modelBuilder.Entity<CharacterEquipment>(entity =>
        {
            entity.ToTable("CharacterEquipment");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.EquipmentItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CharacterWeapon>(entity =>
        {
            entity.ToTable("CharacterWeapons");
            entity.HasKey(w => w.Id);
            entity.HasOne(w => w.Weapon).WithMany().HasForeignKey(w => w.WeaponId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CharacterArmor>(entity =>
        {
            entity.ToTable("CharacterArmor");
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Armor).WithMany().HasForeignKey(a => a.ArmorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEquipment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Weapon>(entity =>
        {
            entity.ToTable("Weapons");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
            entity.Property(w => w.DamageDice).IsRequired().HasMaxLength(20);
            entity.Property(w => w.DamageType).IsRequired().HasMaxLength(20);
            entity.Property(w => w.Properties).HasMaxLength(500);
            entity.HasMany(w => w.WeaponProperties).WithMany(p => p.Weapons);
        });

        modelBuilder.Entity<Armor>(entity =>
        {
            entity.ToTable("Armors");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.ArmorClassFormula).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Category).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<EquipmentItem>(entity =>
        {
            entity.ToTable("EquipmentItems");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Category).HasMaxLength(50);
        });

        modelBuilder.Entity<WeaponProperty>(entity =>
        {
            entity.ToTable("WeaponProperties");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<DamageType>(entity =>
        {
            entity.ToTable("DamageTypes");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Condition>(entity =>
        {
            entity.ToTable("Conditions");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(30);
        });
    }

    private static void ConfigureEnemies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enemy>(entity =>
        {
            entity.ToTable("Enemies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatureType).HasMaxLength(50);
            entity.Property(e => e.ChallengeRating).HasMaxLength(20);
            entity.HasMany(e => e.Attacks).WithOne().HasForeignKey(a => a.EnemyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.SpecialAbilities).WithOne().HasForeignKey(a => a.EnemyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EnemyAttack>(entity =>
        {
            entity.ToTable("EnemyAttacks");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.DamageDice).IsRequired().HasMaxLength(50);
            entity.Property(a => a.DamageType).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Type).HasConversion<string>();
        });

        modelBuilder.Entity<EnemySpecialAbility>(entity =>
        {
            entity.ToTable("EnemySpecialAbilities");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Description).IsRequired().HasMaxLength(2000);
            entity.Property(a => a.Type).HasConversion<string>();
            entity.Property(a => a.Recharge).HasConversion<string>();
        });
    }

    private static void ConfigureCombat(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.ToTable("Encounters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Participants).WithOne().HasForeignKey(p => p.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.CombatLog).WithOne().HasForeignKey(l => l.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CombatParticipant>(entity =>
        {
            entity.ToTable("CombatParticipants");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Type).HasConversion<string>();
            entity.HasIndex(p => new { p.EncounterId, p.Initiative });
        });

        modelBuilder.Entity<CombatLogEntry>(entity =>
        {
            entity.ToTable("CombatLog");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ActorName).IsRequired().HasMaxLength(100);
            entity.Property(l => l.ActionType).IsRequired().HasMaxLength(50);
            entity.Property(l => l.Description).HasMaxLength(1000);
        });
    }

    private static void SeedLookupData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DamageType>().HasData(
            Enum.GetValues<DamageTypeEnum>().Select(v => new DamageType { Id = (int)v, Name = v.ToString() }));
        modelBuilder.Entity<Condition>().HasData(
            Enum.GetValues<ConditionEnum>().Select(v => new Condition { Id = (int)v, Name = v.ToString() }));
        modelBuilder.Entity<WeaponProperty>().HasData(
            Enum.GetValues<WeaponPropertyEnum>().Select(v => new WeaponProperty { Id = (int)v, Name = v.ToString() }));
    }

    private static string GetDatabasePath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string directory = Path.Combine(appData, "DnDCharacterManager");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "data.db");
    }
}
