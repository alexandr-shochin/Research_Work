using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    public class ProhibitionZone : IElement
    {
        private Rectangle _rect;

        #region Construcors

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="location">Объект Point, представляющий левый верхний угол прямоугольной области.</param>
        /// <param name="w">Ширина прямоугольной области</param>
        /// <param name="h">Высота прямоугольной области</param>
        public ProhibitionZone(Point location, int width, int height)
        {
            _rect = new Rectangle(location.x, location.y, width, height);
            _Color = Color.Red;
        }

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="x">Координата по оси X левого верхнего угла прямоугольной области</param>
        /// <param name="y">Координата по оси Y левого верхнего угла прямоугольной области</param>
        /// <param name="w">Ширина прямоугольной области</param>
        /// <param name="h">Высота прямоугольной области</param>
        public ProhibitionZone(int x, int y, int width, int height)
        {
            _rect = new Rectangle(x, y, width, height);
            _Color = Color.Red;
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
        /// Определяет, пересекается ли данная зона запрета с зоной запрета pin.
        /// </summary>
        /// <param name="prohibitionZone">ProhibitionZone для проверки</param>
        /// <returns>При наличии пересечения этот метод возвращает значение true, в противном случае возвращается значение false.</returns>
        public bool IntersectsWith(IElement prohibitionZone)
        {
            return this._rect.IntersectsWith(new Rectangle(prohibitionZone.X, prohibitionZone.Y, prohibitionZone.Width, prohibitionZone.Height));
        }

        public void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(_Color), X + 1, Y + 1, Width - 1, Height - 1);
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
        public int Bottom { get { return _rect.Y + _rect.Height; } }

        /// <summary>
        /// Возвращает высоту прямоугольной области
        /// </summary>
        public int Height { get { return _rect.Height; } }

        /// <summary>
        /// Данное свойство возвращает значение true, если значения всех свойств Width, Height, X и Y равны нулю. В противном случае возвращается значение false.
        /// </summary>
        public bool IsEmpty { get { return _rect.IsEmpty; } }

        /// <summary>
        /// Возвращает координату по оси X левого края прямоугольной области
        /// </summary>
        public int Left { get { return _rect.X; } }

        /// <summary>
        /// Возвращает координату по оси X прямоугольной области, являющуюся суммой значений свойств X и Width.
        /// </summary>
        public int Right { get { return _rect.X + _rect.Width; } }

        /// <summary>
        /// Возвращает координату по оси Y верхнего края прямоугольной области
        /// </summary>
        public int Top { get { return _rect.Y; } }

        /// <summary>
        /// Ширина прямоугольной области
        /// </summary>
        public int Width { get { return _rect.Width; } }

        /// <summary>
        /// Возвращает координату по оси X левого верхнего угла прямоугольной области
        /// </summary>
        public int X { get { return _rect.X; } }
        /// <summary>
        /// Возвращает координату по оси Y левого верхнего угла прямоугольной области
        /// </summary>
        public int Y { get { return _rect.Y; } }

        /// <summary>
        /// Возвращает или задаёт цвет прямоугольной области
        /// </summary>
        public Color _Color { get; set; }

        #endregion  
    }
}
