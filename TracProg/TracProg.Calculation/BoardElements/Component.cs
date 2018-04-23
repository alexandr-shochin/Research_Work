using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.BoardElements
{
    /// <summary>
    /// Графичесий элемент компоненты
    /// </summary>
    public class Component : BoardElement
    {
        public Component(string ID, int x, int y, int width, int height) : base(ID, x, y, width, height) { }

        public override bool Add(IBoardElement el)
        {
            if (el.X >= X && el.X <= Right && el.Y <= Y && el.Y >= Bottom) // если не выходит за границы компоненты 
            {
                IBoardElement child;
                if (!_childs.TryGetValue(el.ID, out child))
                {
                    _childs[el.ID] = el;
                    return true;
                }
            }

            return false;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color), X, Y, Width, Height); // сначала отрисовываем сам компонент

            // отрисовываем pin'ы
            foreach (var pin in _childs)
            {
                pin.Value.Draw(graphics);
            }
        }

        public override Color Color { get { return Color.FromArgb(255, 0, 0); } }
    }
}
