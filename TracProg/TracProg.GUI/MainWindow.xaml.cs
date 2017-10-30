using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TracProg.Calculation;
using TracProg.Calculation.Algoriths;

namespace TracProg.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _experimentSystem.Click += _experimentSystem_Click;
        }

        void _experimentSystem_Click(object sender, RoutedEventArgs e)
        {
            SystemForTesting _systemForTesting = new SystemForTesting();
            _systemForTesting.Show();
        }
    }
}
