using System.Collections.Generic;
using System.Threading.Tasks;
using TracProg.Calculation.BoardElements;

namespace TracProg.Calculation.Algorithms
{
    public class WaveTraceAlgScheme
    {
        private readonly TraceGrid _grid;

        private readonly Dictionary<int, int> _allRealizedNodes = new Dictionary<int, int>();

        private string _netName;
        private Net _net;

        private readonly object _lock = new object();

        public WaveTraceAlgScheme(TraceGrid grid)
        {
            _grid = grid;
        }

        /// <summary>
        /// Найти трассу
        /// </summary>
        /// <param name="net"></param>
        /// <param name="netName"></param>
        /// <param name="realizedTracks"></param>
        /// <param name="nonRealizedPins"></param>
        /// <returns></returns>
        public bool FindPath(string netName, Net net, out List<List<int>> realizedTracks, out List<int> nonRealizedPins)
        {
            int currentIter = 1;

            _netName = netName;
            _net = new Net(net.ToArray());
            nonRealizedPins = new List<int>();

            realizedTracks = new List<List<int>>();
            for (int numEl = 0; numEl < _net.Count; numEl++)
            {
                Set set = new Set();
                int start = _net[numEl];
                int finish;

                if (WavePropagation(set, start, out finish))
                {
                    List<int> subPath;
                    RestorationPath(set, out subPath);
                    realizedTracks.Add(subPath);

                    // металлизируем
                    foreach (int node in subPath)
                    {
                        TraceGrid.TraceGridElement el = _grid[node];
                        el.MetalId = _netName;
                        _grid[node] = el;
                    }

                    break;
                }
                
            }

            if (realizedTracks.Count != 0)
            {
                foreach (int node in realizedTracks[0])
                {
                    _allRealizedNodes[node] = node;
                }
                
            }
            else //если ни один пин не смогли реализовать
            {
                nonRealizedPins.AddRange(net.ToArray());
                return false;
            }

            for (int i = currentIter; i < net.Count - 1; i++) // TODO подумать над условием выхода и если не удётся реализовать подтрасу
            {
                List<int> record = new List<int>();
                //foreach (int node in realizedTrack)
                Parallel.ForEach(_allRealizedNodes, (node) =>
                {
                    Set set = new Set();
                    int start = node.Key;

                    int finish;
                    if (WavePropagation(set, start, out finish))
                    {
                        List<int> subPath;
                        RestorationPath(set, out subPath);

                        lock (_lock)
                        {
                            if (record.Count == 0 || record.Count > subPath.Count)
                            {
                                record = new List<int>(subPath);
                            }
                        }
                    }
                });

                if (record.Count != 0)
                {
                    realizedTracks.Add(new List<int>(record));
                    foreach (int node in record) // металлизируем
                    {
                        TraceGrid.TraceGridElement el = _grid[node];
                        el.MetalId = _netName;
                        _grid[node] = el;
                    }

                    foreach (int node in record)
                    {
                        _allRealizedNodes[node] = node;
                    }
                }
                else
                {
                    if (i < net.Count - 1)
                    {
                        _grid.MetallizeTracks(realizedTracks, netName);

                        nonRealizedPins.AddRange(net.ToArray());
                        foreach (List<int> realizedTrack in realizedTracks)
                        {
                            foreach (int node in realizedTrack)
                            {
                                if (_grid.IsPin(node))
                                {
                                    if (nonRealizedPins.Remove(node))
                                    {

                                    }
                                }
                            }
                        }
                        return false; // не смогли реализовать трассу из текущей частично сформированной компоненты
                    }
                }
            }

            _grid.MetallizeTracks(realizedTracks, netName);

            if (realizedTracks.Count < net.Count - 1) // TODO значит, что все пины реализованы, но компонента связности не одна
            {
                //nonRealizedPins.AddRange(net.ToArray());
                return false;
            }

            return true;
        }

        public void AddRealizedNodes(int node)
        {
            _allRealizedNodes[node] = node;
        }

