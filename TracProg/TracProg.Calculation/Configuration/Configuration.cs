using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TracProg.Calculation.BoardElements;

namespace TracProg.Calculation
{
    public class Configuration
    {
        private string _filePath;

        private int _koeff = 0;

        private Dictionary<string, IBoardElement> _pins = new Dictionary<string, IBoardElement>();
        private Dictionary<string, IBoardElement> _prohibitionZones = new Dictionary<string, IBoardElement>();

        private TraceGrid _grid = null;
        private Dictionary<string, Net> _nets = new Dictionary<string, Net>();

        public void ReadFromFile(string pathConfigFile)
        {
            string line;
            int x, y, w, h;
            Dictionary<string, IBoardElement> gridElements = new Dictionary<string, IBoardElement>();
            int index;

            using (StreamReader sr = new StreamReader(pathConfigFile, System.Text.Encoding.UTF8))
            {
                _filePath = pathConfigFile;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineSplit = line.Split();

                    switch (lineSplit[0])
                    {
                        case "KOEFF":
                            {
                                _koeff = int.Parse(lineSplit[1]);
                                if (_koeff < 4)
                                {
                                    throw new Exception("Коэфициент отображения должен быть не меньше 4!");
                                }
                                break;
                            }
                        case "GRID":
                            {
                                w = int.Parse(lineSplit[1]);
                                h = int.Parse(lineSplit[2]);

                                _grid = new TraceGrid("Grid", w * _koeff, h * _koeff, _koeff);
                                break;
                            }
                        case "COMPONENTS":
                            {
                                int countComponents = int.Parse(lineSplit[1]);

                                index = 0;
                                while ((line = sr.ReadLine()) != null && index < countComponents)
                                {
                                    lineSplit = line.Split();
                                    string name = lineSplit[0];
                                    x = int.Parse(lineSplit[1]);
                                    y = int.Parse(lineSplit[2]);
                                    w = int.Parse(lineSplit[3]);
                                    h = int.Parse(lineSplit[4]);

                                    gridElements.Add(name, new Pin(name, x * _koeff, y * _koeff, w * _koeff, h * _koeff));

                                    index++;
                                }
                                break;
                            }
                        case "PROHIBITION_ZONE":
                            {
                                int countProhZones = int.Parse(lineSplit[1]);

                                index = 0;
                                while ((line = sr.ReadLine()) != null && index < countProhZones)
                                {
                                    lineSplit = line.Split();
                                    string name = lineSplit[0];
                                    x = int.Parse(lineSplit[1]);
                                    y = int.Parse(lineSplit[2]);
                                    w = int.Parse(lineSplit[3]);
                                    h = int.Parse(lineSplit[4]);

                                    gridElements.Add(name, new ProhibitionZone(name, x * _koeff, y * _koeff, w * _koeff, h * _koeff));

                                    index++;
                                }
                                break;
                            }
                        case "NETS":
                            {
                                int countNets = int.Parse(lineSplit[1]);
                                _nets = new Dictionary<string, Net>();
                                List<Tuple<string, Net>> nets = new List<Tuple<string, Net>>();

                                index = 0;
                                while ((line = sr.ReadLine()) != null && index < countNets)
                                {
                                    lineSplit = line.Split();
                                    int[] net = new int[lineSplit.Length - 1];
                                    for (int i = 0; i < net.Length; ++i)
                                    {
                                        if (lineSplit[i + 1] != "")
                                        {
                                            IBoardElement el = gridElements[lineSplit[i + 1]];
                                            int j, k;
                                            _grid.GetIndexes(el.X, el.Y, out j, out k);
                                            net[i] = _grid.GetNum(j, k);
                                        }
                                    }
                                    nets.Add(Tuple.Create(lineSplit[0], new Net(net)));

                                    index++;
                                }

                                nets.Sort(delegate(Tuple<string, Net> T1, Tuple<string, Net> T2)
                                {
                                    if (T1.Item2.Count > T2.Item2.Count) return 1;
                                    if (T1.Item2.Count < T2.Item2.Count) return -1;
                                    return 0;
                                });

                                foreach (var item in nets)
                                {
                                    _nets[item.Item1] = item.Item2;
                                }

                                break;
                            }
                    }
                }
            }

            List<string> errorElements = new List<string>();
            foreach (var element in gridElements)
	        {
                if (_grid.Contains(element.Value))
                {
                    if (element.Value is Pin)
                    {
                        _pins[element.Key] = element.Value;
                    }
                    else if (element.Value is ProhibitionZone)
                    {
                        _prohibitionZones[element.Key] = element.Value;
                    }

                    _grid.Add(element.Value);
                }
                else
                {
                    errorElements.Add(element.Key);
                }
	        }
        }

