using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    [Serializable]
    public class Point
    {
        /// <summary>
        /// Координата по оси X
        /// </summary>
        public int x { get; private set; }
        /// <summary>
        /// Координата по оси Y
        /// </summary>
        public int y { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="x">Координата по оси X</param>
        /// <param name="y">Координата по оси Y</param>
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return x + ", " + y;
        }
    }
}
