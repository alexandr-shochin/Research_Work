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
        private int[] _netElements;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="netElements">Массив NetElement'ов</param>
        public Net(int[] netElements)
        {
            _netElements = new int[netElements.Length];
            Array.Copy(netElements, 0, _netElements, 0, netElements.Length);
        }

        public bool Contains(int net)
        {
            return Array.IndexOf(this._netElements, net) != -1 ? true : false;
        }

        /// <summary>
        /// Доступ к отдельным элементам Net
        /// </summary>
        /// <param name="index">Индекс (начиная 0)</param>
        /// <returns>Экземпляр NetElement</returns>
        /// <exception cref="OverflowException">Индекс находился вне границ массива.</exception>
        public int this[int index]
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
            return string.Join(", ", _netElements);
        }

        public int Count
        {
            get
            {
                if (_netElements.Length == 0)
                {
                    return 0;
                }
                else
                {
                    return _netElements.Length;
                }
            }
        }
    }
}