        public void GenerateRandomConfig(int n, int m, int countNets, int countProhibitionZone, int countPinsInNet, int koeff = 4, int radius = 25)
        {
            _grid = new TraceGrid("Random_grid", n * koeff, m * koeff, koeff);

            Dictionary<string, IBoardElement> gridElements = new Dictionary<string, IBoardElement>();

            Random rand = new Random();

            List<Point> points = new List<Point>();

            //Генерация Pins
            _nets = new Dictionary<string, Net>();
            List<Tuple<string, Net>> nets = new List<Tuple<string, Net>>();
            int currentNumPin = 0;
            int _countNets = 0;
            while(true)
            {
                Point p_1 = new Point(rand.Next(0, n - 1), rand.Next(0, m - 1));

                currentNumPin++;
                gridElements.Add(currentNumPin.ToString() + "_pin", new Pin(currentNumPin.ToString() + "_pin", p_1.X * koeff, p_1.Y * koeff, koeff, koeff));

                List<int> net = new List<int>();
                if (!points.Contains(p_1))
                {
                    points.Add(p_1);

                    int l, k;
                    _grid.GetIndexes(p_1.X * koeff, p_1.Y * koeff, out l, out k);
                    net.Add(_grid.GetNum(l, k));

                    int _countPinsInNet = 1;
                    while (true)
                    {
                        int x = p_1.X + rand.Next(0, radius);
                        int y = p_1.Y + rand.Next(0, radius);
                        Point p_i = new Point(x <= n - 1 ? x : n - 1,
                                              y <= m - 1 ? y : m - 1);

                        if (!points.Contains(p_i))
                        {
                            points.Add(p_i);

                            currentNumPin++;
                            gridElements.Add(currentNumPin.ToString() + "_pin", new Pin(currentNumPin.ToString() + "_pin", p_i.X * koeff, p_i.Y * koeff, koeff, koeff));

                            _grid.GetIndexes(p_i.X * koeff, p_i.Y * koeff, out l, out k);
                            net.Add(_grid.GetNum(l, k));
                            _countPinsInNet++;
                        }
                        if (_countPinsInNet == countPinsInNet) break;
                    }

                    nets.Add(Tuple.Create(_countNets + "_net", new Net(net.ToArray())));
                    _countNets++;
                }

                if (_countNets == countNets) break;
            }

            nets.Sort(delegate(Tuple<string, Net> T1, Tuple<string, Net> T2)
            {
                if (T1.Item2.Count > T2.Item2.Count) return 1;
                if (T1.Item2.Count < T2.Item2.Count) return -1;
                return 0;
            });
            foreach (var item in nets)
            {
                _nets[item.Item1] = item.Item2;
            }

            //Генерация ProhibitionZone
            for (int i = 1; i <= countProhibitionZone; i++)
            {
                Point pZ = new Point(rand.Next(0, n - 1), rand.Next(0, m - 1));

                if (!points.Contains(pZ))
                {
                    points.Add(pZ);

                    gridElements.Add(i.ToString() + "_prZone", new ProhibitionZone(i.ToString() + "_prZone", pZ.X * koeff, pZ.Y * koeff, koeff, koeff));
                }
            }

            // добавление элементов в сетку
            foreach (var element in gridElements)
            {
                if (element.Value is Pin)
                {
                    _pins[element.Key] = element.Value;
                }
                else if (element.Value is ProhibitionZone)
                {
                    _prohibitionZones[element.Key] = element.Value;
                }

                _grid.Add(element.Value);
            }
        }

        #region Properties

        public string FilePath { get { return _filePath; } }

        public int Koeff { get { return _koeff; } }

        /// <summary>
        /// Пины печатной платы
        /// </summary>
        public Dictionary<string, IBoardElement> Pins { get { return _pins; } }

        /// <summary>
        /// Зоны запрета трассировки печатной платы
        /// </summary>
        public Dictionary<string, IBoardElement> ProhibitionZones { get { return _prohibitionZones; } }

        /// <summary>
        /// Сетка трассировки
        /// </summary>
        public TraceGrid Grid { get { return _grid; } }

        /// <summary>
        /// Трассы печатной платы
        /// </summary>
        public Dictionary<string, Net> Nets { get { return _nets; } }

        #endregion
    }
}
