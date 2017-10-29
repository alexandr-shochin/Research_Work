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
        Configuration config;
        Graphics g;
        Bitmap bmp;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                _loadConfiguration.IsEnabled = false;

                _getSolutionButton.IsEnabled = false;
                _clearSolutionButton.IsEnabled = false;

            });

            config = new Configuration();
            config.ReadFromFile(@"D:\Program Files\Dropbox\Research_Work\TracProg\config.mydeflef");
            config.Grid.Metalize += Grid_Metalize;

            

            Li li = new Li(config.Grid, config.Net);
            li.CalculateIsComplete += li_CalculateIsComplete;

            Thread thread = new Thread(delegate()
            {
                li.FindPath();
            });
            thread.IsBackground = true;
            thread.Start();
        }

        void li_CalculateIsComplete()
        {
            Dispatcher.Invoke(delegate
            {
                _loadConfiguration.IsEnabled = true;

                _getSolutionButton.IsEnabled = true;
                _clearSolutionButton.IsEnabled = true;

            });
        }

        void Grid_Metalize()
        {
            try
            {
                Dispatcher.Invoke(delegate
                    {
                        config.Grid.Draw(ref g);
                        //System.Windows.Media.Imaging.BitmapSource b =
                        //System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        //       bmp.GetHbitmap(),
                        //       IntPtr.Zero,
                        //       Int32Rect.Empty,
                        //       BitmapSizeOptions.FromEmptyOptions());
                        //_image.Source = b;
                        bmp.Save("test.bmp");
                    });
            }
            catch (System.Runtime.InteropServices.ExternalException) { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            config = null;
            g = null;
            bmp = null;
            _image.Source = null;
        }

        private void _newConfiguration_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            int y = 0;

            int n = 1000;
            int m = 1000;

            int countPins = 5;
            int countProhibitionZone = 100000;
            int countNodeInNet = 5;

            int koeff = 10;

            config = new Configuration();
            config.GenerateRandomConfig(x, y, n, m, countPins, countProhibitionZone, countNodeInNet, koeff);
            config.Grid.Metalize += Grid_Metalize;

            bmp = new Bitmap(config.Grid.Width, config.Grid.Height);

            g = Graphics.FromImage(bmp);

            Li li = new Li(config.Grid, config.Net);
            li.CalculateIsComplete += li_CalculateIsComplete;

            Thread thread = new Thread(delegate()
            {
                li.FindPath();
            });
            thread.IsBackground = true;
            thread.Start();

        //    try
        //    {
        //        Dispatcher.Invoke(delegate
        //        {
        //            config.Grid.Draw(ref g);
        //            System.Windows.Media.Imaging.BitmapSource b =
        //            System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                   bmp.GetHbitmap(),
        //                   IntPtr.Zero,
        //                   Int32Rect.Empty,
        //                   BitmapSizeOptions.FromEmptyOptions());
        //            _image.Source = b;
        //            bmp.Save("test.bmp");
        //        });
        //    }
        //    catch (System.Runtime.InteropServices.ExternalException) { }
        }
    }
}
