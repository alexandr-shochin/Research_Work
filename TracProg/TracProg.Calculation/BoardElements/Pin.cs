using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.BoardElements
{
    public enum PinState
    {
        NonRealized,
        Realized,
        InProccessRetrace
    }

    /// <summary>
    /// Графический элемент пина
    /// </summary>
    public class Pin : BoardElement
    {
        private Color _color;

        public Pin(string id, int x, int y, int width, int height) : base(id, x, y, width, height)
        {
            PinState = PinState.NonRealized;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color.Gray), X + 1, Y + 1, Width - 1, Height - 1);
            graphics.FillRectangle(new SolidBrush(Color), 
                X + (Width - 1 - (Width - 1) / 2.0f) / 2.0f, 
                Y + (Height - 1 - (Height - 1) / 2.0f) / 2.0f, 
                (Width + 1) / 2.0f, 
                (Height + 1) / 2.0f);
        }

        public override Color Color { get { return _color; } }

        public IBoardElement NextMetal { get; set; }

        public PinState PinState
        {
            set
            {

                switch (value)
                {
                    case PinState.NonRealized:
                    {
                        _color = Color.Red;
                        break;
                    }
                    case PinState.InProccessRetrace:
                    {
                        _color = Color.DarkOrange;
                        break;
                    }
                    case PinState.Realized:
                    {
                        _color = Color.ForestGreen; 
                        break;
                    }
                }
            }
        }
    }
}
