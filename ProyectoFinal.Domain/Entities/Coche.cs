// ProyectoFinal.Domain/Entities/Coche.cs
namespace ProyectoFinal.Domain.Entities;

public class Coche : Vehiculo
{
    public int NumPuertas { get; set; }
    public int NumPlazas { get; set; }
    public double CapacidadMaleteroLitros { get; set; }
}
