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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TracProg.Calculation;
using TracProg.Calculation.Algorithms;
using DataGrid = System.Windows.Controls.DataGrid;

namespace TracProg.GUI
{
    public partial class SystemForTesting : Window
    {
        private class RowTest
        {
            public RowTest(int id, long time, int allNets, int countNonRealizedNetsBefore, int countNonRealizedNetsAfter, float percentageTracingBefore, float percentageTracingAfter)
            {
                ID = id;
                Time = time;
                AllNets = allNets;
                CountNonRealizedNetsBefore = countNonRealizedNetsBefore;
                CountNonRealizedNetsAfter = countNonRealizedNetsAfter;
                PercentageTracingBefore = percentageTracingBefore;
                PercentageTracingAfter = percentageTracingAfter;
            }

            /// <summary>
            /// Номер эксперимента
            /// </summary>
            public int ID { get; }

            public int AllNets { get; }
            public int CountNonRealizedNetsBefore { get; }
            public int CountNonRealizedNetsAfter { get; }
            public float PercentageTracingBefore { get; }
            public float PercentageTracingAfter { get; }

            /// <summary>
            /// Время выполнения
            /// </summary>
            public long Time { get; }

            internal static object GetRussianNameField(string name)
            {
                switch (name)
                {
                    case "ID":
                        {
                            return "№";
                        }
                    case "AllNets":
                        {
                            return "N";
                        }
                    case "CountNonRealizedNetsBefore":
                        {
                            return "n1";
                        }
                    case "CountNonRealizedNetsAfter":
                        {
                            return "n2";
                        }
                    case "PercentageTracingBefore":
                        {
                            return "p1";
                        }
                    case "PercentageTracingAfter":
                        {
                            return "p2";
                        }
                    case "Time":
                        {
                            return "t (ms)";
                        }
                    default: return "";
                }
            }
        }

        private TestSettings _testSettings;
        private Thread _thread;

        private int _id;

        private readonly List<RowTest> _lists;

        private string _filePathImport;

        private bool _isSingleMode;

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

            _importButton.Click += _importButton_Click;

            _settingsButton.Click += _settingsButton_Click;
            _testStartButton.Click += _testStartButton_Click;
            _testStopButton.Click += _testStopButton_Click;

            
        }

