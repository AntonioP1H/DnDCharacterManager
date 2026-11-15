using DnDCharacterManager.Models.Combat;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface IEncounterRepository
{
    Task<Encounter?> GetByIdAsync(int id);
    Task<IReadOnlyList<Encounter>> GetAllAsync();
    Task<Encounter> AddAsync(Encounter encounter);
    Task<Encounter> UpdateAsync(Encounter encounter);
    Task DeleteAsync(int id);
    Task<Encounter?> GetWithParticipantsAsync(int id);
}
