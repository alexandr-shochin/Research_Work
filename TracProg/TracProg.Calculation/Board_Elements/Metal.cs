using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    public class Metal : IElement // TODO передалать все поля и метод draw
    {
        private Rectangle _rectFrom;
        private Rectangle _rectIn;

        #region Construcors

        public Metal(Rectangle reactFrom, Rectangle reactIn)
        {
            _rectFrom = reactFrom;
            _rectIn = reactIn;
            _Color = Color.FromArgb(211, 211, 211);
        }

        #endregion

        #region Public Methods

        public ErrorCode Add(IElement el)
        {
            return ErrorCode.ADD_ERROR;
        }
        public ErrorCode Remove(IElement el)
        {
            return ErrorCode.REMOVE_ERROR;
        }
        public ErrorCode Contains(IElement el)
        {
            return ErrorCode.CONTAINS_ERROR;
        }

        /// <summary>
        /// Сравнивает два объекта и возвращает значение, указывающее, что один объект меньше, равняется или больше другого.
        /// Объект счтитается больше если лежит левее или выше другого объекта
        /// </summary>
        /// <param name="a">Первый из сравниваемых объектов.</param>
        /// <param name="b">Второй из сравниваемых объектов.</param>
        /// <returns>Меньше нуля - Значение x меньше y, Нуль - x равняется y, Больше нуля - Значение x больше значения y.</returns>
        public int Compare(IElement a, IElement b)
        {
            if (a.X == b.X &&
                a.Y == b.Y &&
                a.Width == b.Width &&
                a.Height == b.Height) // если равны
            {
                return 0;
            }
            else if (a.X < b.X || a.Y > b.Y) // если больше (достаточно рассмотреть угловые точки, поскольку нет пересечений pin'ов)
            {
                return 1;
            }
            return -1; // иначе меньше
        }

        /// <summary>
        /// Сравнивает текущий экземпляр с другим объектом того же типа и возвращает целое число, 
        /// которое показывает, расположен ли текущий экземпляр перед, после или на той же позиции в порядке сортировки, что и другой объект.
        /// Объект счтитается больше если лежит левее или выше другого объекта
        /// </summary>
        /// <param name="other">Объект для сравнения с данным экземпляром.</param>
        /// <returns>Меньше нуля - Значение x меньше y, Нуль - x равняется y, Больше нуля - Значение x больше значения y.</returns>
        public int CompareTo(IElement other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Определяет, пересекается ли данный Pin с Pin'ом pin.
        /// </summary>
        /// <param name="metal">Pin для проверки</param>
        /// <returns>При наличии пересечения этот метод возвращает значение true, в противном случае возвращается значение false.</returns>
        public bool IntersectsWith(IElement metal)
        {
            return this._rectIn.IntersectsWith(new Rectangle(metal.X, metal.Y, metal.Width, metal.Height));
        }

        public void Draw(ref Graphics graphics)
        {
            Point p1 = new Point((_rectFrom.Right - (_rectFrom.Right - _rectFrom.X) / 2), _rectFrom.Bottom - (_rectFrom.Bottom - _rectFrom.Y) / 2);
            Point p2 = new Point((_rectIn.Right - (_rectIn.Right - _rectIn.X) / 2), _rectIn.Bottom - (_rectIn.Bottom - _rectIn.Y) / 2);
            graphics.DrawLine(new Pen(new SolidBrush(_Color)), p1.x, p1.y, p2.x, p2.y);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ") " + Width + " " + Height;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает координату по оси Y прямоугольной области, являющуюся суммой значений свойств Y и Height.
        /// </summary>
        public int Bottom { get { return _rectIn.Y + _rectIn.Height; } }

        /// <summary>
        /// Возвращает высоту прямоугольной области
        /// </summary>
        public int Height { get { return _rectIn.Height; } }

        /// <summary>
        /// Данное свойство возвращает значение true, если значения всех свойств Width, Height, X и Y равны нулю. В противном случае возвращается значение false.
        /// </summary>
        public bool IsEmpty { get { return _rectIn.IsEmpty; } }

        /// <summary>
        /// Возвращает координату по оси X левого края прямоугольной области
        /// </summary>
        public int Left { get { return _rectIn.X; } }

        /// <summary>
        /// Возвращает координату по оси X прямоугольной области, являющуюся суммой значений свойств X и Width.
        /// </summary>
        public int Right { get { return _rectIn.X + _rectIn.Width; } }

        /// <summary>
        /// Возвращает координату по оси Y верхнего края прямоугольной области
        /// </summary>
        public int Top { get { return _rectIn.Y; } }

        /// <summary>
        /// Ширина прямоугольной области
        /// </summary>
        public int Width { get { return _rectIn.Width; } }

        /// <summary>
        /// Возвращает координату по оси X левого верхнего угла прямоугольной области
        /// </summary>
        public int X { get { return _rectIn.X; } }
        /// <summary>
        /// Возвращает координату по оси Y левого верхнего угла прямоугольной области
        /// </summary>
        public int Y { get { return _rectIn.Y; } }

        /// <summary>
        /// Возвращает или задаёт цвет прямоугольной области
        /// </summary>
        public Color _Color { get; set; }

        #endregion    
    }
}
