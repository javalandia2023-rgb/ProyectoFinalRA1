using System;
using System.Windows.Input;

namespace ProyectoFinal.WPFClient.Commands
{
    // Esta clase es una versión genérica de RelayCommand.
    // Permite pasar un parámetro (en nuestro caso, un Vehiculo) al ejecutar el comando.
    // Se usa para los botones de Editar y Eliminar del DataGrid.
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }
    }
}

