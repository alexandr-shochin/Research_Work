using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    public struct GridElement
    {
        public IElement ViewElement { get; set; }
        public string MetalID { get; set; }
        public float WidthMetal { get; set; }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(MetalID) ? MetalID : string.Empty;
        }
    }

    [Serializable]
    public class Grid //: IElement
    {
        private Point[] _nodes;
        //private List<IElement> _elements;
        private GridElement[] _grid;

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="width">Ширина прямоугольной области</param>
        /// <param name="height">Высота прямоугольной области</param>
        /// <param name="koeff">Шаг для генерации точек узлов сетки</param>
        public Grid(int width, int height, int koeff)
        {
            Init(0, 0, width, height, koeff);
        }

        public Grid(GridElement[] grid, int x0, int y0, int width, int height, int koeff)
        {
            _grid = new GridElement[grid.Length];
            Array.Copy(grid, 0,_grid, 0, grid.Length);

            X = x0;
            Y = y0;
            Width = width + 1;
            Height = height + 1;
            Koeff = koeff;

            CountColumn = (width / Koeff) * 2 - 1;
            CountRows = (height / Koeff) * 2 - 1;

            _nodes = new Point[((width / Koeff) + 2) * ((height / Koeff) + 2)];

            int[] xs = new int[((width / Koeff) + 2)];
            int[] ys = new int[((height / Koeff) + 2)];

            int tmpX = X;
            for (int i = 0; i < xs.Length; ++i)
            {
                xs[i] = tmpX;
                tmpX += koeff;
            }

            int tmpY = Y;
            for (int i = 0; i < ys.Length; ++i)
            {
                ys[i] = tmpY;
                tmpY += koeff;
            }

            int index = 0;
            for (int i = 0; i < xs.Length; ++i)
            {
                for (int j = 0; j < ys.Length; ++j)
                {
                    _nodes[index] = new Point(xs[i], ys[j]);
                    index++;
                }
            }
            _Color = Color.Black;
        }

        #region Public methods

        private void Init(int x, int y, int width, int height, int koeff)
        {
            GenerateCoord(width, height, koeff);

            CountColumn = (width / Koeff) * 2 - 1;
            CountRows = (height / Koeff) * 2 - 1;
            _grid = new GridElement[CountColumn * CountRows];

            _Color = Color.Black;
        }

        /// <summary>
        /// Добавить элемент в сетку
        /// </summary>
        /// <param name="el">Добавляемый элемент</param>
        /// <returns>Код ошибки</returns>
        public ErrorCode Add(IElement el)
        {
            Tuple<int, int> indexes;
            if(GetIndexesRowCol(el.X, el.Y, out indexes))
            {
                try
                {
                    _grid[GetNum(indexes.Item1, indexes.Item2)].ViewElement = el;

                    return ErrorCode.NO_ERROR;
                }
                catch (Exception ex)
                {
                    return ErrorCode.ADD_ERROR;
                }
            }
            else
            {
                return ErrorCode.ADD_ERROR;
            }
        }

        public void Draw(Dictionary<string, Net> nets, Graphics graphics)
        {
            Random rand = new Random();
            Dictionary<string, Color> colors = new Dictionary<string, Color>();
            foreach (var net in nets)
            {
                colors[net.Key] = Color.FromArgb(rand.Next(1, 255), rand.Next(1, 255), rand.Next(1, 255));
            }

            for (int i = 0; i < _nodes.Length; ++i)
            {
                graphics.FillRectangle(new SolidBrush(_Color), _nodes[i].x, _nodes[i].y, 1, 1);
            }

            for (int el = 0; el < _grid.Length; ++el)
            {
                if (!string.IsNullOrEmpty(_grid[el].MetalID) && !(_grid[el].ViewElement is Pin))
                {
                    Point p = GetCoordCell(el);
                    _grid[el].ViewElement = new Metal(p.x, p.y, Koeff, Koeff, colors[_grid[el].MetalID]);
                }
                if (_grid[el].ViewElement != null)
                {
                    _grid[el].ViewElement.Draw(graphics);
                }
            }
        }

        public void MetallizeTrack(List<List<int>> tracks, float widthMetal, string metalID)
        {
            foreach (List<int> track in tracks)
            {
                foreach (int node in track)
                {
                    if (!IsPin(node))
                    {
                        Point p = GetCoordCell(node);
                        _grid[node].ViewElement = new Metal(p.x, p.y, Koeff, Koeff, Color.White);
                        _grid[node].MetalID = metalID;
                        _grid[node].WidthMetal = widthMetal;
                    }
                    else
                    {
                        _grid[node].MetalID = metalID;
                        _grid[node].WidthMetal = widthMetal;
                    }
                }
            }
        }

        /// <summary>
        /// Получить значение в сетке
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public GridElement this[int i, int j]
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
        public GridElement this[int num]
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
        /// <returns></returns>
        public bool IsOwnMetal(int num, string metalID)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            return !string.IsNullOrEmpty(_grid[num].MetalID) && _grid[num].MetalID == metalID;
        }

        public bool IsOwnMetal(int i, int j, string metalID)
        {
            return IsOwnMetal(GetNum(i, j), metalID);
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

        public bool IsFreeMetal(int i, int j, string metalID)
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
                if (!string.IsNullOrEmpty(this[i, j - 1].MetalID) && !string.IsNullOrEmpty(this[i, j + 1].MetalID))
                {
                    if (this[i, j - 1].MetalID == this[i, j + 1].MetalID)
                    {
                        result = false;
                    }
                }
            }
            if (i > 0 && i < CountRows - 1)
            {
                if (!string.IsNullOrEmpty(this[i - 1, j].MetalID) && !string.IsNullOrEmpty(this[i + 1, j].MetalID))
                {
                    if (this[i - 1, j].MetalID == this[i + 1, j].MetalID)
                    {
                        result = false;
                    }
                }
            }

            return !IsProhibitionZone(i, j) && !IsPin(i, j) && string.IsNullOrEmpty(this[i, j].MetalID) && result;
        }

        public bool IsForeignMetal(int i, int j, string metalID)
        {
            if (i < 0 || i >= CountRows)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= CountColumn)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            if (!string.IsNullOrEmpty(this[i, j].MetalID) && this[i, j].MetalID != metalID)
            {
                return true;
            }
            else
            {
                if (j > 0 && j < CountColumn - 1)
                {
                    if (this[i, j - 1].MetalID == this[i, j + 1].MetalID && !string.IsNullOrEmpty(this[i, j - 1].MetalID) && !string.IsNullOrEmpty(this[i, j + 1].MetalID))
                    {
                        return true;
                    }
                }
                if (i > 0 && i < CountRows - 1)
                {
                    if (this[i - 1, j].MetalID == this[i + 1, j].MetalID && !string.IsNullOrEmpty(this[i - 1, j].MetalID) && !string.IsNullOrEmpty(this[i + 1, j].MetalID))
                    {
                        return true;
                    }
                }

                return false;
            }
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
                    string _str = "n0";
                    if (!string.IsNullOrEmpty(this[i, j].MetalID))
                    {
                        _str = this[i, j].MetalID;
                    }

                    str += _str;
                }
                lines.Add(str);
            }
            File.WriteAllLines(path, lines);
        }

        #endregion

        #region Private methods

        private void GenerateCoord(int width, int height, int koeff)
        {
            X = 0;
            Y = 0;
            Width = width + 1;
            Height = height + 1;
            Koeff = koeff;

            _nodes = new Point[((width / Koeff) + 2) * ((height / Koeff) + 2)];

            int[] xs = new int[((width / Koeff) + 2)];
            int[] ys = new int[((height / Koeff) + 2)];

            int tmpX = 0;
            for (int i = 0; i < xs.Length; ++i)
            {
                xs[i] = tmpX;
                tmpX += koeff;
            }

            int tmpY = 0;
            for (int i = 0; i < ys.Length; ++i)
            {
                ys[i] = tmpY;
                tmpY += koeff;
            }

            int index = 0;
            for (int i = 0; i < xs.Length; ++i)
            {
                for (int j = 0; j < ys.Length; ++j)
                {
                    _nodes[index] = new Point(xs[i], ys[j]);
                    index++;
                }
            }
        }

        /// <summary>
        /// Получить значение строки и столбца по координатам ячейки
        /// </summary>
        /// <param name="x">Координата ячейки по оси X</param>
        /// <param name="y">Координата ячейки по оси Y</param>
        /// <returns>Значение строки и столбца</returns>
        private bool GetIndexesRowCol(int x, int y, out Tuple<int, int> indexes) // TODO обработать ошибки
        {
            indexes = null;

            try
            {
                int i = 0;
                int tmpX = X;
                while (tmpX != x)
                {
                    tmpX += Koeff;
                    i+=2;
                }

                int j = 0;
                int tmpY = Y;
                while (tmpY != y)
                {
                    tmpY += Koeff;
                    j+=2;
                }

                indexes = Tuple.Create(j, i);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает координату по оси Y прямоугольной области, являющуюся суммой значений свойств Y и Height.
        /// </summary>
        public int Bottom { get { return Y + Height; } }

        /// <summary>
        /// Возвращает высоту прямоугольной области
        /// </summary>
        public int Height { get; private set; }

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
        public int Width { get; private set; }

        /// <summary>
        /// Возвращает координату по оси X левого верхнего угла прямоугольной области
        /// </summary>
        public int X { get; private set; }
        /// <summary>
        /// Возвращает координату по оси Y левого верхнего угла прямоугольной области
        /// </summary>
        public int Y { get; private set; }

        public int Koeff { get; private set; }

        /// <summary>
        /// Возвращает или задаёт цвет для узлов сетки
        /// </summary>
        public Color _Color { get; set; }

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
        public int CountColumn { get; private set; }

        /// <summary>
        /// Количество строк в сетке
        /// </summary>
        public int CountRows { get; private set; }

        #endregion
    }
}
