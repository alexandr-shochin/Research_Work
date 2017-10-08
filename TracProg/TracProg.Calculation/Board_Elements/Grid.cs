using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    [Serializable]
    public class Grid
    {
        private byte[] _grid;

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;

            _grid = new byte[Width * Height];
        }

        #region Public methods

        /// <summary>
        /// Получить значение в сетке
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public byte GetElement(int i, int j)
        {
            return _grid[i + j * Width];
        }

        /// <summary>
        /// Получить значение в сетке
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public byte GetElement(int num)
        {
            return _grid[num];
        }

        /// <summary>
        /// Получить номера строки и столбца элемента в сетке по номеру ячейки
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public Point GetCoords(int num)
        {
            int i = num % Width;
            int j = (num - i) / Width;

            return new Point(i, j);
        }

        /// <summary>
        /// Получить номер ячейки в сетке по номеру строки и столбца
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public int GetNum(int i, int j)
        {
            return i + j * Width;
        }

        public void SetValue(int num, GridValue value)
        {
            SetBit(ref _grid[num], (int)value, true);
        }


        /// <summary>
        /// Проверка, что данная ячейка является металом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsMetal(int num)
        {
            return GetBit(_grid[num], 0);
        }

        /// <summary>
        /// Проверка, что данная ячейка является Pin'ом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsPin(int num)
        {
            return GetBit(_grid[num], 1);
        }

        /// <summary>
        /// Проверка, что данная ячейка является зоной запрета
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsProhibitionZone(int num)
        {
            return GetBit(_grid[num], 2);
        }

        public override string ToString()
        {
            return "Count = " + Count;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Установить бит в байте
        /// </summary>
        /// <param name="el">Байт в котором нужно установить бит</param>
        /// <param name="numBit">Номер бита</param>
        /// <param name="value">значение: true - 1, false - 0</param>
        private void SetBit(ref byte el, int numBit, bool value)
        {
            byte mask = (byte)(1 << numBit); // 0000100000....

            if (value)
            {
                el = (byte)(el | mask);// 1 | x = 1
            }
            else
            {
                el = (byte)(el & ~mask); // 0 & x = 0, x = 111110111111..
            }

        }

        /// <summary>
        /// Получить значение бита в байте
        /// </summary>
        /// <param name="el">Байт в котором нужно получить занчение бита</param>
        /// <param name="numBit">Номер бита</param>
        /// <param name="value">значение: true - 1, false - 0</param>
        private bool GetBit(byte el, int numBit)
        {
            byte mask = (byte)(1 << numBit); // 0000100000....
            el = (byte)(mask & el);
            if (el > 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ширина сетки
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Высота сетки
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Количество элементов в сетке
        /// </summary>
        public int Count
        {
            get 
            {
                if (_grid == null || _grid.Length == 0)
                {
                    return 0;
                }
                else
                {
                    return _grid.Length;
                }
            }
        }

        #endregion
    }
}
