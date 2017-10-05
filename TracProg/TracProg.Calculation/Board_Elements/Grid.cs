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
        /// Проверка, что данная ячейка является Pin'ом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsPin(int num)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Проверка, что данная ячейка является зоной запрета
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsProhibitionZone(int num)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Проверка, что данная ячейка является собственным металом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsOwnMetal(int num)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Проверка, что данная ячейка является чужим металом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsForeignMetal(int num)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Проверка, что данная ячейка является свободным металом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsFreeMetal(int num)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Ширина сетки
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высота сетки
        /// </summary>
        public int Height { get; private set; }

    }
}
