// ProyectoFinal.Domain/Entities/Vehiculo.cs
namespace ProyectoFinal.Domain.Entities;

public abstract class Vehiculo
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Año { get; set; }
    public decimal Precio { get; set; }
    public string TipoCombustible { get; set; } = string.Empty; // Petrol, Diesel, CNG, Hybrid, Electric
    public string Transmision { get; set; } = string.Empty; // Manual, Automatic
    public double CilindradaCC { get; set; }
    public double PotenciaHP { get; set; }
    public double ConsumoKmPorLitro { get; set; }
    public double EmisionesCO2 { get; set; }
    public string CategoriaVehiculo { get; set; } = string.Empty; // Hatchback, Sedan, SUV, Truck, etc.
}
