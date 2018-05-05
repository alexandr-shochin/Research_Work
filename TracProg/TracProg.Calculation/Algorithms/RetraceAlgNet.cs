using System;
using System.Collections.Generic;
using System.Drawing;
using TracProg.Calculation.BoardElements;

namespace TracProg.Calculation.Algorithms
{
    public class RetraceAlgNet
    {
        private class Node
        {
            public int I { get; set; }
            public int J { get; set; }

            public override string ToString()
            {
                return I + ", " + J;
            }
        }

        private readonly Set _set;
        private readonly Set _virtualSet;

        private int _leftBorderLimitingRectangle;
        private int _upBorderLimitingRectangle;
        private int _rightBorderLimitingRectangle;
        private int _downBorderLimitingRectangle;

        private List<Tuple<Node, IBoardElement>> _pinnedNodes;

        private TraceGrid _oldGrid;
        private TraceGrid _newGrid;

        private readonly string _nonRealizedMetalId;

        private Point _p0;

        private readonly List<int> _allPins;

        public RetraceAlgNet(TraceGrid oldGrid, string nonRealizedMetalId, List<int> allPins)
        {
            _oldGrid = oldGrid;
            _set = new Set();
            _virtualSet = new Set();

            _nonRealizedMetalId = nonRealizedMetalId;
            _allPins = allPins;
        }

