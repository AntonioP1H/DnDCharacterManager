using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DnDCharacterManager.Services.Data;

/// <summary>
/// Idempotently creates the Phase 2 tables for existing SQLite databases.
/// Keep this in sync with the Phase 2 entities and Phase2ModelConfiguration.
/// </summary>
public static class Phase2SchemaInitializer
{
    private static readonly string[] Statements =
    {
        """
        CREATE TABLE IF NOT EXISTS "CharacterClasses" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_CharacterClasses" PRIMARY KEY AUTOINCREMENT,
            "Name" TEXT NOT NULL,
            "HitDie" INTEGER NOT NULL,
            "PrimaryAbility" TEXT NOT NULL,
            "SavingThrows" TEXT NOT NULL,
            "SkillChoices" TEXT NOT NULL,
            "SkillCount" INTEGER NOT NULL,
            "ArmorProficiencies" TEXT NOT NULL,
            "WeaponProficiencies" TEXT NOT NULL,
            "ToolProficiencies" TEXT NOT NULL,
            "ClassFeatures" TEXT NOT NULL,
            "Subclasses" TEXT NOT NULL,
            "SpellcastingAbility" TEXT NOT NULL,
            "SpellcastingLevel" INTEGER NOT NULL,
            "IsOfficial" INTEGER NOT NULL,
            "SourceBook" TEXT NULL,
            "SourcePage" INTEGER NULL,
            "Description" TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS "Races" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Races" PRIMARY KEY AUTOINCREMENT,
            "Name" TEXT NOT NULL,
            "Size" TEXT NOT NULL,
            "Speed" INTEGER NOT NULL,
            "AbilityBonuses" TEXT NOT NULL,
            "Traits" TEXT NOT NULL,
            "Languages" TEXT NOT NULL,
            "Subraces" TEXT NOT NULL,
            "SubraceTraits" TEXT NOT NULL,
            "IsOfficial" INTEGER NOT NULL,
            "SourceBook" TEXT NULL,
            "SourcePage" INTEGER NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS "Backgrounds" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Backgrounds" PRIMARY KEY AUTOINCREMENT,
            "Name" TEXT NOT NULL,
            "SkillProficiencies" TEXT NOT NULL,
            "Languages" INTEGER NOT NULL,
            "ToolProficiencies" TEXT NOT NULL,
            "Equipment" TEXT NOT NULL,
            "Feature" TEXT NOT NULL,
            "FeatureDescription" TEXT NOT NULL,
            "PersonalityTraits" TEXT NOT NULL,
            "Ideals" TEXT NOT NULL,
            "Bonds" TEXT NOT NULL,
            "Flaws" TEXT NOT NULL,
            "IsOfficial" INTEGER NOT NULL,
            "SourceBook" TEXT NULL,
            "SourcePage" INTEGER NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS "Spells" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Spells" PRIMARY KEY AUTOINCREMENT,
            "Name" TEXT NOT NULL,
            "Level" INTEGER NOT NULL,
            "School" TEXT NOT NULL,
            "CastingTime" TEXT NOT NULL,
            "Range" TEXT NOT NULL,
            "Components" TEXT NOT NULL,
            "MaterialComponent" TEXT NOT NULL,
            "Duration" TEXT NOT NULL,
            "Concentration" INTEGER NOT NULL,
            "Ritual" INTEGER NOT NULL,
            "Description" TEXT NOT NULL,
            "HigherLevels" TEXT NOT NULL,
            "Classes" TEXT NOT NULL,
            "Subclasses" TEXT NOT NULL,
            "IsOfficial" INTEGER NOT NULL,
            "SourceBook" TEXT NULL,
            "SourcePage" INTEGER NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS "CharacterSpells" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_CharacterSpells" PRIMARY KEY AUTOINCREMENT,
            "CharacterId" INTEGER NOT NULL,
            "SpellId" INTEGER NOT NULL,
            "IsPrepared" INTEGER NOT NULL,
            "IsKnown" INTEGER NOT NULL,
            "SlotsUsed" INTEGER NOT NULL,
            CONSTRAINT "FK_CharacterSpells_Characters_CharacterId"
                FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_CharacterSpells_Spells_SpellId"
                FOREIGN KEY ("SpellId") REFERENCES "Spells" ("Id") ON DELETE CASCADE
        );
        """,
        "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CharacterClasses_Name\" ON \"CharacterClasses\" (\"Name\");",
        "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Races_Name\" ON \"Races\" (\"Name\");",
        "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Backgrounds_Name\" ON \"Backgrounds\" (\"Name\");",
        "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Spells_Name_Level\" ON \"Spells\" (\"Name\", \"Level\");",
        "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CharacterSpells_CharacterId_SpellId\" ON \"CharacterSpells\" (\"CharacterId\", \"SpellId\");"
    };

    public static async Task EnsurePhase2SchemaAsync(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);

        foreach (string statement in Statements)
            await db.Database.ExecuteSqlRawAsync(statement);
    }
}
