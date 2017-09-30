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
           
            Bitmap bmp = new Bitmap((int)_image.Width, (int)_image.Height);

            Graphics g = Graphics.FromImage(bmp);

            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 255, 0, 0);

            g.DrawLines(new System.Drawing.Pen(c), new System.Drawing.Point[] { new System.Drawing.Point(0, 0), new System.Drawing.Point(50, 50) });

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
