using System;
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
            /// Компоненты печатной платы
            /// </summary>
            
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
                    List<IElement> list = new List<IElement>();
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
                                                x = int.Parse(lineSplit[1]);
                                                y = int.Parse(lineSplit[2]);
                                                w = int.Parse(lineSplit[3]);
                                                h = int.Parse(lineSplit[4]);

                                                list.Add(new Pin(x * koeff, y * koeff, w * koeff, h * koeff));

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
                                                x = int.Parse(lineSplit[1]);
                                                y = int.Parse(lineSplit[2]);
                                                w = int.Parse(lineSplit[3]);
                                                h = int.Parse(lineSplit[4]);

                                                list.Add(new ProhibitionZone(x * koeff, y * koeff, w * koeff, h * koeff));

                                                index++;
                                            }
                                            break;
                                        }
                                        catch (Exception)
                                        {

                                            throw;
                                        }
                                    }
                            }
                        }
                    }

                    for (int i = 0; i < list.Count; ++i)
                    {
                        _config.Grid.Add(list[i]);
                    }
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
        /// Доступ к отдельным сетям
        /// </summary>
        /// <param name="index">Индекс (начиная 0)</param>
        /// <returns>Экземпляр Net</returns>
        /// <exception cref="OverflowException">Индекс находился вне границ массива.</exception>
        public Net GetNet(int index)
        {
            if (_config.Nets == null || index < 0 || index >= _config.Nets.Length)
                throw new OverflowException("Индекс находился вне границ массива.");
            return _config.Nets[index];
        }

        public string LastErr { get; private set; }
    }
}
