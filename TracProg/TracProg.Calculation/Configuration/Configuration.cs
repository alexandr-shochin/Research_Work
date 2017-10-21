﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        public Configuration(string pathConfigFile)
        {
            int koeff = 0;
            _config = new ConfigGrid();
            
            try
            {
                if (File.Exists(pathConfigFile))
                {
                    string line;
                    int x, y, w, h;
                    Dictionary<string, IElement> gridElements = new Dictionary<string,IElement>();
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
                                            x = int.Parse(lineSplit[1]);
                                            y = int.Parse(lineSplit[2]);
                                            w = int.Parse(lineSplit[3]);
                                            h = int.Parse(lineSplit[4]);

                                            _config.Grid = new Grid(x * koeff, y * koeff, w * koeff, h * koeff, koeff);
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

        public ErrorCode Serialize(string path)
        {
            throw new NotImplementedException();
        }

        public ErrorCode Deserialize(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает сетку трассировки
        /// </summary>
        public Grid Grid { get { return _config.Grid; } }

        /// <summary>
        /// Возвращает трассы
        /// </summary>
        public Net[] Net { get { return _config.Nets; } }


        public string LastErr { get; private set; }
    }
}
