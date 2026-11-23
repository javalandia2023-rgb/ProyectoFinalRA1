using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Domain.Entities;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Persistence.Contexts;

namespace ProyectoFinal.Persistence.Repositories
{
    public class EfVehiculoRepository : IVehiculoRepository
    {
        private readonly AppDbContext _context;

        public EfVehiculoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            return await _context.Vehiculos.ToListAsync();
        }

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            return await _context.Vehiculos.FindAsync(id);
        }

        public async Task AddAsync(Vehiculo vehiculo)
        {
            await _context.Vehiculos.AddAsync(vehiculo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehiculo vehiculo)
        {
            _context.Vehiculos.Update(vehiculo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ImportAsync(IEnumerable<Vehiculo> vehiculos)
        {
            await _context.Vehiculos.AddRangeAsync(vehiculos);
            await _context.SaveChangesAsync();
        }
    }
}
/*
¿Qué hace este código?
Implementa IVehiculoRepository: Cumple el mismo contrato que el InMemoryVehiculoRepository, pero usando EF Core.

_context.Vehiculos.ToListAsync(): Hace un SELECT * FROM Vehiculos en MySQL (asíncrono, para no bloquear).

_context.Vehiculos.FindAsync(id): Busca un vehículo por su clave primaria.

_context.Vehiculos.AddAsync(vehiculo): Hace un INSERT.

_context.SaveChangesAsync(): Confirma los cambios en la base de datos.

_context.Vehiculos.Update(vehiculo): Hace un UPDATE.

_context.Vehiculos.Remove(vehiculo): Hace un DELETE.

_context.Vehiculos.AddRangeAsync(vehiculos): Hace un INSERT masivo (para la importación del CSV). 
 
¿Qué es InMemoryVehiculoRepository?
Es una implementación de la interfaz IVehiculoRepository que guarda los datos en la memoria RAM (usando un DataSet de ADO.NET). 
No necesita un servidor de base de datos externo; los datos viven mientras la aplicación esté encendida.

¿Por qué es importante?
Porque cumple dos propósitos fundamentales en tu proyecto:
A) Es el "modo de desarrollo rápido"
Imagina que quieres probar una nueva funcionalidad en el WPF (por ejemplo, un filtro de búsqueda). No quieres tener que arrancar MySQL,
crear la base de datos, importar los 10.000 vehículos y esperar a que cargue. Con InMemoryVehiculoRepository, arrancas la API y ya 
tienes los datos (o los importas con el CSV en 2 segundos). Es instantáneo y ligero, ideal para tu PC con 8 GB de RAM.

B) Es la prueba del Principio Abierto/Cerrado (OCP) y la Inversión de Dependencias (DIP)
El controlador de la API (VehiculoController) no sabe si los datos vienen de un DataSet o de MySQL. Solo conoce la interfaz 
IVehiculoRepository.
•	Si en el appsettings.json pones "PersistenceMode": "Memory", la API usará InMemoryVehiculoRepository.
•	Si pones "PersistenceMode": "MySQL", usará EfVehiculoRepository.
El controlador no cambia ni una línea de código. Eso es la magia de SOLID: la lógica de negocio (el controlador) depende de una 
abstracción (la interfaz), no de una implementación concreta.

¿Qué papel juega ahora con MySQL?
Ahora que vamos a añadir EfVehiculoRepository, el InMemoryVehiculoRepository no desaparece. Se queda como una alternativa.
•	Para el día a día: Usas "Memory" (rápido, ligero, no necesita Docker).
•	Para la entrega final y las pruebas de MySQL: Usas "MySQL" (persistente).


 
 */

