using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Windows.Shapes;
using TracProg.Calculation;
using TracProg.Calculation.Algoriths;

namespace TracProg.GUI
{
    public partial class SystemForTesting : Window
    {
        private class RowTest
        {
            public RowTest(int id, long time)
            {
                ID = id;
                Time = time;
            }

            /// <summary>
            /// Номер эксперимента
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Время выполнения
            /// </summary>
            public long Time { get; private set; }

            internal static object GetRussianNameField(string name)
            {
                switch (name)
                {
                    case "ID":
                        {
                            return "№";
                        }
                    case "Time":
                        {
                            return "Время работы алгоритма (ms)";
                        }
                    default: return "";
                }
            }
        }

        private TestSettings _testSettings;
        private Configuration config;
        private Thread _thread;

        private int _id;

        private List<RowTest> _lists;

        public SystemForTesting()
        {
            InitializeComponent();

            _progressBar.Visibility = Visibility.Collapsed;
            _testStopButton.IsEnabled = false;
            _settingsButton.IsEnabled = true;

            _lists = new List<RowTest>();
            SetGrigProperties(_dataGrid);
            _dataGrid.AutoGeneratingColumn += _dataGrid_AutoGeneratingColumn;
            _dataGrid.MouseDoubleClick += _dataGrid_MouseDoubleClick;
            _dataGrid.ItemsSource = _lists;

            _settingsButton.Click += _settingsButton_Click;
            _testStartButton.Click += _testStartButton_Click;
            _testStopButton.Click += _testStopButton_Click;
        }

        void _testStopButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate() { _statusBar.Text = string.Empty; });
            Dispatcher.Invoke(delegate() { _progressBar.Visibility = System.Windows.Visibility.Collapsed; });
            if (_thread != null && _thread.ThreadState == System.Threading.ThreadState.Background)
            {
                try
                {
                    _thread.Abort();
                }
                catch (Exception ex) { }
            }

            _testStartButton.IsEnabled = true;
            _testStopButton.IsEnabled = false;
            _settingsButton.IsEnabled = true;
        }

        void _dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_dataGrid.SelectedItem != null)
            {
                RowTest row = _dataGrid.SelectedItem as RowTest;
                if(row != null)
                {
                    string path = _testSettings.FileOutPath + "\\test_" + row.ID + ".bmp";
                    if(File.Exists(path))
                        Process.Start(path);
                }
            }
        }

        private void _testStartButton_Click(object sender, RoutedEventArgs e)
        {   
            _thread = new Thread(delegate ()
            {
                if (_testSettings != null && _testSettings.IsConfigurationCompleted == true)
                {
                    Dispatcher.Invoke(delegate()
                    {
                        _statusBar.Text = string.Empty;
                        _testStartButton.IsEnabled = false;
                        _testStopButton.IsEnabled = true;
                        _settingsButton.IsEnabled = false;
                    });
                    _lists.Clear();
                    _id = 0;
                    
                    int x = 0;
                    int y = 0;

                    int koeff = 4;

                    Graphics g;
                    config = new Configuration();

                    Dispatcher.Invoke(delegate() { _progressBar.Visibility = System.Windows.Visibility.Visible; });
                    Dispatcher.Invoke(delegate() { _progressBar.Maximum = _testSettings.CountRuns; });

                    for (int i = 0; i < _testSettings.CountRuns; ++i)
                    {
                        config.GenerateRandomConfig(x, y, _testSettings.N, _testSettings.M, _testSettings.CountPins, _testSettings.CountProhibitionZones, _testSettings.CountPinsInNet, koeff);
                        Li li = new Li(config.Grid, config.Net);
                        Bitmap bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                        g = Graphics.FromImage(bmp);

                        long time = li.FindPath()[0];

                        config.Grid.Draw(ref g);

                        string path = _testSettings.FileOutPath + "\\test_" + i + ".bmp";
                        bmp.Save(path);

                        AddRow(time);
                        g = null;

                        Dispatcher.Invoke(delegate() { _progressBar.Value = i + 1; });
                    }

                    if (_lists.Count > 0)
                    {
                        long average = 0;
                        for (int i = 0; i < _lists.Count; ++i)
                        {
                            average += _lists[i].Time;
                        }
                        average = average / _lists.Count;
                        Dispatcher.Invoke(delegate() { _statusBar.Text = "Среднее время: " + average.ToString() + " ms"; });
                    }

                    Dispatcher.Invoke(delegate() 
                    { 
                        _progressBar.Visibility = System.Windows.Visibility.Collapsed;

                        _testStartButton.IsEnabled = true;
                        _testStopButton.IsEnabled = false;
                        _settingsButton.IsEnabled = true;
                    });
                }
            });
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void _dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = RowTest.GetRussianNameField(e.PropertyName);
        }

        private void AddRow(long time)
        {
            Dispatcher.Invoke(delegate()
            {
                _lists.Add(new RowTest(_id++, time));
                _dataGrid.Items.Refresh();
            });
        }

        private void _settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_testSettings == null)
                _testSettings = new TestSettings();
            _testSettings.ShowDialog();
        }

        private DataGrid SetGrigProperties(DataGrid data)
        {
            data.AutoGenerateColumns = true;
            data.CanUserAddRows = false;
            data.CanUserDeleteRows = false;
            data.CanUserReorderColumns = false;
            data.CanUserResizeColumns = true;
            data.CanUserResizeRows = false;
            data.CanUserSortColumns = true;
            data.IsReadOnly = true;
            data.AlternatingRowBackground = System.Windows.Media.Brushes.LightGray;
            return data;
        }
    }
}
