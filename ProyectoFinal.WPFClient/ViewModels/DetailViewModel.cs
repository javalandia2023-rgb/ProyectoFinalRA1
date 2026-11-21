using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using ProyectoFinal.Domain.Entities;
using ProyectoFinal.WPFClient.Commands;

namespace ProyectoFinal.WPFClient.ViewModels
{
    public class DetailViewModel : INotifyPropertyChanged
    {
        private string _marca = string.Empty;
        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(nameof(Marca)); }
        }

        private string _modelo = string.Empty;
        public string Modelo
        {
            get => _modelo;
            set { _modelo = value; OnPropertyChanged(nameof(Modelo)); }
        }

        private string _precio = "0";
        public string Precio
        {
            get => _precio;
            set { _precio = value; OnPropertyChanged(nameof(Precio)); }
        }

        public RelayCommand GuardarCommand { get; }

        public DetailViewModel()
        {
            GuardarCommand = new RelayCommand(async () => await GuardarVehiculoAsync());
        }

        private async Task GuardarVehiculoAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

            var nuevoVehiculo = new Coche
            {
                Marca = this.Marca,
                Modelo = this.Modelo,
                Precio = decimal.TryParse(this.Precio, out var p) ? p : 0
            };

            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:5168/api/Vehiculo", nuevoVehiculo);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Vehículo creado correctamente.");
                }
                else
                {
                    MessageBox.Show("Error al crear el vehículo.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
