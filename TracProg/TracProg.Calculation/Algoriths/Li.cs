using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    public class Li
    {
        private Grid _grid;
        private Net[] _net;
        private Set _set;

        public event Action CalculateIsComplete;

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
        /// <returns>Время, затраченное на работу алгоритма</returns>
        public long[] FindPath(out List<List<List<int>>> nets)
        {
            nets = new List<List<List<int>>>();
            List<long> times = new List<long>();
            for (int numNet = 0; numNet < _net.Length; ++numNet)
            {
                List<List<int>> path = new List<List<int>>();

                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                for (int numEl = 1; numEl < _net[numNet].Count; numEl++)
                {
                    List<int> subPath = new List<int>();

                    int start = _net[numNet][numEl];
                    int finish = _net[numNet][numEl - 1];

                    if (WavePropagation(start, finish) == true)
                    {
                        RestorationPath(ref subPath);
                    }
                    else //если какие-то подтрассы нашли и какую-то не смогли реализовать
                    {
                        _grid.WriteToFile("matrixTest.txt");
                        Alg alg = new Alg();
                        alg.FindPath(ref _grid, start, finish);
                    }
                    _set.Clear();
                    if (subPath.Count != 0)
                    {
                        path.Add(subPath);
                    }
                    else // если не можем реализовать трассу
                    {
                        subPath.Add(-1); // индикатор того, что трасса не реализована
                        for (int i = 0; i < _net[numNet].Count; ++i)
                        {
                            subPath.Add(_net[numNet][i]);
                        }
                        path.Add(subPath);
                        break;
                    }
                }
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
                _grid.MetallizeTrack(path, (numNet + 1));

                nets.Add(path);
            }

            if (CalculateIsComplete != null)
                CalculateIsComplete.Invoke();

            return times.ToArray();
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

        /// <summary>
        /// Метод восстановления пути
        /// </summary>
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

            _grid.SetValue(currentNumCell, GridValue.OWN_METAL); // металлизируем
            path.Add(currentNumCell); // добавляем в путь
            while (currentLevel > 0)
            {   
                currentLevel--;

                _grid.GetIndexes(currentNumCell, out i, out j);
                if (i - 2 >= 0 && SetMetalCell(_grid.GetNum(i - 2, j), currentLevel, ref currentNumCell, ref path)) // left
                {
                    continue;
                }
                if (i + 2 < _grid.CountRows && SetMetalCell(_grid.GetNum(i + 2, j), currentLevel, ref currentNumCell, ref path)) // right
                {
                    continue;
                }
                if (j - 2 >= 0 && SetMetalCell(_grid.GetNum(i, j - 2), currentLevel, ref currentNumCell, ref path)) // up
                {
                    continue;
                }
                if (j + 2 < _grid.CountColumn && SetMetalCell(_grid.GetNum(i, j + 2), currentLevel, ref currentNumCell, ref path)) // down
                {
                    continue;
                }
            }

            return true;
        }
        private bool SetMetalCell(int numCell, int currentLevel, ref int currentNumCell, ref List<int> path)
        {
            if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
            {
                _grid.SetValue(numCell, GridValue.OWN_METAL);
                path.Add(numCell);
                currentNumCell = numCell;
                return true;
            }
            return false;
        }

    }
}
