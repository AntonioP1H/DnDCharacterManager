using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Enemies;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UglyToad.PdfPig;

namespace DnDCharacterManager.Services.Import;

/// <summary>
/// Reads the selected PDF and seeds a small, curated starter catalogue.
/// It does not scrape arbitrary PDF layouts. See README.md before using this with a production database.
/// </summary>
public sealed class RulebookImporter : IRulebookImporter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RulebookImporter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task ImportBasicRulesAsync(string pdfPath, IProgress<string>? progress = null)
    {
        progress?.Report($"Opening Basic Rules PDF: {Path.GetFileName(pdfPath)}");
        var extractedCharacters = ExtractPdfText(pdfPath);
        ReportExtractionResult(extractedCharacters, progress);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedClassesAsync(db, progress);
        await SeedRacesAsync(db, progress);
        await SeedBackgroundsAsync(db, progress);
        await SeedSpellsAsync(db, progress);

        progress?.Report("Basic Rules starter catalogue import complete.");
    }

    public async Task ImportMonsterManualAsync(string pdfPath, IProgress<string>? progress = null)
    {
        progress?.Report($"Opening Monster Manual PDF: {Path.GetFileName(pdfPath)}");
        var extractedCharacters = ExtractPdfText(pdfPath);
        ReportExtractionResult(extractedCharacters, progress);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedMonsterExamplesAsync(db, progress);

        progress?.Report("Monster starter subset import complete (3 example creatures; the PDF was not parsed into stat blocks).");
    }

    public Task ImportPhb2eAsync(string pdfPath, IProgress<string>? progress = null)
    {
        progress?.Report($"Opening 2e Player's Handbook for reference: {Path.GetFileName(pdfPath)}");
        var extractedCharacters = ExtractPdfText(pdfPath);
        ReportExtractionResult(extractedCharacters, progress);
        progress?.Report("2e PHB was checked for reference only; no 2e rules were inserted into the 5e database.");
        return Task.CompletedTask;
    }

    private static int ExtractPdfText(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
            throw new ArgumentException("A PDF path is required.", nameof(pdfPath));

        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("The selected rulebook PDF could not be found.", pdfPath);

        var characterCount = 0;
        using var document = PdfDocument.Open(pdfPath);

        foreach (var page in document.GetPages())
            characterCount += page.Text?.Length ?? 0;

        return characterCount;
    }

    private static void ReportExtractionResult(int characterCount, IProgress<string>? progress)
    {
        progress?.Report($"PDF text extraction finished: {characterCount:N0} characters.");
        if (characterCount == 0)
            progress?.Report("Warning: no selectable text was found. The PDF may be scanned; OCR is not performed.");
        progress?.Report("Note: extracted PDF text is not automatically converted into rules or stat blocks.");
    }

    private static async Task SeedClassesAsync(AppDbContext db, IProgress<string>? progress)
    {
        var classes = new[]
        {
            new CharacterClass { Name = "Barbarian", HitDie = 12, PrimaryAbility = "Strength", SavingThrows = Json(new[] { "Strength", "Constitution" }), SkillChoices = Json(new[] { "Animal Handling", "Athletics", "Intimidation", "Nature", "Perception", "Survival" }), SkillCount = 2, SourceBook = "PHB", IsOfficial = true, Description = "A fierce warrior who can enter a battle rage." },
            new CharacterClass { Name = "Bard", HitDie = 8, PrimaryAbility = "Charisma", SavingThrows = Json(new[] { "Dexterity", "Charisma" }), SkillChoices = Json(new[] { "Acrobatics", "Animal Handling", "Arcana", "Athletics", "Deception", "History", "Insight", "Intimidation", "Investigation", "Medicine", "Nature", "Perception", "Performance", "Persuasion", "Religion", "Sleight of Hand", "Stealth", "Survival" }), SkillCount = 3, SourceBook = "PHB", IsOfficial = true, Description = "An inspiring performer whose magic is woven through music and words." },
            new CharacterClass { Name = "Cleric", HitDie = 8, PrimaryAbility = "Wisdom", SavingThrows = Json(new[] { "Wisdom", "Charisma" }), SkillChoices = Json(new[] { "History", "Insight", "Medicine", "Persuasion", "Religion" }), SkillCount = 2, SourceBook = "PHB", IsOfficial = true, Description = "A champion empowered by divine magic." },
            new CharacterClass { Name = "Druid", HitDie = 8, PrimaryAbility = "Wisdom", SavingThrows = Json(new[] { "Intelligence", "Wisdom" }), SkillChoices = Json(new[] { "Arcana", "Animal Handling", "Insight", "Medicine", "Nature", "Perception", "Religion", "Survival" }), SkillCount = 2, SourceBook = "PHB", IsOfficial = true, Description = "A nature-focused spellcaster and guardian of the natural world." },
            new CharacterClass { Name = "Fighter", HitDie = 10, PrimaryAbility = "Strength or Dexterity", SavingThrows = Json(new[] { "Strength", "Constitution" }), SkillChoices = Json(new[] { "Acrobatics", "Animal Handling", "Athletics", "History", "Insight", "Intimidation", "Perception", "Survival" }), SkillCount = 2, SourceBook = "PHB", IsOfficial = true, Description = "A versatile master of martial combat." },
            new CharacterClass { Name = "Monk", HitDie = 8, PrimaryAbility = "Dexterity and Wisdom", SavingThrows = Json(new[] { "Strength", "Dexterity" }), SkillChoices = Json(new[] { "Acrobatics", "Athletics", "History", "Insight", "Religion", "Stealth" }), SkillCount = 2, SourceBook = "PHB", IsOfficial = true, Description = "A disciplined martial artist who harnesses inner power." },
            new CharacterClass { Name = "Paladin", HitDie = 10, PrimaryAbility = "Strength and Charisma", SavingThrows = Json(new[] { "Wisdom", "Charisma" }), SkillChoices = Json(new[] { "Athletics", "Insight", "Intimidation", "Medicine", "Persuasion", "Religion" }), SkillCount = 2, SourceBook = "PHB", IsOfficial = true, Description = "A holy warrior bound by a sacred oath." },
            new CharacterClass { Name = "Ranger", HitDie = 10, PrimaryAbility = "Dexterity and Wisdom", SavingThrows = Json(new[] { "Strength", "Dexterity" }), SkillChoices = Json(new[] { "Animal Handling", "Athletics", "Insight", "Investigation", "Nature", "Perception", "Stealth", "Survival" }), SkillCount = 3, SourceBook = "PHB", IsOfficial = true, Description = "A warrior skilled at surviving and fighting in the wild." },
            new CharacterClass { Name = "Rogue", HitDie = 8, PrimaryAbility = "Dexterity", SavingThrows = Json(new[] { "Dexterity", "Intelligence" }), SkillChoices = Json(new[] { "Acrobatics", "Athletics", "Deception", "Insight", "Intimidation", "Investigation", "Perception", "Performance", "Persuasion", "Sleight of Hand", "Stealth" }), SkillCount = 4, SourceBook = "PHB", IsOfficial = true, Description = "A specialist in stealth, precision, and trickery." },
            new CharacterClass { Name = "Sorcerer", HitDie = 6, PrimaryAbility = "Charisma", SavingThrows = Json(new[] { "Constitution", "Charisma" }), SkillChoices = Json(new[] { "Arcana", "Deception", "Insight", "Intimidation", "Persuasion", "Religion" }), SkillCount = 2, SpellcastingAbility = "Charisma", SpellcastingLevel = 1, SourceBook = "PHB", IsOfficial = true, Description = "A spellcaster with innate magical power." },
            new CharacterClass { Name = "Warlock", HitDie = 8, PrimaryAbility = "Charisma", SavingThrows = Json(new[] { "Wisdom", "Charisma" }), SkillChoices = Json(new[] { "Arcana", "Deception", "History", "Intimidation", "Investigation", "Nature", "Religion" }), SkillCount = 2, SpellcastingAbility = "Charisma", SpellcastingLevel = 1, SourceBook = "PHB", IsOfficial = true, Description = "A spellcaster whose magic comes from a pact with a powerful entity." },
            new CharacterClass { Name = "Wizard", HitDie = 6, PrimaryAbility = "Intelligence", SavingThrows = Json(new[] { "Intelligence", "Wisdom" }), SkillChoices = Json(new[] { "Arcana", "History", "Insight", "Investigation", "Medicine", "Religion" }), SkillCount = 2, SpellcastingAbility = "Intelligence", SpellcastingLevel = 1, SourceBook = "PHB", IsOfficial = true, Description = "A scholarly spellcaster who studies arcane magic." }
        };

        var set = db.Set<CharacterClass>();
        var existing = await set.AsNoTracking().Select(x => x.Name).ToListAsync();
        var names = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var newItems = classes.Where(item => names.Add(item.Name)).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Classes: added {newItems.Length}; already present {classes.Length - newItems.Length}.");
    }

    private static async Task SeedRacesAsync(AppDbContext db, IProgress<string>? progress)
    {
        var races = new[]
        {
            new Race { Name = "Dragonborn", Size = "Medium", Speed = 30, AbilityBonuses = Json(new Dictionary<string, int> { ["Strength"] = 2, ["Charisma"] = 1 }), Traits = Json(new[] { "Draconic Ancestry", "Breath Weapon", "Damage Resistance" }), Languages = Json(new[] { "Common", "Draconic" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Dwarf", Size = "Medium", Speed = 25, AbilityBonuses = Json(new Dictionary<string, int> { ["Constitution"] = 2 }), Traits = Json(new[] { "Darkvision", "Dwarven Resilience", "Dwarven Combat Training", "Stonecunning" }), Languages = Json(new[] { "Common", "Dwarvish" }), Subraces = Json(new[] { "Hill Dwarf", "Mountain Dwarf" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Elf", Size = "Medium", Speed = 30, AbilityBonuses = Json(new Dictionary<string, int> { ["Dexterity"] = 2 }), Traits = Json(new[] { "Darkvision", "Keen Senses", "Fey Ancestry", "Trance" }), Languages = Json(new[] { "Common", "Elvish" }), Subraces = Json(new[] { "High Elf", "Wood Elf", "Drow" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Gnome", Size = "Small", Speed = 25, AbilityBonuses = Json(new Dictionary<string, int> { ["Intelligence"] = 2 }), Traits = Json(new[] { "Darkvision", "Gnome Cunning" }), Languages = Json(new[] { "Common", "Gnomish" }), Subraces = Json(new[] { "Forest Gnome", "Rock Gnome" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Half-Elf", Size = "Medium", Speed = 30, AbilityBonuses = Json(new Dictionary<string, int> { ["Charisma"] = 2 }), Traits = Json(new[] { "Darkvision", "Fey Ancestry", "Skill Versatility" }), Languages = Json(new[] { "Common", "Elvish", "Choice" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Half-Orc", Size = "Medium", Speed = 30, AbilityBonuses = Json(new Dictionary<string, int> { ["Strength"] = 2, ["Constitution"] = 1 }), Traits = Json(new[] { "Darkvision", "Menacing", "Relentless Endurance", "Savage Attacks" }), Languages = Json(new[] { "Common", "Orc" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Halfling", Size = "Small", Speed = 25, AbilityBonuses = Json(new Dictionary<string, int> { ["Dexterity"] = 2 }), Traits = Json(new[] { "Lucky", "Brave", "Halfling Nimbleness" }), Languages = Json(new[] { "Common", "Halfling" }), Subraces = Json(new[] { "Lightfoot Halfling", "Stout Halfling" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Human", Size = "Medium", Speed = 30, AbilityBonuses = Json(new Dictionary<string, int> { ["All"] = 1 }), Traits = "[]", Languages = Json(new[] { "Common", "Choice" }), Subraces = Json(new[] { "Variant Human" }), SourceBook = "PHB", IsOfficial = true },
            new Race { Name = "Tiefling", Size = "Medium", Speed = 30, AbilityBonuses = Json(new Dictionary<string, int> { ["Charisma"] = 2, ["Intelligence"] = 1 }), Traits = Json(new[] { "Darkvision", "Hellish Resistance", "Infernal Legacy" }), Languages = Json(new[] { "Common", "Infernal" }), SourceBook = "PHB", IsOfficial = true }
        };

        var set = db.Set<Race>();
        var existing = await set.AsNoTracking().Select(x => x.Name).ToListAsync();
        var names = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var newItems = races.Where(item => names.Add(item.Name)).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Races: added {newItems.Length}; already present {races.Length - newItems.Length}.");
    }

    private static async Task SeedBackgroundsAsync(AppDbContext db, IProgress<string>? progress)
    {
        var backgrounds = new[]
        {
            new Background { Name = "Acolyte", SkillProficiencies = Json(new[] { "Insight", "Religion" }), Languages = 2, ToolProficiencies = "[]", Equipment = Json(new[] { "Holy symbol", "Prayer book", "Incense", "Vestments", "Common clothes", "15 gp" }), Feature = "Shelter of the Faithful", FeatureDescription = "Members of your faith may provide shelter and basic assistance." },
            new Background { Name = "Charlatan", SkillProficiencies = Json(new[] { "Deception", "Sleight of Hand" }), ToolProficiencies = Json(new[] { "Disguise kit", "Forgery kit" }), Equipment = Json(new[] { "Fine clothes", "Disguise kit", "Con tools", "15 gp" }), Feature = "False Identity", FeatureDescription = "You have created a second identity with supporting acquaintances and documentation." },
            new Background { Name = "Criminal", SkillProficiencies = Json(new[] { "Deception", "Stealth" }), ToolProficiencies = Json(new[] { "Gaming set", "Thieves' tools" }), Equipment = Json(new[] { "Crowbar", "Dark common clothes", "Thieves' tools", "15 gp" }), Feature = "Criminal Contact", FeatureDescription = "You have a reliable contact who connects you to a criminal network." },
            new Background { Name = "Entertainer", SkillProficiencies = Json(new[] { "Acrobatics", "Performance" }), ToolProficiencies = Json(new[] { "Disguise kit", "Musical instrument" }), Equipment = Json(new[] { "Musical instrument", "Admirer's favor", "Costume", "15 gp" }), Feature = "By Popular Demand", FeatureDescription = "You can usually find a place to perform and receive modest hospitality." },
            new Background { Name = "Folk Hero", SkillProficiencies = Json(new[] { "Animal Handling", "Survival" }), ToolProficiencies = Json(new[] { "Artisan's tools", "Vehicles (land)" }), Equipment = Json(new[] { "Artisan's tools", "Shovel", "Iron pot", "Common clothes", "10 gp" }), Feature = "Rustic Hospitality", FeatureDescription = "Common folk are willing to shelter and help you." },
            new Background { Name = "Guild Artisan", SkillProficiencies = Json(new[] { "Insight", "Persuasion" }), Languages = 1, ToolProficiencies = Json(new[] { "Artisan's tools" }), Equipment = Json(new[] { "Artisan's tools", "Guild introduction", "Traveler's clothes", "15 gp" }), Feature = "Guild Membership", FeatureDescription = "Your guild can provide connections and limited support." },
            new Background { Name = "Hermit", SkillProficiencies = Json(new[] { "Medicine", "Religion" }), Languages = 1, ToolProficiencies = Json(new[] { "Herbalism kit" }), Equipment = Json(new[] { "Herbalism kit", "Notes", "Winter blanket", "Common clothes", "5 gp" }), Feature = "Discovery", FeatureDescription = "Your seclusion led to a unique and important discovery." },
            new Background { Name = "Noble", SkillProficiencies = Json(new[] { "History", "Persuasion" }), Languages = 1, ToolProficiencies = Json(new[] { "Gaming set" }), Equipment = Json(new[] { "Fine clothes", "Signet ring", "Pedigree scroll", "25 gp" }), Feature = "Position of Privilege", FeatureDescription = "You are welcome in high society and can usually secure an audience." },
            new Background { Name = "Outlander", SkillProficiencies = Json(new[] { "Athletics", "Survival" }), Languages = 1, ToolProficiencies = Json(new[] { "Musical instrument" }), Equipment = Json(new[] { "Staff", "Hunting trap", "Animal trophy", "Traveler's clothes", "10 gp" }), Feature = "Wanderer", FeatureDescription = "You have an excellent memory for geography and can forage in the wild." },
            new Background { Name = "Sage", SkillProficiencies = Json(new[] { "Arcana", "History" }), Languages = 2, ToolProficiencies = "[]", Equipment = Json(new[] { "Ink", "Quill", "Small knife", "Colleague's letter", "Common clothes", "10 gp" }), Feature = "Researcher", FeatureDescription = "You know how to find information and identify likely sources of lore." },
            new Background { Name = "Sailor", SkillProficiencies = Json(new[] { "Athletics", "Perception" }), ToolProficiencies = Json(new[] { "Navigator's tools", "Vehicles (water)" }), Equipment = Json(new[] { "Belaying pin", "Silk rope", "Lucky charm", "Common clothes", "10 gp" }), Feature = "Ship's Passage", FeatureDescription = "You can often secure passage on a sailing ship." },
            new Background { Name = "Soldier", SkillProficiencies = Json(new[] { "Athletics", "Intimidation" }), ToolProficiencies = Json(new[] { "Gaming set", "Vehicles (land)" }), Equipment = Json(new[] { "Rank insignia", "Enemy trophy", "Bone dice", "Common clothes", "10 gp" }), Feature = "Military Rank", FeatureDescription = "Soldiers from your former organization recognize your rank and influence." },
            new Background { Name = "Urchin", SkillProficiencies = Json(new[] { "Sleight of Hand", "Stealth" }), ToolProficiencies = Json(new[] { "Disguise kit", "Thieves' tools" }), Equipment = Json(new[] { "Small knife", "City map", "Pet mouse", "Token from your past", "Common clothes", "10 gp" }), Feature = "City Secrets", FeatureDescription = "You know hidden routes and shortcuts through urban areas." }
        };

        foreach (var background in backgrounds)
        {
            background.SourceBook = "PHB";
            background.IsOfficial = true;
        }

        var set = db.Set<Background>();
        var existing = await set.AsNoTracking().Select(x => x.Name).ToListAsync();
        var names = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var newItems = backgrounds.Where(item => names.Add(item.Name)).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Backgrounds: added {newItems.Length}; already present {backgrounds.Length - newItems.Length}.");
    }

    private static async Task SeedSpellsAsync(AppDbContext db, IProgress<string>? progress)
    {
        var spells = new[]
        {
            SpellEntry("Fire Bolt", 0, "Evocation", "1 action", "120 ft", "V, S", "Instantaneous", "Make a ranged spell attack. On a hit, the target takes 1d10 fire damage.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Guidance", 0, "Divination", "1 action", "Touch", "V, S", "Concentration, up to 1 minute", "The target can add 1d4 to one ability check before the spell ends.", new[] { "Cleric", "Druid" }, concentration: true),
            SpellEntry("Light", 0, "Evocation", "1 action", "Touch", "V, M", "1 hour", "An object sheds bright light in a 20-foot radius and dim light for another 20 feet.", new[] { "Bard", "Cleric", "Sorcerer", "Wizard" }),
            SpellEntry("Mage Hand", 0, "Conjuration", "1 action", "30 ft", "V, S", "1 minute", "A spectral hand manipulates or carries objects within the spell's limits.", new[] { "Bard", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Vicious Mockery", 0, "Enchantment", "1 action", "60 ft", "V", "Instantaneous", "The target makes a Wisdom saving throw or takes psychic damage and has disadvantage on its next attack roll.", new[] { "Bard" }),
            SpellEntry("Bless", 1, "Enchantment", "1 action", "30 ft", "V, S, M", "Concentration, up to 1 minute", "Up to three creatures add 1d4 to attack rolls and saving throws.", new[] { "Cleric", "Paladin" }, concentration: true),
            SpellEntry("Cure Wounds", 1, "Evocation", "1 action", "Touch", "V, S", "Instantaneous", "A creature you touch regains hit points.", new[] { "Bard", "Cleric", "Druid", "Paladin", "Ranger" }),
            SpellEntry("Magic Missile", 1, "Evocation", "1 action", "120 ft", "V, S", "Instantaneous", "You create three darts of magical force that each deal force damage and automatically hit.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Shield", 1, "Abjuration", "1 reaction", "Self", "V, S", "1 round", "An invisible barrier grants a +5 bonus to AC and prevents damage from Magic Missile.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Fireball", 3, "Evocation", "1 action", "150 ft", "V, S, M", "Instantaneous", "A fiery explosion fills a 20-foot-radius sphere; creatures make a Dexterity saving throw, taking fire damage on a failure.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Counterspell", 3, "Abjuration", "1 reaction", "60 ft", "S", "Instantaneous", "You attempt to interrupt a creature in the process of casting a spell.", new[] { "Sorcerer", "Warlock", "Wizard" })
        };

        var set = db.Set<Spell>();
        var existing = await set.AsNoTracking().Select(x => new { x.Name, x.Level }).ToListAsync();
        var keys = new HashSet<string>(existing.Select(x => SpellKey(x.Name, x.Level)), StringComparer.OrdinalIgnoreCase);
        var newItems = spells.Where(item => keys.Add(SpellKey(item.Name, item.Level))).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Spells: added {newItems.Length}; already present {spells.Length - newItems.Length}.");
    }

    private static Spell SpellEntry(string name, int level, string school, string castingTime,
        string range, string components, string duration, string description, string[] classes,
        bool concentration = false)
    {
        return new Spell
        {
            Name = name,
            Level = level,
            School = school,
            CastingTime = castingTime,
            Range = range,
            Components = components,
            Duration = duration,
            Concentration = concentration,
            Description = description,
            Classes = Json(classes),
            SourceBook = "PHB",
            IsOfficial = true
        };
    }

    private static async Task SeedMonsterExamplesAsync(AppDbContext db, IProgress<string>? progress)
    {
        var monsters = new[] { CreateGoblin(), CreateSkeleton(), CreateZombie() };
        var set = db.Set<Enemy>();
        var existing = await set.AsNoTracking().Where(x => x.IsOfficial).Select(x => x.Name).ToListAsync();
        var names = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var newItems = monsters.Where(item => names.Add(item.Name)).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Monster examples: added {newItems.Length}; already present {monsters.Length - newItems.Length}.");
    }

    private static Enemy CreateGoblin() => new()
    {
        Name = "Goblin", CreatureType = "Humanoid (goblinoid)", ChallengeRating = "1/4", ProficiencyBonus = 2,
        Strength = 8, Dexterity = 14, Constitution = 10, Intelligence = 10, Wisdom = 8, Charisma = 8,
        MaxHitPoints = 7, HitDice = "2d6", ArmorClass = 15, ArmorType = "Leather armor, shield", SpeedWalk = 30,
        SavingThrowProficiencies = "[]", SkillProficiencies = Json(new[] { new { name = "Stealth", bonus = 6 } }),
        DamageResistances = "[]", DamageImmunities = "[]", DamageVulnerabilities = "[]", ConditionImmunities = "[]",
        PassivePerception = 9, Senses = "Darkvision 60 ft., passive Perception 9", Languages = Json(new[] { "Common", "Goblin" }),
        IsOfficial = true, SourceBook = "MM", SourcePage = 165,
        Attacks = new List<EnemyAttack>
        {
            new() { Name = "Scimitar", Type = AttackType.MeleeWeapon, Reach = 5, AttackBonus = 4, DamageDice = "1d6+2", DamageType = "Slashing" },
            new() { Name = "Shortbow", Type = AttackType.RangedWeapon, RangeNormal = 80, RangeLong = 320, AttackBonus = 4, DamageDice = "1d6+2", DamageType = "Piercing" }
        }
    };

    private static Enemy CreateSkeleton() => new()
    {
        Name = "Skeleton", CreatureType = "Undead", ChallengeRating = "1/4", ProficiencyBonus = 2,
        Strength = 10, Dexterity = 14, Constitution = 15, Intelligence = 6, Wisdom = 8, Charisma = 5,
        MaxHitPoints = 13, HitDice = "2d8+4", ArmorClass = 13, ArmorType = "Armor scraps", SpeedWalk = 30,
        SavingThrowProficiencies = "[]", SkillProficiencies = "[]", DamageResistances = "[]",
        DamageImmunities = Json(new[] { "Poison" }), DamageVulnerabilities = Json(new[] { "Bludgeoning" }),
        ConditionImmunities = Json(new[] { "Exhaustion", "Poisoned" }), PassivePerception = 9,
        Senses = "Darkvision 60 ft., passive Perception 9", Languages = Json(new[] { "Understands languages known in life" }),
        IsOfficial = true, SourceBook = "MM", SourcePage = 272,
        Attacks = new List<EnemyAttack>
        {
            new() { Name = "Shortsword", Type = AttackType.MeleeWeapon, Reach = 5, AttackBonus = 4, DamageDice = "1d6+2", DamageType = "Piercing" },
            new() { Name = "Shortbow", Type = AttackType.RangedWeapon, RangeNormal = 80, RangeLong = 320, AttackBonus = 4, DamageDice = "1d6+2", DamageType = "Piercing" }
        }
    };

    private static Enemy CreateZombie() => new()
    {
        Name = "Zombie", CreatureType = "Undead", ChallengeRating = "1/4", ProficiencyBonus = 2,
        Strength = 13, Dexterity = 6, Constitution = 16, Intelligence = 3, Wisdom = 6, Charisma = 5,
        MaxHitPoints = 22, HitDice = "3d8+9", ArmorClass = 8, ArmorType = "None", SpeedWalk = 20,
        SavingThrowProficiencies = "[]", SkillProficiencies = "[]", DamageResistances = "[]",
        DamageImmunities = Json(new[] { "Poison" }), DamageVulnerabilities = "[]",
        ConditionImmunities = Json(new[] { "Poisoned" }), PassivePerception = 8,
        Senses = "Darkvision 60 ft., passive Perception 8", Languages = "[]",
        IsOfficial = true, SourceBook = "MM", SourcePage = 315,
        Attacks = new List<EnemyAttack>
        {
            new() { Name = "Slam", Type = AttackType.MeleeWeapon, Reach = 5, AttackBonus = 3, DamageDice = "1d6+1", DamageType = "Bludgeoning" }
        }
    };

    private static string Json<T>(T value) => JsonSerializer.Serialize(value);

    private static string SpellKey(string name, int level) => $"{name.Trim().ToUpperInvariant()}|{level}";
}