        /// <summary>
        /// Метод распространения волны
        /// </summary>
        /// <param name="set"></param>
        /// <param name="start"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        private bool WavePropagation(Set set, int start, out int finish)
        {
            int numLevel = 0;
            set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;
            finish = -1;

            int countAdded = 1;
            for (int index = 0; index < set.Count && !isFoundFinish;)
            {
                int prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    int i, j;
                    _grid.GetIndexes(set[index + elEdded].NumCell, out i, out j);

                    if (set[index].NumLevel == numLevel - 1)
                    {
                        if (j - 2 >= 0 && CheckCell(set, i, j - 2, numLevel, ref countAdded)) // left
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i, j - 2);
                            break;
                        }
                        if (j + 2 < _grid.CountColumn && CheckCell(set, i, j + 2, numLevel, ref countAdded)) // right
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i, j + 2);
                            break;
                        }
                        if (i - 2 >= 0 && CheckCell(set, i - 2, j, numLevel, ref countAdded)) // up
                        {
                            isFoundFinish = true;
                            finish = _grid.GetNum(i - 2, j);
                            break;
                        }
                        if (i + 2 < _grid.CountRows && CheckCell(set, i + 2, j, numLevel, ref countAdded)) // down
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

        private bool CheckCell(Set set, int i, int j, int numLevel, ref int countAdded)
        {
            if (_allRealizedNodes.ContainsKey(_grid.GetNum(i, j))) // определили, что вершина принадлежит компоненте
            {
                return false;
            }

            if (_grid.IsProhibitionZone(i, j)) //  Если зона запрета
            {
                return false;
            }
            else
            {
                if (_net.Contains(_grid.GetNum(i, j)) && set.Add(_grid.GetNum(i, j), numLevel)) // финишный Pin
                {
                    countAdded++;
                    return true;
                }
                else
                {
                    if(_grid.IsFreeMetal(i, j) && set.Add(_grid.GetNum(i, j), numLevel)) // Если свободный метал 
                    {
                        countAdded++;
                        return false;
                    }
                    else
                    {
                        if(_grid.IsOwnMetal(i, j, _netName) && set.Add(_grid.GetNum(i, j), numLevel)) // если свой метал
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
        /// <param name="set"></param>
        /// <param name="path">Итоговый список с номерами ячеек, которые вошли в качесте пути для данной трассы</param>
        /// <returns></returns>
        private void RestorationPath(Set set, out List<int> path)
        {
            path = new List<int>();

            Set.ElementSet elSet = set[set.Count - 1];
            int currentNumCell = elSet.NumCell;
            int currentLevel = elSet.NumLevel;

            path.Add(currentNumCell); // добавляем в путь
            while (currentLevel > 0)
            {   
                currentLevel--;

                int i, j;
                _grid.GetIndexes(currentNumCell, out i, out j);
                if (j - 2 >= 0 && SetMetalCell(set, _grid.GetNum(i, j - 2), currentLevel, ref currentNumCell, ref path)) // left
                {
                    continue;
                }
                if (j + 2 < _grid.CountColumn && SetMetalCell(set, _grid.GetNum(i, j + 2), currentLevel, ref currentNumCell, ref path)) // right
                {
                    continue;
                }
                if (i - 2 >= 0 && SetMetalCell(set, _grid.GetNum(i - 2, j), currentLevel, ref currentNumCell, ref path)) // uo
                {
                    continue;
                }
                if (i + 2 < _grid.CountRows && SetMetalCell(set, _grid.GetNum(i + 2, j), currentLevel, ref currentNumCell, ref path)) // down
                {
                    continue;
                }
            }
        }
        private bool SetMetalCell(Set set, int numCell, int currentLevel, ref int currentNumCell, ref List<int> path)
        {
            if (set.ContainsNumCell(numCell) && set.GetNumLevel(numCell) == currentLevel)
            {
                path.Add(numCell);
                currentNumCell = numCell;
                return true;
            }
            return false;
        }

    }
}
