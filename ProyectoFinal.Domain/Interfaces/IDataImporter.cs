// ProyectoFinal.Domain/Interfaces/IDataImporter.cs
using ProyectoFinal.Domain.Entities;

namespace ProyectoFinal.Domain.Interfaces;

public interface IDataImporter
{
    Task<IEnumerable<Vehiculo>> ImportAsync(string filePath);
}

