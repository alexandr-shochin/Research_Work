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
            /// Сети печатной платы
            /// </summary>
            public Net[] Nets { get; set; }
        }

        private ConfigGrid _config;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="pathConfigFile">Путь к файлу конфигурации с расширением *.mydeflef</param>
        public Configuration()
        {
            _config = new ConfigGrid();
        }

        /// <summary>
        /// Прочитать из файла
        /// </summary>
        /// <param name="pathConfigFile">Абсолютный путь до файла конфигурации</param>
        public void ReadFromFile(string pathConfigFile)
        {
            int koeff = 0;

            try
            {
                if (File.Exists(pathConfigFile))
                {
                    string line;
                    int x, y, w, h;
                    Dictionary<string, IElement> gridElements = new Dictionary<string, IElement>();
                    int index;
                    List<Net> nets = new List<Net>();

                    using (StreamReader sr = new StreamReader(pathConfigFile, System.Text.Encoding.UTF8))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] lineSplit = line.Split();

                            switch (lineSplit[0])
                            {
                                case "KOEFF":
                                    {
                                        try
                                        {
                                            koeff = int.Parse(lineSplit[1]);
                                            if (koeff < 4)
                                            {
                                                throw new Exception("Коэфициент отображения должен быть не меньше 4!");
                                            }
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            throw ex;
                                        }


                                    }
                                case "GRID":
                                    {
                                        try
                                        {
                                            w = int.Parse(lineSplit[1]);
                                            h = int.Parse(lineSplit[2]);

                                            _config.Grid = new Grid(w * koeff, h * koeff, koeff);
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            throw ex;
                                        }
                                    }
                                case "COMPONENTS":
                                    {
                                        try
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
                                        catch (Exception ex)
                                        {
                                            throw ex;
                                        }
                                    }
                                case "PROHIBITION_ZONE":
                                    {
                                        try
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
                                        catch (Exception ex)
                                        {

                                            throw ex;
                                        }
                                    }
                                case "NETS":
                                    {
                                        try
                                        {
                                            int countNets = int.Parse(lineSplit[1]);
                                            _config.Nets = new Net[countNets];

                                            index = 0;
                                            while ((line = sr.ReadLine()) != null && index < countNets)
                                            {
                                                lineSplit = line.Split();
                                                int[] net = new int[lineSplit.Length - 1];
                                                for (int i = 0; i < net.Length; ++i)
                                                {
                                                    IElement el = gridElements[lineSplit[i + 1]];
                                                    int j, k;
                                                    _config.Grid.GetIndexes(el.X, el.Y, out j, out k);
                                                    net[i] = _config.Grid.GetNum(j, k);
                                                }
                                                nets.Add(new Net(net));

                                                index++;
                                            }
                                            break;
                                        }
                                        catch (Exception ex)
                                        {

                                            throw ex;
                                        }
                                    }
                            }
                        }
                    }

                    for (int i = 0; i < gridElements.Count; ++i)
                    {
                        _config.Grid.Add(gridElements.ElementAt(i).Value);
                    }

                    _config.Nets = nets.ToArray();
                }
                else
                {
                    throw new Exception("Файла " + pathConfigFile + " не существует");
                }
            }
            catch (Exception ex)
            {
                LastErr = ex.Message;
            }
        }

        /// <summary>
        /// Генерация случайной конфигурации
        /// </summary>
        /// <param name="n">Высота сетки</param>
        /// <param name="m">Ширина сетки</param>
        /// <param name="koeff">Коэфициент масштабирования</param>
        public void GenerateRandomConfig(int n, int m, int countPairPins, int countProhibitionZone, int countNets, int koeff = 4)
        {
            _config.Grid = new Grid(n * koeff, m * koeff, koeff);

            int radius = 25;

            Dictionary<string, IElement> gridElements = new Dictionary<string, IElement>();

            Random rand = new Random();

            //Генерация Pins
            List<Tuple<int, int>> pairs = new List<Tuple<int, int>>();
            List<Net> nets = new List<Net>();
            int countp = 0;
            int j = 0;
            while(true)
            {
                try
                {
                    Tuple<int, int> pair_i = Tuple.Create(rand.Next(0, n - 1), rand.Next(0, m - 1));

                    int pair_j_1 = pair_i.Item1 + rand.Next(0, radius);
                    int pair_j_2 = pair_i.Item2 + rand.Next(0, radius);
                    pair_j_1 = pair_j_1 <= n - 1 ? pair_j_1 : n - 1;
                    pair_j_2 = pair_j_2 <= n - 1 ? pair_j_2 : m - 1;

                    Tuple<int, int> pair_j = Tuple.Create(pair_j_1, pair_j_2);
                    if (pairs.FindIndex(x => x.Item1 == pair_i.Item1 && x.Item2 == pair_i.Item2) == -1 &&
                        pairs.FindIndex(x => x.Item1 == pair_j.Item1 && x.Item2 == pair_j.Item2) == -1)
                    {
                        gridElements.Add(countp.ToString() + "_pin", new Pin(pair_i.Item1 * koeff, pair_i.Item2 * koeff, koeff, koeff));
                        gridElements.Add((countp + 1).ToString() + "_pin", new Pin(pair_j.Item1 * koeff, pair_j.Item2 * koeff, koeff, koeff));
                        j++;
                        countp += 2;

                        // генерация nets
                        int[] nums = new int[2];
                        int l, k;
                        _config.Grid.GetIndexes(pair_i.Item1 * koeff, pair_i.Item2 * koeff, out l, out k);
                        nums[0] = _config.Grid.GetNum(l, k);
                        _config.Grid.GetIndexes(pair_j.Item1 * koeff, pair_j.Item2 * koeff, out l, out k);
                        nums[1] = _config.Grid.GetNum(l, k);
                        nets.Add(new Net(nums));
                        
                    }
                    if (j == countPairPins) break;
                    
                }
                catch (ArgumentException) { }
                
            }
            _config.Nets = nets.ToArray();

            

            //Генерация ProhibitionZone           
            for (int i = 0; i < countProhibitionZone; )
            {
                try
                {
                    gridElements.Add(i.ToString() + "_prZone", new ProhibitionZone(rand.Next(0, n - 1) * koeff, rand.Next(0, m - 1) * koeff, koeff, koeff));
                    i++;
                }
                catch (ArgumentException) { }
            }

            // добавление элементов в сетку
            foreach (var el in gridElements)
            {
                _config.Grid.Add(el.Value);
            }
        }

        public ErrorCode Serialize(string path)
        {
            throw new NotImplementedException();
        }

        public ErrorCode Deserialize(string path)
        {
            throw new NotImplementedException();
        }

        #region Properties

        /// <summary>
        /// Возвращает сетку трассировки
        /// </summary>
        public Grid Grid { get { return _config.Grid; } }

        /// <summary>
        /// Возвращает трассы
        /// </summary>
        public Net[] Net { get { return _config.Nets; } }


        public string LastErr { get; private set; }

        #endregion
    }
}
