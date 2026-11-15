using System.Collections.Generic;
using System.Threading.Tasks;
using DnDCharacterManager.Models.Characters;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface IRaceRepository
{
    Task<Race?> GetByIdAsync(int id);
    Task<Race?> GetByNameAsync(string name);
    Task<IReadOnlyList<Race>> GetAllAsync();
    Task<Race> AddAsync(Race race);
    Task<Race> UpdateAsync(Race race);
}
