// ProyectoFinal.Domain/Entities/Moto.cs
namespace ProyectoFinal.Domain.Entities;

public class Moto : Vehiculo
{
    public string TipoMoto { get; set; } = string.Empty; // Scooter, Sportbike, Cruiser
    public bool TieneABS { get; set; }
}

