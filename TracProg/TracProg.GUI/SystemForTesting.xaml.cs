using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
                            return "ID";
                        }
                    case "Time":
                        {
                            return "Время";
                        }
                    default: return "";
                }
            }
        }

        private TestSettings _testSettings;

        private int _id;

        private List<RowTest> _lists;

        public SystemForTesting()
        {
            InitializeComponent();

            _progressBar.Visibility = System.Windows.Visibility.Collapsed;
            _settingsButton.Click += _settingsButton_Click;
            _dataGrid.AutoGeneratingColumn += _dataGrid_AutoGeneratingColumn;
            _lists = new List<RowTest>();
            SetGrigProperties(_dataGrid);
            _dataGrid.ItemsSource = _lists;
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
            data.AlternatingRowBackground = Brushes.LightGray;
            return data;
        }
    }
}
