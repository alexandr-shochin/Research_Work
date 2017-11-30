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
        public int MetalID { get; set; }
        public float WidthMetal { get; set; }
        public byte ByteInfo { get; set; }

        public override string ToString()
        {
            return MetalID.ToString();
        }
    }

    [Serializable]
    public class Grid : IElement
    {
        private int _currentIDMetalTrack;

        private Point[] _nodes;
        //private List<IElement> _elements;
        private GridElement[] _grid;

        private List<IElement> _metalizedTracks;

        public event Action Metalize;

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

        #region Public methods

        private void Init(int x, int y, int width, int height, int koeff)
        {
            _metalizedTracks = new List<IElement>();
            _currentIDMetalTrack = 1;
            GenerateCoord(width, height, koeff);
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
                    //_elements.Add(el);
                    _grid[GetNum(indexes.Item1, indexes.Item2)].ViewElement = el;
                    if (el is Pin)
                    {
                        SetValue(indexes.Item1, indexes.Item2, GridValue.PIN);
                    }
                    else if (el is ProhibitionZone)
                    {
                        SetValue(indexes.Item1, indexes.Item2, GridValue.PROHIBITION_ZONE);
                    }
                    else if (el is Metal)
                    {
                        SetValue(indexes.Item1, indexes.Item2, GridValue.FOREIGN_METAL);
                    }
                    return ErrorCode.NO_ERROR;
                }
                catch (Exception)
                {
                    return ErrorCode.ADD_ERROR;
                }
            }
            else
            {
                return ErrorCode.ADD_ERROR;
            }
        }

        /// <summary>
        /// Удаляет элемент из сетки
        /// </summary>
        /// <param name="el">Удаляемый элемент</param>
        /// <returns>Код ошибки</returns>
        public ErrorCode Remove(IElement el) // TODO
        {
            Tuple<int, int> indexes;
            if (GetIndexesRowCol(el.X, el.Y, out indexes))
            {
                try
                {
                    if (el is Pin)
                    {
                        UnsetValue(indexes.Item1, indexes.Item2, GridValue.PIN);
                    }
                    else if (el is ProhibitionZone)
                    {
                        UnsetValue(indexes.Item1, indexes.Item2, GridValue.PROHIBITION_ZONE);
                    }
                    else if (el is Metal)
                    {
                        SetValue(indexes.Item1, indexes.Item2, GridValue.FOREIGN_METAL);
                    }
                    return ErrorCode.NO_ERROR;
                }
                catch (Exception)
                {
                    return ErrorCode.REMOVE_ERROR;
                }
            }
            else
            {
                return ErrorCode.REMOVE_ERROR;
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, содержит ли сетка элемент переданный в качестве параметра.
        /// </summary>
        /// <param name="el">Элемент для поиска</param>
        /// <returns></returns>
        public ErrorCode Contains(IElement el) // TODO
        {
            Tuple<int, int> indexes;
            if (GetIndexesRowCol(el.X, el.Y, out indexes))
            {
                try
                {
                    if (el is Pin)
                    {
                        if (IsPin(indexes.Item1, indexes.Item2))
                        {
                            return ErrorCode.PIN_WAS_FOUND;
                        }
                    }
                    else if (el is ProhibitionZone)
                    {
                        if (IsPin(indexes.Item1, indexes.Item2))
                        {
                            return ErrorCode.PROHIBITION_ZONE_WAS_FOUND;
                        }
                    }
                    return ErrorCode.NO_ERROR;
                }
                catch (Exception)
                {
                    return ErrorCode.CONTAINS_ERROR;
                }
            }
            else
            {
                return ErrorCode.CONTAINS_ERROR;
            }
        }

        public void Draw(ref Graphics graphics)
        {
            for (int i = 0; i < _nodes.Length; ++i)
            {
                graphics.FillRectangle(new SolidBrush(_Color), _nodes[i].x, _nodes[i].y, 1, 1);
            }

            for (int i = 0; i < _metalizedTracks.Count; i++)
            {
                _metalizedTracks[i].Draw(ref graphics);
            }

            for (int i = _grid.Length - 1; i >= 0; --i)
            {
                if (_grid[i].ViewElement != null)
                {
                    _grid[i].ViewElement.Draw(ref graphics);
                }
            }
        }

        public void MetallizeTrack(List<List<int>> track, float widthMetal)
        {
            if (track != null && track.Count != 0 && track[0][0] != -1) // -1 значит трасса не была реализована, начиная со второго индекса передана нереализуемая цепь
            {
                for (int numSubPath = 0; numSubPath < track.Count; ++numSubPath)
                {
                    UnsetValue(track[numSubPath][0], GridValue.OWN_METAL);
                    _grid[track[numSubPath][0]].MetalID = _currentIDMetalTrack;
                    _grid[track[numSubPath][0]].WidthMetal = widthMetal;

                    for (int node = 1; node < track[numSubPath].Count; ++node)
                    {
                        // 1. Добавялем элемент метал откуда куда (track[i - 1] - track[i])                     
                        Point pFrom = GetCoordCell(track[numSubPath][node - 1]);
                        Point pIn = GetCoordCell(track[numSubPath][node]);
                        _metalizedTracks.Add(new Metal(new Rectangle(pFrom.x, pFrom.y, 1 * Koeff, 1 * Koeff), new Rectangle(pIn.x, pIn.y, 1 * Koeff, 1 * Koeff), widthMetal));

                        // 2. У ячеек которые металлизируем, значение свой метал поменять на чужой
                        SetValue(track[numSubPath][node], GridValue.FOREIGN_METAL);
                        _grid[track[numSubPath][node]].MetalID = _currentIDMetalTrack;
                        _grid[track[numSubPath][node]].WidthMetal = widthMetal;
                    }
                }
                if (Metalize != null)
                {
                    Metalize.Invoke();
                }
            }
            else // трасса не построена
            {
                for (int node = 1; node < track[0].Count; ++node)
                {
                    Point p = GetCoordCell(track[0][node]);
                    try
                    {
                        _grid[track[0][node]].ViewElement._Color = Color.Red;
                    }
                    catch (NullReferenceException) { }   
                }

                if (Metalize != null)
                {
                    Metalize.Invoke();
                }
            }
            _currentIDMetalTrack++;
        }

        public int Compare(IElement x, IElement y)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IElement other)
        {
            throw new NotImplementedException();
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
                return _grid[i + j * CountRows];
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

            i = (int)Math.Floor((double)num % CountRows);
            j = (num - i) / CountRows;
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

            i = ((x - X) / Koeff) * 2;
            j = ((y - Y) / Koeff) * 2;
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

            return i + j * CountRows;
        }

        /// <summary>
        /// Установить значение флага одного из параметров
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <param name="value">Параметр</param>
        public void SetValue(int num, GridValue value)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            SetBit(ref _grid[num], (int)value, true);
        }

        /// <summary>
        /// Установить значение флага одного из параметров
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <param name="value">Параметр</param>
        public void SetValue(int i, int j, GridValue value)
        {
            SetValue(GetNum(i, j), value);
        }

        /// <summary>
        /// Снять значение флага одного из параметров
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <param name="value">Параметр</param>
        public void UnsetValue(int num, GridValue value)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            SetBit(ref _grid[num], (int)value, false);
        }

        /// <summary>
        /// Снять значение флага одного из параметров
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <param name="value">Параметр</param>
        public void UnsetValue(int i, int j, GridValue value)
        {
            UnsetValue(GetNum(i, j), value);
        }

        /// <summary>
        /// Проверка, что данная ячейка является металом
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public bool IsOwnMetal(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }



            return GetBit(_grid[num], (int)GridValue.OWN_METAL);
        }

        public bool IsOwnMetal(int i, int j)
        {
            return IsOwnMetal(GetNum(i, j));
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

            return GetBit(_grid[num], (int)GridValue.PIN);
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

            return GetBit(_grid[num], (int)GridValue.PROHIBITION_ZONE);
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

        public bool IsForeignMetal(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            return GetBit(_grid[num], (int)GridValue.FOREIGN_METAL);
        }

        public bool IsForeignMetal(int i, int j)
        {
            if (i < 0 || i >= CountRows)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= CountColumn)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            bool result = false;
            if (j > 0 && j < CountColumn - 1)
            {
                if (this[i, j - 1].MetalID == this[i, j + 1].MetalID && this[i, j - 1].MetalID != 0 && this[i, j + 1].MetalID != 0)
                {
                    result = true;
                }
            }
            if (i > 0 && i < CountRows - 1)
            {
                if (this[i - 1, j].MetalID == this[i + 1, j].MetalID && this[i - 1, j].MetalID != 0 && this[i + 1, j].MetalID != 0)
                {
                    result = true;
                }
            }

            result |= IsForeignMetal(GetNum(i, j));

            return result;
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

            return new Point(X + (Koeff * (i / 2)), Y + (Koeff * (j / 2)));
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

        public void WriteToFile()
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < CountRows; i++)
            {
                string str = "";
                for (int j = 0; j < CountColumn; j++)
                {
                    str += this[j, i].MetalID.ToString();
                }
                lines.Add(str);
            }
            File.WriteAllLines("matrixTest.txt", lines);
        }

        #endregion

        #region Private methods

        private void Grid_IsChanged(int numElement)
        {
            throw new NotImplementedException();
        }

        private void GenerateCoord(int width, int height, int koeff)
        {
            X = 0;
            Y = 0;
            Width = width + koeff;
            Height = height + koeff;
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

            CountColumn = (height / Koeff) * 2 - 1;
            CountRows = (width / Koeff) * 2 - 1;
            _grid = new GridElement[CountColumn * CountRows];
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

                indexes = Tuple.Create(i, j);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Установить бит в байте
        /// </summary>
        /// <param name="el">Байт в котором нужно установить бит</param>
        /// <param name="numBit">Номер бита</param>
        /// <param name="value">значение: true - 1, false - 0</param>
        private void SetBit(ref GridElement el, int numBit, bool value)
        {
            byte mask = (byte)(1 << numBit); // 0000100000....

            if (value)
            {
                el.ByteInfo = (byte)(el.ByteInfo | mask);// 1 | x = 1
            }
            else
            {
                el.ByteInfo = (byte)(el.ByteInfo & ~mask); // 0 & x = 0, x = 111110111111..
            }

        }

        /// <summary>
        /// Получить значение бита в байте
        /// </summary>
        /// <param name="el">Байт в котором нужно получить занчение бита</param>
        /// <param name="numBit">Номер бита</param>
        /// <param name="value">значение: true - 1, false - 0</param>
        private bool GetBit(GridElement el, int numBit)
        {
            byte mask = (byte)(1 << numBit); // 0000100000....
            el.ByteInfo = (byte)(mask & el.ByteInfo);
            if (el.ByteInfo > 0)
            {
                return true;
            }
            return false;
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
