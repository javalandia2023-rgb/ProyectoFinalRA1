using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using ProyectoFinal.Domain.Entities;
using ProyectoFinal.WPFClient.Commands;

namespace ProyectoFinal.WPFClient.ViewModels;

public class MasterViewModel : INotifyPropertyChanged
{
    // La lista que se enlazará al DataGrid.
    // ObservableCollection avisa a la vista de los cambios.
    private ObservableCollection<Vehiculo> _vehiculos;
    public ObservableCollection<Vehiculo> Vehiculos
    {
        get => _vehiculos;
        set
        {
            _vehiculos = value;
            OnPropertyChanged(nameof(Vehiculos));
        }
    }

    // El comando que ejecutará el botón "Cargar Vehículos"
    public RelayCommand CargarCommand { get; }

    public MasterViewModel()
    {
        Vehiculos = new ObservableCollection<Vehiculo>();
        CargarCommand = new RelayCommand(async () => await CargarVehiculosAsync());
    }

    private async Task CargarVehiculosAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

        try
        {
            var response = await client.GetAsync("http://localhost:5168/api/Vehiculo");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new VehiculoConverter());
                var lista = JsonConvert.DeserializeObject<List<Vehiculo>>(json, settings);

                // Llenamos la ObservableCollection (la vista se actualiza sola)
                Vehiculos.Clear();
                foreach (var v in lista) Vehiculos.Add(v);
            }
            else
            {
                MessageBox.Show("Error al cargar los vehículos. ¿Has hecho Login?");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    // Implementación de INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

