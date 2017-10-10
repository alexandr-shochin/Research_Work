using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    /// <summary>
    /// Компонент печатной платы
    /// </summary>
    [Serializable]
    public class Component : IElement
    {
        private Rectangle _rect;
        private List<IElement> _pins;

        #region Construcors

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="core">Объект Point, представляющий левый верхний угол прямоугольной области.</param>
        /// <param name="w">Ширина прямоугольной области</param>
        /// <param name="h">Высота прямоугольной области</param>
        public Component(Point location, int width, int height)
        {
            _rect = new Rectangle(location.x, location.y, width, height);

            _pins = new List<IElement>();
        }

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="x">Координата по оси X левого верхнего угла прямоугольной области</param>
        /// <param name="y">Координата по оси Y левого верхнего угла прямоугольной области</param>
        /// <param name="w">Ширина прямоугольной области</param>
        /// <param name="h">Высота прямоугольной области</param>
        public Component(int x, int y, int width, int height)
        {
            _rect = new Rectangle(x, y, width, height);

            _pins = new List<IElement>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Добавить Pin в компоненту
        /// </summary>
        /// <param name="pin">Добавляемый pin</param>
        /// <returns>Код ошибки</returns>
        public ErrorCode Add(IElement pin)
        {
            if (pin.X >= X &&  pin.X <= Right && pin.Y <= Y && pin.Y >= Bottom) // если не выходит за границы компоненты 
            {
                // прверяем что добавляемый pin не пересекается с другими pin
                foreach (Pin p in _pins)
                {
                    if (p.IntersectsWith(pin))
                    {
                        return ErrorCode.PIN_INTERSECT_WITH_ANOTHER_PIN;
                    }
                }

                // если не пересекается
                _pins.Add(pin);
                return ErrorCode.NO_ERROR;
            }
            else
            {
                return ErrorCode.PIN_OUT_OF_BOUNDS; // выход за границы компоненты
            }
        }

        /// <summary>
        /// Удаляет Pin из компоненты
        /// </summary>
        /// <param name="pin">Удаляемый pin</param>
        /// <returns>Код ошибки</returns>
        public ErrorCode Remove(IElement pin)
        {
            return _pins.Remove(pin) == true ? ErrorCode.PIN_WAS_DELETED : ErrorCode.PIN_WAS_NOT_DELETED;
        }

        /// <summary>
        /// Возвращает значение, указывающее, содержит ли компонента Pin переданный в качестве параметра.
        /// </summary>
        /// <param name="pin">Pin для поиска</param>
        /// <returns></returns>
        public ErrorCode Contains(IElement pin)
        {
            return _pins.Contains(pin) == true ? ErrorCode.PIN_WAS_FOUND : ErrorCode.PIN_WAS_NOT_FOUND;
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

        public void Draw(ref Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(_Color), X, Y, Width, Height); // сначала отрисовываем сам компонент

            // отрисовываем pin'ы
            foreach (var pin in _pins)
            {
                pin.Draw(ref graphics);
            }
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ") Width: " + Width + " Height: " + Height + " Count pins: " + _pins.Count;
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
        public Color _Color { get { return Color.FromArgb(255, 0, 0); } }

        /// <summary>
        /// Доступ к отдельным элементам компоненты
        /// </summary>
        /// <param name="index">Индекс (начиная 0)</param>
        /// <returns>Экземпляр Pin</returns>
        /// <exception cref="OverflowException">Индекс находился вне границ массива.</exception>
        public IElement this[int index]
        {
            get
            {
                if (_pins == null || index < 0 || index >= _pins.Count)
                    throw new OverflowException("Индекс находился вне границ массива.");
                return _pins[index];
            }
        }

        #endregion  
    }
}
