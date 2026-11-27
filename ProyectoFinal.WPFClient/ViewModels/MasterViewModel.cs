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

namespace ProyectoFinal.WPFClient.ViewModels
{
    public class MasterViewModel : INotifyPropertyChanged
    {
        // =================================================================================
        // PROPIEDADES
        // =================================================================================

        // Referencia a la ventana principal para poder navegar a la DetailView
        public Window? MainWindow { get; set; }

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

        // =================================================================================
        // COMANDOS
        // =================================================================================

        public RelayCommand CargarCommand { get; }
        public RelayCommand<Vehiculo> EditarCommand { get; }
        public RelayCommand<Vehiculo> EliminarCommand { get; }

        // =================================================================================
        // CONSTRUCTOR
        // =================================================================================

        public MasterViewModel()
        {
            Vehiculos = new ObservableCollection<Vehiculo>();

            CargarCommand = new RelayCommand(async () => await CargarVehiculosAsync());
            EditarCommand = new RelayCommand<Vehiculo>(EditarVehiculo);
            EliminarCommand = new RelayCommand<Vehiculo>(async (v) => await EliminarVehiculoAsync(v));
        }

        // =================================================================================
        // LÓGICA DE NEGOCIO
        // =================================================================================

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

        // MÉTODO MODIFICADO: Ahora usa la propiedad MainWindow en lugar de Application.Current
        private void EditarVehiculo(Vehiculo vehiculo)
        {
            if (vehiculo == null)
            {
                MessageBox.Show("No se ha seleccionado ningún vehículo.");
                return;
            }

            if (MainWindow is MainWindow main)
            {
                main.ShowView(new DetailView(vehiculo.Id));
            }
            else
            {
                MessageBox.Show("MainWindow es null. Revisa el MasterView.xaml.cs.");
            }
        }

        private async Task EliminarVehiculoAsync(Vehiculo vehiculo)
        {
            var result = MessageBox.Show($"¿Eliminar el vehículo {vehiculo.Marca} {vehiculo.Modelo}?", "Confirmar", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

                var response = await client.DeleteAsync($"http://localhost:5168/api/Vehiculo/{vehiculo.Id}");
                if (response.IsSuccessStatusCode)
                {
                    Vehiculos.Remove(vehiculo);
                    MessageBox.Show("Vehículo eliminado.");
                }
                else
                {
                    MessageBox.Show("Error al eliminar.");
                }
            }
        }

        // =================================================================================
        // INotifyPropertyChanged
        // =================================================================================

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

