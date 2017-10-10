﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TracProg.Calculation;

namespace TracProg_Calculation_Tests
{
    [TestClass]
    public class Grid_Tests
    {
        private Grid _grid;

        [TestInitialize]
        public void Init()
        {
            _grid = new Grid(4, 4);
        }

        [TestMethod]
        public void TestMethod_SetValueMetal_return_True()
        {
            _grid.SetValue(0, GridValue.METAL);

            bool expected = true;
            bool actual = _grid.IsMetal(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValueMetal_return_False()
        {
            _grid.SetValue(0, GridValue.METAL);

            bool expected = false;
            bool actual = _grid.IsPin(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValuePin_return_True()
        {
            _grid.SetValue(0, GridValue.PIN);

            bool expected = true;
            bool actual = _grid.IsPin(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValuePin_return_False()
        {
            _grid.SetValue(0, GridValue.PIN);

            bool expected = false;
            bool actual = _grid.IsProhibitionZone(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValueProhibitionZone_return_True()
        {
            _grid.SetValue(0, GridValue.PROHIBITION_ZONE);

            bool expected = true;
            bool actual = _grid.IsProhibitionZone(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValueProhibitionZone_return_False()
        {
            _grid.SetValue(0, GridValue.PROHIBITION_ZONE);

            bool expected = false;
            bool actual = _grid.IsMetal(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValueMetal_And_Pin_return_True()
        {
            _grid.SetValue(0, GridValue.METAL);
            _grid.SetValue(0, GridValue.PIN);

            bool expected = true;
            bool actual = _grid.IsMetal(0) && _grid.IsPin(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_SetValueMetal_And_Pin_return_False()
        {
            _grid.SetValue(0, GridValue.METAL);
            _grid.SetValue(0, GridValue.PIN);

            bool expected = false;
            bool actual = _grid.IsMetal(0) && _grid.IsProhibitionZone(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_SetValue_invalid_num_one_return_exception()
        {
            _grid.SetValue(-1, GridValue.METAL);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_SetValue_invalid_num_two_return_exception()
        {
            _grid.SetValue(_grid.Count, GridValue.METAL);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_IsMetal_invalid_num_one_return_exception()
        {
            _grid.IsMetal(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_IsMetal_invalid_num_two_return_exception()
        {
            _grid.IsMetal(_grid.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_IsPin_invalid_num_one_return_exception()
        {
            _grid.IsPin(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_IsPin_invalid_num_two_return_exception()
        {
            _grid.IsPin(_grid.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_IsProhibitionZone_invalid_num_one_return_exception()
        {
            _grid.IsProhibitionZone(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_IsProhibitionZone_invalid_num_two_return_exception()
        {
            _grid.IsProhibitionZone(_grid.Count);
        }

        [TestMethod]
        public void TestMethod_Count_Grid_count_0_return_0()
        {
            Grid grid = new Grid(0, 0);

            int expected = 0;
            int actual = grid.Count;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_Count_Grid_return()
        {
            int expected = 16;
            int actual = _grid.Count;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethos_UnsetValue_invalid_num_one_return_exception()
        {
            _grid.UnsetValue(-1, GridValue.METAL);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethos_UnsetValue_invalid_num_two_return_exception()
        {
            _grid.UnsetValue(_grid.Count, GridValue.METAL);
        }

        [TestMethod]
        public void TestMethos_UnsetValue_return_true()
        {
            _grid.SetValue(0, GridValue.METAL);
            _grid.UnsetValue(0, GridValue.METAL);

            bool expected = false;
            bool actual = _grid.IsMetal(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_GetCoords_invalid_num_one_retun_exception()
        {
            Point p =  _grid.GetCoords(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_GetCoords_invalid_num_two_retun_exception()
        {
            Point p = _grid.GetCoords(_grid.Count);
        }

        [TestMethod]
        public void TestMethod_GetCoords_num_9_retun_1_2()
        {
            Point p = _grid.GetCoords(9);

            bool expected = true;
            bool actual = p.x == 1 && p.y == 2;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_GetCoords_num_14_retun_2_3()
        {
            Point p = _grid.GetCoords(14);

            bool expected = true;
            bool actual = p.x == 2 && p.y == 3;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс i находился вне границ сетки.")]
        public void TestMethod_GetNum_invalid_i_one_retun_exception()
        {
            int n = _grid.GetNum(-1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс i находился вне границ сетки.")]
        public void TestMethod_GetNum_invalid_i_two_retun_exception()
        {
            int n = _grid.GetNum(_grid.Width, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс j находился вне границ сетки.")]
        public void TestMethod_GetNum_invalid_j_one_retun_exception()
        {
            int n = _grid.GetNum(1, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс j находился вне границ сетки.")]
        public void TestMethod_GetNum_invalid_j_two_retun_exception()
        {
            int n = _grid.GetNum(1, _grid.Height);
        }

        [TestMethod]
        public void TestMethod_GetNum_i_1_j_2_retun_9()
        {
            int expected = 9;
            int actual = _grid.GetNum(1, 2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_GetNum_i_2_j_3_retun_14()
        {
            int expected = 14;
            int actual = _grid.GetNum(2, 3);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_GetItem_int_int_return_byte()
        {
            _grid.SetValue(_grid.GetNum(0, 0), GridValue.METAL);

            byte expected = 1;
            byte actual = _grid[0, 0];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс i находился вне границ сетки.")]
        public void TestMethod_GetItem_int_int_invalid_i_one_return_exception()
        {
            byte actual = _grid[-1, 0];
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс i находился вне границ сетки.")]
        public void TestMethod_GetItem_int_int_invalid_i_two_return_exception()
        {
            byte actual = _grid[_grid.Width, 0];
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс j находился вне границ сетки.")]
        public void TestMethod_GetItem_int_int_invalid_j_one_return_exception()
        {
            byte actual = _grid[0, -1];
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Индекс j находился вне границ сетки.")]
        public void TestMethod_GetItem_int_int_invalid_j_two_return_exception()
        {
            byte actual = _grid[0, _grid.Height];
        }

        [TestMethod]
        public void TestMethod_GetItem_int_return_byte()
        {
            _grid.SetValue(0, GridValue.METAL);

            byte expected = 1;
            byte actual = _grid[0];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_GetItem_int_invalid_one_return_exception()
        {
            byte actual = _grid[-1];
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException), "Номер ячейки находился вне границ.")]
        public void TestMethod_GetItem_int_invalid_two_return_exception()
        {
            byte actual = _grid[_grid.Count];
        }
    }
}
