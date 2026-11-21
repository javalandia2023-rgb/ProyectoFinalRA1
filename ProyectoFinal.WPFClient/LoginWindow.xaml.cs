using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace ProyectoFinal.WPFClient
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text;
            var password = txtPassword.Password;

            using var client = new HttpClient();
            var loginData = new { Username = username, Password = password };

            try
            {
                // Hacemos la petición POST a la API (asegúrate de que la API está corriendo en el puerto 5168)
                var response = await client.PostAsJsonAsync("http://localhost:5168/api/Auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    // Guardamos el token en una variable global (o en un archivo de configuración)
                    //App.Token = result?.Token;
                    Session.Token = result?.Token;
                    MessageBox.Show("Login exitoso", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Abrimos la ventana principal y cerramos la de login
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
