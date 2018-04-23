using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    /// <summary>
    /// Элемент печатной платы
    /// </summary>
    public interface IBoardElement
    {
        #region Public methods

        ErrorCode Add(IBoardElement el);
        ErrorCode Remove(IBoardElement el);
        ErrorCode Contains(IBoardElement el);

        void Draw(Graphics graphics);

        #endregion

        #region Properties

        /// <summary>
        /// Уникальный идентификатор элемента
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Возвращает координату по оси Y прямоугольной области, являющуюся суммой значений свойств Y и Height.
        /// </summary>
        int Bottom { get; }

        /// <summary>
        /// Возвращает высоту прямоугольной области
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Данное свойство возвращает значение true, если значения всех свойств Width, Height, X и Y равны нулю. В противном случае возвращается значение false.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Возвращает координату по оси X левого края прямоугольной области
        /// </summary>
        int Left { get; }

        /// <summary>
        /// Возвращает координату по оси X прямоугольной области, являющуюся суммой значений свойств X и Width.
        /// </summary>
        int Right { get; }

        /// <summary>
        /// Возвращает координату по оси Y верхнего края прямоугольной области
        /// </summary>
        int Top { get; }

        /// <summary>
        /// Ширина прямоугольной области
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Возвращает координату по оси X левого верхнего угла прямоугольной области
        /// </summary>
        int X { get; }
        /// <summary>
        /// Возвращает координату по оси Y левого верхнего угла прямоугольной области
        /// </summary>
        int Y { get; }

        /// <summary>
        /// Возвращает цвет прямоугольной области
        /// </summary>
        Color Color { get; }

        #endregion
    }
}
