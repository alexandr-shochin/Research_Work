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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            config = new Configuration(@"D:\Program Files\Dropbox\Research_Work\TracProg\config.mydeflef");
            config.Grid.Metalize += Grid_Metalize;

            bmp = new Bitmap((int)(_image.Width), (int)(_image.Height));

            g = Graphics.FromImage(bmp);

            Li li = new Li(config.Grid, config.Net);

            Thread thread = new Thread(delegate()
            {
                li.FindPath();
            });
            thread.IsBackground = true;
            thread.Start();
        }

        void Grid_Metalize()
        {
            try
            {
                Dispatcher.Invoke(delegate
                    {
                        config.Grid.Draw(ref g);
                        System.Windows.Media.Imaging.BitmapSource b =
                        System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                               bmp.GetHbitmap(),
                               IntPtr.Zero,
                               Int32Rect.Empty,
                               BitmapSizeOptions.FromEmptyOptions());
                        _image.Source = b;
                        bmp.Save("test.bmp");
                    });
            }
            catch (System.Runtime.InteropServices.ExternalException) { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _image.Source = null;
        }
    }
}
