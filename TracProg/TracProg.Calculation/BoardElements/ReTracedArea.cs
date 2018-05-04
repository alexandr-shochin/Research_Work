using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.BoardElements
{
    /// <summary>
    /// Графический элемент - перетрассированная область
    /// </summary>
    public class ReTracedArea : BoardElement
    {
        public ReTracedArea(string ID, int x, int y, int width, int height): base(ID, x, y, width, height) { }

        public override void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color), X + 1, Y + 1, Width - 1, Height - 1);
        }

        public override Color Color { get { return Color.Chartreuse; } }
    }
}
