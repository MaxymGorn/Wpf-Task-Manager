using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManager.Command;

namespace WpfApp15.ViewModel
{
    class MainViewModel
    {
        public ICommand QuitterCommand { get; private set; }

        public ICommand ToggleCommand { get; private set; }

        public ICommand MinimizeCommand { get; private set; }



        public MainViewModel()
        {
            QuitterCommand = new RelayCommand2(() =>
            {
                Application.Current.MainWindow.Close();
            });

            ToggleCommand = new RelayCommand2(() =>
            {
                Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            });

            MinimizeCommand = new RelayCommand2(() =>
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            });

        
        }
    }
}
