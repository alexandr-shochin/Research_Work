using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.BoardElements
{
    public abstract class BoardElement : IBoardElement
    {
        private Rectangle _rect = new Rectangle();

        protected Dictionary<string, IBoardElement> _childs = new Dictionary<string, IBoardElement>();

        protected BoardElement() { }

        protected BoardElement(string ID, int x, int y, int width, int height)
        {
            _rect = new Rectangle(x, y, width, height);
            this.ID = ID;
        }

        public virtual bool Add(IBoardElement el)
        {
            IBoardElement child;
            if (!_childs.TryGetValue(el.ID, out child))
            {
                _childs[el.ID] = el;
                return true;
            }

            return false;
        }
        public virtual bool Remove(IBoardElement el)
        {
            return _childs.Remove(el.ID);
        }
        public virtual bool Contains(IBoardElement el)
        {
            return _childs.ContainsKey(el.ID);
        }

        public virtual void Draw(Graphics graphics) { }

        public override string ToString()
        {
            return "(" + X + "," + Y + ") W:" + Width + " H:" + Height + " Childs: " + _childs.Count; ;
        }

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

        public virtual Color Color { get { return Color.White; } }
    }
}
