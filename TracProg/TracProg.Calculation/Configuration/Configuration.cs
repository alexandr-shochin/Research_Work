using System;
using System.Collections;
using System.Collections.Generic;
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
            public IElement[] Components { get; set; }
            /// <summary>
            /// Сети печатной платы
            /// </summary>
            public Net[] Nets { get; set; }
        }

        private ConfigGrid _config;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="pathLEF">Путь к файлу конфигурации с расширением *.mylef</param>
        /// <param name="pathDEF">Путь к файлу конфигурации с расширением *.mydef</param>
        public Configuration(string pathLEF, string pathDEF)
        { 
            throw new NotImplementedException();
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="grid">Сетка трассировки</param>
        /// <param name="components">Компоненты печатной платы</param>
        /// <param name="nets">Сети печатной платы</param>
        public Configuration(Grid grid, IElement[] components, Net[] nets)
        {
            _config.Grid = grid;

            _config.Components = new IElement[components.Length];
            Array.Copy(components, 0, _config.Components, 0, components.Length);

            _config.Nets = new Net[nets.Length];
            Array.Copy(nets, 0, _config.Nets, 0, nets.Length);
        }

        public ErrorCode Serialize(string path)
        {
            throw new NotImplementedException();
        }

        public ErrorCode Deserialize(string path)
        {
            throw new NotImplementedException();
        }

        public void Create()
        { 
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает сетку трассировки
        /// </summary>
        public Grid Grid { get { return _config.Grid; } }

        /// <summary>
        /// Доступ к отдельным rомпонентам печатной платы
        /// </summary>
        /// <param name="index">Индекс (начиная 0)</param>
        /// <returns>Экземпляр Component</returns>
        /// <exception cref="OverflowException">Индекс находился вне границ массива.</exception>
        public IElement GetComponent(int index)
        {
            if (_config.Components == null || index < 0 || index >= _config.Components.Length)
                    throw new OverflowException("Индекс находился вне границ массива.");
            return _config.Components[index];    
        }

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
    }
}
