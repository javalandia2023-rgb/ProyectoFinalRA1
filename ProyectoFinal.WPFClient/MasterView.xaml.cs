using System.Windows;
using System.Windows.Controls;
using ProyectoFinal.WPFClient.ViewModels;

namespace ProyectoFinal.WPFClient
{
    public partial class MasterView : UserControl
    {
        public MasterView()
        {
            InitializeComponent();

            // Añadimos un evento para cuando la vista esté cargada
            this.Loaded += MasterView_Loaded;
        }

        private void MasterView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ahora sí, la vista está dentro de la ventana, por lo que Window.GetWindow(this) no será null
            var vm = (MasterViewModel)this.DataContext;
            if (vm != null)
            {
                vm.MainWindow = Window.GetWindow(this);
            }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowView(new DetailView());
            }
        }
    }
}

