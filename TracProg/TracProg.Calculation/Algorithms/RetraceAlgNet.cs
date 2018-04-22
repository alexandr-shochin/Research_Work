using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    public class RetraceAlgNet
    {
        private Set _set;
        private Set _virtualSet;

        private int _leftBorderLimitingRectangle;
        private int _upBorderLimitingRectangle;
        private int _rightBorderLimitingRectangle;
        private int _downBorderLimitingRectangle;

        private List<Tuple<int, int, IElement>> _pinnedNodes;

        private Grid _oldGrid;
        private Grid _newGrid;

        private string _nonRealizedMetalID;

        private Point p0;

        private Dictionary<string, Net> _nets;

        private List<int> _allPins;

        public RetraceAlgNet(Grid oldGrid, string nonRealizedMetalID, List<int> allPins)
        {
            _oldGrid = oldGrid;
            _set = new Set();
            _virtualSet = new Set();

            _nonRealizedMetalID = nonRealizedMetalID;
            _allPins = allPins;
        }

        public bool FindPath(Dictionary<string, Net> nets, int start, out int finish, out bool isFinishPin)
        {
            _nets = nets;

            finish = -1;
            isFinishPin = false;
            if (!WavePropagation(ref _oldGrid, start) == true && VirtualWavePropagation(ref _oldGrid, start, out finish, out isFinishPin) == true) // нашли трассу через междоузлие
            {
                List<int> pathWithInternodes = new List<int>();
                RestorationPath(ref _oldGrid, ref pathWithInternodes);

                // получаем координаты ограничивающего прямоугольника
                GetCoordLimitingRectangle(ref _oldGrid, ref pathWithInternodes, 20, 20, 20, 20);

                // копируем нужные элементы сетки и создаём новую
                GridElement[] gridElements = new GridElement[((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 1) * ((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 1)];
                Dictionary<string, List<Tuple<int, int>>> futurePins = new Dictionary<string, List<Tuple<int, int>>>(); //<MetalID, список с Pin или граничным узлом>
                Dictionary<string, List<Tuple<int, int>>> tracks = new Dictionary<string, List<Tuple<int, int>>>(); // реализованные трассы в прямоугольнике в том числе с междоузлие

                int numElement = 0;
                for (int i = _upBorderLimitingRectangle; i <= _downBorderLimitingRectangle; ++i)
                {
                    for (int j = _leftBorderLimitingRectangle; j <= _rightBorderLimitingRectangle; ++j)
                    {
                        gridElements[numElement] = _oldGrid[i, j];
                        numElement++;
                    }
                }

                p0 = _oldGrid.GetCoordCell(_upBorderLimitingRectangle, _leftBorderLimitingRectangle); // координата смещения

                _newGrid = new Grid(gridElements, p0.x, p0.y,
                    (((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 2) / 2) * _oldGrid.Koeff,
                    (((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 2) / 2) * _oldGrid.Koeff,
                    _oldGrid.Koeff);

                // ищем граничные узлы
                int oldI = _upBorderLimitingRectangle;
                int oldJ = _leftBorderLimitingRectangle;
                for (int newI = 0; newI < _newGrid.CountRows; newI++)
                {
                    oldJ = _leftBorderLimitingRectangle;
                    for (int newJ = 0; newJ < _newGrid.CountColumn; newJ++)
                    {
                        if (!string.IsNullOrEmpty(_newGrid[newI, newJ].MetalID))
                        {
                            // ищем реализованные трассы в нашем прямоугольнике
                            List<Tuple<int, int>> trackList;
                            if (!tracks.TryGetValue(_newGrid[newI, newJ].MetalID, out trackList))
                            {
                                tracks[_newGrid[newI, newJ].MetalID] = new List<Tuple<int, int>>();
                                tracks[_newGrid[newI, newJ].MetalID].Add(Tuple.Create(newI, newJ)); 
                            }
                            else
                            {
                                trackList.Add(Tuple.Create(newI, newJ)); 
                            }

                            if (IsBoardGridElement(ref _oldGrid, ref _newGrid, newI, newJ, oldI, oldJ))
                            {
                                List<Tuple<int, int>> list;
                                if (!futurePins.TryGetValue(_newGrid[newI, newJ].MetalID, out list))
                                {
                                    futurePins[_newGrid[newI, newJ].MetalID] = new List<Tuple<int, int>>();
                                    futurePins[_newGrid[newI, newJ].MetalID].Add(Tuple.Create(newI, newJ));
                                }
                                else
                                {
                                    list.Add(Tuple.Create(newI, newJ));
                                }
                            }
                            else if (_newGrid.IsPin(newI, newJ))
                            {
                                List<Tuple<int, int>> list;
                                if(!futurePins.TryGetValue(_newGrid[newI, newJ].MetalID, out list))
                                {
                                    futurePins[_newGrid[newI, newJ].MetalID] = new List<Tuple<int, int>>();
                                    futurePins[_newGrid[newI, newJ].MetalID].Add(Tuple.Create(newI, newJ));
                                }
                                else
                                {
                                    list.Add(Tuple.Create(newI, newJ));
                                }
                            }
                        }

                        oldJ++;
                    }

                    oldI++;
                }
                // добаявлем start и finish
                int k, l;

                _oldGrid.GetIndexes(start, out k, out l);
                List<Tuple<int, int>> _listStart;
                if (!futurePins.TryGetValue(_nonRealizedMetalID, out _listStart))
                {
                    futurePins[_nonRealizedMetalID] = new List<Tuple<int,int>>();
                    futurePins[_nonRealizedMetalID].Add(Tuple.Create(k - _upBorderLimitingRectangle, l - _leftBorderLimitingRectangle));
                }
                else
                {
                    _listStart.Add(Tuple.Create(k - _upBorderLimitingRectangle, l - _leftBorderLimitingRectangle));
                }

                _oldGrid.GetIndexes(finish, out k, out l);
                List<Tuple<int, int>> _listFinish;
                if (!futurePins.TryGetValue(_nonRealizedMetalID, out _listFinish))
                {
                    futurePins[_nonRealizedMetalID] = new List<Tuple<int, int>>();
                    futurePins[_nonRealizedMetalID].Add(Tuple.Create(k - _upBorderLimitingRectangle, l - _leftBorderLimitingRectangle));
                }
                else
                {
                    _listFinish.Add(Tuple.Create(k - _upBorderLimitingRectangle, l - _leftBorderLimitingRectangle));
                }

                // добавляем узлы из трассы с междоузлием    
                for (int i = 0; i < pathWithInternodes.Count; i++)
                {
                    _oldGrid.GetIndexes(pathWithInternodes[i], out k, out l);

                    List<Tuple<int, int>> trackList;
                    if(!tracks.TryGetValue(_nonRealizedMetalID, out trackList))
                    {
                        tracks[_nonRealizedMetalID] = new List<Tuple<int, int>>();
                        tracks[_nonRealizedMetalID].Add(Tuple.Create(k - _upBorderLimitingRectangle, l - _leftBorderLimitingRectangle));
                    }
                    else
                    {
                        trackList.Add(Tuple.Create(k - _upBorderLimitingRectangle, l - _leftBorderLimitingRectangle));
                    }
                }

                // граничные узлы делаем pin'ами и запоминаем какие узлы мы сделали 
                _pinnedNodes = new List<Tuple<int, int, IElement>>();
                foreach (var track in futurePins)
                {
                    for (int i = 0; i < track.Value.Count; i++)
                    {
                        IElement elem = _newGrid[track.Value[i].Item1, track.Value[i].Item2].ViewElement;
                        if (!_newGrid.IsPin(track.Value[i].Item1, track.Value[i].Item2))
                        {
                            Point p = _oldGrid.GetCoordCell(track.Value[i].Item1, track.Value[i].Item2);

                            GridElement el = _newGrid[track.Value[i].Item1, track.Value[i].Item2];
                            IElement prevEl = el.ViewElement;

                            el.ViewElement = new Pin(p.x + p0.x, p.y + p0.y, 1 * _newGrid.Koeff, 1 * _newGrid.Koeff);
                            _newGrid[track.Value[i].Item1, track.Value[i].Item2] = el;

                            _pinnedNodes.Add(Tuple.Create(track.Value[i].Item1, track.Value[i].Item2, prevEl));
                        }
                    }
                }

                //формируем матрицу коэфициентов-штрафов
                int[,] penaltyMatrix = new int[_newGrid.CountRows, _newGrid.CountColumn];
                int startKoeff = Math.Max(_newGrid.CountColumn, _newGrid.CountRows);
                FormPenaltyMatrix(ref _oldGrid, startKoeff, ref penaltyMatrix, ref tracks);

                // сосчитать сумарный штраф для каждой трассы реализованной и нет (как считать, если в пути указаны узлы, как реальные так и виртуальные? дополнять до полной(реальная + виртуальная) трассы уже реализованные)
                Dictionary<string, int> penalty = new Dictionary<string, int>();
                CalculatePenalty(_oldGrid, tracks, penaltyMatrix, ref penalty);

                // перетрассировать 
                if (Retracing(ref _newGrid, tracks, futurePins, penalty))
                {
                    DrawStagesForNewGridDebug("retracing_" + _nonRealizedMetalID + "_" + start + "_" + finish + "_");

                    // превратить те узлы что стали пинами обртано в просто узлы
                    for (int i = 0; i < _pinnedNodes.Count; i++)
                    {
                        GridElement el = _newGrid[_pinnedNodes[i].Item1, _pinnedNodes[i].Item2];
                        el.ViewElement = _pinnedNodes[i].Item3;
                        _newGrid[_pinnedNodes[i].Item1, _pinnedNodes[i].Item2] = el;
                    }

                    // копируем матрицу обратно
                    numElement = 0;
                    for (int i = _upBorderLimitingRectangle; i <= _downBorderLimitingRectangle; ++i)
                    {
                        for (int j = _leftBorderLimitingRectangle; j <= _rightBorderLimitingRectangle; ++j)
                        {
                            _oldGrid[i, j] = _newGrid[numElement];
                            numElement++;
                        }
                    }

                    return true;
                }
                else
                {
                    //DrawStagesForNewGridDebug("non_retracing_" + _nonRealizedMetalID + "_" + start + "_" + finish + "_");

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void CalculatePenalty(Grid grid, Dictionary<string, List<Tuple<int, int>>> tracks, int[,] penaltyMatrix, ref Dictionary<string, int> penalty)
        {
            foreach (var track in tracks)
            {
                if (track.Key != _nonRealizedMetalID)
                {
                    int sum = 0;
                    for (int item = 0; item < track.Value.Count; ++item)
                    {
                        int i = track.Value[item].Item1;
                        int j = track.Value[item].Item2;
                        sum += penaltyMatrix[i, j];
                    }
                    penalty.Add(track.Key, sum);
                }
            }
        }

        private bool Retracing(ref Grid grid, Dictionary<string, List<Tuple<int, int>>> tracks, Dictionary<string, List<Tuple<int, int>>> futurePins, Dictionary<string, int> penalty)
        {
            WaveTraceAlgScheme li = new WaveTraceAlgScheme(grid);
            long time;

            Bitmap bmp = new Bitmap(grid.Width, grid.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.TranslateTransform(-p0.x, -p0.y);

            while (penalty.Count != 0)
            {
                // 0. ищем максимальный штраф
                string MetalID = null;
                int max = 0;
                foreach (var track in penalty)
                {
                    if (max < track.Value)
                    {
                        max = track.Value;
                        MetalID = track.Key;
                    }
                }
                if (!string.IsNullOrEmpty(MetalID))
                {
                    penalty.Remove(MetalID);

                    // 1. снимаем трассу с максимальным штрафом
                    for (int i = 0; i < _newGrid.Count; i++)
                    {
                        if (grid[i].MetalID == MetalID)
                        {
                            GridElement el = _newGrid[i];
                            el.MetalID = null;
                            if (_newGrid[i].ViewElement is Metal)
                            {
                                el.ViewElement = null;
                            }
                            _newGrid[i] = el;
                        }
                    }

                    // 2. перетрассирум трассу которую не смогли
                    List<int> _netList = new List<int>();
                    List<List<int>> _trackList;
                    foreach (var pin in futurePins[_nonRealizedMetalID])
                    {
                        _netList.Add(grid.GetNum(pin.Item1, pin.Item2));
                    }
                    List<int> nonRealized;
                    li.FindPath(_nonRealizedMetalID, new Net(_netList.ToArray()), out _trackList, out nonRealized, out time);
                    if (nonRealized.Count == 0)
                    {
                        if (_trackList.Count != 0)
                        {
                            grid.MetallizeTrack(_trackList, 1.0f, _nonRealizedMetalID);
                        }

                        //DrawStagesForNewGridDebug("AfterNonRealTest_1.bmp");

                        // 3. перетрассируем трассу с максимальным штрафом
                        List<int> netList = new List<int>();
                        List<List<int>> trackList;
                        foreach (var pin in futurePins[MetalID])
                        {
                            netList.Add(grid.GetNum(pin.Item1, pin.Item2));
                        }
                        li.FindPath(MetalID, new Net(netList.ToArray()), out trackList, out nonRealized, out time);
                        if (nonRealized.Count == 0)
                        {
                            if (trackList.Count != 0)
                            {
                                grid.MetallizeTrack(trackList, 1.0f, MetalID);

                                //DrawStagesForNewGridDebug("AfterNonRealTest_2.bmp");

                                return true;
                            }
                            else
                            {
                                // снимаем трассу с non_realized
                                foreach (var track in _trackList)
                                {
                                    foreach (var item in track)
                                    {
                                        if (grid[item].MetalID == _nonRealizedMetalID)
                                        {
                                            GridElement el = _newGrid[item];
                                            el.MetalID = null;
                                            if (_newGrid[item].ViewElement is Metal)
                                            {
                                                el.ViewElement = null;
                                            }

                                            _newGrid[item] = el;
                                        }
                                    }
                                }

                                // восстановить снятую трассу max_penalty
                                List<List<int>> list_track = new List<List<int>>();
                                List<int> list = new List<int>();
                                foreach (var item in tracks[MetalID])
                                {
                                    list.Add(grid.GetNum(item.Item1, item.Item2));
                                }
                                list_track.Add(list);
                                grid.MetallizeTrack(list_track, 1.0f, MetalID);
                            }
                        }
                        else
                        {
                            // снимаем трассу с non_realized
                            foreach (var track in _trackList)
                            {
                                foreach (var item in track)
                                {
                                    if (grid[item].MetalID == _nonRealizedMetalID)
                                    {
                                        GridElement el = _newGrid[item];
                                        el.MetalID = null;
                                        if (_newGrid[item].ViewElement is Metal)
                                        {
                                            el.ViewElement = null;
                                        }

                                        _newGrid[item] = el;
                                    }
                                }
                            }

                            // восстановить снятую трассу max_penalty
                            List<List<int>> list_track = new List<List<int>>();
                            List<int> list = new List<int>();
                            foreach (var item in tracks[MetalID])
                            {
                                list.Add(grid.GetNum(item.Item1, item.Item2));
                            }
                            list_track.Add(list);
                            grid.MetallizeTrack(list_track, 1.0f, MetalID);
                        }
                    }
                    else
                    {
                        // восстанавливаем снятую трассу
                        List<List<int>> list_track = new List<List<int>>();
                        List<int> list = new List<int>();
                        foreach (var item in tracks[MetalID])
                        {
                            list.Add(grid.GetNum(item.Item1, item.Item2));
                        }
                        list_track.Add(list);
                        grid.MetallizeTrack(list_track, 1.0f, MetalID);
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private void FormPenaltyMatrix(ref Grid grid, int startKoeff, ref int[,] fineMmatrix, ref Dictionary<string, List<Tuple<int, int>>> tracks)
        {
            _set.Clear();
            
            for (int item = 0; item < tracks[_nonRealizedMetalID].Count; item++)
            {
                fineMmatrix[tracks[_nonRealizedMetalID][item].Item1,
                            tracks[_nonRealizedMetalID][item].Item2] = startKoeff;
                _set.Add(_newGrid.GetNum(tracks[_nonRealizedMetalID][item].Item1, tracks[_nonRealizedMetalID][item].Item2), startKoeff);
            }

            startKoeff--;

            int i = 0;
            int j = 0;

            int prevCountAdded;
            int countAdded = _set.Count;
            for (int index = 0; index < _set.Count;)
            {
                prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    if (index + elEdded < _set.Count)
                    {
                        _newGrid.GetIndexes(_set[index + elEdded].NumCell, out i, out j);

                        if (_set[index].NumLevel == startKoeff + 1)
                        {
                            if (i - 1 >= 0 && fineMmatrix[i - 1, j] == 0) // up
                            {
                                fineMmatrix[i - 1, j] = startKoeff;
                                _set.Add((i - 1) * _newGrid.CountColumn + j, startKoeff);
                                countAdded++;
                            }
                            if (i + 1 < _newGrid.CountRows && fineMmatrix[i + 1, j] == 0) // down
                            {
                                fineMmatrix[i + 1, j] = startKoeff;
                                _set.Add((i + 1) * _newGrid.CountColumn + j, startKoeff);
                                countAdded++;

                            }
                            if (j - 1 >= 0 && fineMmatrix[i, j - 1] == 0) // left
                            {
                                fineMmatrix[i, j - 1] = startKoeff;
                                _set.Add(i * _newGrid.CountColumn + (j - 1), startKoeff);
                                countAdded++;
                            }
                            if (j + 1 < _newGrid.CountColumn && fineMmatrix[i, j + 1] == 0) // right
                            {
                                fineMmatrix[i, j + 1] = startKoeff;
                                _set.Add(i * _newGrid.CountColumn + (j + 1), startKoeff);
                                countAdded++;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                if (countAdded > 0)
                {
                    index += prevCountAdded;
                    startKoeff--;
                }
                if (countAdded == 0) // условие выхода
                {
                    return;
                }
            }
        }

        private bool IsBoardGridElement(ref Grid grid, ref Grid newGrid, int newI, int newJ, int oldI, int oldJ)
        {
            try
            {
                if (newJ - 1 == -1) // left
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldI, oldJ - 2].MetalID)
                    {
                        return true;
                    }
                }
                if (newJ + 1 == newGrid.CountColumn) // right
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldI, oldJ + 2].MetalID)
                    {
                        return true;
                    }
                }
                if (newI - 1 == -1) // up
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldI - 2, oldJ].MetalID)
                    {
                        return true;
                    }
                }
                if (newI + 1 == newGrid.CountRows) // down
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldI + 2, oldJ].MetalID)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }  
        }

        private void GetCoordLimitingRectangle(ref Grid grid, ref List<int> path, int additLeftParam, int additUpParam, int additRightParam, int additDownParam)
        {
            _leftBorderLimitingRectangle = int.MaxValue;
            _upBorderLimitingRectangle = int.MaxValue;
            for (int node = 0; node < path.Count; node++) 
            {
                int i, j;
                grid.GetIndexes(path[node], out i, out j);

                if (j < _leftBorderLimitingRectangle) // left
                {
                    _leftBorderLimitingRectangle = j;
                }

                if (_rightBorderLimitingRectangle < j) // right
                {
                    _rightBorderLimitingRectangle = j;
                }

                if (i < _upBorderLimitingRectangle) // up
                {
                    _upBorderLimitingRectangle = i;
                }

                if (_downBorderLimitingRectangle < i) // down
                {
                    _downBorderLimitingRectangle = i;
                }
            }

            if (_leftBorderLimitingRectangle % 2 == 1) _leftBorderLimitingRectangle++;
            if (_upBorderLimitingRectangle % 2 == 1) _upBorderLimitingRectangle++;
            if (_rightBorderLimitingRectangle % 2 == 1) _rightBorderLimitingRectangle++;
            if (_downBorderLimitingRectangle % 2 == 1) _downBorderLimitingRectangle++;


            _leftBorderLimitingRectangle -= (2 * additLeftParam);
            if (_leftBorderLimitingRectangle < 0) 
                _leftBorderLimitingRectangle = 0;

            _upBorderLimitingRectangle -= (2 * additUpParam);
            if (_upBorderLimitingRectangle < 0) 
                _upBorderLimitingRectangle = 0;

            _rightBorderLimitingRectangle += (2 * additRightParam);
            if (_rightBorderLimitingRectangle > grid.CountColumn) 
                _rightBorderLimitingRectangle = (grid.CountColumn % 2 == 0) ? grid.CountColumn : grid.CountColumn - 1;

            _downBorderLimitingRectangle += (2 * additDownParam);
            if (_downBorderLimitingRectangle > grid.CountRows) 
                _downBorderLimitingRectangle = (grid.CountRows % 2 == 0) ? grid.CountRows : grid.CountRows - 1;
        }

        private bool WavePropagation(ref Grid grid, int start)
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
                    grid.GetIndexes(_set[index + elEdded].NumCell, out i, out j);

                    if (_set[index].NumLevel == numLevel - 1)
                    {
                        if (i - 2 >= 0 && CheckNormalCell(ref grid, i - 2, j, numLevel, ref countAdded)) // left
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (i + 2 < grid.CountRows && CheckNormalCell(ref grid, i + 2, j, numLevel, ref countAdded)) // right
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j - 2 >= 0 && CheckNormalCell(ref grid, i, j - 2, numLevel, ref countAdded)) // up
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j + 2 < grid.CountColumn && CheckNormalCell(ref grid, i, j + 2, numLevel, ref countAdded)) // down
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
                {
                    return false;
                }
            }

            return isFoundFinish;
        }
        private bool CheckNormalCell(ref Grid grid, int i, int j, int numLevel, ref int countAdded)
        {
            if (grid.IsProhibitionZone(i, j)) //  Если зона запрета
            {
                return false;
            }
            else
            {
                if (_allPins.Contains(grid.GetNum(i, j)) && _set.Add(grid.GetNum(i, j), numLevel)) // финишный Pin
                {
                    countAdded++;
                    return true;
                }
                else
                {
                    if (grid.IsFreeMetal(i, j, _nonRealizedMetalID) && _set.Add(grid.GetNum(i, j), numLevel)) // Если свободный метал 
                    {
                        countAdded++;
                        return false;
                    }
                    else
                    {
                        if (grid.IsOwnMetal(i, j, _nonRealizedMetalID) && _set.Add(grid.GetNum(i, j), numLevel)) // если свой метал
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

        private bool VirtualWavePropagation(ref Grid grid, int start, out int finish, out bool isFinishPin)
        { 
             int numLevel = 0;
             _set.Clear();
            _set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;
            finish = -1;
            isFinishPin = false;

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
                    grid.GetIndexes(_set[index + elEdded].NumCell, out i, out j);

                    if (_set[index].NumLevel == numLevel - 1)
                    {
                        if (i - 1 >= 0 && CheckAllCell(ref grid, i - 1, j, numLevel, ref countAdded, ref isFinishPin)) // left
                        {
                            isFoundFinish = true;
                            finish = grid.GetNum(i - 1, j);
                            break;
                        }
                        if (i + 1 < grid.CountRows && CheckAllCell(ref grid, i + 1, j, numLevel, ref countAdded, ref isFinishPin)) // right
                        {
                            isFoundFinish = true;
                            finish = grid.GetNum(i + 1, j);
                            break;
                        }
                        if (j - 1 >= 0 && CheckAllCell(ref grid, i, j - 1, numLevel, ref countAdded, ref isFinishPin)) // up
                        {
                            isFoundFinish = true;
                            finish = grid.GetNum(i, j - 1);
                            break;
                        }
                        if (j + 1 < grid.CountColumn && CheckAllCell(ref grid, i, j + 1, numLevel, ref countAdded, ref isFinishPin)) // down
                        {
                            isFoundFinish = true;
                            finish = grid.GetNum(i, j + 1);
                            break;
                        }
                    }
                }
                if (countAdded > 0)
                {
                    index += prevCountAdded;
                    numLevel++;
                }
                if (countAdded == 0) // условие, что нельзя реализовать
                {
                    return false;
                }
            }

            return isFoundFinish;
        }
        private bool CheckAllCell(ref Grid grid, int i, int j, int numLevel, ref int countAdded, ref bool isFinishPin)
        {
            if (grid.IsProhibitionZone(i, j)) //  Если зона запрета
            {
                return false;
            }
            else
            {
                if (_allPins.Contains(grid.GetNum(i, j)) && _set.Add(grid.GetNum(i, j), numLevel)) // финишный Pin
                {
                    countAdded++;
                    isFinishPin = true;
                    return true;
                }
                else
                {
                    if (grid.IsFreeMetal(i, j, _nonRealizedMetalID) && _set.Add(grid.GetNum(i, j), numLevel)) // Если свободный метал 
                    {
                        if (j - 1 >= 0 && j + 1 < grid.CountColumn && i - 1 >= 0 && i + 1 < grid.CountRows)
                        {
                            // условие что найдено междоузлие
                            if (((grid[i, j - 1].MetalID != grid[i, j + 1].MetalID) && !string.IsNullOrEmpty(grid[i, j - 1].MetalID) && !string.IsNullOrEmpty(grid[i, j + 1].MetalID)) ||
                                (grid[i - 1, j].MetalID != grid[i + 1, j].MetalID && !string.IsNullOrEmpty(grid[i - 1, j].MetalID) && !string.IsNullOrEmpty(grid[i + 1, j].MetalID)))
                            {
                                _virtualSet.Add(grid.GetNum(i, j), numLevel);
                            }
                        }

                        countAdded++;
                        return false;
                    }
                    else
                    {
                        if (grid.IsOwnMetal(i, j, _nonRealizedMetalID) && _set.Add(grid.GetNum(i, j), numLevel)) // если свой метал
                        {
                            countAdded++;
                            isFinishPin = false;
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

        private bool RestorationPath(ref Grid grid, ref List<int> path)
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

            path.Add(currentNumCell); // добавляем в путь
            while (currentLevel > 0)
            {
                currentLevel--;

                grid.GetIndexes(currentNumCell, out i, out j);
                if (i - 1 >= 0 && SetMetalCell(ref grid, grid.GetNum(i - 1, j), currentLevel, ref currentNumCell, ref path)) // left
                {
                    continue;
                }
                if (i + 1 < grid.CountRows && SetMetalCell(ref grid, grid.GetNum(i + 1, j), currentLevel, ref currentNumCell, ref path)) // right
                {
                    continue;
                }
                if (j - 1 >= 0 && SetMetalCell(ref grid, grid.GetNum(i, j - 1), currentLevel, ref currentNumCell, ref path)) // up
                {
                    continue;
                }
                if (j + 1 < grid.CountColumn && SetMetalCell(ref grid, grid.GetNum(i, j + 1), currentLevel, ref currentNumCell, ref path)) // down
                {
                    continue;
                }
            }

            return true;
        }
        private bool SetMetalCell(ref Grid grid, int numCell, int currentLevel, ref int currentNumCell, ref List<int> path)
        {
            if (_set.ContainsNumCell(numCell) && _set.GetNumLevel(numCell) == currentLevel)
            {
                path.Add(numCell);
                currentNumCell = numCell;
                return true;
            }
            return false;
        }

        private void DrawStagesForNewGridDebug(string stageName)
        {
            Bitmap bmp = new Bitmap(_newGrid.Width, _newGrid.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.TranslateTransform(-p0.x, -p0.y);
            g.Clear(System.Drawing.Color.Black);
            _newGrid.Draw(g);
            bmp.Save(stageName + "_Stage.bmp");
        }

        private void DrawStagesForOldGridDebug(string stageName)
        {
            Bitmap bmp = new Bitmap(_oldGrid.Width, _oldGrid.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(System.Drawing.Color.Black);
            _oldGrid.Draw(g);
            bmp.Save(stageName + "_Stage.bmp");
        }
    }
}
