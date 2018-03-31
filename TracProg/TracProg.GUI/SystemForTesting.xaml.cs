﻿using System;
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
using TracProg.Calculation.Algoriths;

namespace TracProg.GUI
{
    public partial class SystemForTesting : Window
    {
        private class RowTest
        {
            public RowTest(int id, long time, int allNets, int countNonRealizedNetsBefore, int countNonRealizedNetsAfter)
            {
                ID = id;
                Time = time;
                AllNets = allNets;
                CountNonRealizedNetsBefore = countNonRealizedNetsBefore;
                CountNonRealizedNetsAfter = countNonRealizedNetsAfter;
            }

            /// <summary>
            /// Номер эксперимента
            /// </summary>
            public int ID { get; private set; }

            public int AllNets { get; private set; }
            public int CountNonRealizedNetsBefore { get; private set; }
            public int CountNonRealizedNetsAfter { get; private set; }

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
                    case "AllNets":
                        {
                            return "Всего трасс";
                        }
                    case "CountNonRealizedNetsBefore":
                        {
                            return "Не реализованных до ";
                        }
                    case "CountNonRealizedNetsAfter":
                        {
                            return "Не реализованных после";
                        }
                    case "Time":
                        {
                            return "Время (ms)";
                        }
                    default: return "";
                }
            }
        }

        private TestSettings _testSettings;
        private Thread _thread;

        private int _id;

        private List<RowTest> _lists;

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

            _exportButton.Click += _exportButton_Click;
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
                _progressBar.Visibility = System.Windows.Visibility.Collapsed;

                _testStartButton.IsEnabled = true;
                _testStopButton.IsEnabled = false;
                _settingsButton.IsEnabled = true;
            });
        }

        private void _exportButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void _testStopButton_Click(object sender, RoutedEventArgs e)
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

            UnlockInterface();
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
            Configuration config = new Configuration();

            Graphics old_g;
            Graphics new_g;
            Li li;

            if (!_isSingleMode)
            {
                _thread = new Thread(delegate()
                {
                    if (_testSettings != null && _testSettings.IsConfigurationCompleted == true)
                    {
                        LockInterface();
                        _lists.Clear();
                        _id = 0;

                        int koeff = 4;

                        int countIter = 10;
                        Dispatcher.Invoke(delegate() { _progressBar.Visibility = System.Windows.Visibility.Visible; });
                        Dispatcher.Invoke(delegate() { _progressBar.Maximum = countIter; });
                        Dispatcher.Invoke(delegate () { _progressBar.Value = 0; });

                        for (int i = 0; i < _testSettings.CountRuns; ++i)
                        {
                            config.GenerateRandomConfig(_testSettings.N, _testSettings.M, _testSettings.CountPins, _testSettings.CountProhibitionZones, _testSettings.CountPinsInNet, koeff);
                            li = new Li(config.Grid);

                            
                            Dictionary<int, Net> nonRealized = new Dictionary<int, Net>();
                            for (int numNet = 0; numNet < config.Net.Length; numNet++)
                            {
                                long localTime;
                                List<List<int>> track;
                                if (!li.FindPath(config.Net[numNet], out track, out localTime))
                                {
                                    nonRealized.Add(numNet + 1, config.Net[numNet]);
                                    config.Grid.MetallizeTrack(track, 1.0f, numNet + 1);
                                }
                                else
                                {
                                    config.Grid.MetallizeTrack(track, 1.0f, numNet + 1);
                                    //time += localTime;
                                }
                            }
                            Bitmap old_bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                            old_g = Graphics.FromImage(old_bmp);
                            old_g.Clear(System.Drawing.Color.Black);
                            config.Grid.Draw(old_g);
                            string path = _testSettings.FileOutPath + "\\test_old_" + i + ".bmp";
                            old_bmp.Save(path);

                            Bitmap new_bmp;
                            Dictionary<int, Net> goodRetracing = new Dictionary<int, Net>();

                            long time = 0;
                            Stopwatch sw = new Stopwatch();
                            int countNonRealizedNetsBefore = nonRealized.Count;
                            for (int curIter = 0; curIter < countIter; curIter++)
                            {
                                sw.Reset();
                                sw.Start();
                                foreach (var net in nonRealized)
                                {
                                    Alg alg = new Alg(config.Grid, config.Net.Length, net.Key);
                                    if (alg.FindPath(net.Value[0], net.Value[1]))
                                    {
                                        config.Grid[net.Value[0]].ViewElement._Color = System.Drawing.Color.FromArgb(0, 100, 0);
                                        config.Grid[net.Value[1]].ViewElement._Color = System.Drawing.Color.FromArgb(0, 100, 0);

                                        goodRetracing.Add(net.Key, net.Value);
                                    }
                                }
                                sw.Stop();
                                time += sw.ElapsedMilliseconds;

                                foreach (var item in goodRetracing)
                                {
                                    if (nonRealized.ContainsKey(item.Key))
                                    {
                                        nonRealized.Remove(item.Key);
                                    }
                                }

                                Dispatcher.Invoke(delegate () { _progressBar.Value = curIter + 1; });
                            }

                            new_bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                            new_g = Graphics.FromImage(new_bmp);
                            new_g.Clear(System.Drawing.Color.Black);

                            config.Grid.Draw(new_g);
                            path = _testSettings.FileOutPath + "\\test_new_" + i + ".bmp";
                            new_bmp.Save(path);
                            new_bmp = null;
                            new_g = null;

                            AddRow(time, config.Net.Length, countNonRealizedNetsBefore, nonRealized.Count);
                            old_g = null;
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

                        DataRowsToExel(_testSettings.FileOutPath);

                        UnlockInterface();
                    }
                });
                _thread.IsBackground = true;
                _thread.Start();
            }
            else
            {
                // FOR DEBUG

                config.ReadFromFile(_filePathImport);
                li = new Li(config.Grid);
                Bitmap bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                old_g = Graphics.FromImage(bmp);

                long time = 0;
                Dictionary<int, Net> nonRealized = new Dictionary<int, Net>();
                for (int numNet = 0; numNet < config.Net.Length; numNet++)
                {
                    long localTime;
                    List<List<int>> track;
                    if (!li.FindPath(config.Net[numNet], out track, out localTime))
                    {
                        nonRealized.Add(numNet + 1, config.Net[numNet]);
                    }
                    else
                    {
                        config.Grid.MetallizeTrack(track, 1.0f, numNet + 1);
                        time += localTime;
                    }
                }
                Bitmap old_bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                old_g = Graphics.FromImage(old_bmp);
                old_g.Clear(System.Drawing.Color.Empty);
                config.Grid.Draw(old_g);
                string path = "test_old.bmp";
                old_bmp.Save(path);

                int countNonRealizedNetsBefore = nonRealized.Count;
                Bitmap new_bmp;
                Dictionary<int, Net> goodRetracing = new Dictionary<int, Net>();

                //foreach (var net in nonRealized)
                {
                    //for (int pin = 0; pin < net.Value.Count - 1; pin++)
                    {
                        Alg alg = new Alg(config.Grid, config.Net.Length, nonRealized.ElementAt(0).Key);
                        if (alg.FindPath(nonRealized.ElementAt(0).Value[0], nonRealized.ElementAt(0).Value[1]))
                        {
                            goodRetracing.Add(nonRealized.ElementAt(0).Key, nonRealized.ElementAt(0).Value);
                        }
                    }
                }
                new_bmp = new Bitmap(config.Grid.Width, config.Grid.Height);
                new_g = Graphics.FromImage(new_bmp);
                new_g.Clear(System.Drawing.Color.Empty);

                config.Grid.Draw(new_g);
                path = "test_new.bmp";
                new_bmp.Save(path);
                new_bmp = null;
                new_g = null;

                AddRow(time, config.Net.Length, countNonRealizedNetsBefore, nonRealized.Count);
                old_g = null;
            }
        }

        private void _dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = RowTest.GetRussianNameField(e.PropertyName);
        }

        private void DataRowsToExel(string filePath)
        {
            var csv = new StringBuilder();
            foreach (RowTest row in _lists)
            {
                var newLine = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", row.ID, row.AllNets, row.CountNonRealizedNetsBefore, row.CountNonRealizedNetsAfter, row.Time);
                csv.AppendLine(newLine);
            }
            File.WriteAllText(filePath + @"\test.scv", csv.ToString());
        }

        private void AddRow(long time, int allNets, int countNonRealizedNetsBefore, int countNonRealizedNetsAfter)
        {
            Dispatcher.Invoke(delegate()
            {
                _lists.Add(new RowTest(_id++, time, allNets, countNonRealizedNetsBefore, countNonRealizedNetsAfter));
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

        private System.Windows.Controls.DataGrid SetGrigProperties(System.Windows.Controls.DataGrid data)
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