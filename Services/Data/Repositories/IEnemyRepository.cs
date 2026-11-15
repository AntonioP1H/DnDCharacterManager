using DnDCharacterManager.Models.Enemies;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface IEnemyRepository
{
    Task<Enemy?> GetByIdAsync(int id);
    Task<IReadOnlyList<Enemy>> GetAllAsync();
    Task<IReadOnlyList<Enemy>> GetByChallengeRatingAsync(string challengeRating);
    Task<Enemy> AddAsync(Enemy enemy);
    Task<Enemy> UpdateAsync(Enemy enemy);
    Task DeleteAsync(int id);
    Task<Enemy?> GetWithDetailsAsync(int id);
}
