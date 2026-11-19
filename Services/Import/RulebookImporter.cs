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
/// Validates and reads selectable text from the supplied PDF, then inserts a curated,
/// idempotent starter catalogue. The PDF text itself is not parsed into game rules.
/// The simplified additional monster examples are explicitly marked as unofficial.
/// </summary>
public sealed class RulebookImporter : IRulebookImporter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RulebookImporter(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

    public async Task ImportBasicRulesAsync(string pdfPath, IProgress<string>? progress = null)
    {
        progress?.Report($"Opening Basic Rules PDF: {Path.GetFileName(pdfPath)}");
        int extractedCharacters = await Task.Run(() => ExtractPdfText(pdfPath));
        ReportExtractionResult(extractedCharacters, progress);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            await SeedClassesAsync(db, progress);
            await SeedRacesAsync(db, progress);
            await SeedBackgroundsAsync(db, progress);
            await SeedSpellsAsync(db, progress);
            await transaction.CommitAsync();
        }

        progress?.Report("Basic Rules starter catalogue import complete. PDF text was not automatically parsed.");
        await ReportCatalogueCountsAsync(db, progress);
    }

    public async Task ImportMonsterManualAsync(string pdfPath, IProgress<string>? progress = null)
    {
        progress?.Report($"Opening Monster Manual PDF: {Path.GetFileName(pdfPath)}");
        int extractedCharacters = await Task.Run(() => ExtractPdfText(pdfPath));
        ReportExtractionResult(extractedCharacters, progress);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            await SeedMonsterCatalogAsync(db, progress);
            await transaction.CommitAsync();
        }

        progress?.Report("Monster starter catalogue import complete. Only the 3 detailed examples are official; other starter entries are simplified and unofficial.");
        await ReportCatalogueCountsAsync(db, progress);
    }

    public async Task ImportPhb2eAsync(string pdfPath, IProgress<string>? progress = null)
    {
        progress?.Report($"Opening 2e Player's Handbook for reference: {Path.GetFileName(pdfPath)}");
        int extractedCharacters = await Task.Run(() => ExtractPdfText(pdfPath));
        ReportExtractionResult(extractedCharacters, progress);
        progress?.Report("2e PHB was checked for reference only; no 2e rules were inserted into the 5e database.");
    }

    private static int ExtractPdfText(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
            throw new ArgumentException("A PDF path is required.", nameof(pdfPath));
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("The selected rulebook PDF could not be found.", pdfPath);

        int characterCount = 0;
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
        var spells = new[] {
            SpellEntry("Acid Splash", 0, "Conjuration", "1 action", "60 ft", "V, S", "Instantaneous", "You fling a splash of acid at one or two nearby creatures; each target makes a Dexterity save to avoid the acid damage.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Chill Touch", 0, "Necromancy", "1 action", "120 ft", "V, S", "1 round", "A spectral hand delivers necrotic damage and prevents the target from regaining hit points until your next turn.", new[] { "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Dancing Lights", 0, "Evocation", "1 action", "120 ft", "V, S", "Concentration, up to 1 minute", "Create up to four small lights that can move independently or combine into a faintly humanoid shape.", new[] { "Bard", "Sorcerer", "Wizard" }, concentration: true),
            SpellEntry("Druidcraft", 0, "Transmutation", "1 action", "30 ft", "V, S", "Instantaneous", "Create a minor natural sign, such as a brief weather effect, a flower, or a harmless sensory effect.", new[] { "Druid" }),
            SpellEntry("Eldritch Blast", 0, "Evocation", "1 action", "120 ft", "V, S", "Instantaneous", "Make a ranged spell attack that fires a beam of crackling force at a creature.", new[] { "Warlock" }),
            SpellEntry("Fire Bolt", 0, "Evocation", "1 action", "120 ft", "V, S", "Instantaneous", "Make a ranged spell attack. On a hit, the target takes fire damage.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Guidance", 0, "Divination", "1 action", "Touch", "V, S", "Concentration, up to 1 minute", "The target can add a small die to one ability check before the spell ends.", new[] { "Cleric", "Druid" }, concentration: true),
            SpellEntry("Light", 0, "Evocation", "1 action", "Touch", "V, M", "1 hour", "An object sheds bright light nearby and dim light farther out.", new[] { "Bard", "Cleric", "Sorcerer", "Wizard" }),
            SpellEntry("Mage Hand", 0, "Conjuration", "1 action", "30 ft", "V, S", "1 minute", "A spectral hand manipulates or carries light objects within the spell limits.", new[] { "Bard", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Mending", 0, "Transmutation", "1 minute", "Touch", "V, S, M", "Instantaneous", "Repair one break or tear in a nonmagical object within the spell limits.", new[] { "Bard", "Cleric", "Druid", "Sorcerer", "Wizard" }),
            SpellEntry("Minor Illusion", 0, "Illusion", "1 action", "30 ft", "S, M", "1 minute", "Create a sound or a small image of an object; physical interaction can reveal an image as an illusion.", new[] { "Bard", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Poison Spray", 0, "Conjuration", "1 action", "10 ft", "V, S", "Instantaneous", "A creature in range makes a Constitution save or takes poison damage.", new[] { "Druid", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Prestidigitation", 0, "Transmutation", "1 action", "10 ft", "V, S", "Up to 1 hour", "Create a minor magical effect, such as a harmless sensory effect, a small cleanup, or a brief mark.", new[] { "Bard", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Produce Flame", 0, "Conjuration", "1 action", "Self", "V, S", "10 minutes", "A flame appears in your hand, sheds light, and can be thrown as a ranged spell attack.", new[] { "Druid" }),
            SpellEntry("Sacred Flame", 0, "Evocation", "1 action", "60 ft", "V, S", "Instantaneous", "A creature visible to you makes a Dexterity save or takes radiant damage; cover offers no benefit.", new[] { "Cleric" }),
            SpellEntry("Shocking Grasp", 0, "Evocation", "1 action", "Touch", "V, S", "Instantaneous", "Make a melee spell attack; a hit deals lightning damage and prevents reactions briefly.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Vicious Mockery", 0, "Enchantment", "1 action", "60 ft", "V", "Instantaneous", "A creature that hears your taunt makes a Wisdom save or takes psychic damage and has its next attack hindered.", new[] { "Bard" }),
            SpellEntry("Bless", 1, "Enchantment", "1 action", "30 ft", "V, S, M", "Concentration, up to 1 minute", "Up to three creatures add a small die to attack rolls and saving throws.", new[] { "Cleric", "Paladin" }, concentration: true),
            SpellEntry("Charm Person", 1, "Enchantment", "1 action", "30 ft", "V, S", "1 hour", "A humanoid makes a Wisdom save or regards you as a friendly acquaintance for the duration.", new[] { "Bard", "Druid", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Command", 1, "Enchantment", "1 action", "60 ft", "V", "1 round", "Speak a one-word command; a target that fails a Wisdom save follows it on its next turn.", new[] { "Cleric", "Paladin" }),
            SpellEntry("Cure Wounds", 1, "Evocation", "1 action", "Touch", "V, S", "Instantaneous", "A creature you touch regains hit points.", new[] { "Bard", "Cleric", "Druid", "Paladin", "Ranger" }),
            SpellEntry("Detect Magic", 1, "Divination", "1 action", "Self", "V, S", "Concentration, up to 10 minutes", "Sense magic nearby and, when visible, perceive an aura around a magical source.", new[] { "Bard", "Cleric", "Druid", "Paladin", "Ranger", "Sorcerer", "Wizard" }, concentration: true),
            SpellEntry("Feather Fall", 1, "Transmutation", "1 reaction", "60 ft", "V, M", "1 minute", "Choose falling creatures; their descent slows and they avoid damage from the fall while the spell lasts.", new[] { "Bard", "Sorcerer", "Wizard" }),
            SpellEntry("Healing Word", 1, "Evocation", "1 bonus action", "60 ft", "V", "Instantaneous", "A creature you can see regains a small number of hit points.", new[] { "Bard", "Cleric", "Druid" }),
            SpellEntry("Hunter's Mark", 1, "Divination", "1 bonus action", "90 ft", "V", "Concentration, up to 1 hour", "Mark a creature; your weapon attacks deal extra damage to it and you can more easily track it.", new[] { "Ranger" }, concentration: true),
            SpellEntry("Magic Missile", 1, "Evocation", "1 action", "120 ft", "V, S", "Instantaneous", "Create three force darts that strike creatures of your choice and deal force damage.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Shield", 1, "Abjuration", "1 reaction", "Self", "V, S", "1 round", "A magical barrier briefly increases your Armor Class and blocks the magic missiles spell.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Sleep", 1, "Enchantment", "1 action", "90 ft", "V, S, M", "1 minute", "Creatures in a small area may fall unconscious, beginning with those that have the fewest current hit points.", new[] { "Bard", "Sorcerer", "Wizard" }),
            SpellEntry("Thunderwave", 1, "Evocation", "1 action", "Self (15-ft cube)", "V, S", "Instantaneous", "A wave of thunder damages creatures nearby and can push them away from you.", new[] { "Bard", "Druid", "Sorcerer", "Wizard" }),
            SpellEntry("Hold Person", 2, "Enchantment", "1 action", "60 ft", "V, S, M", "Concentration, up to 1 minute", "A humanoid that fails a Wisdom save becomes paralyzed and can repeat the save at the end of its turns.", new[] { "Bard", "Cleric", "Druid", "Sorcerer", "Warlock", "Wizard" }, concentration: true),
            SpellEntry("Invisibility", 2, "Illusion", "1 action", "Touch", "V, S, M", "Concentration, up to 1 hour", "A creature you touch becomes invisible until the spell ends or it attacks or casts a spell.", new[] { "Bard", "Sorcerer", "Warlock", "Wizard" }, concentration: true),
            SpellEntry("Misty Step", 2, "Conjuration", "1 bonus action", "Self", "V", "Instantaneous", "Briefly wreathed in mist, you teleport to an unoccupied space you can see nearby.", new[] { "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Scorching Ray", 2, "Evocation", "1 action", "120 ft", "V, S", "Instantaneous", "Create three rays of fire and make a ranged spell attack for each ray.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Shatter", 2, "Evocation", "1 action", "60 ft", "V, S, M", "Instantaneous", "A sudden burst of sound damages creatures and objects in a small sphere.", new[] { "Bard", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Spiritual Weapon", 2, "Evocation", "1 bonus action", "60 ft", "V, S", "1 minute", "Create a floating spectral weapon that can strike a creature when you direct it.", new[] { "Cleric" }),
            SpellEntry("Web", 2, "Conjuration", "1 action", "60 ft", "V, S, M", "Concentration, up to 1 hour", "Fill an area with sticky webbing that can restrain creatures and impede movement.", new[] { "Sorcerer", "Wizard" }, concentration: true),
            SpellEntry("Counterspell", 3, "Abjuration", "1 reaction", "60 ft", "S", "Instantaneous", "Attempt to interrupt a creature casting a spell; more powerful spells may require an ability check.", new[] { "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Dispel Magic", 3, "Abjuration", "1 action", "120 ft", "V, S", "Instantaneous", "End a spell affecting a creature, object, or magical effect; stronger magic may require an ability check.", new[] { "Bard", "Cleric", "Druid", "Paladin", "Sorcerer", "Warlock", "Wizard" }),
            SpellEntry("Fireball", 3, "Evocation", "1 action", "150 ft", "V, S, M", "Instantaneous", "A streak of fire erupts into a large sphere; creatures in the area make a Dexterity save against the blast.", new[] { "Sorcerer", "Wizard" }),
            SpellEntry("Lightning Bolt", 3, "Evocation", "1 action", "Self (100-ft line)", "V, S, M", "Instantaneous", "A line of lightning blasts outward; creatures in its path make a Dexterity save.", new[] { "Sorcerer", "Wizard" }),
        };

        var set = db.Set<Spell>();
        var existing = await set.AsNoTracking().Select(x => new { x.Name, x.Level }).ToListAsync();
        var keys = new HashSet<string>(existing.Select(x => SpellKey(x.Name, x.Level)), StringComparer.OrdinalIgnoreCase);
        var newItems = spells.Where(item => keys.Add(SpellKey(item.Name, item.Level))).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Spells: added {newItems.Length}; already present {spells.Length - newItems.Length}. Catalogue target: 40.");
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

    private static async Task SeedMonsterCatalogAsync(AppDbContext db, IProgress<string>? progress)
    {
        var monsters = new List<Enemy> { CreateGoblin(), CreateSkeleton(), CreateZombie() };
        monsters.AddRange(GetStarterMonsterSeeds().Select(CreateStarterMonster));

        var set = db.Set<Enemy>();
        var existing = await set.AsNoTracking().Select(x => x.Name).ToListAsync();
        var names = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var newItems = monsters.Where(item => names.Add(item.Name)).ToArray();
        if (newItems.Length > 0)
            await set.AddRangeAsync(newItems);
        await db.SaveChangesAsync();
        progress?.Report($"Monsters: added {newItems.Length}; already present {monsters.Count - newItems.Length}. Starter catalogue target: 50.");
    }

    private sealed record MonsterSeed(string Name, string CreatureType, string Cr, int ArmorClass, int HitPoints,
        string HitDice, int WalkSpeed, int Strength, int Dexterity, int Constitution, int Intelligence, int Wisdom,
        int Charisma, string AttackName, int AttackBonus, string DamageDice, string DamageType, int FlySpeed = 0);

    private static IEnumerable<MonsterSeed> GetStarterMonsterSeeds() => new MonsterSeed[]
    {
        new("Bandit", "Humanoid", "1/8", 12, 11, "2d8+2", 30, 11, 12, 12, 10, 10, 10, "Scimitar", 3, "1d6+1", "Slashing", 0),
        new("Cultist", "Humanoid", "1/8", 12, 9, "2d8", 30, 11, 12, 10, 10, 11, 10, "Scimitar", 3, "1d6+1", "Slashing", 0),
        new("Wolf", "Beast", "1/4", 13, 11, "2d8+2", 40, 12, 15, 12, 3, 12, 6, "Bite", 4, "2d4+2", "Piercing", 0),
        new("Giant Rat", "Beast", "1/8", 12, 7, "2d6", 30, 7, 15, 11, 2, 10, 4, "Bite", 4, "1d4+2", "Piercing", 0),
        new("Kobold", "Humanoid (kobold)", "1/8", 12, 5, "2d6-2", 30, 7, 15, 9, 8, 7, 8, "Dagger", 4, "1d4+2", "Piercing", 0),
        new("Goblin Boss", "Humanoid (goblinoid)", "1", 16, 21, "6d6", 30, 10, 15, 12, 10, 9, 9, "Scimitar", 4, "1d6+2", "Slashing", 0),
        new("Orc", "Humanoid (orc)", "1/2", 13, 15, "2d8+6", 30, 16, 12, 13, 7, 11, 10, "Greataxe", 5, "1d12+3", "Slashing", 0),
        new("Gnoll", "Humanoid (gnoll)", "1/2", 15, 22, "5d8", 30, 14, 13, 11, 6, 10, 7, "Spear", 4, "1d6+2", "Piercing", 0),
        new("Acolyte", "Humanoid", "1/4", 10, 9, "2d8", 30, 10, 10, 10, 10, 14, 11, "Club", 2, "1d4", "Bludgeoning", 0),
        new("Guard", "Humanoid", "1/8", 16, 11, "2d8+2", 30, 13, 12, 12, 10, 11, 10, "Spear", 3, "1d6+1", "Piercing", 0),
        new("Thug", "Humanoid", "1/2", 11, 32, "5d8+10", 30, 15, 11, 14, 10, 10, 11, "Mace", 4, "1d6+2", "Bludgeoning", 0),
        new("Giant Spider", "Beast", "1", 14, 26, "4d10+4", 30, 14, 16, 12, 4, 12, 6, "Bite", 5, "1d8+3", "Piercing", 0),
        new("Dire Wolf", "Beast", "1", 14, 37, "5d10+10", 50, 17, 15, 15, 3, 12, 7, "Bite", 5, "2d6+3", "Piercing", 0),
        new("Bugbear", "Humanoid (goblinoid)", "1", 16, 27, "5d8+5", 30, 15, 14, 13, 8, 11, 9, "Morningstar", 4, "2d8+2", "Piercing", 0),
        new("Ogre", "Giant", "2", 11, 59, "7d10+21", 40, 19, 8, 16, 5, 7, 7, "Greatclub", 6, "2d8+4", "Bludgeoning", 0),
        new("Wererat", "Humanoid (shapechanger)", "2", 12, 33, "6d8+6", 30, 10, 15, 12, 11, 10, 8, "Scimitar", 4, "1d6+2", "Slashing", 0),
        new("Harpy", "Monstrosity", "1", 11, 38, "7d8+7", 20, 12, 13, 12, 7, 10, 13, "Club", 3, "1d4+1", "Bludgeoning", 40),
        new("Grick", "Monstrosity", "2", 14, 27, "5d10", 30, 14, 14, 11, 3, 14, 5, "Tentacles", 4, "2d6+2", "Slashing", 0),
        new("Dryad", "Fey", "1", 11, 22, "5d8", 30, 10, 12, 11, 14, 15, 14, "Club", 2, "1d4", "Bludgeoning", 0),
        new("Specter", "Undead", "1", 12, 22, "5d8", 0, 1, 14, 11, 10, 10, 11, "Life Drain", 4, "3d6", "Necrotic", 50),
        new("Owlbear", "Monstrosity", "3", 13, 59, "7d10+21", 40, 20, 12, 17, 3, 12, 7, "Claws", 7, "2d8+5", "Slashing", 0),
        new("Troll", "Giant", "5", 15, 84, "8d10+40", 30, 18, 13, 20, 7, 9, 7, "Claws", 7, "2d6+4", "Slashing", 0),
        new("Werewolf", "Humanoid (shapechanger)", "3", 12, 58, "9d8+18", 30, 15, 13, 14, 10, 11, 10, "Bite", 4, "1d8+2", "Piercing", 0),
        new("Manticore", "Monstrosity", "3", 14, 68, "8d10+24", 30, 17, 16, 15, 7, 12, 8, "Claw", 5, "1d6+3", "Slashing", 50),
        new("Griffon", "Monstrosity", "2", 12, 59, "7d10+21", 30, 18, 15, 15, 2, 13, 8, "Claws", 6, "2d6+4", "Slashing", 80),
        new("Wight", "Undead", "3", 14, 45, "6d8+18", 30, 15, 14, 15, 11, 13, 15, "Longsword", 4, "1d8+2", "Slashing", 0),
        new("Minotaur", "Monstrosity", "3", 14, 76, "9d10+27", 40, 18, 11, 16, 6, 16, 9, "Greataxe", 6, "2d12+4", "Slashing", 0),
        new("Gorgon", "Monstrosity", "5", 19, 114, "12d10+48", 40, 20, 11, 18, 2, 12, 7, "Gore", 6, "2d10+5", "Piercing", 0),
        new("Banshee", "Undead", "4", 12, 58, "13d8", 0, 1, 14, 12, 12, 11, 17, "Corrupting Touch", 4, "3d6", "Necrotic", 40),
        new("Vampire Spawn", "Undead", "5", 15, 82, "11d8+33", 30, 16, 16, 16, 11, 12, 14, "Claws", 6, "2d6+3", "Slashing", 0),
        new("Wraith", "Undead", "5", 13, 67, "9d8+27", 0, 6, 16, 14, 12, 14, 15, "Life Drain", 6, "4d8", "Necrotic", 60),
        new("Wereboar", "Humanoid (shapechanger)", "4", 11, 78, "12d8+24", 30, 17, 10, 16, 9, 11, 8, "Maul", 5, "2d6+3", "Bludgeoning", 0),
        new("Weretiger", "Humanoid (shapechanger)", "4", 12, 120, "16d8+48", 30, 17, 15, 16, 10, 13, 11, "Claw", 5, "1d10+3", "Slashing", 0),
        new("Chimera", "Monstrosity", "6", 14, 114, "12d10+48", 30, 19, 11, 17, 3, 13, 9, "Bite", 6, "2d6+4", "Piercing", 60),
        new("Wyvern", "Dragon", "6", 13, 110, "13d10+39", 20, 19, 10, 16, 5, 12, 6, "Sting", 7, "2d6+4", "Piercing", 80),
        new("Young White Dragon", "Dragon", "6", 17, 133, "14d10+56", 40, 18, 10, 18, 6, 11, 12, "Bite", 7, "2d10+4", "Piercing", 80),
        new("Young Black Dragon", "Dragon", "7", 18, 127, "15d10+45", 40, 19, 14, 17, 12, 11, 15, "Bite", 7, "2d10+4", "Piercing", 80),
        new("Young Green Dragon", "Dragon", "8", 18, 136, "16d10+48", 40, 19, 12, 17, 16, 13, 15, "Bite", 7, "2d10+4", "Piercing", 80),
        new("Young Blue Dragon", "Dragon", "9", 18, 152, "16d10+64", 40, 21, 10, 19, 14, 13, 17, "Bite", 7, "2d10+5", "Piercing", 80),
        new("Young Red Dragon", "Dragon", "10", 18, 178, "17d10+85", 40, 23, 10, 21, 14, 11, 19, "Bite", 10, "2d10+6", "Piercing", 80),
        new("Medusa", "Monstrosity", "6", 15, 127, "17d8+51", 30, 10, 15, 14, 12, 13, 15, "Snake Hair", 5, "1d4+2", "Piercing", 0),
        new("Vampire", "Undead", "13", 16, 144, "17d8+68", 30, 18, 18, 18, 17, 15, 18, "Unarmed Strike", 9, "1d8+4", "Bludgeoning", 0),
        new("Lich", "Undead", "21", 17, 135, "18d8+54", 30, 11, 16, 16, 20, 14, 16, "Paralyzing Touch", 12, "3d6", "Cold", 0),
        new("Beholder", "Aberration", "13", 18, 180, "19d10+76", 0, 10, 14, 18, 17, 15, 17, "Bite", 5, "4d6", "Piercing", 20),
        new("Mind Flayer", "Aberration", "7", 15, 71, "13d8+13", 30, 11, 12, 12, 19, 17, 17, "Tentacles", 7, "2d10+4", "Psychic", 0),
        new("Adult Red Dragon", "Dragon", "17", 19, 256, "19d12+133", 40, 27, 10, 25, 16, 13, 21, "Bite", 14, "2d10+8", "Piercing", 80),
        new("Tarrasque", "Monstrosity", "30", 25, 676, "33d20+330", 40, 30, 11, 30, 3, 11, 11, "Bite", 19, "4d12+10", "Piercing", 0),
    };

    private static Enemy CreateStarterMonster(MonsterSeed seed)
    {
        bool ranged = seed.AttackName.Contains("bow", StringComparison.OrdinalIgnoreCase) ||
                     seed.AttackName.Contains("crossbow", StringComparison.OrdinalIgnoreCase) ||
                     seed.AttackName.Contains("javelin", StringComparison.OrdinalIgnoreCase);
        int proficiency = GetProficiencyFromCr(seed.Cr);
        return new Enemy
        {
            Name = seed.Name,
            CreatureType = seed.CreatureType,
            ChallengeRating = seed.Cr,
            ProficiencyBonus = proficiency,
            Strength = seed.Strength, Dexterity = seed.Dexterity, Constitution = seed.Constitution,
            Intelligence = seed.Intelligence, Wisdom = seed.Wisdom, Charisma = seed.Charisma,
            MaxHitPoints = seed.HitPoints, HitDice = seed.HitDice, ArmorClass = seed.ArmorClass,
            ArmorType = "Starter profile", SpeedWalk = seed.WalkSpeed, SpeedFly = seed.FlySpeed,
            SavingThrowProficiencies = "[]", SkillProficiencies = "[]", DamageResistances = "[]",
            DamageImmunities = "[]", DamageVulnerabilities = "[]", ConditionImmunities = "[]",
            PassivePerception = 10 + (seed.Wisdom - 10) / 2,
            Senses = "Passive Perception " + (10 + (seed.Wisdom - 10) / 2),
            Languages = "[]",
            IsOfficial = false, SourceBook = "Starter Catalogue (simplified sample; not parsed from PDF)",
            Attacks = new List<EnemyAttack>
            {
                new()
                {
                    Name = seed.AttackName,
                    Type = ranged ? AttackType.RangedWeapon : AttackType.MeleeWeapon,
                    Reach = ranged ? 0 : 5,
                    RangeNormal = ranged ? 80 : 0,
                    RangeLong = ranged ? 320 : 0,
                    AttackBonus = seed.AttackBonus,
                    DamageDice = seed.DamageDice,
                    DamageType = seed.DamageType
                }
            }
        };
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
    } ;

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
    } ;

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
    } ;


    private static async Task ReportCatalogueCountsAsync(AppDbContext db, IProgress<string>? progress)
    {
        int classCount = await db.Set<CharacterClass>().CountAsync();
        int raceCount = await db.Set<Race>().CountAsync();
        int backgroundCount = await db.Set<Background>().CountAsync();
        int spellCount = await db.Set<Spell>().CountAsync();
        int monsterCount = await db.Set<Enemy>().CountAsync();
        progress?.Report($"Database totals — Classes: {classCount}; Races: {raceCount}; Backgrounds: {backgroundCount}; Spells: {spellCount}; Monsters: {monsterCount}.");
    }

    private static int GetProficiencyFromCr(string cr) => cr switch
    {
        "0" or "1/8" or "1/4" or "1/2" or "1" or "2" or "3" or "4" => 2,
        "5" or "6" or "7" or "8" => 3,
        "9" or "10" or "11" or "12" => 4,
        "13" or "14" or "15" or "16" => 5,
        "17" or "18" or "19" or "20" => 6,
        "21" or "22" or "23" or "24" => 7,
        "25" or "26" or "27" or "28" => 8,
        "29" or "30" => 9,
        _ => 2
    };

    private static string Json<T>(T value) => JsonSerializer.Serialize(value);
    private static string SpellKey(string name, int level) => $"{name.Trim().ToUpperInvariant()}|{level}";
}
