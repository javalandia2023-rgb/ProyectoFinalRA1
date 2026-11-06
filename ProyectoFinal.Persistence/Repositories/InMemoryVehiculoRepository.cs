using ProyectoFinal.Domain.Entities;
using ProyectoFinal.Domain.Interfaces;
using System.Data;

namespace ProyectoFinal.Persistence.Repositories;

public class InMemoryVehiculoRepository : IVehiculoRepository
{
    private readonly DataSet _dataSet;

    public InMemoryVehiculoRepository()
    {
        _dataSet = new DataSet("VehiculosDB");
        InitializeDataSet();
    }

    private void InitializeDataSet()
    {
        var table = new DataTable("Vehiculos");

        // Columnas básicas
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Marca", typeof(string));
        table.Columns.Add("Modelo", typeof(string));
        table.Columns.Add("Año", typeof(int));
        table.Columns.Add("Precio", typeof(decimal));
        table.Columns.Add("TipoCombustible", typeof(string));
        table.Columns.Add("Transmision", typeof(string));
        table.Columns.Add("CilindradaCC", typeof(double));
        table.Columns.Add("PotenciaHP", typeof(double));
        table.Columns.Add("ConsumoKmPorLitro", typeof(double));
        table.Columns.Add("EmisionesCO2", typeof(double));
        table.Columns.Add("CategoriaVehiculo", typeof(string));

        // Columnas específicas (las ponemos "nullable" para el DataSet)
        table.Columns.Add("NumPuertas", typeof(int));
        table.Columns.Add("NumPlazas", typeof(int));
        table.Columns.Add("CapacidadMaleteroLitros", typeof(double));
        table.Columns.Add("CapacidadCargaKg", typeof(double));
        table.Columns.Add("NumEjes", typeof(int));
        table.Columns.Add("TipoMoto", typeof(string));
        table.Columns.Add("TieneABS", typeof(bool));

        table.PrimaryKey = new DataColumn[] { table.Columns["Id"]! };
        _dataSet.Tables.Add(table);
    }

    public async Task<IEnumerable<Vehiculo>> GetAllAsync()
    {
        // Simulamos un pequeño retardo asíncrono para que la UI no parpadee (opcional)
        await Task.Delay(10);

        var table = _dataSet.Tables["Vehiculos"]!;
        var vehiculos = new List<Vehiculo>();

        foreach (DataRow row in table.Rows)
        {
            vehiculos.Add(MapRowToVehiculo(row));
        }

        return vehiculos;
    }

    public async Task<Vehiculo?> GetByIdAsync(int id)
    {
        await Task.Delay(10);
        var table = _dataSet.Tables["Vehiculos"]!;

        // LINQ to DataSet
        var row = table.AsEnumerable().FirstOrDefault(r => r.Field<int>("Id") == id);

        return row != null ? MapRowToVehiculo(row) : null;
    }

    public async Task AddAsync(Vehiculo vehiculo)
    {
        await Task.Delay(10);
        var table = _dataSet.Tables["Vehiculos"]!;

        // Autoincrementar el ID si viene a 0
        if (vehiculo.Id == 0 && table.Rows.Count > 0)
            vehiculo.Id = table.AsEnumerable().Max(r => r.Field<int>("Id")) + 1;
        else if (vehiculo.Id == 0)
            vehiculo.Id = 1;

        var row = table.NewRow();
        MapVehiculoToRow(vehiculo, row);
        table.Rows.Add(row);
    }

    public async Task UpdateAsync(Vehiculo vehiculo)
    {
        await Task.Delay(10);
        var table = _dataSet.Tables["Vehiculos"]!;
        var row = table.AsEnumerable().FirstOrDefault(r => r.Field<int>("Id") == vehiculo.Id);

        if (row != null)
        {
            MapVehiculoToRow(vehiculo, row);
        }
    }

    public async Task DeleteAsync(int id)
    {
        await Task.Delay(10);
        var table = _dataSet.Tables["Vehiculos"]!;
        var row = table.AsEnumerable().FirstOrDefault(r => r.Field<int>("Id") == id);

        if (row != null)
        {
            table.Rows.Remove(row);
        }
    }

