using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            Bitmap bmp = new Bitmap(100, 100);

            Graphics g = Graphics.FromImage(bmp);

            Configuration config = new Configuration(@"D:\Program Files\Dropbox\Research_Work\TracProg\config.mydeflef");
            config.Grid.Draw(ref g);

            bmp.Save("test.bmp");

            System.Windows.Media.Imaging.BitmapSource b =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       bmp.GetHbitmap(),
                       IntPtr.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
            _image.Source = b;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _image.Source = null;
        }
    }
}
