using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    [Serializable]
    public class Net
    {
        public class NetElement
        {
            /// <summary>
            /// Индекс компонены в массиве всех компонент
            /// </summary>
            public int IndexComponent { get; set; }
            /// <summary>
            /// Индкс Pin'а в массиве всех Pin'ов у Components[IndexComponent]
            /// </summary>
            public int IndexPinInComponent { get; set; }

            public override string ToString()
            {
                return IndexComponent + ", " + IndexPinInComponent;
            }
        }

        private NetElement[] _netElements;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="countElements">Количество NetElement'ов</param>
        /// <param name="netElements">Массив NetElement'ов</param>
        public Net(int countElements, NetElement[] netElements)
        {
            _netElements = new NetElement[countElements];
            Array.Copy(netElements, 0, _netElements, 0, countElements);
        }

        /// <summary>
        /// Доступ к отдельным элементам Net
        /// </summary>
        /// <param name="index">Индекс (начиная 0)</param>
        /// <returns>Экземпляр NetElement</returns>
        /// <exception cref="OverflowException">Индекс находился вне границ массива.</exception>
        public NetElement this[int index]
        {
            get
            {
                if (_netElements == null || index < 0 || index >= _netElements.Length)
                    throw new OverflowException("Индекс находился вне границ массива.");
                return _netElements[index];
            }
        }

        public override string ToString()
        {
            return "Count = " + _netElements.Length;
        }
    }
}