    public async Task ImportAsync(IEnumerable<Vehiculo> vehiculos)
    {
        await Task.Delay(10);
        var table = _dataSet.Tables["Vehiculos"]!;

        // Si ya hay datos, empezamos a contar desde el ID más alto
        int nextId = table.Rows.Count > 0 ? table.AsEnumerable().Max(r => r.Field<int>("Id")) + 1 : 1;

        foreach (var v in vehiculos)
        {
            v.Id = nextId++;
            var row = table.NewRow();
            MapVehiculoToRow(v, row);
            table.Rows.Add(row);
        }
    }

    // --- Métodos de Mapeo (DataSet es plano, así que unificamos las columnas) ---

    private void MapVehiculoToRow(Vehiculo v, DataRow row)
    {
        row["Id"] = v.Id;
        row["Marca"] = v.Marca;
        row["Modelo"] = v.Modelo;
        row["Año"] = v.Año;
        row["Precio"] = v.Precio;
        row["TipoCombustible"] = v.TipoCombustible;
        row["Transmision"] = v.Transmision;
        row["CilindradaCC"] = v.CilindradaCC;
        row["PotenciaHP"] = v.PotenciaHP;
        row["ConsumoKmPorLitro"] = v.ConsumoKmPorLitro;
        row["EmisionesCO2"] = v.EmisionesCO2;
        row["CategoriaVehiculo"] = v.CategoriaVehiculo;

        // Propiedades específicas
        if (v is Coche c)
        {
            row["NumPuertas"] = c.NumPuertas;
            row["NumPlazas"] = c.NumPlazas;
            row["CapacidadMaleteroLitros"] = c.CapacidadMaleteroLitros;
        }
        else if (v is Camion ca)
        {
            row["CapacidadCargaKg"] = ca.CapacidadCargaKg;
            row["NumEjes"] = ca.NumEjes;
        }
        else if (v is Moto m)
        {
            row["TipoMoto"] = m.TipoMoto;
            row["TieneABS"] = m.TieneABS;
        }
    }

    private Vehiculo MapRowToVehiculo(DataRow row)
    {
        string categoria = row.Field<string>("CategoriaVehiculo") ?? "";
        Vehiculo v;

        if (categoria.Contains("Truck", StringComparison.OrdinalIgnoreCase))
        {
            v = new Camion
            {
                CapacidadCargaKg = row.Field<double?>("CapacidadCargaKg") ?? 0,
                NumEjes = row.Field<int?>("NumEjes") ?? 0
            };
        }
        else if (categoria.Contains("Moto", StringComparison.OrdinalIgnoreCase))
        {
            v = new Moto
            {
                TipoMoto = row.Field<string>("TipoMoto") ?? "",
                TieneABS = row.Field<bool?>("TieneABS") ?? false
            };
        }
        else
        {
            v = new Coche
            {
                NumPuertas = row.Field<int?>("NumPuertas") ?? 0,
                NumPlazas = row.Field<int?>("NumPlazas") ?? 0,
                CapacidadMaleteroLitros = row.Field<double?>("CapacidadMaleteroLitros") ?? 0
            };
        }

        v.Id = row.Field<int>("Id");
        v.Marca = row.Field<string>("Marca") ?? "";
        v.Modelo = row.Field<string>("Modelo") ?? "";
        v.Año = row.Field<int>("Año");
        v.Precio = row.Field<decimal>("Precio");
        v.TipoCombustible = row.Field<string>("TipoCombustible") ?? "";
        v.Transmision = row.Field<string>("Transmision") ?? "";
        v.CilindradaCC = row.Field<double>("CilindradaCC");
        v.PotenciaHP = row.Field<double>("PotenciaHP");
        v.ConsumoKmPorLitro = row.Field<double>("ConsumoKmPorLitro");
        v.EmisionesCO2 = row.Field<double>("EmisionesCO2");
        v.CategoriaVehiculo = categoria;

        return v;
    }
}
