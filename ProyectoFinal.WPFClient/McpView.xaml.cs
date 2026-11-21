using ProyectoFinal.WPFClient.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoFinal.WPFClient
{
    public partial class McpView : UserControl
    {
        private readonly McpService _mcpService = new McpService();
        private bool _isReady = false;

        public McpView()
        {
            InitializeComponent();
            _ = InitializeServiceAsync(); // Arrancamos la IA en segundo plano
        }

        private async Task InitializeServiceAsync()
        {
            try
            {
                txtRespuesta.Text = "Iniciando el cerebro de IA (esto puede tardar unos segundos)...";
                await _mcpService.InitializeAsync();
                _isReady = true;
                txtRespuesta.Text = "¡IA lista! Hazme una pregunta.";
            }
            catch (Exception ex)
            {
                txtRespuesta.Text = $"Error al inicializar la IA: {ex.Message}";
            }
        }

        private async void BtnPreguntar_Click(object sender, RoutedEventArgs e)
        {
            if (!_isReady)
            {
                MessageBox.Show("La IA todavía se está iniciando. Espera un momento.");
                return;
            }

            var pregunta = txtPregunta.Text;
            if (string.IsNullOrWhiteSpace(pregunta)) return;

            BtnPreguntar.IsEnabled = false;
            txtRespuesta.Text = "Pensando...";

            try
            {
                var respuesta = await _mcpService.AskQuestionAsync(pregunta);
                txtRespuesta.Text = respuesta;
            }
            catch (Exception ex)
            {
                txtRespuesta.Text = $"Error: {ex.Message}";
            }
            finally
            {
                BtnPreguntar.IsEnabled = true;
            }
        }
    }
}

