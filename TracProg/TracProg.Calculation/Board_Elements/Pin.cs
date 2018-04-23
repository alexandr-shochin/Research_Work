using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    /// <summary>
    /// Pin элемент компонены печатной платы
    /// </summary>
    [Serializable]
    public class Pin : IBoardElement
    {
        private Rectangle _rect;
        private Color _color = Color.FromArgb(0, 100, 0);

        #region Construcors

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="x">Координата по оси X левого верхнего угла прямоугольной области</param>
        /// <param name="y">Координата по оси Y левого верхнего угла прямоугольной области</param>
        /// <param name="w">Ширина прямоугольной области</param>
        /// <param name="h">Высота прямоугольной области</param>
        public Pin(string ID, int x, int y, int width, int height)
        {
            _rect = new Rectangle(x, y, width, height);
            this.ID = ID;
        }

        #endregion

        #region Public Methods

        public ErrorCode Add(IBoardElement el)
        {
            return ErrorCode.ADD_ERROR;
        }
        public ErrorCode Remove(IBoardElement el)
        {
            return ErrorCode.REMOVE_ERROR;
        }
        public ErrorCode Contains(IBoardElement el)
        {
            return ErrorCode.CONTAINS_ERROR;
        }

        public void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(211, 211, 211)), X + 1, Y + 1, Width - 1, Height - 1);
            graphics.FillRectangle(new SolidBrush(Color), 
                (X + 0.5f) + (((Width - 1) - ((Width - 1) / 2.0f))) / 2.0f, 
                (Y + 0.5f) + (((Height - 1) - ((Height - 1) / 2.0f))) / 2.0f, 
                (Width - 1) / 2.0f, 
                (Height - 1) / 2.0f);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ") " + Width + " " + Height;
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
