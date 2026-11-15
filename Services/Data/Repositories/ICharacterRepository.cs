using DnDCharacterManager.Models.Characters;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(int id);
    Task<IReadOnlyList<Character>> GetAllAsync();
    Task<Character> AddAsync(Character character);
    Task<Character> UpdateAsync(Character character);
    Task DeleteAsync(int id);
    Task<Character?> GetWithDetailsAsync(int id);
}
