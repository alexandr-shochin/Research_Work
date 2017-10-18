using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation
{
    public enum ErrorCode // Разделить коды (возможно сделать через константы)
    {
        NO_ERROR = 1,                       // Без ошибок

        PIN_ALREADY_CONTAINTED = 2,         // Pin уже содержится в компоненте
        PIN_OUT_OF_BOUNDS = 3,              // Pin выходит за границы компоненты 
        PIN_INTERSECT_WITH_ANOTHER_PIN = 4, // Pin пересекается с другим pin

        ADD_ERROR = 5,                      // Ошибка при добавлении в элемент
        REMOVE_ERROR = 6,                   // Ошибка при удалении элемента
        CONTAINS_ERROR = 7,                 // Ошибка при поиске элемента

        PIN_WAS_FOUND = 8,                  // Pin был найден
        PIN_WAS_NOT_FOUND = 9,              // Pin не был найден

        PIN_WAS_DELETED = 10,                // Pin был удалён
        PIN_WAS_NOT_DELETED = 11,             // Pin не был удалён

        PROHIBITION_ZONE_WAS_FOUND = 12,
        PROHIBITION_ZONE_WAS_NOT_FOUND = 13
    }

    public enum GridValue
    { 
        FREE = 0,
        METAL = 1,
        PIN = 2,
        PROHIBITION_ZONE = 3,
        LEVEL = 4
    }
}
