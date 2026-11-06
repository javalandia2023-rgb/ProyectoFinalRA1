// ProyectoFinal.Domain/Entities/Camion.cs
namespace ProyectoFinal.Domain.Entities;

public class Camion : Vehiculo
{
    public double CapacidadCargaKg { get; set; }
    public int NumEjes { get; set; }
}