        public bool FindPath(int start, out int finish, out bool isFinishPin)
        {
            finish = -1;
            isFinishPin = false;
            if (!WavePropagation(ref _oldGrid, start) && VirtualWavePropagation(ref _oldGrid, start, out finish, out isFinishPin)) // нашли трассу через междоузлие
            {
                var pathWithInternodes = new List<int>();
                RestorationPath(ref _oldGrid, ref pathWithInternodes);

                // получаем координаты ограничивающего прямоугольника
                GetCoordLimitingRectangleByTrack(ref _oldGrid, ref pathWithInternodes, 5, 5, 5, 5,
                    out _leftBorderLimitingRectangle, out _upBorderLimitingRectangle, out _rightBorderLimitingRectangle, out _downBorderLimitingRectangle);

                // копируем нужные элементы сетки и создаём новую
                TraceGrid.TraceGridElement[] gridElements = new TraceGrid.TraceGridElement[(_rightBorderLimitingRectangle - _leftBorderLimitingRectangle + 1) * (_downBorderLimitingRectangle - _upBorderLimitingRectangle + 1)];
                Dictionary<string, List<Node>> futurePins = new Dictionary<string, List<Node>>(); //<MetalID, список с Pin или граничным узлом>
                Dictionary<string, List<Node>> tracks = new Dictionary<string, List<Node>>(); // реализованные трассы в прямоугольнике в том числе с междоузлие

                int numElement = 0;
                for (int i = _upBorderLimitingRectangle; i <= _downBorderLimitingRectangle; ++i)
                {
                    for (int j = _leftBorderLimitingRectangle; j <= _rightBorderLimitingRectangle; ++j)
                    {
                        gridElements[numElement] = _oldGrid[i, j];
                        numElement++;
                    }
                }

                _p0 = _oldGrid.GetCoordCell(_upBorderLimitingRectangle, _leftBorderLimitingRectangle); // координата смещения

                _newGrid = new TraceGrid("Bounding_box_" + _nonRealizedMetalId, gridElements, _p0.X, _p0.Y,
                    (((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 2) / 2) * _oldGrid.Koeff,
                    (((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 2) / 2) * _oldGrid.Koeff,
                    _oldGrid.Koeff);

                // ищем граничные узлы
                int oldI = _upBorderLimitingRectangle;
                for (int newI = 0; newI < _newGrid.CountRows; newI++)
                {
                    int oldJ = _leftBorderLimitingRectangle;
                    for (int newJ = 0; newJ < _newGrid.CountColumn; newJ++)
                    {
                        if (!string.IsNullOrEmpty(_newGrid[newI, newJ].MetalId))
                        {
                            // ищем реализованные трассы в нашем прямоугольнике
                            List<Node> trackList;
                            if (!tracks.TryGetValue(_newGrid[newI, newJ].MetalId, out trackList))
                            {
                                tracks[_newGrid[newI, newJ].MetalId] = new List<Node> {new Node()
                                {
                                    I = newI,
                                    J = newJ
                                }};
                            }
                            else
                            {
                                trackList.Add(new Node()
                                {
                                    I = newI,
                                    J = newJ
                                });
                            }

                            if (IsBoardGridElement(newI, newJ, oldI, oldJ))
                            {
                                List<Node> list;
                                if (!futurePins.TryGetValue(_newGrid[newI, newJ].MetalId, out list))
                                {
                                    futurePins[_newGrid[newI, newJ].MetalId] = new List<Node> { new Node()
                                    {
                                        I = newI,
                                        J = newJ
                                    }};
                                }
                                else
                                {
                                    list.Add(new Node() { I = newI, J = newJ });
                                }
                            }
                            else if (_newGrid.IsPin(newI, newJ))
                            {
                                List<Node> list;
                                if (!futurePins.TryGetValue(_newGrid[newI, newJ].MetalId, out list))
                                {
                                    futurePins[_newGrid[newI, newJ].MetalId] = new List<Node> { new Node()
                                    {
                                        I = newI,
                                        J = newJ
                                    }};
                                }
                                else
                                {
                                    list.Add(new Node()
                                    {
                                        I = newI,
                                        J = newJ
                                    });
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
                List<Node> listStart;
                if (!futurePins.TryGetValue(_nonRealizedMetalId, out listStart))
                {
                    futurePins[_nonRealizedMetalId] = new List<Node> { new Node()
                    {
                        I = k - _upBorderLimitingRectangle,
                        J = l - _leftBorderLimitingRectangle
                    }};
                }
                else
                {
                    listStart.Add( new Node()
                    {
                        I = k - _upBorderLimitingRectangle,
                        J = l - _leftBorderLimitingRectangle
                    });
                }

                _oldGrid.GetIndexes(finish, out k, out l);
                List<Node> listFinish;
                if (!futurePins.TryGetValue(_nonRealizedMetalId, out listFinish))
                {
                    futurePins[_nonRealizedMetalId] = new List<Node> { new Node()
                    {
                        I = k - _upBorderLimitingRectangle,
                        J = l - _leftBorderLimitingRectangle
                    }};
                }
                else
                {
                    listFinish.Add(new Node()
                    {
                        I  = k - _upBorderLimitingRectangle,
                        J  = l - _leftBorderLimitingRectangle
                    });
                }

                // добавляем узлы из трассы с междоузлием    
                foreach (int internode in pathWithInternodes)
                {
                    _oldGrid.GetIndexes(internode, out k, out l);

                    List<Node> trackList;
                    if (!tracks.TryGetValue(_nonRealizedMetalId, out trackList))
                    {
                        tracks[_nonRealizedMetalId] = new List<Node> { new Node()
                        {
                            I = k - _upBorderLimitingRectangle,
                            J = l - _leftBorderLimitingRectangle
                        }};
                    }
                    else
                    {
                        trackList.Add(new Node()
                        {
                            I = k - _upBorderLimitingRectangle,
                            J = l - _leftBorderLimitingRectangle
                        });
                    }
                }

                // граничные узлы делаем pin'ами и запоминаем какие узлы мы сделали 
                _pinnedNodes = new List<Tuple<Node, IBoardElement>>();
                foreach (var track in futurePins)
                {
                    foreach (Node node in track.Value)
                    {
                        if (!_newGrid.IsPin(node.I, node.J))
                        {
                            Point p = _oldGrid.GetCoordCell(node.I, node.J);

                            TraceGrid.TraceGridElement el = _newGrid[node.I, node.J];
                            IBoardElement prevEl = el.ViewElement;

                            el.ViewElement = new Pin(prevEl.ID, p.X + _p0.X, p.Y + _p0.Y, 1 * _newGrid.Koeff, 1 * _newGrid.Koeff);
                            _newGrid[node.I, node.J] = el;

                            _pinnedNodes.Add(Tuple.Create(node, prevEl));
                        }
                    }
                }

                //формируем матрицу коэфициентов-штрафов
                int[,] penaltyMatrix = new int[_newGrid.CountRows, _newGrid.CountColumn];
                int startKoeff = Math.Max(_newGrid.CountColumn, _newGrid.CountRows);
                FormPenaltyMatrix(startKoeff, ref penaltyMatrix, ref tracks);

                Dictionary<string, int> penalty = new Dictionary<string, int>();
                CalculatePenalty(tracks, penaltyMatrix, ref penalty);

                // перетрассировать 
                if (Retracing(tracks, futurePins, penalty))
                {
                    //DrawStagesForNewGridDebug("retracing_" + _nonRealizedMetalId + "_" + start + "_" + finish + "_");

                    // превратить те узлы что стали пинами обртано в просто узлы
                    foreach (var pinnedNode in _pinnedNodes)
                    {
                        TraceGrid.TraceGridElement el = _newGrid[pinnedNode.Item1.I, pinnedNode.Item1.J];
                        el.ViewElement = pinnedNode.Item2;
                        _newGrid[pinnedNode.Item1.I, pinnedNode.Item1.J] = el;
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
            }

            return false;
        }

        private void CalculatePenalty(Dictionary<string, List<Node>> tracks, int[,] penaltyMatrix, ref Dictionary<string, int> penalty)
        {
            foreach (var track in tracks)
            {
                if (track.Key != _nonRealizedMetalId)
                {
                    int sum = 0;
                    foreach (Node node in track.Value)
                    {
                        sum += penaltyMatrix[node.I, node.J];
                    }
                    penalty.Add(track.Key, sum);
                }
            }
        }

        private bool Retracing(Dictionary<string, List<Node>> tracks, Dictionary<string, List<Node>> futurePins, Dictionary<string, int> penalty)
        {
            WaveTraceAlgScheme li = new WaveTraceAlgScheme(_newGrid);

            while (penalty.Count != 0)
            {
                // 0. ищем максимальный штраф
                string maxPenaltyMetalId = null;
                int max = 0;
                foreach (var track in penalty)
                {
                    if (max < track.Value)
                    {
                        max = track.Value;
                        maxPenaltyMetalId = track.Key;
                    }
                }
                if (!string.IsNullOrEmpty(maxPenaltyMetalId))
                {
                    penalty.Remove(maxPenaltyMetalId);

                    // 1. снимаем трассу с максимальным штрафом

                    List<List<int>> maxPenaltyList = new List<List<int>>() { new List<int>() };
                    foreach (Node node in tracks[maxPenaltyMetalId])
                    {
                        maxPenaltyList[0].Add(_newGrid.GetNum(node.I, node.J));
                    }
                    _newGrid.UnmetalizeTracks(maxPenaltyList);

                    //for (int i = 0; i < _newGrid.Count; i++)
                    //{
                    //    if (_newGrid[i].MetalId == maxPenaltyMetalId)
                    //    {
                    //        TraceGrid.TraceGridElement el = _newGrid[i];
                            
                    //        if (_newGrid[i].ViewElement is Metal)
                    //        {
                    //            el.MetalId = null;
                    //            el.ViewElement = null;
                    //            el.IsReTracedArea = null;
                    //        }

                    //        if (_newGrid[i].ViewElement is Pin)
                    //        {
                    //            el.MetalId = null;
                    //            (el.ViewElement as Pin).IsRealized = false;
                    //        }

                    //        _newGrid[i] = el;
                    //    }
                    //}

                    // 2. перетрассирум трассу которую не смогли
                    List<int> _netList = new List<int>();
                    List<List<int>> _realizedTracks;
                    foreach (Node node in futurePins[_nonRealizedMetalId])
                    {
                        _netList.Add(_newGrid.GetNum(node.I, node.J));
                    }

                    List<int> nonRealizedPins;
                    li.FindPath(_nonRealizedMetalId, new Net(_netList.ToArray()), out _realizedTracks, out nonRealizedPins);
                    if (nonRealizedPins.Count == 0)
                    {
                        if (_realizedTracks.Count != 0)
                        {
                            _newGrid.MetallizeTracks(_realizedTracks, _nonRealizedMetalId);

                            foreach (List<int> track in _realizedTracks)
                            {
                                foreach (int node in track)
                                {
                                    Point p = _newGrid.GetCoordCell(node);
                                    TraceGrid.TraceGridElement el = _newGrid[node];
                                    el.IsReTracedArea = new ReTracedArea("ReTracedArea", p.X, p.Y, 1 * _oldGrid.Koeff, 1 * _oldGrid.Koeff);
                                    _newGrid[node] = el;
                                }
                            }
                        }

                        // 3. перетрассируем трассу с максимальным штрафом
                        List<int> netList = new List<int>();
                        List<List<int>> realizedTracks;
                        foreach (Node pin in futurePins[maxPenaltyMetalId])
                        {
                            netList.Add(_newGrid.GetNum(pin.I, pin.J));
                        }
                        li.FindPath(maxPenaltyMetalId, new Net(netList.ToArray()), out realizedTracks, out nonRealizedPins);
                        if (nonRealizedPins.Count == 0)
                        {
                            if (realizedTracks.Count != 0)
                            {
                                _newGrid.MetallizeTracks(realizedTracks, maxPenaltyMetalId);

                                foreach (List<int> track in realizedTracks)
                                {
                                    foreach (int node in track)
                                    {
                                        Point p = _newGrid.GetCoordCell(node);
                                        TraceGrid.TraceGridElement el = _newGrid[node];
                                        el.IsReTracedArea = new ReTracedArea("ReTracedArea", p.X, p.Y, 1 * _oldGrid.Koeff, 1 * _oldGrid.Koeff);
                                        _newGrid[node] = el;
                                    }
                                }

                                return true;
                            }
                            else
                            {
                                _newGrid.UnmetalizeTracks(_realizedTracks); // снимаем трассу с non_realized

                                // восстановить снятую трассу max_penalty
                                List<List<int>> list_track = new List<List<int>>();
                                List<int> list = new List<int>();
                                foreach (Node node in tracks[maxPenaltyMetalId])
                                {
                                    list.Add(_newGrid.GetNum(node.I, node.J));
                                }
                                list_track.Add(list);
                                _newGrid.MetallizeTracks(list_track, maxPenaltyMetalId);
                            }
                        }
                        else
                        {
                            _newGrid.UnmetalizeTracks(_realizedTracks); // снимаем трассу с non_realized

                            // восстановить снятую трассу max_penalty
                            List<List<int>> list_track = new List<List<int>>();
                            List<int> list = new List<int>();
                            foreach (Node node in tracks[maxPenaltyMetalId])
                            {
                                list.Add(_newGrid.GetNum(node.I, node.J));
                            }
                            list_track.Add(list);
                            _newGrid.MetallizeTracks(list_track, maxPenaltyMetalId);
                        }
                    }
                    else
                    {
                        // восстанавливаем снятую трассу
                        List<List<int>> list_track = new List<List<int>>();
                        List<int> list = new List<int>();
                        foreach (Node node in tracks[maxPenaltyMetalId])
                        {
                            list.Add(_newGrid.GetNum(node.I, node.J));
                        }
                        list_track.Add(list);
                        _newGrid.MetallizeTracks(list_track, maxPenaltyMetalId);
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private void FormPenaltyMatrix(int startKoeff, ref int[,] fineMmatrix, ref Dictionary<string, List<Node>> tracks)
        {
            _set.Clear();
            
            for (int item = 0; item < tracks[_nonRealizedMetalId].Count; item++)
            {
                fineMmatrix[tracks[_nonRealizedMetalId][item].I, tracks[_nonRealizedMetalId][item].J] = startKoeff;
                _set.Add(_newGrid.GetNum(tracks[_nonRealizedMetalId][item].I, tracks[_nonRealizedMetalId][item].J), startKoeff);
            }

            startKoeff--;

            int countAdded = _set.Count;
            for (int index = 0; index < _set.Count;)
            {
                var prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    if (index + elEdded < _set.Count)
                    {
                        int i, j;
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

        private bool IsBoardGridElement(int newI, int newJ, int oldI, int oldJ)
        {
            try
            {
                if (newJ - 1 == -1) // left
                {
                    if (_newGrid[newI, newJ].MetalId == _oldGrid[oldI, oldJ - 2].MetalId)
                    {
                        return true;
                    }
                }
                if (newJ + 1 == _newGrid.CountColumn) // right
                {
                    if (_newGrid[newI, newJ].MetalId == _oldGrid[oldI, oldJ + 2].MetalId)
                    {
                        return true;
                    }
                }
                if (newI - 1 == -1) // up
                {
                    if (_newGrid[newI, newJ].MetalId == _oldGrid[oldI - 2, oldJ].MetalId)
                    {
                        return true;
                    }
                }
                if (newI + 1 == _newGrid.CountRows) // down
                {
                    if (_newGrid[newI, newJ].MetalId == _oldGrid[oldI + 2, oldJ].MetalId)
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

        private void GetCoordLimitingRectangleByTrack(ref TraceGrid grid, ref List<int> path, int additLeftParam, int additUpParam, int additRightParam, int additDownParam, 
            out int leftBorderLimitingRectangle, out int upBorderLimitingRectangle, out int rightBorderLimitingRectangle, out int downBorderLimitingRectangle)
        {
            rightBorderLimitingRectangle = 0;
            downBorderLimitingRectangle = 0;
            leftBorderLimitingRectangle = int.MaxValue;
            upBorderLimitingRectangle = int.MaxValue;

            foreach (int node in path)
            {
                int i, j;
                grid.GetIndexes(node, out i, out j);

                if (j < leftBorderLimitingRectangle) // left
                    leftBorderLimitingRectangle = j;

                if (rightBorderLimitingRectangle < j) // right
                    rightBorderLimitingRectangle = j;

                if (i < upBorderLimitingRectangle) // up
                    upBorderLimitingRectangle = i;

                if (downBorderLimitingRectangle < i) // down
                    downBorderLimitingRectangle = i;
            }

            if (leftBorderLimitingRectangle % 2 == 1)
                leftBorderLimitingRectangle++;

            if (upBorderLimitingRectangle % 2 == 1)
                upBorderLimitingRectangle++;

            if (rightBorderLimitingRectangle % 2 == 1)
                rightBorderLimitingRectangle++;

            if (downBorderLimitingRectangle % 2 == 1)
                downBorderLimitingRectangle++;

            leftBorderLimitingRectangle -= (2 * additLeftParam);
            if (leftBorderLimitingRectangle < 0) 
                leftBorderLimitingRectangle = 0;

            upBorderLimitingRectangle -= (2 * additUpParam);
            if (upBorderLimitingRectangle < 0) 
                upBorderLimitingRectangle = 0;

            rightBorderLimitingRectangle += (2 * additRightParam);
            if (rightBorderLimitingRectangle > grid.CountColumn) 
                rightBorderLimitingRectangle = (grid.CountColumn % 2 == 0) ? grid.CountColumn : grid.CountColumn - 1;

            downBorderLimitingRectangle += (2 * additDownParam);
            if (downBorderLimitingRectangle > grid.CountRows) 
                downBorderLimitingRectangle = (grid.CountRows % 2 == 0) ? grid.CountRows : grid.CountRows - 1;
        }

        private bool WavePropagation(ref TraceGrid grid, int start)
        {
            int numLevel = 0;
            _set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;

            int countAdded = 1;
            for (int index = 0; index < _set.Count && !isFoundFinish; )
            {
                var prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    int i, j;
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
        private bool CheckNormalCell(ref TraceGrid grid, int i, int j, int numLevel, ref int countAdded)
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
                    if (grid.IsFreeMetal(i, j) && _set.Add(grid.GetNum(i, j), numLevel)) // Если свободный метал 
                    {
                        countAdded++;
                        return false;
                    }
                    else
                    {
                        if (grid.IsOwnMetal(i, j, _nonRealizedMetalId) && _set.Add(grid.GetNum(i, j), numLevel)) // если свой метал
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

        private bool VirtualWavePropagation(ref TraceGrid grid, int start, out int finish, out bool isFinishPin)
        { 
             int numLevel = 0;
             _set.Clear();
            _set.Add(start, numLevel);
            numLevel++;

            bool isFoundFinish = false;
            finish = -1;
            isFinishPin = false;

            int countAdded = 1;
            for (int index = 0; index < _set.Count && !isFoundFinish; )
            {
                var prevCountAdded = countAdded;
                countAdded = 0;
                for (int elEdded = 0; elEdded < prevCountAdded; ++elEdded)
                {
                    int i, j;
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
        private bool CheckAllCell(ref TraceGrid grid, int i, int j, int numLevel, ref int countAdded, ref bool isFinishPin)
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
                    if (grid.IsFreeMetal(i, j) && _set.Add(grid.GetNum(i, j), numLevel)) // Если свободный метал 
                    {
                        if (j - 1 >= 0 && j + 1 < grid.CountColumn && i - 1 >= 0 && i + 1 < grid.CountRows)
                        {
                            // условие что найдено междоузлие
                            if (((grid[i, j - 1].MetalId != grid[i, j + 1].MetalId) && !string.IsNullOrEmpty(grid[i, j - 1].MetalId) && !string.IsNullOrEmpty(grid[i, j + 1].MetalId)) ||
                                (grid[i - 1, j].MetalId != grid[i + 1, j].MetalId && !string.IsNullOrEmpty(grid[i - 1, j].MetalId) && !string.IsNullOrEmpty(grid[i + 1, j].MetalId)))
                            {
                                _virtualSet.Add(grid.GetNum(i, j), numLevel);
                            }
                        }

                        countAdded++;
                        return false;
                    }
                    else
                    {
                        if (grid.IsOwnMetal(i, j, _nonRealizedMetalId) && _set.Add(grid.GetNum(i, j), numLevel)) // если свой метал
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

        private void RestorationPath(ref TraceGrid grid, ref List<int> path)
        {
            if (path == null)
            {
                path = new List<int>();
            }

            Set.ElementSet elSet = _set[_set.Count - 1];
            int currentNumCell = elSet.NumCell;
            int currentLevel = elSet.NumLevel;

            path.Add(currentNumCell); // добавляем в путь
            while (currentLevel > 0)
            {
                currentLevel--;

                int i, j;
                grid.GetIndexes(currentNumCell, out i, out j);
                if (i - 1 >= 0 && SetMetalCell(grid.GetNum(i - 1, j), currentLevel, ref currentNumCell, ref path)) // left
                {
                    continue;
                }
                if (i + 1 < grid.CountRows && SetMetalCell(grid.GetNum(i + 1, j), currentLevel, ref currentNumCell, ref path)) // right
                {
                    continue;
                }
                if (j - 1 >= 0 && SetMetalCell(grid.GetNum(i, j - 1), currentLevel, ref currentNumCell, ref path)) // up
                {
                    continue;
                }
                if (j + 1 < grid.CountColumn && SetMetalCell(grid.GetNum(i, j + 1), currentLevel, ref currentNumCell, ref path)) // down
                {
                    continue;
                }
            }
        }
        private bool SetMetalCell(int numCell, int currentLevel, ref int currentNumCell, ref List<int> path)
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
            g.TranslateTransform(-_p0.X, -_p0.Y);
            g.Clear(Color.White);
            _newGrid.Draw(g);
            bmp.Save(stageName + "_Stage.bmp");
        }

        private void DrawStagesForOldGridDebug(string stageName)
        {
            Bitmap bmp = new Bitmap(_oldGrid.Width, _oldGrid.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            _oldGrid.Draw(g);
            bmp.Save(stageName + "_Stage.bmp");
        }
    }
}