        void _importButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы конфигурации (*.mydeflef)|*.mydeflef";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _filePathImport = openFileDialog.FileName;
                _isSingleMode = true;
            }
        }

        private void LockInterface()
        {
            Dispatcher.Invoke(delegate()
            {
                _statusBar.Text = string.Empty;
                _testStartButton.IsEnabled = false;
                _testStopButton.IsEnabled = true;
                _settingsButton.IsEnabled = false;
            });
        }

        private void UnlockInterface()
        {
            Dispatcher.Invoke(delegate()
            {
                _progressBar.Visibility = Visibility.Collapsed;

                _testStartButton.IsEnabled = true;
                _testStopButton.IsEnabled = false;
                _settingsButton.IsEnabled = true;
            });
        }

        private void _testStopButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate() { _statusBar.Text = string.Empty; });
            Dispatcher.Invoke(delegate() { _progressBar.Visibility = Visibility.Collapsed; });
            if (_thread != null && _thread.ThreadState == System.Threading.ThreadState.Background)
            {
                try
                {
                    _thread.Abort();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            UnlockInterface();
        }

        void _dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_dataGrid.SelectedItem != null)
            {
                RowTest row = _dataGrid.SelectedItem as RowTest;
                if (row != null)
                {
                    string path = _testSettings.FileOutPath + "\\test_" + row.ID + ".bmp";
                    if (File.Exists(path))
                        Process.Start(path);
                }
            }
        }

        private void _testStartButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration config = new Configuration();

            Graphics oldG;
            Graphics newG;
            WaveTraceAlgScheme li;
            _thread = new Thread(delegate()
                {
                    int countIter = 5;
                    Dispatcher.Invoke(delegate() { _progressBar.Visibility = Visibility.Visible; });
                    Dispatcher.Invoke(delegate() { _progressBar.Maximum = countIter; });
                    Dispatcher.Invoke(delegate() { _progressBar.Value = 0; });

                    if (!_isSingleMode)
                    {

                        if (_testSettings != null && _testSettings.IsConfigurationCompleted)
                        {
                            LockInterface();
                            _lists.Clear();
                            _id = 0;

                            int koeff = 4;

                            AddTitleToExel(_testSettings.FileOutPath);
                            for (int i = 0; i < _testSettings.CountRuns; ++i)
                            {
                                config.GenerateRandomConfig(_testSettings.N, _testSettings.M, _testSettings.CountNets, _testSettings.CountProhibitionZones, _testSettings.CountPinsInNet, koeff, 25);
                                li = new WaveTraceAlgScheme(config.Grid);

                                Bitmap bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                                oldG = Graphics.FromImage(bmp);
                                oldG.Clear(System.Drawing.Color.White);
                                config.Grid.Draw(oldG);
                                string path = "test_clear.bmp";
                                bmp.Save(path);

                                int countRealizedPinsBefore = 0;
                                Dictionary<string, Tuple<List<int>, List<int>>> allNonRealizedTracks = new Dictionary<string, Tuple<List<int>, List<int>>>();
                                foreach (var net in config.Nets)
                                {
                                    int localCountRealizedPinsBefore = net.Value.Count;

                                    List<List<int>> realizedTracks;
                                    List<int> nonRealizedPins;
                                    li.FindPath(net.Key, net.Value, out realizedTracks, out nonRealizedPins);
                                    if (nonRealizedPins.Count != 0)
                                    {
                                        localCountRealizedPinsBefore -= nonRealizedPins.Count;

                                        List<int> allPins = new List<int>();
                                        for (int n = 0; n < net.Value.Count; ++n)
                                        {
                                            allPins.Add(net.Value[n]);
                                        }

                                        allNonRealizedTracks[net.Key] = Tuple.Create(allPins, nonRealizedPins);
                                    }

                                    countRealizedPinsBefore += localCountRealizedPinsBefore;
                                }
                                Bitmap oldBmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                                oldG = Graphics.FromImage(oldBmp);
                                oldG.Clear(System.Drawing.Color.White);
                                config.Grid.Draw(oldG);
                                path = _testSettings.FileOutPath + "\\test_old_" + i + ".bmp";
                                oldBmp.Save(path);

                                RetraceAlgScheme retraceAlgScheme = new RetraceAlgScheme(config, countIter, countRealizedPinsBefore, allNonRealizedTracks);
                                retraceAlgScheme.IterFinishEvent += (numIter) =>
                                    {
                                        Dispatcher.Invoke(delegate() { _progressBar.Value = numIter; });
                                    };
                                int countRealizedPinsAfter;
                                long time = retraceAlgScheme.Calculate(out countRealizedPinsAfter);

                                Bitmap newBmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                                newG = Graphics.FromImage(newBmp);
                                newG.Clear(System.Drawing.Color.White);

                                config.Grid.Draw(newG);
                                path = _testSettings.FileOutPath + "\\test_new_" + i + ".bmp";
                                newBmp.Save(path);
                                newBmp = null;
                                newG = null;

                                float percentageTracingAfter = (float)((100.0 * countRealizedPinsAfter) / config.Pins.Count);
                                float percentageTracingBefore = (float)((100.0 * countRealizedPinsBefore) / config.Pins.Count);
                                AddRow(time, config.Pins.Count, countRealizedPinsBefore, countRealizedPinsAfter, percentageTracingBefore, percentageTracingAfter);
                                oldG = null;
                            }

                            DataRowsToExel(_testSettings.FileOutPath);

                            if (_lists.Count > 0)
                            {
                                long average = 0;
                                float perAvAfter = 0.0f;
                                float perAvBefore = 0.0f;
                                for (int i = 0; i < _lists.Count; ++i)
                                {
                                    average += _lists[i].Time;
                                    perAvAfter += _lists[i].PercentageTracingAfter;
                                    perAvBefore += _lists[i].PercentageTracingBefore;
                                }
                                average = average / _lists.Count;
                                perAvAfter = perAvAfter / _lists.Count;
                                perAvBefore = perAvBefore / _lists.Count;
                                WriteAllTextToExel(_testSettings.FileOutPath, "Средний процент реализованных пинов до процедуры перетрассировки: " + perAvBefore);
                                WriteAllTextToExel(_testSettings.FileOutPath, "Средний процент реализованных пинов после процедуры перетрассировки: " + perAvAfter);
                                WriteAllTextToExel(_testSettings.FileOutPath, "Средний процент улучшения трассировки: " + (perAvAfter - perAvBefore));
                                Dispatcher.Invoke(delegate() { _statusBar.Text = "Среднее время: " + average + " ms | " + "Средняя разница в процентах трассировки: " + (perAvAfter - perAvBefore); });
                            }

                            UnlockInterface();
                        }
                    }
                    else
                    {
                        config.ReadFromFile(_filePathImport);
                        li = new WaveTraceAlgScheme(config.Grid);
                        Bitmap bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                        oldG = Graphics.FromImage(bmp);
                        oldG.Clear(System.Drawing.Color.White);
                        config.Grid.Draw(oldG);
                        string path = "test_clear.bmp";
                        bmp.Save(path);

                        int countRealizedPinsBefore = 0;
                        Dictionary<string, Tuple<List<int>, List<int>>> allNonRealizedTracks = new Dictionary<string, Tuple<List<int>, List<int>>>();
                        foreach (var net in config.Nets)
                        {
                            int localCountRealizedPinsBefore = net.Value.Count;

                            List<List<int>> realizedTracks;
                            List<int> nonRealizedPins;
                            li.FindPath(net.Key, net.Value, out realizedTracks, out nonRealizedPins);
                            if (nonRealizedPins.Count != 0)
                            {
                                localCountRealizedPinsBefore -= nonRealizedPins.Count;

                                List<int> allPins = new List<int>();
                                for (int n = 0; n < net.Value.Count; ++n)
                                {
                                    allPins.Add(net.Value[n]);
                                }

                                allNonRealizedTracks[net.Key] = Tuple.Create(allPins, nonRealizedPins);
                            }

                            countRealizedPinsBefore += localCountRealizedPinsBefore;
                        }
                        Bitmap oldBmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                        oldG = Graphics.FromImage(oldBmp);
                        oldG.Clear(System.Drawing.Color.White);
                        config.Grid.Draw(oldG);
                        path = "test_old.bmp";
                        oldBmp.Save(path);

                        RetraceAlgScheme retraceAlgScheme = new RetraceAlgScheme(config, countIter, countRealizedPinsBefore, allNonRealizedTracks);
                        retraceAlgScheme.IterFinishEvent += (numIter) =>
                        {
                            Dispatcher.Invoke(delegate() { _progressBar.Value = numIter; });
                        };
                        int countRealizedPinsAfter;
                        long time = retraceAlgScheme.Calculate(out countRealizedPinsAfter);

                        Bitmap newBmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                        newG = Graphics.FromImage(newBmp);
                        newG.Clear(System.Drawing.Color.White);

                        config.Grid.Draw(newG);
                        path = "test_new.bmp";
                        newBmp.Save(path);
                        newG = null;

                        float percentageTracingAfter = (float)((100.0 * countRealizedPinsAfter) / config.Pins.Count);
                        float percentageTracingBefore = (float)((100.0 * countRealizedPinsBefore) / config.Pins.Count);
                        AddRow(time, config.Pins.Count, countRealizedPinsBefore, countRealizedPinsAfter, percentageTracingBefore, percentageTracingAfter);
                        oldG = null;
                    }
                });
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void _dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = RowTest.GetRussianNameField(e.PropertyName);
        }

        private void WriteAllTextToExel(string filePath, string text)
        {
            string fullName = filePath + @"\report.txt";

            File.AppendAllText(fullName, text + "\n");
        }
        private void AddTitleToExel(string filePath)
        {
            string fullName = filePath + @"\report.txt";
            if (File.Exists(fullName)) File.Delete(fullName);
            var csv = new StringBuilder();
            csv.AppendLine("Ширина сетки трассировки: " + _testSettings.M);
            csv.AppendLine("Высота сетки трассировки: " + _testSettings.N);
            csv.AppendLine("Количество трасс: " + _testSettings.CountNets);
            csv.AppendLine("Количество зон запрета: " + _testSettings.CountProhibitionZones);
            csv.AppendLine("Количество элементов в трассе: " + _testSettings.CountPinsInNet);
            csv.AppendLine("Количество запусков: " + _testSettings.CountRuns);
            csv.AppendLine("");
            csv.AppendLine(string.Format("№\tN\tn1\tn2\tp1\tp2\tt(ms)"));

            WriteAllTextToExel(filePath, csv.ToString());
        }
        private void DataRowsToExel(string filePath)
        {
            var csv = new StringBuilder();
            foreach (RowTest row in _lists)
            {
                var newLine = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", row.ID, row.AllNets, row.CountNonRealizedNetsBefore, row.CountNonRealizedNetsAfter, row.PercentageTracingBefore, row.PercentageTracingAfter, row.Time);
                csv.AppendLine(newLine);
            }

            WriteAllTextToExel(filePath, csv.ToString());
        }

        private void AddRow(long time, int allNets, int countNonRealizedNetsBefore, int countNonRealizedNetsAfter, float percentageTracingBefore, float percentageTracingAfter)
        {
            Dispatcher.Invoke(delegate()
            {
                _lists.Add(new RowTest(_id++, time, allNets, countNonRealizedNetsBefore, countNonRealizedNetsAfter, percentageTracingBefore, percentageTracingAfter));
                _dataGrid.Items.Refresh();
            });
        }

        private void _settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_testSettings == null)
            {
                _testSettings = new TestSettings();
                _isSingleMode = false;
            }
            _testSettings.ShowDialog();
        }

        private void SetGrigProperties(DataGrid data)
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
            return;
        }
    }
}
