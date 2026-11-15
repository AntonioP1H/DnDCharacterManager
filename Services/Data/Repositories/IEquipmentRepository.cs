using DnDCharacterManager.Models.Equipment;

namespace DnDCharacterManager.Services.Data.Repositories;

public interface IEquipmentRepository
{
    Task<Weapon?> GetWeaponByIdAsync(int id);
    Task<IReadOnlyList<Weapon>> GetAllWeaponsAsync();
    Task<IReadOnlyList<Weapon>> GetWeaponsByCategoryAsync(string category);
    Task<Armor?> GetArmorByIdAsync(int id);
    Task<IReadOnlyList<Armor>> GetAllArmorAsync();
    Task<IReadOnlyList<Armor>> GetArmorByCategoryAsync(string category);
    Task<IReadOnlyList<EquipmentItem>> GetAllItemsAsync();
    Task SeedCoreEquipmentAsync();
}
