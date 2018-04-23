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
    public class Component : IBoardElement, IEnumerable<Component>
    {
        private Rectangle _rect;
        private Color _color = Color.FromArgb(255, 0, 0);

        private Dictionary<string, IBoardElement> _pins = new Dictionary<string, IBoardElement>();

        #region Construcors

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="x">Координата по оси X левого верхнего угла прямоугольной области</param>
        /// <param name="y">Координата по оси Y левого верхнего угла прямоугольной области</param>
        /// <param name="w">Ширина прямоугольной области</param>
        /// <param name="h">Высота прямоугольной области</param>
        public Component(string ID, int x, int y, int width, int height)
        {
            _rect = new Rectangle(x, y, width, height);
            this.ID = ID;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Добавить Pin в компоненту
        /// </summary>
        /// <param name="pin">Добавляемый pin</param>
        /// <returns>Код ошибки</returns>
        public ErrorCode Add(IBoardElement pin)
        {
            if (pin.X >= X &&  pin.X <= Right && pin.Y <= Y && pin.Y >= Bottom) // если не выходит за границы компоненты 
            {
                _pins[pin.ID] = pin;
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
        public ErrorCode Remove(IBoardElement pin)
        {
            return _pins.Remove(pin.ID) == true ? ErrorCode.PIN_WAS_DELETED : ErrorCode.PIN_WAS_NOT_DELETED;
        }

        /// <summary>
        /// Возвращает значение, указывающее, содержит ли компонента Pin переданный в качестве параметра.
        /// </summary>
        /// <param name="pin">Pin для поиска</param>
        /// <returns></returns>
        public ErrorCode Contains(IBoardElement pin)
        {
            return _pins.ContainsKey(pin.ID) == true ? ErrorCode.PIN_WAS_FOUND : ErrorCode.PIN_WAS_NOT_FOUND;
        }

        public void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color), X, Y, Width, Height); // сначала отрисовываем сам компонент

            // отрисовываем pin'ы
            foreach (var pin in _pins)
            {
                pin.Value.Draw(graphics);
            }
        }

        public IEnumerator<Component> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ") Width: " + Width + " Height: " + Height + " Count pins: " + _pins.Count;
        }

        #endregion

        #region Properties

        public string ID { get; private set; }

        public int Bottom { get { return _rect.Y + _rect.Height; } }

        public int Height { get { return _rect.Height; } }

        public bool IsEmpty { get { return _rect.IsEmpty; } }

        public int Left { get { return _rect.X; } }

        public int Right { get { return _rect.X + _rect.Width; } }

        public int Top { get { return _rect.Y; } }

        public int Width { get { return _rect.Width; } }

        public int X { get { return _rect.X; } }

        public int Y { get { return _rect.Y; } }

        public Color Color { get { return _color; } }

        #endregion  
    }
}
