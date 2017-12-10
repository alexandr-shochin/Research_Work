using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
                    List<int> path = new List<int>();
                    RestorationPath(ref grid, ref path);

                    // получаем координаты ограничивающего прямоугольника
                    GetCoordLimitingRectangle(ref grid, ref path, 1, 1, 1, 1);

                    // копируем нужные элементы сетки и создаём новую
                    GridElement[] gridElements = new GridElement[((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 1) * ((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 1)];
                    Dictionary<int, List<int>> tracks = new Dictionary<int, List<int>>();
                    int numElement = 0;
                    for (int i = _leftBorderLimitingRectangle; i <= _rightBorderLimitingRectangle; ++i)
                    {
                        for (int j = _upBorderLimitingRectangle; j <= _downBorderLimitingRectangle; ++j)
                        {
                            gridElements[numElement] = grid[i, j];
                            numElement++;
                        }
                    }

                    Point p0 = grid.GetCoordCell(_leftBorderLimitingRectangle, _upBorderLimitingRectangle);

                    _newGrid = new Grid(gridElements, p0.x, p0.y,
                        (((_rightBorderLimitingRectangle - _leftBorderLimitingRectangle) + 2) / 2) * grid.Koeff,
                        (((_downBorderLimitingRectangle - _upBorderLimitingRectangle) + 2) / 2) * grid.Koeff,
                        grid.Koeff);

                    _newGrid.WriteToFile("matrixTest.txt");
                    Bitmap bmp = new Bitmap(_newGrid.Width, _newGrid.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    g.TranslateTransform(-p0.x, -p0.y);

                    _newGrid.Draw(ref g);

                    string pathStr = "test.bmp";
                    bmp.Save(pathStr);


                    for (int i = 0; i < _newGrid.CountRows; i++)
                    {
                        for (int j = 0; j < _newGrid.CountColumn; j++)
                        {
                            if (_newGrid[i, j].MetalID != 0) // ячейка является частью какой-то трассы
                            {
                                if (_newGrid.IsPin(i, j))
                                {
                                    if (!tracks.ContainsKey(_newGrid[i, j].MetalID))
                                    {
                                        tracks.Add(_newGrid[i, j].MetalID, new List<int>());
                                        tracks[_newGrid[i, j].MetalID].Add(_newGrid.GetNum(i, j));
                                    }
                                    else
                                    {
                                        tracks[_newGrid[i, j].MetalID].Add(_newGrid.GetNum(i, j));
                                    }
                                }
                                if (IsBoardGridElement(ref _newGrid, i, j))
                                {
                                    if (!tracks.ContainsKey(_newGrid[i, j].MetalID))
                                    {
                                        tracks.Add(_newGrid[i, j].MetalID, new List<int>());

                                        tracks[_newGrid[i, j].MetalID].Add(_newGrid.GetNum(i, j));
                                    }
                                    else
                                    {
                                        tracks[_newGrid[i, j].MetalID].Add(_newGrid.GetNum(i, j));
                                    }
                                }
                            }
                        }
                    }

                    
                }
            }
        }

        private bool IsBoardGridElement(ref Grid grid, int i, int j)
        {
            if (i - 1 == -1 || 
                i + 1 == grid.CountRows ||
                j - 1 == 0 ||
                j + 1 == grid.CountColumn)
            {
                return true;
            }
            return false;
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
