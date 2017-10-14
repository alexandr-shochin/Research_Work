﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    [Serializable]
    public class Grid : IElement
    {
        private Point[] _nodes;
        public List<IElement> _elements;
        private byte[] _grid;

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="location">Объект Point, представляющий левый верхний угол прямоугольной области.</param>
        /// <param name="width">Ширина прямоугольной области</param>
        /// <param name="height">Высота прямоугольной области</param>
        /// <param name="koeff">Шаг для генерации точек узлов сетки</param>
        public Grid(Point location, int width, int height, int koeff)
        {
            GenerateCoord(location.x, location.y, width, height, koeff);
        }

        /// <summary>
        /// Конструтор
        /// </summary>
        /// <param name="x">Координата по оси X левого верхнего угла прямоугольной области</param>
        /// <param name="">Координата по оси Y левого верхнего угла прямоугольной области</param>
        /// <param name="width">Ширина прямоугольной области</param>
        /// <param name="height">Высота прямоугольной области</param>
        /// <param name="koeff">Шаг</param>
        public Grid(int x, int y, int width, int height, int koeff)
        {
            GenerateCoord(x, y, width, height, koeff);
        }

        #region Public methods

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
                    _elements.Add(el);
                    if (el is Pin)
                    {
                        SetValue(indexes.Item1, indexes.Item2, GridValue.PIN);
                    }
                    else if (el is ProhibitionZone)
                    {
                        SetValue(indexes.Item1, indexes.Item2, GridValue.PROHIBITION_ZONE);
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

            for (int i = 0; i < _elements.Count; ++i)
            {
                _elements[i].Draw(ref graphics);
            }
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
        public byte this[int i, int j]
        {
            get
            {
                if (i < 0 || i >= Width)
                {
                    throw new OverflowException("Индекс i находился вне границ сетки.");
                }
                if (j < 0 || j >= Height)
                {
                    throw new OverflowException("Индекс j находился вне границ сетки.");
                }
                return _grid[i + j * Width];
            }
        }

        /// <summary>
        /// Получить значение в сетке
        /// </summary>
        /// <param name="num">Номер ячейки</param>
        /// <returns></returns>
        public byte this[int num]
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
        public Tuple<int, int> GetIndexes(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            int i = num % (Width / Koeff);
            int j = (num - i) / (Width / Koeff);

            return Tuple.Create(i, j);
        }

        /// <summary>
        /// Получить номер ячейки в сетке по номеру строки и столбца
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns></returns>
        public int GetNum(int i, int j)
        {
            if (i < 0 || i >= Width / Koeff)
            {
                throw new OverflowException("Индекс i находился вне границ сетки.");
            }
            if (j < 0 || j >= Height / Koeff)
            {
                throw new OverflowException("Индекс j находился вне границ сетки.");
            }

            return i + j * (Width / Koeff);
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
        public bool IsMetal(int num)
        {
            if (num < 0 || num >= _grid.Length)
            {
                throw new OverflowException("Номер ячейки находился вне границ.");
            }

            return GetBit(_grid[num], 0);
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

            return GetBit(_grid[num], 1);
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

            return GetBit(_grid[num], 2);
        }

        /// <summary>
        /// Получить координаты ячейки по номеру строки и столбца
        /// </summary>
        /// <param name="i">Номер строки</param>
        /// <param name="j">Номер столбца</param>
        /// <returns>Возвращает координаты ячейки</returns>
        public Point GetCoordCell(int i, int j)
        {
            return new Point(X + (Width * i), Y - (Height * j));
        }

        public override string ToString()
        {
            return "Count = " + Count;
        }

        #endregion

        #region Private methods

        private void GenerateCoord(int x, int y, int width, int height, int koeff)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Koeff = koeff;

            _elements = new List<IElement>();
            _nodes = new Point[((width / Koeff) + 1) * ((height / Koeff) + 1)];

            // TODO определять квадрант

            int[] xs = new int[((width / Koeff) + 1)];
            int[] ys = new int[((height / Koeff) + 1)];

            if (x < 0 && y > 0)
            { 
            
            }
            else if (x > 0 && y > 0)
            { 
            
            }
            else if (x < 0 && y < 0)
            { 
            
            }
            else if (x > 0 && y < 0)
            { 
                
            }

            if (y > 0)
            {
                y = y - height;
                for (int i = ys.Length - 1; i >= 0; --i)
                {
                    ys[i] = y;
                    y += koeff;
                }
            }
            else
            {
                for (int i = 0; i < ys.Length; ++i)
                {
                    ys[i] = -y;
                    y += koeff;
                }
            }

            for (int i = 0; i < xs.Length; ++i)
            {
                xs[i] = x;
                x += koeff;
            }

            int index = 0;
            for (int j = 0; j < ys.Length; ++j)
            {
                for (int i = 0; i < xs.Length; ++i)
                {
                    _nodes[index] = new Point(xs[i], ys[j]);
                    index++;
                }
            }

                _grid = new byte[(width / Koeff) * (height / Koeff)];
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
                    i++;
                }

                int j = 0;
                int tmpY = Y;
                while (tmpY != y)
                {
                    tmpY -= Koeff;
                    j++;
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
        private void SetBit(ref byte el, int numBit, bool value)
        {
            byte mask = (byte)(1 << numBit); // 0000100000....

            if (value)
            {
                el = (byte)(el | mask);// 1 | x = 1
            }
            else
            {
                el = (byte)(el & ~mask); // 0 & x = 0, x = 111110111111..
            }

        }

        /// <summary>
        /// Получить значение бита в байте
        /// </summary>
        /// <param name="el">Байт в котором нужно получить занчение бита</param>
        /// <param name="numBit">Номер бита</param>
        /// <param name="value">значение: true - 1, false - 0</param>
        private bool GetBit(byte el, int numBit)
        {
            byte mask = (byte)(1 << numBit); // 0000100000....
            el = (byte)(mask & el);
            if (el > 0)
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
        public Color _Color { get { return Color.Black; } }

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

        #endregion
    }
}
