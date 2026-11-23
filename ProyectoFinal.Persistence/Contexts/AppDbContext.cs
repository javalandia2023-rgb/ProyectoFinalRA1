using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Domain.Entities;

namespace ProyectoFinal.Persistence.Contexts
{
    public class AppDbContext : DbContext
    {
        // El constructor recibe las opciones de configuración (cadena de conexión, etc.)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aquí mapeamos nuestras entidades a tablas.
        // DbSet<Vehiculo> significa que habrá una tabla llamada "Vehiculos".
        public DbSet<Vehiculo> Vehiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración para el polimorfismo (Herencia)
            // Como Vehiculo es abstracta, EF Core necesita saber cómo mapearla.
            // Usamos "Table Per Hierarchy" (TPH): todos los vehículos van a la misma tabla,
            // pero se diferencian por una columna llamada "TipoVehiculo".
            modelBuilder.Entity<Vehiculo>()
                .HasDiscriminator<string>("TipoVehiculo")
                .HasValue<Coche>("Coche")
                .HasValue<Camion>("Camion")
                .HasValue<Moto>("Moto");
            // --- SE AÑADE ESTO ---
            // Configuración de tipos de datos para evitar problemas en MySQL
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.Property(v => v.Marca).HasMaxLength(100);
                entity.Property(v => v.Modelo).HasMaxLength(100);
                entity.Property(v => v.TipoCombustible).HasMaxLength(50);
                entity.Property(v => v.Transmision).HasMaxLength(50);
                entity.Property(v => v.CategoriaVehiculo).HasMaxLength(50);
                entity.Property(v => v.Precio).HasPrecision(18, 2); // Precisión estándar para dinero
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
/*
 ¿Qué hace exactamente este código?
•	DbSet<Vehiculo> Vehiculos: Le dice a EF Core que cree una tabla llamada Vehiculos donde se guardarán los vehículos.
•	OnModelCreating: Es el método donde configuramos cómo se mapean las clases a la base de datos. En este caso, le decimos que:
   o	Use una sola tabla para todos los tipos de vehículos (TPH).
   o	Añada una columna TipoVehiculo que guarde el nombre de la clase (Coche, Camion o Moto). Así, cuando EF Core lea una fila, sabrá qué objeto crear.

 
 */