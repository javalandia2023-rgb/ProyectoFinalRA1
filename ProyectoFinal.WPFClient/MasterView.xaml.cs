using System.Windows;
using System.Windows.Controls;

namespace ProyectoFinal.WPFClient
{
    public partial class MasterView : UserControl
    {
        public MasterView()
        {
            InitializeComponent();
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            // Navegar a la DetailView (lo haremos después)
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowView(new DetailView());
            }
        }
    }
}