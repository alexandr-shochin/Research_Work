using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    public class Alg
    {
        private Grid _grid;
        private Net _net;
        private Set _set;

        public Alg(Grid grid, Net net)
        {
            _grid = grid;
            _net = net;

            _set = new Set();
        }

        public void FindPath()
        {
            for (int numEl = 1; numEl < _net.Count; numEl++)
            {
                int start = _net[numEl];
                int finish = _net[numEl - 1];

                if (WavePropagation(start, finish) == true)
                {
                    //RestorationPath(ref path);
                }
                else //если какие-то подтрассы нашли и какую-то не смогли реализовать
                {
                }
            }
        }

        private bool WavePropagation(int start, int finish) // TODO
        {
            int numLevel = 0;
            _set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;

            int i = 0;
            int j = 0;

            int prevCountAdded;
            int countAdded = 1;
            for (int index = 0; index < _set.Count && !isFoundFinish; )
            {
                prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    _grid.GetIndexes(_set[index + elEdded].NumCell, out i, out j);

                    if (_set[index].NumLevel == numLevel - 1)
                    {
                        if (i - 2 >= 0 && CheckCell(i - 2, j, numLevel, finish, ref countAdded)) // left
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (i + 2 < _grid.CountRows && CheckCell(i + 2, j, numLevel, finish, ref countAdded)) // right
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j - 2 >= 0 && CheckCell(i, j - 2, numLevel, finish, ref countAdded)) // up
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j + 2 < _grid.CountColumn && CheckCell(i, j + 2, numLevel, finish, ref countAdded)) // down
                        {
                            isFoundFinish = true;
                            break;
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
                    if (_grid.GetNum(i, j) == finish && _set.Add(_grid.GetNum(i, j), numLevel)) // финишный Pin
                    {
                        countAdded++;
                        return true;
                    }
                    else if (_grid.IsOwnMetal(i, j) && _set.Add(_grid.GetNum(i, j), numLevel)) // если и Pin и свой метал
                    {
                        countAdded++;
                        return true;
                    }
                }
                else if (_grid.IsOwnMetal(i, j) && _set.Add(_grid.GetNum(i, j), numLevel)) // если свой метал
                {
                    countAdded++;
                    return true;
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

    }
}
