using System.Collections.Generic;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface IBackgroundRepository
{
    Task<Background?> GetByIdAsync(int id);
    Task<Background?> GetByNameAsync(string name);
    Task<IReadOnlyList<Background>> GetAllAsync();
    Task<Background> AddAsync(Background background);
    Task<Background> UpdateAsync(Background background);
}
