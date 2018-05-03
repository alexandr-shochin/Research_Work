using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.BoardElements
{
    /// <summary>
    /// Графический элемент пина
    /// </summary>
    public class Pin : BoardElement
    {
        public Pin(string ID, int x, int y, int width, int height) : base(ID, x, y, width, height) { }

        public override void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color.DarkGray), X + 1, Y + 1, Width - 1, Height - 1);
            graphics.FillRectangle(new SolidBrush(Color), 
                (X + 0.5f) + (((Width - 1) - ((Width - 1) / 2.0f))) / 2.0f, 
                (Y + 0.5f) + (((Height - 1) - ((Height - 1) / 2.0f))) / 2.0f, 
                (Width - 1) / 2.0f, 
                (Height - 1) / 2.0f);
        }

        public override Color Color { get { return Color.FromArgb(0, 100, 0); } }      
    }
}
