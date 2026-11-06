// ProyectoFinal.Domain/Interfaces/IVehiculoRepository.cs
using ProyectoFinal.Domain.Entities;

namespace ProyectoFinal.Domain.Interfaces;

public interface IVehiculoRepository
{
    Task<IEnumerable<Vehiculo>> GetAllAsync();
    Task<Vehiculo?> GetByIdAsync(int id);
    Task AddAsync(Vehiculo vehiculo);
    Task UpdateAsync(Vehiculo vehiculo);
    Task DeleteAsync(int id);
    Task ImportAsync(IEnumerable<Vehiculo> vehiculos);
}

