using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    internal class Set
    {
        public class ElementSet : IEquatable<ElementSet>
        {
            public int NumCell { get; set; }
            public int NumLevel { get; set; }

            public override string ToString()
            {
                return "Value: " + NumCell + " Level: " + NumLevel;
            }

            /// <summary>
            /// Указывает, равен ли текущий объект другому объекту того же типа.
            /// </summary>
            /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
            /// <returns>true , если текущий объект равен параметру other, в противном случае — false.</returns>
            public bool Equals(ElementSet other)
            {
                return other.NumCell == this.NumCell ? true : false;
            }
        }

        private Dictionary<int, int> _set;
        private List<int> _list;

        public Set()
        {
            _set = new Dictionary<int, int>();
            _list = new List<int>();
        }

        public bool Add(int item, int numLevel)
        {
            if (!ContainsNumCell(item))
            {
                _set.Add(item, numLevel);
                _list.Add(item);
                return true;
            }
            return false;
        }

        public bool ContainsNumCell(int item)
        {
            return _set.ContainsKey(item);
        }

        public int GetNumLevel(int numCell)
        {
            return _set[numCell];
        }

        public void Clear()
        {
            _set.Clear();
            _list.Clear();
        }

        public ElementSet this[int index]
        {
            get
            {
                if (_set == null || index < 0 || index >= _set.Count)
                    throw new OverflowException("Индекс находился вне границ массива.");

                return new ElementSet() { NumCell = _list[index], NumLevel = _set[_list[index]] };
            }
        }

        public int Count
        {
            get
            {
                if (_set.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return _set.Count;
                }
            }
        }

        public override string ToString()
        {
            return "Count = " + Count;
        }
    }
}
