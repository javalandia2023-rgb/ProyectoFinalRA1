using Newtonsoft.Json;
using ProyectoFinal.Domain.Entities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoFinal.WPFClient
{
    public partial class DetailView : UserControl
    {
        private int _vehiculoId = 0;

        public DetailView(int id = 0)
        {
            InitializeComponent();
            _vehiculoId = id;

            if (id > 0)
            {
                _ = CargarVehiculoAsync(id);
            }
        }

        private async Task CargarVehiculoAsync(int id)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

            var response = await client.GetAsync($"http://localhost:5168/api/Vehiculo/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new VehiculoConverter());
                var vehiculo = JsonConvert.DeserializeObject<Vehiculo>(json, settings);

                if (vehiculo != null)
                {
                    // Asignamos los datos a los controles del XAML
                    // (Asegúrate de que los TextBox tienen x:Name en el XAML)
                    txtMarca.Text = vehiculo.Marca;
                    txtModelo.Text = vehiculo.Modelo;
                    txtPrecio.Text = vehiculo.Precio.ToString();
                }
            }
        }

        // El botón Guardar (debes tenerlo en el XAML con Click="BtnGuardar_Click")
        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

            // Si es una edición, hacemos PUT; si es nuevo, POST.
            // (Por simplicidad, este código asume que siempre es edición si _vehiculoId > 0)
            if (_vehiculoId > 0)
            {
                var vehiculoActualizado = new Coche
                {
                    Id = _vehiculoId,
                    Marca = txtMarca.Text,
                    Modelo = txtModelo.Text,
                    Precio = decimal.TryParse(txtPrecio.Text, out var p) ? p : 0
                };

                var response = await client.PutAsJsonAsync($"http://localhost:5168/api/Vehiculo/{_vehiculoId}", vehiculoActualizado);
                if (response.IsSuccessStatusCode)
                    MessageBox.Show("Vehículo actualizado.");
                else
                    MessageBox.Show("Error al actualizar.");
            }
            else
            {
                // Lógica para crear (POST) - ya la tenías
            }
        }
    }
}



