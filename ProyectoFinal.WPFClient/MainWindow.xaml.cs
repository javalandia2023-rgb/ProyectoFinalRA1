using System.Windows;
using System.Windows.Controls;

namespace ProyectoFinal.WPFClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*
      
        Tenemos la lógica de abrir la vista directamente dentro del evento 
		BtnMaster_Click (líneas 15-19). Eso está bien para el botón del menú, 
		pero la MasterView necesita un método público para poder pedirle a la
		ventana principal que cambie de vista (cuando pulses "Nuevo Vehículo").
        
         */
        public void ShowView(UserControl view)
        {
            MainContentArea.Children.Clear();
            MainContentArea.Children.Add(view);
        }


        private void BtnMaster_Click(object sender, RoutedEventArgs e)
        {
            // Abrimos la vista Master dentro del área central
            var masterView = new MasterView();

            // Limpiamos el área y ponemos la nueva vista
            MainContentArea.Children.Clear();
            MainContentArea.Children.Add(masterView);
        }

        private void BtnMcp_Click(object sender, RoutedEventArgs e)
        {
            // Aquí abriremos la vista MCP más adelante
            //Cambiamod el método BtnMcp_Click para que abra la McpView:
            //MessageBox.Show("La pantalla MCP se implementará en la Fase 4.", "En construcción", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowView(new McpView()); // <-- Abre la vista MCP
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
