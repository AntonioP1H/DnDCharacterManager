using System;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Equipment;
using Microsoft.EntityFrameworkCore;

namespace DnDCharacterManager.Services.Data;

/// <summary>
/// Apply this configuration from the existing AppDbContext.OnModelCreating method.
/// </summary>
public static class Phase2ModelConfiguration
{
    public static void ConfigurePhase2(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<CharacterClass>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Background>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Spell>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(x => new { x.Name, x.Level }).IsUnique();
        });

        modelBuilder.Entity<CharacterSpell>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.CharacterId, x.SpellId }).IsUnique();
            entity.HasOne<Character>()
                .WithMany()
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Spell>()
                .WithMany()
                .HasForeignKey(x => x.SpellId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
