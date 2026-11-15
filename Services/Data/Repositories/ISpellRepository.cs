using System.Collections.Generic;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Equipment;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface ISpellRepository
{
    Task<Spell?> GetByIdAsync(int id);
    Task<Spell?> GetByNameAndLevelAsync(string name, int level);
    Task<IReadOnlyList<Spell>> GetAllAsync();
    Task<IReadOnlyList<Spell>> GetByLevelAsync(int level);
    Task<IReadOnlyList<Spell>> GetByClassAsync(string className);
    Task<IReadOnlyList<Spell>> GetBySchoolAsync(string school);
    Task<Spell> AddAsync(Spell spell);
    Task<Spell> UpdateAsync(Spell spell);
}
