using System.Collections.Generic;
using System.Diagnostics;
using TracProg.Calculation.BoardElements;

namespace TracProg.Calculation.Algorithms
{
    public class WaveTraceAlgScheme
    {
        private readonly TraceGrid _grid;
        private readonly Set _set;

        private string _netName;
        private Net _net;

        public WaveTraceAlgScheme(TraceGrid grid)
        {
            _grid = grid;
            _set = new Set();
        }

        /// <summary>
        /// Найти трассу
        /// </summary>
        /// <param name="net"></param>
        /// <param name="netName"></param>
        /// <param name="realizedTracks"></param>
        /// <param name="nonRealizedPins"></param>
        /// <returns></returns>
        public void FindPath(string netName, Net net, out List<List<int>> realizedTracks, out List<int> nonRealizedPins)
        {
            _netName = netName;
            _net = net;
            nonRealizedPins = new List<int>();

            _set.Clear();

            realizedTracks = new List<List<int>>();
            for (int numEl = 0; numEl < _net.Count; numEl++)
            {
                int start = _net[numEl];
                int finish;

                if (WavePropagation(start, out finish))
                {
                    List<int> subPath;
                    RestorationPath(out subPath);
                    _set.Clear();
                    realizedTracks.Add(subPath);
                }
                else //если какой-то пин не смогли реализовать
                {
                    nonRealizedPins.Add(start);
                }
            }

            _grid.MetallizeTracks(realizedTracks, netName);
        }

        /// <summary>
        /// Метод распространения волны
        /// </summary>
        /// <param name="start"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        private bool WavePropagation(int start, out int finish)
        {
            int numLevel = 0;
            _set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;
            finish = -1;

            int prevCountAdded;
            int countAdded = 1;
            for (int index = 0; index < _set.Count && !isFoundFinish;)
            {
                prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    int i, j;
                    _grid.GetIndexes(_set[index + elEdded].NumCell, out i, out j);

                    if (_set[index].NumLevel == numLevel - 1)
                    {
                        if (j - 2 >= 0 && CheckCell(i, j - 2, numLevel, ref countAdded)) // left
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i, j - 2);
                            break;
                        }
                        if (j + 2 < _grid.CountColumn && CheckCell(i, j + 2, numLevel, ref countAdded)) // right
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i, j + 2);
                            break;
                        }
                        if (i - 2 >= 0 && CheckCell(i - 2, j, numLevel, ref countAdded)) // up
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i - 2, j);
                            break;
                        }
                        if (i + 2 < _grid.CountRows && CheckCell(i + 2, j, numLevel, ref countAdded)) // down
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i + 2, j);
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

        private bool CheckCell(int i, int j, int numLevel, ref int countAdded)
        {
            if(_grid.IsProhibitionZone(i, j)) //  Если зона запрета
            {
                return false;
            }
            else
            {
                if (_net.Contains(_grid.GetNum(i, j)) && _set.Add(_grid.GetNum(i, j), numLevel)) // финишный Pin
                {
                    countAdded++;
                    return true;
                }
                else
                {
                    if(_grid.IsFreeMetal(i, j) && _set.Add(_grid.GetNum(i, j), numLevel)) // Если свободный метал 
                    {
                        countAdded++;
                        return false;
                    }
                    else
                    {
                        if(_grid.IsOwnMetal(i, j, _netName) && _set.Add(_grid.GetNum(i, j), numLevel)) // если свой метал
                        {
                            countAdded++;
                            return true;
                        }
                        else // чужой метал
                        {
                            return false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод восстановления пути
        /// </summary>
        /// <param name="path">Итоговый список с номерами ячеек, которые вошли в качесте пути для данной трассы</param>
        /// <returns></returns>
        private void RestorationPath(out List<int> path)
        {
            path = new List<int>();

            Set.ElementSet elSet = _set[_set.Count - 1];
            int currentNumCell = elSet.NumCell;
            int currentLevel = elSet.NumLevel;

            // металлизируем
            TraceGrid.TraceGridElement el = _grid[currentNumCell];
            el.MetalId = _netName;
            _grid[currentNumCell] = el;

            path.Add(currentNumCell); // добавляем в путь
            while (currentLevel > 0)
            {   
                currentLevel--;

                int i, j;
                _grid.GetIndexes(currentNumCell, out i, out j);
                if (j - 2 >= 0 && SetMetalCell(_grid.GetNum(i, j - 2), currentLevel, ref currentNumCell, ref path)) // left
                {
                    continue;
                }
                if (j + 2 < _grid.CountColumn && SetMetalCell(_grid.GetNum(i, j + 2), currentLevel, ref currentNumCell, ref path)) // right
                {
                    continue;
                }
                if (i - 2 >= 0 && SetMetalCell(_grid.GetNum(i - 2, j), currentLevel, ref currentNumCell, ref path)) // uo
                {
                    continue;
                }
                if (i + 2 < _grid.CountRows && SetMetalCell(_grid.GetNum(i + 2, j), currentLevel, ref currentNumCell, ref path)) // down
                {
                    continue;
                }
            }
        }
        private bool SetMetalCell(int numCell, int currentLevel, ref int currentNumCell, ref List<int> path)
        {
            if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
            {
                TraceGrid.TraceGridElement el = _grid[numCell];
                el.MetalId = _netName;
                _grid[numCell] = el;

                path.Add(numCell);
                currentNumCell = numCell;
                return true;
            }
            return false;
        }

    }
}
