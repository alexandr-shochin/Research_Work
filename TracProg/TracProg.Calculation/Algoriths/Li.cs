using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    public class Li
    {
        private class Set
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

            public Set()
            {
                _set = new Dictionary<int, int>();
            }

            public bool Add(int item, int numLevel)
            {
                if (!ContainsNumCell(item))
                {
                    _set.Add(item, numLevel);
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
            }

            public ElementSet this[int index]
            {
                get
                {
                    if (_set == null || index < 0 || index >= _set.Count)
                        throw new OverflowException("Индекс находился вне границ массива.");

                    KeyValuePair<int, int> key_value = _set.ElementAt(index);

                    return new ElementSet() { NumCell = key_value.Key, NumLevel = key_value.Value };
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

        private Grid _grid;
        private Net[] _net;
        private Set _set;

        public Li(Grid grid, Net[] net)
        {
            _grid = grid;
            _net = net;

            _set = new Set();
        }

        /// <summary>
        /// Найти трассу
        /// </summary>
        /// <param name="path">Итоговый список с номерами ячеек, которые вошли в качесте пути для данной трассы</param>
        /// <returns></returns>
        public bool FindPath()
        {
            List<int> path = new List<int>();
            for (int numNet = 0; numNet < _net.Length; ++numNet)
            {
                for (int numEl = 1; numEl < _net[numNet].Count; numEl++)
                {
                    int start = _net[numNet][numEl];
                    int finish = _net[numNet][numEl - 1];

                    if (WavePropagation(start, finish) == true)
                    {
                        RestorationPath(ref path);
                    }
                    else //если какие-то подтрассы нашли и какую-то не смогли реализовать
                    {
                        path.Clear();
                        path.Add(-1); // индикатор того, что трасса не реализована
                        for (int i = 0; i < _net[numNet].Count; ++i)
                        {
                            path.Add(_net[numNet][i]);
                        }
                        break;
                    }
                    _set.Clear();
                    if (path.Count != 0)
                    {
                        path.Add(-1); // добавляем -1, чтобы разделить трассы
                    }
                    else // если не можем реализовать трассу
                    {
                        path.Add(-1); // индикатор того, что трасса не реализована
                        for (int i = 0; i < _net[numNet].Count; ++i)
                        {
                            path.Add(_net[numNet][i]);
                        }
                        break;
                    }
                }
                _grid.MetallizeTrack(path);
                path.Clear();
            }
            return true;
        }

        /// <summary>
        /// Метод распространения волны
        /// </summary>
        /// <param name="start"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        private bool WavePropagation(int start, int finish)
        {
            int numLevel = 0;
            _set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;

            int i = 0;
            int j = 0;

            int prevCountAdded;
            int countAdded = 1;
            for (int index = 0; index < _set.Count && !isFoundFinish;)
            {
                prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    _grid.GetIndexes(_set[index + elEdded].NumCell, out i, out j);

                    if (_set[index].NumLevel == numLevel - 1)
                    {
                        if (i - 1 >= 0) // left
                        {
                            if (CheckCell(i - 1, j, numLevel, finish, ref countAdded))
                            {
                                isFoundFinish = true;
                                break;
                            }
                        }
                        if (i + 1 < _grid.CountRows) // right
                        {
                            if(CheckCell(i + 1, j, numLevel, finish, ref countAdded))
                            {
                                isFoundFinish = true;
                                break;
                            }
                        }
                        if (j - 1 >= 0) // up
                        {
                            if(CheckCell(i, j - 1, numLevel, finish, ref countAdded))
                            {
                                isFoundFinish = true;
                                break;
                            }
                        }
                        if (j + 1 < _grid.CountColumn) // down
                        {
                            if(CheckCell(i, j + 1, numLevel, finish, ref countAdded))
                            {
                                isFoundFinish = true;
                                break;
                            }
                        }
                    }
                }
                if (countAdded > 0)
                {
                    index += prevCountAdded;
                    numLevel++;
                }
                if (countAdded == 0) // условие, что нельзя реализовать трассу
                    return false;
            }

            return isFoundFinish;
        }

        private bool CheckCell(int i, int j, int numLevel, int finish, ref int countAdded)
        {
            if (!(_grid.IsProhibitionZone(i, j) || _grid.IsForeignMetal(i, j)))
            {
                if (_grid.IsPin(i, j)) // если это пин
                {
                    if (_grid.GetNum(i, j) == finish) // финишный Pin
                    {
                        if (_set.Add(_grid.GetNum(i, j), numLevel))
                        {
                            countAdded++;
                            return true;
                        }
                    }
                    else if (_grid.IsOwnMetal(i, j)) // если и Pin и свой метал
                    {
                        if (_set.Add(_grid.GetNum(i, j), numLevel))
                        {
                            countAdded++;
                            return true;
                        }
                    }
                }
                else if (_grid.IsOwnMetal(i, j)) // если свой метал
                {
                    if (_set.Add(_grid.GetNum(i, j), numLevel))
                    {
                        countAdded++;
                        return true;
                    }
                }
                else // если не Pin
                {
                    if (_set.Add(_grid.GetNum(i, j), numLevel))
                    {
                        countAdded++;
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Метод восстановления пути
        /// </summary>
        /// <param name="start">Стартовая ячейка</param>
        /// <param name="finish">Финишная ячейка</param>
        /// <param name="path">Итоговый список с номерами ячеек, которые вошли в качесте пути для данной трассы</param>
        /// <returns></returns>
        private bool RestorationPath(ref List<int> path)
        {
            if (path == null)
            {
                path = new List<int>();
            }

            int start = _set[0].NumCell;

            Set.ElementSet elSet = _set[_set.Count - 1];
            int currentNumCell = elSet.NumCell;
            int currentLevel = elSet.NumLevel;

            int i = 0;
            int j = 0;

            int numCell;

            _grid.SetValue(currentNumCell, GridValue.OWN_METAL); // металлизируем
            path.Add(currentNumCell); // добавляем в путь
            while (currentLevel > 0)
            {   
                currentLevel--;

                _grid.GetIndexes(currentNumCell, out i, out j);
                if (i - 1 >= 0) // left
                {
                    numCell = _grid.GetNum(i - 1, j);

                    if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
                    {
                        _grid.SetValue(numCell, GridValue.OWN_METAL); // металлизируем
                        path.Add(numCell); // добавляем в путь
                        currentNumCell = numCell;
                        continue;
                    }
                }
                if (i + 1 < _grid.CountRows) // right
                {
                    numCell = _grid.GetNum(i + 1, j);
                    if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
                    {
                        _grid.SetValue(numCell, GridValue.OWN_METAL);
                        path.Add(numCell);
                        currentNumCell = numCell;
                        continue;
                    }
                }
                if (j - 1 >= 0) // up
                {
                    numCell = _grid.GetNum(i, j - 1);
                    if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
                    {
                        _grid.SetValue(numCell, GridValue.OWN_METAL);
                        path.Add(numCell);
                        currentNumCell = numCell;
                        continue;
                    }
                }
                if (j + 1 < _grid.CountColumn) // down
                {
                    numCell = _grid.GetNum(i, j + 1);
                    if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
                    {
                        _grid.SetValue(numCell, GridValue.OWN_METAL);
                        path.Add(numCell);
                        currentNumCell = numCell;
                        continue;
                    }
                }
            }

            return true;
        }
    }
}
