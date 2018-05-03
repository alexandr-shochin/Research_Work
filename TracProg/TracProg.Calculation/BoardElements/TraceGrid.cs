using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TracProg.Calculation.BoardElements
{
    public class TraceGrid
    {
        public struct TraceGridElement
        {
            public IBoardElement ViewElement { get; set; }
            public IBoardElement IsReTracedArea { get; set; }
            public string MetalId { get; set; }

            public override string ToString()
            {
                return !string.IsNullOrEmpty(MetalId) ? MetalId : string.Empty;
            }
        }

        private readonly TraceGridElement[] _grid;

        private readonly float _brushWidth = 3.0f;
        private readonly Color _brushColor = Color.FromArgb(183, 65, 14);

        public TraceGrid(string id, int width, int height, int koeff)
        {
            Id = id;

            X = 0;
            Y = 0;

            Width = width + 1;
            Height = height + 1;

            Koeff = koeff;

            CountColumn = (Width / Koeff) * 2 - 1;
            CountRows = (Height / Koeff) * 2 - 1;
            _grid = new TraceGridElement[CountColumn * CountRows];

        }

        public TraceGrid(string id, TraceGridElement[] grid, int x0, int y0, int width, int height, int koeff)
        {
            Id = id;
            _grid = new TraceGridElement[grid.Length];
            Array.Copy(grid, 0,_grid, 0, grid.Length);

            X = x0;
            Y = y0;

            Width = width + 1;
            Height = height + 1;

            Koeff = koeff;

            CountColumn = (Width / Koeff) * 2 - 1;
            CountRows = (Height / Koeff) * 2 - 1;
        }

        #region Public methods

        /// <summary>
        /// Добавить элемент в сетку
        /// </summary>
        /// <param name="el">Добавляемый элемент</param>
        /// <returns>Код ошибки</returns>
        public void Add(IBoardElement el)
        {
            int i, j;
            if(GetIndexesRowCol(el.X, el.Y, out i, out j))
            {
                try
                {
                    _grid[GetNum(i, j)].ViewElement = el;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, содержит ли сетка элемент переданный в качестве параметра.
        /// </summary>
        /// <param name="el">Элемент для поиска</param>
        /// <returns></returns>
        public bool Contains(IBoardElement el)
        {
            int i, j;
            if (GetIndexesRowCol(el.X, el.Y, out i, out j))
            {
                try
                {
                    if (this[i, j].ViewElement == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void Draw(Graphics graphics)
        {
            for (int el = 0; el < _grid.Length; el++)
            {
                if (_grid[el].IsReTracedArea != null)
                {
                    if (_grid[el].IsReTracedArea is ReTracedArea)
                    {
                        _grid[el].IsReTracedArea.Draw(graphics);
                    }
                }
            }

            for (int el = 0; el < _grid.Length; el++)
            {
                if (_grid[el].ViewElement != null && !string.IsNullOrEmpty(_grid[el].MetalId))
                {
                    Point centerPoint = GetCoordCell(el);

                    int i, j;
                    GetIndexesRowCol(centerPoint.X, centerPoint.Y, out i, out j);

                    TraceGridElement centerElement = this[i, j];

                    if (j - 2 >= 0)
                    {
                        TraceGridElement upElement = this[i, j - 2];
                        if (upElement.ViewElement != null && centerElement.MetalId == upElement.MetalId)
                        {
                            Point upPoint = GetCoordCell(i, j - 2);
                            DrawLine(ref graphics, centerPoint, upPoint);
                        }
                    }

                    if (j + 2 < CountColumn)
                    {
                        TraceGridElement downElement = this[i, j + 2];
                        if (downElement.ViewElement != null && centerElement.MetalId == downElement.MetalId)
                        {
                            Point downPoint = GetCoordCell(i, j + 2);
                            DrawLine(ref graphics, centerPoint, downPoint);
                        }
                    }

                    if (i - 2 >= 0)
                    {
                        TraceGridElement leftElement = this[i - 2, j];
                        if (leftElement.ViewElement != null && centerElement.MetalId == leftElement.MetalId)
                        {
                            Point leftPoint = GetCoordCell(i - 2, j);
                            DrawLine(ref graphics, centerPoint, leftPoint);
                        }
                    }

                    if (i + 2 < CountRows)
                    {
                        TraceGridElement rightElement = this[i + 2, j];
                        if (rightElement.ViewElement != null && centerElement.MetalId == rightElement.MetalId)
                        {
                            Point rightPoint = GetCoordCell(i + 2, j);
                            DrawLine(ref graphics, centerPoint, rightPoint);
                        }
                    }

                    
                }
            }

            for (int el = 0; el < _grid.Length; el++)
            {
                if (_grid[el].ViewElement != null)
                {
                    if (_grid[el].ViewElement is Pin || _grid[el].ViewElement is ProhibitionZone)
                    {
                        _grid[el].ViewElement.Draw(graphics);
                    }
                }
            }
        }
        private void DrawLine(ref Graphics graphics, Point pointFrom, Point pointTo)
        {
            graphics.DrawLine(new Pen(new SolidBrush(_brushColor), _brushWidth),
                new Point((pointFrom.X + pointFrom.X + Koeff) / 2, (pointFrom.Y + pointFrom.Y + Koeff) / 2),
                new Point((pointTo.X + pointTo.X + Koeff) / 2, (pointTo.Y + pointTo.Y + Koeff) / 2));
        }

        public void MetallizeTrack(List<List<int>> tracks, string metalId)
        {
            foreach (List<int> track in tracks)
            {
                InternalMetallizeTrack(track, metalId);
            }       
        }

        private void InternalMetallizeTrack(List<int> track, string metalId)
        { 
            foreach (int node in track)
            {
                if (!IsPin(node))
                {
                    Point p = GetCoordCell(node);
                    _grid[node].ViewElement = new Metal(metalId, p.X, p.Y, 1, 1);
                    _grid[node].MetalId = metalId;
                }
                else
                {
                    _grid[node].MetalId = metalId;
                }
            }
        }

        /// <summary>
        /// Получить значение в сетке
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public TraceGridElement this[int i, int j]
        {
            get
            {
                if (i < 0 || i >= CountRows)
                {
                    throw new OverflowException("Индекс i находился вне границ сетки.");
                }
                if (j < 0 || j >= CountColumn)
                {
                    throw new OverflowException("Индекс j находился вне границ сетки.");
                }
                return _grid[i * CountColumn + j];
            }
            set
            {
                if (i < 0 || i >= CountRows)
                {
                    throw new OverflowException("Индекс i находился вне границ сетки.");
                }
                if (j < 0 || j >= CountColumn)
                {
                    throw new OverflowException("Индекс j находился вне границ сетки.");
                }
                _grid[i * CountColumn + j] = value;
            }
        }

        /// <summary>
        /// Получить значение в сетке
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public TraceGridElement this[int num]
        {
            get
            {
                if (num < 0 || num >= _grid.Length)
                {
                    throw new OverflowException("Номер ячейки находился вне границ.");
                }
                return _grid[num];
            }
            set
            {
                if (num < 0 || num >= _grid.Length)
                {
                    throw new OverflowException("Номер ячейки находился вне границ.");
                }
                _grid[num] = value;
            }
        }

        /// <summary>
        /// Получить номера строки и столбца элемента в сетке по номеру ячейки
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public void GetIndexes(int num, out int i, out int j)
        {
            i = -1;
            j = -1;
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            j = (int)Math.Floor((double)num % CountColumn);
            i = (num - j) / CountColumn;
        }

        /// <summary>
        /// Получить номера строки и столбца элемента в сетке по координатам ячейки
        /// </summary>
        /// <param name="x">Координата ячейки по оси x</param>
        /// <param name="y">Координата ячейки по оси y</param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public void GetIndexes(int x, int y, out int i, out int j)
        {
            i = -1;
            j = -1;
            if (x < 0 || x >= Right)
            {
                throw new OverflowException("Координата x находилась вне границ сетки.");
            }
            if (y < 0 || j >= Bottom)
            {
                throw new OverflowException("Координата y находилась вне границ сетки.");
            }

            j = ((x - X) / Koeff) * 2;
            i = ((y - Y) / Koeff) * 2;
        }

        /// <summary>
        /// Получить номер ячейки в сетке по номеру строки и столбца
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public int GetNum(int i, int j)
        {
            if (i < 0 || i >= CountRows)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= CountColumn)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            return i * CountColumn + j;
        }

        /// <summary>
        /// Проверка, что данная ячейка является металом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <param name="metalId"></param>
        /// <returns></returns>
        public bool IsOwnMetal(int num, string metalId)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            return !string.IsNullOrEmpty(_grid[num].MetalId) && _grid[num].MetalId == metalId;
        }

        public bool IsOwnMetal(int i, int j, string metalId)
        {
            return IsOwnMetal(GetNum(i, j), metalId);
        }

        /// <summary>
        /// Проверка, что данная ячейка является Pin'ом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsPin(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            return _grid[num].ViewElement is Pin;
        }

        /// <summary>
        /// Проверка, что данная ячейка является Pin'ом
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public bool IsPin(int i, int j)
        {
            return IsPin(GetNum(i, j));
        }

        /// <summary>
        /// Проверка, что данная ячейка является зоной запрета
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsProhibitionZone(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            return _grid[num].ViewElement is ProhibitionZone;
        }

        public bool IsProhibitionZone(int i, int j)
        {
            if (i < 0 || i >= CountRows)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= CountColumn)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            return IsProhibitionZone(GetNum(i, j));
        }

        public bool IsFreeMetal(int i, int j)
        {
            if (i < 0 || i >= CountRows)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= CountColumn)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            bool result = true;
            if (j > 0 && j < CountColumn - 1)
            {
                if (!string.IsNullOrEmpty(this[i, j - 1].MetalId) && !string.IsNullOrEmpty(this[i, j + 1].MetalId))
                {
                    if (this[i, j - 1].MetalId == this[i, j + 1].MetalId)
                    {
                        result = false;
                    }
                }
            }
            if (i > 0 && i < CountRows - 1)
            {
                if (!string.IsNullOrEmpty(this[i - 1, j].MetalId) && !string.IsNullOrEmpty(this[i + 1, j].MetalId))
                {
                    if (this[i - 1, j].MetalId == this[i + 1, j].MetalId)
                    {
                        result = false;
                    }
                }
            }

            return !IsProhibitionZone(i, j) && !IsPin(i, j) && string.IsNullOrEmpty(this[i, j].MetalId) && result;
        }

        /// <summary>
        /// Получить координаты ячейки по номеру строки и столбца
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns>Возвращает координаты ячейки</returns>
        public Point GetCoordCell(int i, int j)
        {
            if (i < 0 || i >= CountRows)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= CountColumn)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            return new Point(X + (Koeff * (j / 2)), Y + (Koeff * (i / 2)));
        }

        public Point GetCoordCell(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            int i, j;
            GetIndexes(num, out i, out j);
            return GetCoordCell(i, j);
        }


        public override string ToString()
        {
            return "Count = " + Count;
        }

        public void WriteToFile(string path)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < CountRows; i++)
            {
                string str = "";
                for (int j = 0; j < CountColumn; j++)
                {
                    string tmp = "n0";
                    if (!string.IsNullOrEmpty(this[i, j].MetalId))
                    {
                        tmp = this[i, j].MetalId;
                    }

                    str += tmp;
                }
                lines.Add(str);
            }
            File.WriteAllLines(path, lines);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Получить значение строки и столбца по координатам ячейки
        /// </summary>
        /// <param name="x">Координата ячейки по оси X</param>
        /// <param name="y">Координата ячейки по оси Y</param>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns>Значение строки и столбца</returns>
        private bool GetIndexesRowCol(int x, int y, out int i, out int j)
        {
            if (x < 0 || x >= Right)
            {
                throw new OverflowException("Координата x находилась вне границ.");
            }
            if (y < 0 || y >= Bottom)
            {
                throw new OverflowException("Координата y находилась вне границ.");
            }

            i = -1;
            j = -1;

            try
            {
                i = 0;
                int tmpY = Y;
                while (tmpY != y)
                {
                    tmpY += Koeff;
                    i += 2;
                }

                j = 0;
                int tmpX = X;
                while (tmpX != x)
                {
                    tmpX += Koeff;
                    j+=2;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Properties

        public string Id { get; }

        /// <summary>
        /// Возвращает координату по оси Y прямоугольной области, являющуюся суммой значений свойств Y и Height.
        /// </summary>
        public int Bottom { get { return Y + Height; } }

        /// <summary>
        /// Возвращает высоту прямоугольной области
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Данное свойство возвращает значение true, если значения всех свойств Width, Height, X и Y равны нулю. В противном случае возвращается значение false.
        /// </summary>
        public bool IsEmpty { get { return Width == 0 && Height == 0; } }

        /// <summary>
        /// Возвращает координату по оси X левого края прямоугольной области
        /// </summary>
        public int Left { get { return X; } }

        /// <summary>
        /// Возвращает координату по оси X прямоугольной области, являющуюся суммой значений свойств X и Width.
        /// </summary>
        public int Right { get { return X + Width; } }

        /// <summary>
        /// Возвращает координату по оси Y верхнего края прямоугольной области
        /// </summary>
        public int Top { get { return Y; } }

        /// <summary>
        /// Ширина прямоугольной области
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Возвращает координату по оси X левого верхнего угла прямоугольной области
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Возвращает координату по оси Y левого верхнего угла прямоугольной области
        /// </summary>
        public int Y { get;  }

        public int Koeff { get;  }

        /// <summary>
        /// Количество ячеек в сетке
        /// </summary>
        public int Count
        {
            get 
            {
                if (_grid.Length == 0)
                {
                    return 0;
                }
                else
                {
                    return _grid.Length;
                }
            }
        }

        /// <summary>
        /// Количество столбцов в сетке
        /// </summary>
        public int CountColumn { get; }

        /// <summary>
        /// Количество строк в сетке
        /// </summary>
        public int CountRows { get; }

        #endregion
    }
}
