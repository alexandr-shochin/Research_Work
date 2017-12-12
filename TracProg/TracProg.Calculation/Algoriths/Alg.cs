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
    public class Alg
    {
        private Set _set;
        private Set _virtualSet;

        private int _leftBorderLimitingRectangle;
        private int _upBorderLimitingRectangle;
        private int _rightBorderLimitingRectangle;
        private int _downBorderLimitingRectangle;

        private List<Tuple<int, int>> _pinnedNodes;

        private Grid _newGrid;

        public Alg()
        {
            _set = new Set();
            _virtualSet = new Set();
        }

        public void FindPath(ref Grid grid, int start, int finish)
        {
            if (WavePropagation(ref grid, start, finish) == true)
            {
                //RestorationPath(ref path);
            }
            else
            {
                if (VirtualWavePropagation(ref grid, start, finish) == true) // нашли трассу через междоузлие
                {
                    List<int> pathWithInternodes = new List<int>();
                    RestorationPath(ref grid, ref pathWithInternodes);

                    // получаем координаты ограничивающего прямоугольника
                    GetCoordLimitingRectangle(ref grid, ref pathWithInternodes, 1, 2, 1, 1);

                    // копируем нужные элементы сетки и создаём новую
                    GridElement[] gridElements = new GridElement[((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 1) * ((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 1)];
                    Dictionary<int, List<Tuple<int, int>>> futurePins = new Dictionary<int, List<Tuple<int, int>>>(); //<MetalID, список с Pin или граничным узлом>
                    Dictionary<int, List<Tuple<int, int>>> tracks = new Dictionary<int,List<Tuple<int,int>>>(); // реализованные трассы в прямоугольнике в том числе с междоузлие
                    for (int i = 0; i < grid.CurrentIDMetalTrack; i++)
                    {
                        futurePins.Add(i + 1, new List<Tuple<int, int>>());
                        tracks.Add(i + 1, new List<Tuple<int, int>>());
                    }
                    int numElement = 0; // TODO важное замечание: исходная сетка хранится по столбцам, а новая по строкам!!!!
                    for (int i = _leftBorderLimitingRectangle; i <= _rightBorderLimitingRectangle; ++i)
                    {
                        for (int j = _upBorderLimitingRectangle; j <= _downBorderLimitingRectangle; ++j)
                        {
                            gridElements[numElement] = grid[i, j];
                            numElement++;
                        }
                    }

                    Point p0 = grid.GetCoordCell(_leftBorderLimitingRectangle, _upBorderLimitingRectangle); // координата смещения

                    _newGrid = new Grid(gridElements, p0.x, p0.y,
                        (((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 2) / 2) * grid.Koeff,
                        (((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 2) / 2) * grid.Koeff,
                        grid.Koeff);

                    
                    // ищем граничные узлы
                    int oldI = _upBorderLimitingRectangle;
                    int oldJ = _leftBorderLimitingRectangle;
                    for (int newI = 0; newI < _newGrid.CountRows; newI++)
                    {
                        oldJ = 0;
                        for (int newJ = 0; newJ < _newGrid.CountColumn; newJ++)
                        {
                            if (_newGrid[newI, newJ].MetalID != 0)
                            {
                                tracks[_newGrid[newI, newJ].MetalID].Add(Tuple.Create(newI, newJ)); // ищем реализованные трассы в нашем прямоугольнике
                                if (IsBoardGridElement(ref grid, ref _newGrid, newI, newJ, oldI, oldJ))
                                {
                                    futurePins[_newGrid[newI, newJ].MetalID].Add(Tuple.Create(newI, newJ));
                                }
                                else if (_newGrid.IsPin(newI, newJ))
                                {
                                    futurePins[_newGrid[newI, newJ].MetalID].Add(Tuple.Create(newI, newJ));
                                }
                            }

                            oldJ++;
                        }

                        oldI++;
                    }
                    // добаявлем start и finish
                    int k,l;
                    grid.GetIndexes(start, out k, out l);
                    futurePins[grid.CurrentIDMetalTrack].Add(Tuple.Create(l - _upBorderLimitingRectangle, k - _leftBorderLimitingRectangle));
                    grid.GetIndexes(finish, out k, out l);
                    futurePins[grid.CurrentIDMetalTrack].Add(Tuple.Create(l - _upBorderLimitingRectangle, k - _leftBorderLimitingRectangle));

                    // добавляем узлы из трассы с междоузлием    
                    for (int i = 0; i < pathWithInternodes.Count; i++)
                    {
                        //if (!(i % 2 == 1)) // будем это учитывать при подсчтёте штрафа делается для того чтобы подсчёт штрафа для каждой трассы был "честным"
                        {
                            grid.GetIndexes(pathWithInternodes[i], out k, out l);
                            tracks[grid.CurrentIDMetalTrack].Add(Tuple.Create(l - _upBorderLimitingRectangle, k - _leftBorderLimitingRectangle));
                        }
                    }

                    // граничные узлы делаем pin'ами и запоминаем какие узлы мы сделали 
                    _pinnedNodes = new List<Tuple<int, int>>();
                    foreach (var track in futurePins)
                    {
                        for (int i = 0; i < track.Value.Count; i++)
                        {
                            if (!_newGrid.IsPin(track.Value[i].Item1, track.Value[i].Item2))
                            {
                                Point p = grid.GetCoordCell(track.Value[i].Item1, track.Value[i].Item2);

                                GridElement el = _newGrid[track.Value[i].Item1, track.Value[i].Item2];
                                el.ViewElement = new Pin(p.y + p0.x, p.x + p0.y, 1 * _newGrid.Koeff, 1 * _newGrid.Koeff);
                                _newGrid[track.Value[i].Item1, track.Value[i].Item2] = el;

                                _newGrid.SetValue(track.Value[i].Item1, track.Value[i].Item2, GridValue.PIN);

                                _pinnedNodes.Add(track.Value[i]);
                            }
                        }
                    }

                    //формируем матрицу коэфициентов-штрафов
                    int[,] penaltyMatrix = new int[_newGrid.CountRows, _newGrid.CountColumn];
                    int startKoeff = Math.Max(_newGrid.CountColumn, _newGrid.CountRows);
                    FormPenaltyMatrix(ref grid, startKoeff, ref penaltyMatrix, ref tracks);

                    // сосчитать сумарный штраф для каждой трассы реализованной и нет (как считать, если в пути указаны узлы, как реальные так и виртуальные? дополнять до полной(реальная + виртуальная) трассы уже реализованные)
                    Dictionary<int, int> penalty = new Dictionary<int, int>();
                    CalculatePenalty(grid, tracks, penaltyMatrix, ref penalty);

                    // обнуляем весь метал
                    for (int i = 0; i < _newGrid.Count; i++)
                    {
                        GridElement el = _newGrid[i];
                        el.MetalID = 0;
                        _newGrid[i] = el;
                        _newGrid.UnsetValue(i, GridValue.FOREIGN_METAL);
                        _newGrid.UnsetValue(i, GridValue.OWN_METAL);
                    }

                    // перетрассировать в порядке убывания, начиная с самой дорогой
                    Retracing(ref _newGrid, futurePins, penalty);

                    // превратить те узлы что стали пинами обртано в просто узлы

                    _newGrid.WriteToFile("matrixTest.txt");
                    Bitmap bmp = new Bitmap(_newGrid.Width, _newGrid.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    g.TranslateTransform(-p0.x, -p0.y);

                    _newGrid.Draw(ref g);

                    string pathStr = "AlgTest.bmp";
                    bmp.Save(pathStr);
                }
            }
        }

        private void CalculatePenalty(Grid grid, Dictionary<int, List<Tuple<int, int>>> tracks, int[,] penaltyMatrix, ref Dictionary<int, int> penalty)
        {
            foreach (var track in tracks)
            {
                if (track.Key != grid.CurrentIDMetalTrack)
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
                else
                {
                    int sum = 0;
                    for (int item = 0; item < track.Value.Count; ++item)
                    {
                        if (!(item % 2 == 1)) // будем это учитывать при подсчтёте штрафа делается для того чтобы подсчёт штрафа для каждой трассы был "честным"
                        {
                            int i = track.Value[item].Item1;
                            int j = track.Value[item].Item2;
                            sum += penaltyMatrix[i, j];
                        }
                    }
                    penalty.Add(track.Key, sum);
                }
            }
        }

        private bool Retracing(ref Grid grid, Dictionary<int, List<Tuple<int, int>>> futurePins, Dictionary<int, int> penalty)
        {
            // очередёность - penalty
            // сетка - на чём будем  - _newGrid
            // стартовые и финишные вершины - tracks

            while (penalty.Count != 0)
            {
                // ищем максимальный штраф
                int MetalID = 0;
                int max = 0;
                foreach (var track in penalty)
                {
                    if (max < track.Value)
                    {
                        max = track.Value;
                        MetalID = track.Key;
                    }
                }
                penalty.Remove(MetalID);

                List<int> net = new List<int>();
                foreach (var pin in futurePins[MetalID])
                {
                    net.Add(grid.GetNum(pin.Item1, pin.Item2));
                }
                Li li = new Li(grid, new Net[]{new Net(net.ToArray())});
                List<List<List<int>>> nets; // нужно сформировать словарь - <ID, перетрассированнная трасса>
                li.FindPath(out nets);
            }
            return true;
        }

        private void FormPenaltyMatrix(ref Grid grid, int startKoeff, ref int[,] fineMmatrix, ref Dictionary<int, List<Tuple<int, int>>> tracks)
        {
            _set.Clear();
            
            for (int item = 0; item < tracks[grid.CurrentIDMetalTrack].Count; item++)
            {
                fineMmatrix[tracks[grid.CurrentIDMetalTrack][item].Item1,
                            tracks[grid.CurrentIDMetalTrack][item].Item2] = startKoeff;
                _set.Add(tracks[grid.CurrentIDMetalTrack][item].Item1 * _newGrid.CountColumn + tracks[grid.CurrentIDMetalTrack][item].Item2, startKoeff);
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
                    j = (int)Math.Floor((double)(_set[index + elEdded].NumCell) % _newGrid.CountColumn);
                    i = ((_set[index + elEdded].NumCell) - j) / _newGrid.CountColumn;

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
                    if (newGrid[newI, newJ].MetalID == grid[oldJ - 2, oldI].MetalID)
                    {
                        return true;
                    }
                }
                if (newJ + 1 == newGrid.CountColumn) // right
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldJ + 2, oldI].MetalID)
                    {
                        return true;
                    }
                }
                if (newI - 1 == -1) // up
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldJ, oldI - 2].MetalID)
                    {
                        return true;
                    }
                }
                if (newI + 1 == newGrid.CountRows) // down
                {
                    if (newGrid[newI, newJ].MetalID == grid[oldJ, oldI + 2].MetalID)
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
            // 1. нужно найти корректные() minItem и maxItem
            int minItem = path.Min();
            int maxItem = path.Max();

            grid.GetIndexes(minItem, out _leftBorderLimitingRectangle, out _upBorderLimitingRectangle);
            grid.GetIndexes(maxItem, out _rightBorderLimitingRectangle, out _downBorderLimitingRectangle);

            if (_leftBorderLimitingRectangle % 2 == 1) _leftBorderLimitingRectangle++;
            if (_upBorderLimitingRectangle % 2 == 1) _upBorderLimitingRectangle++;
            if (_rightBorderLimitingRectangle % 2 == 1) _rightBorderLimitingRectangle++;
            if (_downBorderLimitingRectangle % 2 == 1) _downBorderLimitingRectangle++;


            _leftBorderLimitingRectangle -= (2 * additLeftParam);
            if (_leftBorderLimitingRectangle < 0) _leftBorderLimitingRectangle = 0;

            _upBorderLimitingRectangle -= (2 * additUpParam);
            if (_upBorderLimitingRectangle < 0) _upBorderLimitingRectangle = 0;

            _rightBorderLimitingRectangle += (2 * additRightParam);
            if (_rightBorderLimitingRectangle > grid.CountColumn) _rightBorderLimitingRectangle = (grid.CountColumn % 2 == 0) ? grid.CountColumn : grid.CountColumn - 1;

            _downBorderLimitingRectangle += (2 * additDownParam);
            if (_downBorderLimitingRectangle > grid.CountRows) _downBorderLimitingRectangle = (grid.CountRows % 2 == 0) ? grid.CountRows : grid.CountRows - 1;
        }

        private bool WavePropagation(ref Grid grid, int start, int finish) // TODO
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
                        if (i - 2 >= 0 && CheckNormalCell(ref grid, i - 2, j, numLevel, finish, ref countAdded)) // left
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (i + 2 < grid.CountRows && CheckNormalCell(ref grid, i + 2, j, numLevel, finish, ref countAdded)) // right
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j - 2 >= 0 && CheckNormalCell(ref grid, i, j - 2, numLevel, finish, ref countAdded)) // up
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j + 2 < grid.CountColumn && CheckNormalCell(ref grid, i, j + 2, numLevel, finish, ref countAdded)) // down
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
        private bool CheckNormalCell(ref Grid grid, int i, int j, int numLevel, int finish, ref int countAdded)
        {
            if (!(grid.IsProhibitionZone(i, j) || grid.IsForeignMetal(i, j)))
            {
                if (grid.IsPin(i, j)) // если это пин
                {
                    if (grid.GetNum(i, j) == finish && _set.Add(grid.GetNum(i, j), numLevel)) // финишный Pin
                    {
                        countAdded++;
                        return true;
                    }
                    else if (grid.IsOwnMetal(i, j) && _set.Add(grid.GetNum(i, j), numLevel)) // если и Pin и свой метал
                    {
                        countAdded++;
                        return true;
                    }
                }
                else if (grid.IsOwnMetal(i, j) && _set.Add(grid.GetNum(i, j), numLevel)) // если свой метал
                {
                    countAdded++;
                    return true;
                }
                else // если не Pin
                {
                    if (_set.Add(grid.GetNum(i, j), numLevel))
                    {
                        countAdded++;
                        return false;
                    }
                }
            }
            return false;
        }

        private bool VirtualWavePropagation(ref Grid grid, int start, int finish)
        { 
             int numLevel = 0;
             _set.Clear();
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
                        if (i - 1 >= 0 && CheckAllCell(ref grid, i - 1, j, numLevel, finish, ref countAdded)) // left
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (i + 1 < grid.CountRows && CheckAllCell(ref grid, i + 1, j, numLevel, finish, ref countAdded)) // right
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j - 1 >= 0 && CheckAllCell(ref grid, i, j - 1, numLevel, finish, ref countAdded)) // up
                        {
                            isFoundFinish = true;
                            break;
                        }
                        if (j + 1 < grid.CountColumn && CheckAllCell(ref grid, i, j + 1, numLevel, finish, ref countAdded)) // down
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
                if (countAdded == 0) // условие, что нельзя реализовать
                {
                    return false;
                }
            }

            return isFoundFinish;
        }
        private bool CheckAllCell(ref Grid grid, int i, int j, int numLevel, int finish, ref int countAdded)
        {
            if (!(grid.IsProhibitionZone(i, j) || grid.IsForeignMetal(i, j)))
            {
                if (grid.IsPin(i, j)) // если это пин
                {
                    if (grid.GetNum(i, j) == finish && _set.Add(grid.GetNum(i, j), numLevel)) // финишный Pin
                    {
                        countAdded++;
                        return true;
                    }
                }
                else // если не Pin
                {
                    if (_set.Add(grid.GetNum(i, j), numLevel))
                    {
                        countAdded++;
                    }

                    if (j - 1 >= 0 && j + 1 < grid.CountColumn && i - 1 >= 0 && i + 1 < grid.CountRows)
                    {
                        // условие что найдено междоузлие
                        if (((grid[i, j - 1].MetalID != grid[i, j + 1].MetalID) && grid[i, j - 1].MetalID != 0 && grid[i, j + 1].MetalID != 0) || 
                            (grid[i - 1, j].MetalID != grid[i + 1, j].MetalID && grid[i - 1, j].MetalID != 0 && grid[i + 1, j].MetalID != 0))
                        {

                                _virtualSet.Add(grid.GetNum(i, j), numLevel);
                        }
                    }
                }
            }
            return false;
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
    }
}
