using System.Collections.Generic;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface IClassRepository
{
    Task<CharacterClass?> GetByIdAsync(int id);
    Task<CharacterClass?> GetByNameAsync(string name);
    Task<IReadOnlyList<CharacterClass>> GetAllAsync();
    Task<CharacterClass> AddAsync(CharacterClass characterClass);
    Task<CharacterClass> UpdateAsync(CharacterClass characterClass);
}
