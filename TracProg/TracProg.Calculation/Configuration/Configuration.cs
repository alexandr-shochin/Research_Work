using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    public class Configuration
    {
        [Serializable]
        private class ConfigGrid
        {
            /// <summary>
            /// Сетка трассировки
            /// </summary>
            public Grid Grid { get; set; }

            /// <summary>
            /// Трассы печатной платы
            /// </summary>
            public Dictionary<string, Net> Nets { get; set; }
        }

        private ConfigGrid _config = new ConfigGrid();

        public void ReadFromFile(string pathConfigFile)
        {
            int koeff = 0;

                if (File.Exists(pathConfigFile))
                {
                    string line;
                    int x, y, w, h;
                    Dictionary<string, IElement> gridElements = new Dictionary<string, IElement>();
                    int index;

                    using (StreamReader sr = new StreamReader(pathConfigFile, System.Text.Encoding.UTF8))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] lineSplit = line.Split();

                            switch (lineSplit[0])
                            {
                                case "KOEFF":
                                    {
                                        koeff = int.Parse(lineSplit[1]);
                                        if (koeff < 4)
                                        {
                                            throw new Exception("Коэфициент отображения должен быть не меньше 4!");
                                        }
                                        break;
                                    }
                                case "GRID":
                                    {
                                        w = int.Parse(lineSplit[1]);
                                        h = int.Parse(lineSplit[2]);

                                        _config.Grid = new Grid(w * koeff, h * koeff, koeff);
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

                                            gridElements.Add(name, new Pin(x * koeff, y * koeff, w * koeff, h * koeff));

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

                                            gridElements.Add(name, new ProhibitionZone(x * koeff, y * koeff, w * koeff, h * koeff));

                                            index++;
                                        }
                                        break;
                                    }
                                case "NETS":
                                    {
                                        int countNets = int.Parse(lineSplit[1]);
                                        _config.Nets = new Dictionary<string, Calculation.Net>();

                                        index = 0;
                                        while ((line = sr.ReadLine()) != null && index < countNets)
                                        {
                                            lineSplit = line.Split();
                                            int[] net = new int[lineSplit.Length - 1];
                                            for (int i = 0; i < net.Length; ++i)
                                            {
                                                if (lineSplit[i + 1] != "")
                                                {
                                                    IElement el = gridElements[lineSplit[i + 1]];
                                                    int j, k;
                                                    _config.Grid.GetIndexes(el.X, el.Y, out j, out k);
                                                    net[i] = _config.Grid.GetNum(j, k);
                                                }
                                            }
                                            _config.Nets[lineSplit[0]] = new Net(net);

                                            index++;
                                        }
                                        break;
                                    }
                            }
                        }
                    }

                    for (int i = 0; i < gridElements.Count; ++i)
                    {
                        _config.Grid.Add(gridElements.ElementAt(i).Value);
                    }
                }
                else
                {
                    throw new Exception("Файла " + pathConfigFile + " не существует");
                }
        }

        public void GenerateRandomConfig(int n, int m, int countNets, int countProhibitionZone, int countPinsInNet, int koeff = 4, int radius = 25)
        {
            _config.Grid = new Grid(n * koeff, m * koeff, koeff);

            Dictionary<string, IElement> gridElements = new Dictionary<string, IElement>();

            Random rand = new Random();

            List<Point> points = new List<Point>();

            //Генерация Pins
            _config.Nets = new Dictionary<string, Calculation.Net>();
            int currentNumPin = 0;
            int _countNets = 0;
            while(true)
            {
                Point p_1 = new Point(rand.Next(0, n - 1), rand.Next(0, m - 1));

                currentNumPin++;
                gridElements.Add(currentNumPin.ToString() + "_pin", new Pin(p_1.x * koeff, p_1.y * koeff, koeff, koeff));

                List<int> net = new List<int>();
                if (!points.Contains(p_1))
                {
                    points.Add(p_1);

                    int l, k;
                    _config.Grid.GetIndexes(p_1.x * koeff, p_1.y * koeff, out l, out k);
                    net.Add(_config.Grid.GetNum(l, k));

                    int _countPinsInNet = 1;
                    while (true)
                    {
                        int x = p_1.x + rand.Next(0, n - 1);
                        int y = p_1.y + rand.Next(0, m - 1);
                        Point p_i = new Point(x <= n - 1 ? x : n - 1,
                                              y <= m - 1 ? y : m - 1);

                        if (!points.Contains(p_i))
                        {
                            points.Add(p_i);

                            currentNumPin++;
                            gridElements.Add((currentNumPin).ToString() + "_pin", new Pin(p_i.x * koeff, p_i.y * koeff, koeff, koeff));

                            _config.Grid.GetIndexes(p_i.x * koeff, p_i.y * koeff, out l, out k);
                            net.Add(_config.Grid.GetNum(l, k));
                            _countPinsInNet++;
                        }
                        if (_countPinsInNet == countPinsInNet) break;
                    }
                    _config.Nets[_countNets + "_net"] = new Net(net.ToArray());
                    _countNets++;
                }

                if (_countNets == countNets) break;
            }

            //Генерация ProhibitionZone
            for (int i = 1; i <= countProhibitionZone; i++)
            {
                Point pZ = new Point(rand.Next(0, n - 1), rand.Next(0, m - 1));

                if (!points.Contains(pZ))
                {
                    points.Add(pZ);

                    gridElements.Add(i.ToString() + "_prZone", new ProhibitionZone(pZ.x * koeff, pZ.y * koeff, koeff, koeff));
                }
            }

            // добавление элементов в сетку
            foreach (var el in gridElements)
            {
                _config.Grid.Add(el.Value);
            }
        }

        #region Properties

        /// <summary>
        /// Возвращает сетку трассировки
        /// </summary>
        public Grid Grid { get { return _config.Grid; } }

        /// <summary>
        /// Возвращает трассы
        /// </summary>
        public Dictionary<string, Net> Nets { get { return _config.Nets; } }


        public string LastErr { get; private set; }

        #endregion
    }
}
