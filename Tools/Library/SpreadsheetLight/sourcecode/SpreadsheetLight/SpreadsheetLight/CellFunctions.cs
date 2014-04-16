using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    public partial class SLDocument
    {
        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, bool Data)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return SetCellValue(iRowIndex, iColumnIndex, Data);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, bool Data)
        {
            if (!SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                return false;
            }

            SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
            SLCell c;
            if (slws.Cells.ContainsKey(pt))
            {
                c = slws.Cells[pt];
            }
            else
            {
                c = new SLCell();
                uint iStyleIndex = slws.GetExistingRowColumnStyle(RowIndex, ColumnIndex);
                if (iStyleIndex != 0)
                {
                    c.StyleIndex = iStyleIndex;
                }
            }
            c.DataType = CellValues.Boolean;
            c.NumericValue = Data ? 1 : 0;
            slws.Cells[pt] = c;

            return true;
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, float Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, float Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, double Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Data, null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, double Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Data, null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, decimal Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, decimal Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, byte Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, byte Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, short Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, short Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, ushort Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, ushort Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, int Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, int Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, uint Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, uint Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, long Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, long Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, ulong Data)
        {
            return SetCellValueNumberFinal(CellReference, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, ulong Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, true, Convert.ToDouble(Data), null);
        }

        /// <summary>
        /// Set the cell value given a cell reference and a numeric value in string form. Use this when the source data is numeric and is already in string form and parsing the data into numeric form is undesirable. Note that the numeric string must be in invariant-culture mode, so "123456.789" is the accepted form even if the current culture displays that as "123456,789".
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValueNumeric(string CellReference, string Data)
        {
            return SetCellValueNumberFinal(CellReference, false, 0, Data);
        }

        /// <summary>
        /// Set the cell value given the row index and column index and a numeric value in string form. Use this when the source data is numeric and is already in string form and parsing the data into numeric form is undesirable. Note that the numeric string must be in invariant-culture mode, so "123456.789" is the accepted form even if the current culture displays that as "123456,789".
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValueNumeric(int RowIndex, int ColumnIndex, string Data)
        {
            return SetCellValueNumberFinal(RowIndex, ColumnIndex, false, 0, Data);
        }

        /// <summary>
        /// Set the cell value given a cell reference. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, DateTime Data)
        {
            return SetCellValue(CellReference, Data, string.Empty, false);
        }

        /// <summary>
        /// Set the cell value given a cell reference. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, DateTime Data, bool For1904Epoch)
        {
            return SetCellValue(CellReference, Data, string.Empty, For1904Epoch);
        }

        /// <summary>
        /// Set the cell value given a cell reference. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <param name="Format">The format string used if the given date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, DateTime Data, string Format)
        {
            return SetCellValue(CellReference, Data, Format, false);
        }

        /// <summary>
        /// Set the cell value given a cell reference. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <param name="Format">The format string used if the given date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, DateTime Data, string Format, bool For1904Epoch)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return SetCellValue(iRowIndex, iColumnIndex, Data, Format, For1904Epoch);
        }

        /// <summary>
        /// Set the cell value given the row index and column index. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, DateTime Data)
        {
            return SetCellValue(RowIndex, ColumnIndex, Data, string.Empty, false);
        }

        /// <summary>
        /// Set the cell value given the row index and column index. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, DateTime Data, bool For1904Epoch)
        {
            return SetCellValue(RowIndex, ColumnIndex, Data, string.Empty, For1904Epoch);
        }

        /// <summary>
        /// Set the cell value given the row index and column index. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <param name="Format">The format string used if the given date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, DateTime Data, string Format)
        {
            return SetCellValue(RowIndex, ColumnIndex, Data, Format, false);
        }

        /// <summary>
        /// Set the cell value given the row index and column index. Be sure to follow up with a date format style.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <param name="Format">The format string used if the given date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, DateTime Data, string Format, bool For1904Epoch)
        {
            if (!SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                return false;
            }

            SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
            SLCell c;
            if (slws.Cells.ContainsKey(pt))
            {
                c = slws.Cells[pt];
            }
            else
            {
                c = new SLCell();
                uint iStyleIndex = slws.GetExistingRowColumnStyle(RowIndex, ColumnIndex);
                if (iStyleIndex != 0)
                {
                    c.StyleIndex = iStyleIndex;
                }
            }

            if (For1904Epoch) slwb.WorkbookProperties.Date1904 = true;

            double fDateTime = SLTool.CalculateDaysFromEpoch(Data, For1904Epoch);
            // see CalculateDaysFromEpoch to see why there's a difference
            double fDateCheck = For1904Epoch ? 0.0 : 1.0;

            if (fDateTime < fDateCheck)
            {
                // given datetime is earlier than epoch
                // So we set date to string format
                c.DataType = CellValues.SharedString;
                c.NumericValue = this.DirectSaveToSharedStringTable(Data.ToString(Format));
                slws.Cells[pt] = c;
            }
            else
            {
                c.DataType = CellValues.Number;
                c.NumericValue = fDateTime;
                slws.Cells[pt] = c;
            }

            return true;
        }

        private bool SetCellValueNumberFinal(string CellReference, bool IsNumeric, double NumericValue, string NumberData)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return SetCellValueNumberFinal(iRowIndex, iColumnIndex, IsNumeric, NumericValue, NumberData);
        }

        private bool SetCellValueNumberFinal(int RowIndex, int ColumnIndex, bool IsNumeric, double NumericValue, string NumberData)
        {
            if (!SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                return false;
            }

            SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
            SLCell c;
            if (slws.Cells.ContainsKey(pt))
            {
                c = slws.Cells[pt];
            }
            else
            {
                c = new SLCell();
                uint iStyleIndex = slws.GetExistingRowColumnStyle(RowIndex, ColumnIndex);
                if (iStyleIndex != 0)
                {
                    c.StyleIndex = iStyleIndex;
                }
            }
            c.DataType = CellValues.Number;
            if (IsNumeric) c.NumericValue = NumericValue;
            else c.CellText = NumberData;
            slws.Cells[pt] = c;

            return true;
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data in rich text.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, SLRstType Data)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return SetCellValue(iRowIndex, iColumnIndex, Data.ToInlineString());
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data in rich text.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, SLRstType Data)
        {
            return SetCellValue(RowIndex, ColumnIndex, Data.ToInlineString());
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data. Try the SLRstType class for easy InlineString generation.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, InlineString Data)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return SetCellValue(iRowIndex, iColumnIndex, Data);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data. Try the SLRstType class for easy InlineString generation.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, InlineString Data)
        {
            if (!SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                return false;
            }

            SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
            SLCell c;
            if (slws.Cells.ContainsKey(pt))
            {
                c = slws.Cells[pt];
            }
            else
            {
                c = new SLCell();
                uint iStyleIndex = slws.GetExistingRowColumnStyle(RowIndex, ColumnIndex);
                if (iStyleIndex != 0)
                {
                    c.StyleIndex = iStyleIndex;
                }
            }
            c.DataType = CellValues.SharedString;
            c.NumericValue = this.DirectSaveToSharedStringTable(Data);
            slws.Cells[pt] = c;

            return true;
        }

        /// <summary>
        /// Set the cell value given a cell reference.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if the cell reference is invalid. True otherwise.</returns>
        public bool SetCellValue(string CellReference, string Data)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return SetCellValue(iRowIndex, iColumnIndex, Data);
        }

        /// <summary>
        /// Set the cell value given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Data">The cell value data.</param>
        /// <returns>False if either the row index or column index (or both) are invalid. True otherwise.</returns>
        public bool SetCellValue(int RowIndex, int ColumnIndex, string Data)
        {
            if (!SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                return false;
            }

            SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
            SLCell c;
            if (slws.Cells.ContainsKey(pt))
            {
                c = slws.Cells[pt];
            }
            else
            {
                // if there's no existing cell, then we don't have to assign
                // a new cell when the data string is empty
                if (Data == null || Data.Length == 0) return true;

                c = new SLCell();
                uint iStyleIndex = slws.GetExistingRowColumnStyle(RowIndex, ColumnIndex);
                if (iStyleIndex != 0)
                {
                    c.StyleIndex = iStyleIndex;
                }
            }

            if (Data == null || Data.Length == 0)
            {
                c.DataType = CellValues.Number;
                c.CellText = string.Empty;
                slws.Cells[pt] = c;
            }
            else if (Data.StartsWith("="))
            {
                // in case it's just one equal sign
                if (Data.Equals("=", StringComparison.InvariantCultureIgnoreCase))
                {
                    c.DataType = CellValues.SharedString;
                    c.NumericValue = this.DirectSaveToSharedStringTable("=");
                    slws.Cells[pt] = c;
                }
                else
                {
                    // For simplicity, we're gonna assume that if it starts with an equal sign, it's a formula.

                    // TODO Formula calculation engine
                    c.DataType = CellValues.Number;
                    //c.Formula = new CellFormula(slxe.Write(Data.Substring(1)));
                    c.CellFormula = new SLCellFormula();
                    c.CellFormula.FormulaText = SLTool.XmlWrite(Data.Substring(1));
                    c.CellText = string.Empty;
                    slws.Cells[pt] = c;
                }
            }
            else if (Data.StartsWith("'"))
            {
                c.DataType = CellValues.SharedString;
                c.NumericValue = this.DirectSaveToSharedStringTable(SLTool.XmlWrite(Data.Substring(1)));
                slws.Cells[pt] = c;
            }
            else
            {
                c.DataType = CellValues.SharedString;
                c.NumericValue = this.DirectSaveToSharedStringTable(SLTool.XmlWrite(Data));
                slws.Cells[pt] = c;
            }

            return true;
        }

        /// <summary>
        /// Get the cell value as a boolean. If the cell value wasn't originally a boolean value, the return value is undetermined (but is by default false).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A boolean cell value.</returns>
        public bool GetCellValueAsBoolean(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return false;
            }

            return GetCellValueAsBoolean(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a boolean. If the cell value wasn't originally a boolean value, the return value is undetermined (but is by default false).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A boolean cell value.</returns>
        public bool GetCellValueAsBoolean(int RowIndex, int ColumnIndex)
        {
            bool result = false;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Boolean)
                    {
                        double fValue = 0;
                        if (c.CellText != null)
                        {
                            if (double.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out fValue))
                            {
                                if (fValue > 0.5) result = true;
                                else result = false;
                            }
                            else
                            {
                                bool.TryParse(c.CellText, out result);
                            }
                        }
                        else
                        {
                            if (c.NumericValue > 0.5) result = true;
                            else result = false;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as a 32-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A 32-bit integer cell value.</returns>
        public Int32 GetCellValueAsInt32(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return 0;
            }

            return GetCellValueAsInt32(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a 32-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A 32-bit integer cell value.</returns>
        public Int32 GetCellValueAsInt32(int RowIndex, int ColumnIndex)
        {
            Int32 result = 0;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            Int32.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                        }
                        else
                        {
                            result = Convert.ToInt32(c.NumericValue);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as an unsigned 32-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>An unsigned 32-bit integer cell value.</returns>
        public UInt32 GetCellValueAsUInt32(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return 0;
            }

            return GetCellValueAsUInt32(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as an unsigned 32-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>An unsigned 32-bit integer cell value.</returns>
        public UInt32 GetCellValueAsUInt32(int RowIndex, int ColumnIndex)
        {
            UInt32 result = 0;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            UInt32.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                        }
                        else
                        {
                            result = Convert.ToUInt32(c.NumericValue);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as a 64-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A 64-bit integer cell value.</returns>
        public Int64 GetCellValueAsInt64(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return 0;
            }

            return GetCellValueAsInt64(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a 64-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A 64-bit integer cell value.</returns>
        public Int64 GetCellValueAsInt64(int RowIndex, int ColumnIndex)
        {
            Int64 result = 0;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            Int64.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                        }
                        else
                        {
                            result = Convert.ToInt64(c.NumericValue);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as an unsigned 64-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>An unsigned 64-bit integer cell value.</returns>
        public UInt64 GetCellValueAsUInt64(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return 0;
            }

            return GetCellValueAsUInt64(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as an unsigned 64-bit integer. If the cell value wasn't originally an integer, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>An unsigned 64-bit integer cell value.</returns>
        public UInt64 GetCellValueAsUInt64(int RowIndex, int ColumnIndex)
        {
            UInt64 result = 0;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            UInt64.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                        }
                        else
                        {
                            result = Convert.ToUInt64(c.NumericValue);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as a double precision floating point number. If the cell value wasn't originally a floating point number, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A double precision floating point number cell value.</returns>
        public double GetCellValueAsDouble(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return 0.0;
            }

            return GetCellValueAsDouble(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a double precision floating point number. If the cell value wasn't originally a floating point number, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A double precision floating point number cell value.</returns>
        public double GetCellValueAsDouble(int RowIndex, int ColumnIndex)
        {
            double result = 0.0;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            double.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                        }
                        else
                        {
                            result = c.NumericValue;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as a System.Decimal value. If the cell value wasn't originally an integer or floating point number, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A System.Decimal cell value.</returns>
        public decimal GetCellValueAsDecimal(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return 0m;
            }

            return GetCellValueAsDecimal(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a System.Decimal value. If the cell value wasn't originally an integer or floating point number, the return value is undetermined (but is by default 0).
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A System.Decimal cell value.</returns>
        public decimal GetCellValueAsDecimal(int RowIndex, int ColumnIndex)
        {
            decimal result = 0m;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            decimal.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                        }
                        else
                        {
                            result = Convert.ToDecimal(c.NumericValue);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                if (slwb.WorkbookProperties.Date1904) return SLConstants.Epoch1904();
                else return SLConstants.Epoch1900();
            }

            return GetCellValueAsDateTime(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(int RowIndex, int ColumnIndex)
        {
            return GetCellValueAsDateTime(RowIndex, ColumnIndex, string.Empty, false);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(string CellReference, bool For1904Epoch)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                if (slwb.WorkbookProperties.Date1904) return SLConstants.Epoch1904();
                else return SLConstants.Epoch1900();
            }

            return GetCellValueAsDateTime(iRowIndex, iColumnIndex, For1904Epoch);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(int RowIndex, int ColumnIndex, bool For1904Epoch)
        {
            return GetCellValueAsDateTime(RowIndex, ColumnIndex, string.Empty, For1904Epoch);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Format">The format string used to parse the date value in the cell if the date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(string CellReference, string Format)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                if (slwb.WorkbookProperties.Date1904) return SLConstants.Epoch1904();
                else return SLConstants.Epoch1900();
            }

            return GetCellValueAsDateTime(iRowIndex, iColumnIndex, Format);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Format">The format string used to parse the date value in the cell if the date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(int RowIndex, int ColumnIndex, string Format)
        {
            return GetCellValueAsDateTime(RowIndex, ColumnIndex, Format, false);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <param name="Format">The format string used to parse the date value in the cell if the date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(string CellReference, string Format, bool For1904Epoch)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                if (slwb.WorkbookProperties.Date1904) return SLConstants.Epoch1904();
                else return SLConstants.Epoch1900();
            }

            return GetCellValueAsDateTime(iRowIndex, iColumnIndex, Format, For1904Epoch);
        }

        /// <summary>
        /// Get the cell value as a System.DateTime value. If the cell value wasn't originally a date/time value, the return value is undetermined.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="Format">The format string used to parse the date value in the cell if the date is before the date epoch. A date before the date epoch is stored as a string, so the date precision is only as good as the format string. For example, "dd/MM/yyyy HH:mm:ss" is more precise than "dd/MM/yyyy" because the latter loses information about the hours, minutes and seconds.</param>
        /// <param name="For1904Epoch">True if using 1 Jan 1904 as the date epoch. False if using 1 Jan 1900 as the date epoch. This is independent of the workbook's Date1904 property.</param>
        /// <returns>A System.DateTime cell value.</returns>
        public DateTime GetCellValueAsDateTime(int RowIndex, int ColumnIndex, string Format, bool For1904Epoch)
        {
            DateTime dt;
            if (For1904Epoch) dt = SLConstants.Epoch1904();
            else dt = SLConstants.Epoch1900();

            // If the cell data type is Number, then it's on or after the epoch.
            // If it's a string or a shared string, then a string representation of the date
            // is stored, where the date is before the epoch. Then we parse the string to
            // get the date.

            double fDateOffset = 0.0;
            string sDate = string.Empty;

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.DataType == CellValues.Number)
                    {
                        if (c.CellText != null)
                        {
                            if (double.TryParse(c.CellText, NumberStyles.Any, CultureInfo.InvariantCulture, out fDateOffset))
                            {
                                dt = SLTool.CalculateDateTimeFromDaysFromEpoch(fDateOffset, For1904Epoch);
                            }
                        }
                        else
                        {
                            dt = SLTool.CalculateDateTimeFromDaysFromEpoch(c.NumericValue, For1904Epoch);
                        }
                    }
                    else if (c.DataType == CellValues.SharedString)
                    {
                        SLRstType rst = new SLRstType(SimpleTheme.MajorLatinFont, SimpleTheme.MinorLatinFont, SimpleTheme.listThemeColors, SimpleTheme.listIndexedColors);
                        int index = 0;
                        try
                        {
                            if (c.CellText != null)
                            {
                                index = int.Parse(c.CellText);
                            }
                            else
                            {
                                index = Convert.ToInt32(c.NumericValue);
                            }
                            
                            if (index >= 0 && index < listSharedString.Count)
                            {
                                rst.FromHash(listSharedString[index]);
                                sDate = rst.ToPlainString();

                                if (Format.Length > 0)
                                {
                                    dt = DateTime.ParseExact(sDate, Format, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    dt = DateTime.Parse(sDate, CultureInfo.InvariantCulture);
                                }
                            }
                            // no else part, because there's nothing we can do!
                            // Just return the default date value...
                        }
                        catch
                        {
                            // something terrible just happened. (the shared string index probably
                            // isn't even correct!) Don't do anything...
                        }
                    }
                    else if (c.DataType == CellValues.String)
                    {
                        sDate = c.CellText ?? string.Empty;
                        try
                        {
                            if (Format.Length > 0)
                            {
                                dt = DateTime.ParseExact(sDate, Format, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                dt = DateTime.Parse(sDate, CultureInfo.InvariantCulture);
                            }
                        }
                        catch
                        {
                            // don't need to do anything. Just return the default date value.
                            // The point is to avoid throwing exceptions.
                        }
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Get the cell value as a string.
        /// </summary>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        /// <returns>A string cell value.</returns>
        public string GetCellValueAsString(string CellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex))
            {
                return string.Empty;
            }

            return GetCellValueAsString(iRowIndex, iColumnIndex);
        }

        /// <summary>
        /// Get the cell value as a string.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>A string cell value.</returns>
        public string GetCellValueAsString(int RowIndex, int ColumnIndex)
        {
            string result = string.Empty;
            int index = 0;
            SLRstType rst = new SLRstType(SimpleTheme.MajorLatinFont, SimpleTheme.MinorLatinFont, SimpleTheme.listThemeColors, SimpleTheme.listIndexedColors);

            if (SLTool.CheckRowColumnIndexLimit(RowIndex, ColumnIndex))
            {
                SLCellPoint pt = new SLCellPoint(RowIndex, ColumnIndex);
                if (slws.Cells.ContainsKey(pt))
                {
                    SLCell c = slws.Cells[pt];
                    if (c.CellText != null)
                    {
                        if (c.DataType == CellValues.String)
                        {
                            result = c.CellText;
                        }
                        else if (c.DataType == CellValues.SharedString)
                        {
                            try
                            {
                                index = int.Parse(c.CellText);
                                if (index >= 0 && index < listSharedString.Count)
                                {
                                    rst.FromHash(listSharedString[index]);
                                    result = rst.ToPlainString();
                                }
                                else
                                {
                                    result = SLTool.XmlRead(c.CellText);
                                }
                            }
                            catch
                            {
                                // something terrible just happened. We'll just use whatever's in the cell...
                                result = SLTool.XmlRead(c.CellText);
                            }
                        }
                        //else if (c.DataType == CellValues.InlineString)
                        //{
                        //    // there shouldn't be any inline strings
                        //    // because they'd already be transferred to shared strings
                        //}
                        else
                        {
                            result = SLTool.XmlRead(c.CellText);
                        }
                    }
                    else
                    {
                        if (c.DataType == CellValues.Number)
                        {
                            result = c.NumericValue.ToString(CultureInfo.InvariantCulture);
                        }
                        else if (c.DataType == CellValues.SharedString)
                        {
                            index = Convert.ToInt32(c.NumericValue);
                            if (index >= 0 && index < listSharedString.Count)
                            {
                                rst.FromHash(listSharedString[index]);
                                result = rst.ToPlainString();
                            }
                            else
                            {
                                result = SLTool.XmlRead(c.CellText);
                            }
                        }
                        else if (c.DataType == CellValues.Boolean)
                        {
                            if (c.NumericValue > 0.5) result = "true";
                            else result = "false";
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Merge cells given a corner cell of the to-be-merged rectangle of cells, and the opposite corner cell. For example, the top-left corner cell and the bottom-right corner cell. Or the bottom-left corner cell and the top-right corner cell.
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the corner cell, such as "A1".</param>
        /// <param name="EndCellReference">The cell reference of the opposite corner cell, such as "A1".</param>
        /// <returns>True if merging is successful. False otherwise.</returns>
        public bool MergeWorksheetCells(string StartCellReference, string EndCellReference)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex))
            {
                return false;
            }

            return MergeWorksheetCells(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Merge cells given a corner cell of the to-be-merged rectangle of cells, and the opposite corner cell. For example, the top-left corner cell and the bottom-right corner cell. Or the bottom-left corner cell and the top-right corner cell.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the corner cell.</param>
        /// <param name="StartColumnIndex">The column index of the corner cell.</param>
        /// <param name="EndRowIndex">The row index of the opposite corner cell.</param>
        /// <param name="EndColumnIndex">The column index of the opposite corner cell.</param>
        /// <returns>True if merging is successful. False otherwise.</returns>
        public bool MergeWorksheetCells(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1, iStartColumnIndex = 1, iEndColumnIndex = 1;
            if (StartRowIndex < EndRowIndex)
            {
                iStartRowIndex = StartRowIndex;
                iEndRowIndex = EndRowIndex;
            }
            else
            {
                iStartRowIndex = EndRowIndex;
                iEndRowIndex = StartRowIndex;
            }

            if (StartColumnIndex < EndColumnIndex)
            {
                iStartColumnIndex = StartColumnIndex;
                iEndColumnIndex = EndColumnIndex;
            }
            else
            {
                iStartColumnIndex = EndColumnIndex;
                iEndColumnIndex = StartColumnIndex;
            }

            // no point merging one cell
            if (iStartRowIndex == iEndRowIndex && iStartColumnIndex == iEndColumnIndex)
            {
                return false;
            }

            int i;
            bool result = false;
            SLMergeCell mc = new SLMergeCell();
            if (SLTool.CheckRowColumnIndexLimit(iStartRowIndex, iStartColumnIndex) && SLTool.CheckRowColumnIndexLimit(iEndRowIndex, iEndColumnIndex))
            {
                result = true;
                for (i = 0; i < slws.MergeCells.Count; ++i)
                {
                    mc = slws.MergeCells[i];

                    // This comes from the separating axis theorem.
                    // We're checking that the given merged cell does not overlap with
                    // any existing merged cells. The conditions are made easier because
                    // the merged cells are rectangular, the row/column indices are whole numbers,
                    // and they map strictly to a 2D grid.
                    // We've also rearranged values such that the given end row index is equal
                    // to or greater than the given start row index (similarly for the column index).
                    // This means we only need to check for one given value against an existing value.

                    // The given merged cell doesn't overlap if:
                    // 1) it is completely above the existing merged cell OR
                    // 2) it is completely below the existing merged cell OR
                    // 3) it is completely to the left of the existing merged cell OR
                    // 4) it is completely to the right of the existing merged cell

                    if (!(iEndRowIndex < mc.StartRowIndex || iStartRowIndex > mc.EndRowIndex || iEndColumnIndex < mc.StartColumnIndex || iStartColumnIndex > mc.EndColumnIndex))
                    {
                        result = false;
                        break;
                    }
                }

                if (result)
                {
                    SLTable t;
                    for (i = 0; i < slws.Tables.Count; ++i)
                    {
                        t = slws.Tables[i];
                        if (!(iEndRowIndex < t.StartRowIndex || iStartRowIndex > t.EndRowIndex || iEndColumnIndex < t.StartColumnIndex || iStartColumnIndex > t.EndColumnIndex))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            // if all went well!
            if (result)
            {
                mc = new SLMergeCell();
                mc.FromIndices(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
                slws.MergeCells.Add(mc);
            }

            return result;
        }

        /// <summary>
        /// Unmerge cells given a corner cell of an existing merged rectangle of cells, and the opposite corner cell. For example, the top-left corner cell and the bottom-right corner cell. Or the bottom-left corner cell and the top-right corner cell.
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the corner cell, such as "A1".</param>
        /// <param name="EndCellReference">The cell reference of the opposite corner cell, such as "A1".</param>
        /// <returns>True if unmerging is successful. False otherwise.</returns>
        public bool UnmergeWorksheetCells(string StartCellReference, string EndCellReference)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex))
            {
                return false;
            }

            return UnmergeWorksheetCells(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Unmerge cells given a corner cell of an existing merged rectangle of cells, and the opposite corner cell. For example, the top-left corner cell and the bottom-right corner cell. Or the bottom-left corner cell and the top-right corner cell.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the corner cell.</param>
        /// <param name="StartColumnIndex">The column index of the corner cell.</param>
        /// <param name="EndRowIndex">The row index of the opposite corner cell.</param>
        /// <param name="EndColumnIndex">The column index of the opposite corner cell.</param>
        /// <returns>True if unmerging is successful. False otherwise.</returns>
        public bool UnmergeWorksheetCells(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1, iStartColumnIndex = 1, iEndColumnIndex = 1;
            if (StartRowIndex < EndRowIndex)
            {
                iStartRowIndex = StartRowIndex;
                iEndRowIndex = EndRowIndex;
            }
            else
            {
                iStartRowIndex = EndRowIndex;
                iEndRowIndex = StartRowIndex;
            }

            if (StartColumnIndex < EndColumnIndex)
            {
                iStartColumnIndex = StartColumnIndex;
                iEndColumnIndex = EndColumnIndex;
            }
            else
            {
                iStartColumnIndex = EndColumnIndex;
                iEndColumnIndex = StartColumnIndex;
            }

            bool result = false;
            SLMergeCell mc = new SLMergeCell();
            for (int i = 0; i < slws.MergeCells.Count; ++i)
            {
                mc = slws.MergeCells[i];
                if (mc.StartRowIndex == iStartRowIndex && mc.StartColumnIndex == iStartColumnIndex && mc.EndRowIndex == iEndRowIndex && mc.EndColumnIndex == iEndColumnIndex)
                {
                    slws.MergeCells.RemoveAt(i);
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get a list of the existing merged cells in an array.
        /// </summary>
        /// <returns>An array of the merged cells.</returns>
        public SLMergeCell[] GetWorksheetMergeCells()
        {
            return slws.MergeCells.ToArray();
        }

        /// <summary>
        /// Filter data.
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the corner cell, such as "A1".</param>
        /// <param name="EndCellReference">The cell reference of the opposite corner cell, such as "A1".</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool Filter(string StartCellReference, string EndCellReference)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex))
            {
                return false;
            }

            return this.Filter(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Filter data.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the corner cell.</param>
        /// <param name="StartColumnIndex">The column index of the corner cell.</param>
        /// <param name="EndRowIndex">The row index of the opposite corner cell.</param>
        /// <param name="EndColumnIndex">The column index of the opposite corner cell.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool Filter(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1, iStartColumnIndex = 1, iEndColumnIndex = 1;
            if (StartRowIndex < EndRowIndex)
            {
                iStartRowIndex = StartRowIndex;
                iEndRowIndex = EndRowIndex;
            }
            else
            {
                iStartRowIndex = EndRowIndex;
                iEndRowIndex = StartRowIndex;
            }

            if (StartColumnIndex < EndColumnIndex)
            {
                iStartColumnIndex = StartColumnIndex;
                iEndColumnIndex = EndColumnIndex;
            }
            else
            {
                iStartColumnIndex = EndColumnIndex;
                iEndColumnIndex = StartColumnIndex;
            }

            int i;
            bool result = false;
            if (SLTool.CheckRowColumnIndexLimit(iStartRowIndex, iStartColumnIndex) && SLTool.CheckRowColumnIndexLimit(iEndRowIndex, iEndColumnIndex))
            {
                result = true;

                // This comes from the separating axis theorem. See merging cells method for more details.

                // Technically, Excel allows you to filter a grid of cells with merged cells. But the
                // behaviour is a little dependent on the actual data. For example, you either select
                // the whole merged cell or you don't in the filter range. However, I'm not going to
                // enforce this.

                // Also technically speaking, you *can* filter a grid of cells that overlaps a table.
                // But the conditions are specific. The filter range must be completely within the table.
                // But the effect is that you remove the filter from the table. This is Excel! This is
                // because there's a visual interface.
                // So I'm going to assume the given filter range *cannot* overlap a table.
                SLTable t;
                for (i = 0; i < slws.Tables.Count; ++i)
                {
                    t = slws.Tables[i];
                    if (!(iEndRowIndex < t.StartRowIndex || iStartRowIndex > t.EndRowIndex || iEndColumnIndex < t.StartColumnIndex || iStartColumnIndex > t.EndColumnIndex))
                    {
                        result = false;
                        break;
                    }
                }

                if (result)
                {
                    slws.HasAutoFilter = true;
                    slws.AutoFilter = new SLAutoFilter();
                    slws.AutoFilter.StartRowIndex = iStartRowIndex;
                    slws.AutoFilter.StartColumnIndex = iStartColumnIndex;
                    slws.AutoFilter.EndRowIndex = iEndRowIndex;
                    slws.AutoFilter.EndColumnIndex = iEndColumnIndex;
                }
            }

            return result;
        }

        /// <summary>
        /// Removing any data filter.
        /// </summary>
        public void RemoveFilter()
        {
            slws.HasAutoFilter = false;
            slws.AutoFilter = new SLAutoFilter();
        }

        /// <summary>
        /// Copy one cell to another cell.
        /// </summary>
        /// <param name="CellReference">The cell reference of the cell to be copied from, such as "A1".</param>
        /// <param name="AnchorCellReference">The cell reference of the cell to be copied to, such as "A1".</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(string CellReference, string AnchorCellReference)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            int iAnchorRowIndex = -1;
            int iAnchorColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(AnchorCellReference, out iAnchorRowIndex, out iAnchorColumnIndex))
            {
                return false;
            }

            return CopyCell(iRowIndex, iColumnIndex, iRowIndex, iColumnIndex, iAnchorRowIndex, iAnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy one cell to another cell.
        /// </summary>
        /// <param name="CellReference">The cell reference of the cell to be copied from, such as "A1".</param>
        /// <param name="AnchorCellReference">The cell reference of the cell to be copied to, such as "A1".</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(string CellReference, string AnchorCellReference, bool ToCut)
        {
            int iRowIndex = -1;
            int iColumnIndex = -1;
            int iAnchorRowIndex = -1;
            int iAnchorColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out iRowIndex, out iColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(AnchorCellReference, out iAnchorRowIndex, out iAnchorColumnIndex))
            {
                return false;
            }

            return CopyCell(iRowIndex, iColumnIndex, iRowIndex, iColumnIndex, iAnchorRowIndex, iAnchorColumnIndex, ToCut);
        }

        /// <summary>
        /// Copy a range of cells to another range, given the anchor cell of the destination range (top-left cell).
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the start cell of the cell range, such as "A1". This is typically the top-left cell.</param>
        /// <param name="EndCellReference">The cell reference of the end cell of the cell range, such as "A1". This is typically the bottom-right cell.</param>
        /// <param name="AnchorCellReference">The cell reference of the anchor cell, such as "A1".</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(string StartCellReference, string EndCellReference, string AnchorCellReference)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            int iAnchorRowIndex = -1;
            int iAnchorColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(AnchorCellReference, out iAnchorRowIndex, out iAnchorColumnIndex))
            {
                return false;
            }

            return CopyCell(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex, iAnchorRowIndex, iAnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy a range of cells to another range, given the anchor cell of the destination range (top-left cell).
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the start cell of the cell range, such as "A1". This is typically the top-left cell.</param>
        /// <param name="EndCellReference">The cell reference of the end cell of the cell range, such as "A1". This is typically the bottom-right cell.</param>
        /// <param name="AnchorCellReference">The cell reference of the anchor cell, such as "A1".</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(string StartCellReference, string EndCellReference, string AnchorCellReference, bool ToCut)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            int iAnchorRowIndex = -1;
            int iAnchorColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(AnchorCellReference, out iAnchorRowIndex, out iAnchorColumnIndex))
            {
                return false;
            }

            return CopyCell(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex, iAnchorRowIndex, iAnchorColumnIndex, ToCut);
        }

        /// <summary>
        /// Copy one cell to another cell.
        /// </summary>
        /// <param name="RowIndex">The row index of the cell to be copied from.</param>
        /// <param name="ColumnIndex">The column index of the cell to be copied from.</param>
        /// <param name="AnchorRowIndex">The row index of the cell to be copied to.</param>
        /// <param name="AnchorColumnIndex">The column index of the cell to be copied to.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(int RowIndex, int ColumnIndex, int AnchorRowIndex, int AnchorColumnIndex)
        {
            return CopyCell(RowIndex, ColumnIndex, RowIndex, ColumnIndex, AnchorRowIndex, AnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy one cell to another cell.
        /// </summary>
        /// <param name="RowIndex">The row index of the cell to be copied from.</param>
        /// <param name="ColumnIndex">The column index of the cell to be copied from.</param>
        /// <param name="AnchorRowIndex">The row index of the cell to be copied to.</param>
        /// <param name="AnchorColumnIndex">The column index of the cell to be copied to.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(int RowIndex, int ColumnIndex, int AnchorRowIndex, int AnchorColumnIndex, bool ToCut)
        {
            return CopyCell(RowIndex, ColumnIndex, RowIndex, ColumnIndex, AnchorRowIndex, AnchorColumnIndex, ToCut);
        }

        /// <summary>
        /// Copy a range of cells to another range, given the anchor cell of the destination range (top-left cell).
        /// </summary>
        /// <param name="StartRowIndex">The row index of the start cell of the cell range. This is typically the top-left cell.</param>
        /// <param name="StartColumnIndex">The column index of the start cell of the cell range. This is typically the top-left cell.</param>
        /// <param name="EndRowIndex">The row index of the end cell of the cell range. This is typically the bottom-right cell.</param>
        /// <param name="EndColumnIndex">The column index of the end cell of the cell range. This is typically the bottom-right cell.</param>
        /// <param name="AnchorRowIndex">The row index of the anchor cell.</param>
        /// <param name="AnchorColumnIndex">The column index of the anchor cell.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex, int AnchorRowIndex, int AnchorColumnIndex)
        {
            return CopyCell(StartRowIndex, StartColumnIndex, EndRowIndex, EndColumnIndex, AnchorRowIndex, AnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy a range of cells to another range, given the anchor cell of the destination range (top-left cell).
        /// </summary>
        /// <param name="StartRowIndex">The row index of the start cell of the cell range. This is typically the top-left cell.</param>
        /// <param name="StartColumnIndex">The column index of the start cell of the cell range. This is typically the top-left cell.</param>
        /// <param name="EndRowIndex">The row index of the end cell of the cell range. This is typically the bottom-right cell.</param>
        /// <param name="EndColumnIndex">The column index of the end cell of the cell range. This is typically the bottom-right cell.</param>
        /// <param name="AnchorRowIndex">The row index of the anchor cell.</param>
        /// <param name="AnchorColumnIndex">The column index of the anchor cell.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyCell(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex, int AnchorRowIndex, int AnchorColumnIndex, bool ToCut)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1, iStartColumnIndex = 1, iEndColumnIndex = 1;
            if (StartRowIndex < EndRowIndex)
            {
                iStartRowIndex = StartRowIndex;
                iEndRowIndex = EndRowIndex;
            }
            else
            {
                iStartRowIndex = EndRowIndex;
                iEndRowIndex = StartRowIndex;
            }

            if (StartColumnIndex < EndColumnIndex)
            {
                iStartColumnIndex = StartColumnIndex;
                iEndColumnIndex = EndColumnIndex;
            }
            else
            {
                iStartColumnIndex = EndColumnIndex;
                iEndColumnIndex = StartColumnIndex;
            }

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit
                && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit
                && iStartColumnIndex >= 1 && iStartColumnIndex <= SLConstants.ColumnLimit
                && iEndColumnIndex >= 1 && iEndColumnIndex <= SLConstants.ColumnLimit
                && AnchorRowIndex >= 1 && AnchorRowIndex <= SLConstants.RowLimit
                && AnchorColumnIndex >= 1 && AnchorColumnIndex <= SLConstants.ColumnLimit
                && iStartRowIndex != AnchorRowIndex
                && iStartColumnIndex != AnchorColumnIndex)
            {
                result = true;

                int rowdiff = AnchorRowIndex - iStartRowIndex;
                int coldiff = AnchorColumnIndex - iStartColumnIndex;
                Dictionary<SLCellPoint, SLCell> cells = new Dictionary<SLCellPoint, SLCell>();
                List<SLCellPoint> listCellKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                foreach (SLCellPoint pt in listCellKeys)
                {
                    if (pt.RowIndex >= iStartRowIndex && pt.RowIndex <= iEndRowIndex
                        && pt.ColumnIndex >= iStartColumnIndex && pt.ColumnIndex <= iEndColumnIndex)
                    {
                        cells[new SLCellPoint(pt.RowIndex + rowdiff, pt.ColumnIndex + coldiff)] = slws.Cells[pt].Clone();
                        if (ToCut)
                        {
                            slws.Cells.Remove(pt);
                        }
                    }
                }

                int AnchorEndRowIndex = AnchorRowIndex + iEndRowIndex - iStartRowIndex;
                int AnchorEndColumnIndex = AnchorColumnIndex + iEndColumnIndex - iStartColumnIndex;
                listCellKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                foreach (SLCellPoint pt in listCellKeys)
                {
                    // any cell within destination "paste" operation is taken out
                    if (pt.RowIndex >= AnchorRowIndex && pt.RowIndex <= AnchorEndRowIndex
                        && pt.ColumnIndex >= AnchorColumnIndex && pt.ColumnIndex <= AnchorEndColumnIndex)
                    {
                        slws.Cells.Remove(pt);
                    }
                }

                int iNumberOfRows = iEndRowIndex - iStartRowIndex + 1;
                if (AnchorRowIndex <= iStartRowIndex) iNumberOfRows = -iNumberOfRows;
                int iNumberOfColumns = iEndColumnIndex - iStartColumnIndex + 1;
                if (AnchorColumnIndex <= iStartColumnIndex) iNumberOfColumns = -iNumberOfColumns;
                SLCell c;
                foreach (var key in cells.Keys)
                {
                    c = cells[key];
                    this.ProcessCellFormulaDelta(ref c, AnchorRowIndex, iNumberOfRows, AnchorColumnIndex, iNumberOfColumns);
                    slws.Cells[key] = c;
                }

                // TODO: tables!

                // cutting and pasting into a region with merged cells unmerges the existing merged cells
                // copying and pasting into a region with merged cells leaves existing merged cells alone.
                // Why does Excel do that? Don't know.
                // Will just standardise to leaving existing merged cells alone.
                SLMergeCell[] mca = this.GetWorksheetMergeCells();
                foreach (SLMergeCell mc in mca)
                {
                    if (mc.StartRowIndex >= iStartRowIndex && mc.EndRowIndex <= iEndRowIndex
                        && mc.StartColumnIndex >= iStartColumnIndex && mc.EndColumnIndex <= iEndColumnIndex)
                    {
                        if (ToCut)
                        {
                            slws.MergeCells.Remove(mc);
                        }
                        this.MergeWorksheetCells(mc.StartRowIndex + rowdiff, mc.StartColumnIndex + coldiff, mc.EndRowIndex + rowdiff, mc.EndColumnIndex + coldiff);
                    }
                }

                int i = 0;

                #region Calculation cells
                if (slwb.CalculationCells.Count > 0)
                {
                    List<int> listToDelete = new List<int>();
                    int iRowIndex = -1;
                    int iColumnIndex = -1;
                    int iRowLimit = AnchorRowIndex + iStartRowIndex - iEndRowIndex;
                    int iColumnLimit = AnchorColumnIndex + iStartColumnIndex - iEndColumnIndex;
                    for (i = 0; i < slwb.CalculationCells.Count; ++i)
                    {
                        if (slwb.CalculationCells[i].SheetId == giSelectedWorksheetID)
                        {
                            iRowIndex = slwb.CalculationCells[i].RowIndex;
                            iColumnIndex = slwb.CalculationCells[i].ColumnIndex;
                            if (ToCut && iRowIndex >= iStartRowIndex && iRowIndex <= iEndRowIndex
                                    && iColumnIndex >= iStartColumnIndex && iColumnIndex <= iEndColumnIndex)
                            {
                                // just remove because recalculation of cell references is too complicated...
                                if (!listToDelete.Contains(i)) listToDelete.Add(i);
                            }

                            if (iRowIndex >= AnchorRowIndex && iRowIndex <= iRowLimit
                                && iColumnIndex >= AnchorColumnIndex && iColumnIndex <= iColumnLimit)
                            {
                                // existing calculation cell lies within destination "paste" operation
                                if (!listToDelete.Contains(i)) listToDelete.Add(i);
                            }
                        }
                    }

                    for (i = listToDelete.Count - 1; i >= 0; --i)
                    {
                        slwb.CalculationCells.RemoveAt(listToDelete[i]);
                    }
                }
                #endregion

                // defined names is hard to calculate...
                // need to check the row and column indices based on the cell references within.
            }

            return result;
        }

        /// <summary>
        /// Clear all cell content in the worksheet.
        /// </summary>
        /// <returns>True if content has been cleared. False otherwise. If there are no content in the worksheet, false is also returned.</returns>
        public bool ClearCellContent()
        {
            bool result = false;
            List<SLCellPoint> list = slws.Cells.Keys.ToList<SLCellPoint>();
            if (list.Count > 0) result = true;
            foreach (SLCellPoint pt in list)
            {
                this.ClearCellContentData(pt);
            }

            return result;
        }

        /// <summary>
        /// Clear all cell content within specified rows and columns. If the top-left cell of a merged cell is within specified rows and columns, the merged cell content is also cleared.
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the start cell of the cell range to be cleared, such as "A1". This is typically the top-left cell.</param>
        /// <param name="EndCellReference">The cell reference of the end cell of the cell range to be cleared, such as "A1". This is typically the bottom-right cell.</param>
        /// <returns>True if content has been cleared. False otherwise. If there are no content within specified rows and columns, false is also returned.</returns>
        public bool ClearCellContent(string StartCellReference, string EndCellReference)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex)
                || !SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex))
            {
                return false;
            }

            return ClearCellContent(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Clear all cell content within specified rows and columns. If the top-left cell of a merged cell is within specified rows and columns, the merged cell content is also cleared.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the start row. This is typically the top row.</param>
        /// <param name="StartColumnIndex">The column index of the start column. This is typically the left-most column.</param>
        /// <param name="EndRowIndex">The row index of the end row. This is typically the bottom row.</param>
        /// <param name="EndColumnIndex">The column index of the end column. This is typically the right-most column.</param>
        /// <returns>True if content has been cleared. False otherwise. If there are no content within specified rows and columns, false is also returned.</returns>
        public bool ClearCellContent(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1, iStartColumnIndex = 1, iEndColumnIndex = 1;
            bool result = false;
            if (StartRowIndex < EndRowIndex)
            {
                iStartRowIndex = StartRowIndex;
                iEndRowIndex = EndRowIndex;
            }
            else
            {
                iStartRowIndex = EndRowIndex;
                iEndRowIndex = StartRowIndex;
            }

            if (StartColumnIndex < EndColumnIndex)
            {
                iStartColumnIndex = StartColumnIndex;
                iEndColumnIndex = EndColumnIndex;
            }
            else
            {
                iStartColumnIndex = EndColumnIndex;
                iEndColumnIndex = StartColumnIndex;
            }

            if (iStartRowIndex < 1) iStartRowIndex = 1;
            if (iEndRowIndex > SLConstants.RowLimit) iEndRowIndex = SLConstants.RowLimit;
            if (iStartColumnIndex < 1) iStartColumnIndex = 1;
            if (iEndColumnIndex > SLConstants.ColumnLimit) iEndColumnIndex = SLConstants.ColumnLimit;

            long iSize = (iEndRowIndex - iStartRowIndex + 1) * (iEndColumnIndex - iStartColumnIndex + 1);

            int iRowIndex = -1, iColumnIndex = -1;
            if (iSize <= (long)slws.Cells.Count)
            {
                SLCellPoint pt;
                for (iRowIndex = iStartRowIndex; iRowIndex <= iEndRowIndex; ++iRowIndex)
                {
                    for (iColumnIndex = iStartColumnIndex; iColumnIndex <= iEndColumnIndex; ++iColumnIndex)
                    {
                        pt = new SLCellPoint(iRowIndex, iColumnIndex);
                        if (slws.Cells.ContainsKey(pt))
                        {
                            this.ClearCellContentData(pt);
                            result = true;
                        }
                    }
                }
            }
            else
            {
                foreach (SLCellPoint pt in slws.Cells.Keys)
                {
                    if (iStartRowIndex <= pt.RowIndex && pt.RowIndex <= iEndRowIndex && iStartColumnIndex <= pt.ColumnIndex && pt.ColumnIndex <= iEndColumnIndex)
                    {
                        this.ClearCellContentData(pt);
                        result = true;
                    }
                }
            }

            return result;
        }

        private void ClearCellContentData(SLCellPoint pt)
        {
            SLCell c = slws.Cells[pt];
            c.CellFormula = null;
            c.DataType = CellValues.Number;
            c.NumericValue = 0;
            // if the cell still has attributes (say the style index), then update it
            // otherwise remove the cell
            if (c.StyleIndex != 0 || c.CellMetaIndex != 0 || c.ValueMetaIndex != 0 || c.ShowPhonetic != false)
            {
                slws.Cells[pt] = c.Clone();
            }
            else
            {
                slws.Cells.Remove(pt);
            }
        }

        /// <summary>
        /// A negative StartRowIndex skips sections of row manipulations.
        /// A negative StartColumnIndex skips sections of column manipulations.
        /// RowDelta and ColumnDelta can be positive or negative
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="StartRowIndex"></param>
        /// <param name="RowDelta"></param>
        /// <param name="StartColumnIndex"></param>
        /// <param name="ColumnDelta"></param>
        internal void ProcessCellFormulaDelta(ref SLCell cell, int StartRowIndex, int RowDelta, int StartColumnIndex, int ColumnDelta)
        {
            if (cell.CellText != null && cell.CellText.StartsWith("="))
            {
                cell.CellText = AddDeleteCellFormulaDelta(cell.CellText, StartRowIndex, RowDelta, StartColumnIndex, ColumnDelta);
            }
            if (cell.CellFormula != null && cell.CellFormula.FormulaType == CellFormulaValues.Normal)
            {
                cell.CellFormula.FormulaText = AddDeleteCellFormulaDelta(cell.CellFormula.FormulaText, StartRowIndex, RowDelta, StartColumnIndex, ColumnDelta);
                // because we don't know how to calculate formulas yet
                cell.CellText = string.Empty;
            }
        }

        /// <summary>
        /// A negative StartRowIndex skips sections of row manipulations.
        /// A negative StartColumnIndex skips sections of column manipulations.
        /// RowDelta and ColumnDelta can be positive or negative
        /// </summary>
        /// <param name="CellFormula"></param>
        /// <param name="StartRowIndex"></param>
        /// <param name="RowDelta"></param>
        /// <param name="StartColumnIndex"></param>
        /// <param name="ColumnDelta"></param>
        /// <returns></returns>
        internal string AddDeleteCellFormulaDelta(string CellFormula, int StartRowIndex, int RowDelta, int StartColumnIndex, int ColumnDelta)
        {
            string result = string.Empty;
            string sToCheck = CellFormula;
            string sSheetNameRegex = string.Format("({0}!|'{0}'!)?", gsSelectedWorksheetName);
            // This captures A1, A1:B3, Sheet1!A1, 'Sheet1'!A1, B2:Sheet1!C4, Sheet1!B2:C4 and so on.
            // Basically it captures single cell references (A1) and cell ranges (A1:B3).
            // It also captures the worksheet name too.
            // We use the selected worksheet name in the regex because we're only interested in
            // modifying any cell references/ranges on the selected worksheet.
            // This automatically limit the regex matches to those we want.
            // Note that we only care for the ":" as the range character. Apparently, Excel
            // accepts A1.B2 as a valid range, but auto-corrects it to A1:B2 immediately.
            // Otherwise, we could use \s*[:.]\s* and then we have to handle the case with
            // the period . as the range character in the post-processing.
            string sCellRefRegex = @"(?<cellref>" + sSheetNameRegex + @"\$?[a-zA-Z]{1,3}\$?[0-9]{1,7}(\s*:\s*" + sSheetNameRegex + @"\$?[a-zA-Z]{1,3}\$?[0-9]{1,7})?)";
            // The only characters that can be before a valid cell reference are +-*/^=<>,( and the space.
            // The cell reference can also be at the start of the string, thus the ^
            // The only characters that can be after a valid cell reference are +-*/^=<>,) and the space.
            // The cell reference can also be at the end of the string, thus the $
            string sRegexCheck = @"(?<cellrefpre>^|[+\-*/^=<>,(]|\s)" + sCellRefRegex + @"(?<cellrefpost>[+\-*/^=<>,)]|\s|$)";
            int index = 0;
            int iDoubleQuoteCount = 0;
            Match m;
            m = Regex.Match(sToCheck, sRegexCheck);
            while (m.Success)
            {
                index = sToCheck.IndexOf(m.Value);
                result += sToCheck.Substring(0, index) + m.Groups["cellrefpre"].Value;
                sToCheck = sToCheck.Substring(index + m.Value.Length);

                iDoubleQuoteCount = result.Length - result.Replace("\"", "").Length;
                // This checks if there's a matching pair of double quotes.
                // If there's an odd number of double quotes, then the matched
                // value is behind a double quote, and hence should be taken
                // as a literal string.
                if (iDoubleQuoteCount % 2 == 0)
                {
                    result += AddDeleteCellReferenceDelta(m.Groups["cellref"].Value, StartRowIndex, RowDelta, StartColumnIndex, ColumnDelta);
                }
                else
                {
                    result += m.Groups["cellref"].Value;
                }
                result += m.Groups["cellrefpost"].Value;

                m = Regex.Match(sToCheck, sRegexCheck);
            }
            result += sToCheck;

            return result;
        }

        /// <summary>
        /// This closely follows the logic of AddDeleteCellFormulaDelta()
        /// Delta can be positive or negative.
        /// </summary>
        /// <param name="DefinedNameValue"></param>
        /// <param name="CheckForRow"></param>
        /// <param name="StartRange"></param>
        /// <param name="Delta"></param>
        /// <returns></returns>
        internal string AddDeleteDefinedNameRowColumnRangeDelta(string DefinedNameValue, bool CheckForRow, int StartRange, int Delta)
        {
            string result = string.Empty;
            string sToCheck = DefinedNameValue;
            string sSheetNameRegex = string.Format("({0}!|'{0}'!)?", gsSelectedWorksheetName);
            // We want to capture strings such as Sheet1!$B:$D or Sheet1!$3:$9
            // In this case, we only care about the $ for the "absolute-ness"
            // While Sheet1!3:5 may be a valid defined name value (I don't know...),
            // we will ignore that because it's a relative reference.
            string sCellRefRegex;
            if (CheckForRow)
            {
                sCellRefRegex = @"(?<cellref>" + sSheetNameRegex + @"\$[0-9]{1,7}\s*:\s*" + sSheetNameRegex + @"\$[0-9]{1,7})";
            }
            else
            {
                sCellRefRegex = @"(?<cellref>" + sSheetNameRegex + @"\$[a-zA-Z]{1,3}\s*:\s*" + sSheetNameRegex + @"\$[a-zA-Z]{1,3})";
            }
            // The only characters that can be before a valid cell reference are +-*/^=<>,( and the space.
            // The cell reference can also be at the start of the string, thus the ^
            // The only characters that can be after a valid cell reference are +-*/^=<>,) and the space.
            // The cell reference can also be at the end of the string, thus the $
            string sRegexCheck = @"(?<cellrefpre>^|[+\-*/^=<>,(]|\s)" + sCellRefRegex + @"(?<cellrefpost>[+\-*/^=<>,)]|\s|$)";
            int index = 0;
            int iDoubleQuoteCount = 0;
            Match m;
            m = Regex.Match(sToCheck, sRegexCheck);
            while (m.Success)
            {
                index = sToCheck.IndexOf(m.Value);
                result += sToCheck.Substring(0, index) + m.Groups["cellrefpre"].Value;
                sToCheck = sToCheck.Substring(index + m.Value.Length);

                iDoubleQuoteCount = result.Length - result.Replace("\"", "").Length;
                // This checks if there's a matching pair of double quotes.
                // If there's an odd number of double quotes, then the matched
                // value is behind a double quote, and hence should be taken
                // as a literal string.
                if (iDoubleQuoteCount % 2 == 0)
                {
                    result += AddDeleteRowColumnRangeDelta(m.Groups["cellref"].Value, CheckForRow, StartRange, Delta);
                }
                else
                {
                    result += m.Groups["cellref"].Value;
                }
                result += m.Groups["cellrefpost"].Value;

                m = Regex.Match(sToCheck, sRegexCheck);
            }
            result += sToCheck;

            return result;
        }

        /// <summary>
        /// Delta can be positive or negative
        /// </summary>
        /// <param name="Range"></param>
        /// <param name="CheckForRow"></param>
        /// <param name="StartRange"></param>
        /// <param name="Delta"></param>
        /// <returns></returns>
        internal string AddDeleteRowColumnRangeDelta(string Range, bool CheckForRow, int StartRange, int Delta)
        {
            string result = string.Empty;
            string sSheetName = string.Empty, sSheetName2 = string.Empty;
            string sRef1 = string.Empty, sRef2 = string.Empty;
            int iRowIndex = -1, iColumnIndex = -1;
            int iRowIndex2 = -1, iColumnIndex2 = -1;
            int iEndRange = -1;
            int index = 0;
            index = Range.LastIndexOf(":");
            if (index < 0)
            {
                // this case shouldn't happen...
                result = Range;
            }
            else
            {
                sSheetName = Range.Substring(0, index).Trim();
                sSheetName2 = Range.Substring(index + 1).Trim();

                index = sSheetName.LastIndexOf("!");
                if (index < 0)
                {
                    sRef1 = sSheetName.Replace("$", "").Trim();
                    sSheetName = string.Empty;
                }
                else
                {
                    sRef1 = sSheetName.Substring(index + 1).Replace("$", "").Trim();
                    sSheetName = sSheetName.Substring(0, index + 1);
                }

                index = sSheetName2.LastIndexOf("!");
                if (index < 0)
                {
                    sRef2 = sSheetName2.Replace("$", "").Trim();
                    sSheetName2 = string.Empty;
                }
                else
                {
                    sRef2 = sSheetName2.Substring(index + 1).Replace("$", "").Trim();
                    sSheetName2 = sSheetName2.Substring(0, index + 1);
                }

                if (Delta >= 0)
                {
                    iEndRange = StartRange + Delta;
                }
                else
                {
                    iEndRange = StartRange - Delta - 1;
                }

                if (CheckForRow)
                {
                    if (int.TryParse(sRef1, out iRowIndex) && int.TryParse(sRef2, out iRowIndex2))
                    {
                        if (Delta >= 0)
                        {
                            AddRowColumnIndexDelta(StartRange, Delta, true, ref iRowIndex, ref iRowIndex2);
                        }
                        else
                        {
                            if (StartRange <= iRowIndex && iRowIndex2 <= iEndRange)
                            {
                                iRowIndex = -1;
                                iRowIndex2 = -1;
                            }
                            else
                            {
                                DeleteRowColumnIndexDelta(StartRange, iEndRange, -Delta, ref iRowIndex, ref iRowIndex2);
                            }
                        }
                    }
                    else
                    {
                        iRowIndex = -1;
                        iRowIndex2 = -1;
                    }

                    if (iRowIndex < 1 || iRowIndex > SLConstants.RowLimit || iRowIndex2 < 1 || iRowIndex2 > SLConstants.RowLimit)
                    {
                        result = sSheetName + "#REF!";
                    }
                    else
                    {
                        result = string.Format("{0}${1}:{2}${3}", sSheetName, iRowIndex.ToString(CultureInfo.InvariantCulture), sSheetName2, iRowIndex2.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    iColumnIndex = SLTool.ToColumnIndex(sRef1);
                    iColumnIndex2 = SLTool.ToColumnIndex(sRef2);
                    if (iColumnIndex > 0 && iColumnIndex2 > 0)
                    {
                        if (Delta >= 0)
                        {
                            AddRowColumnIndexDelta(StartRange, Delta, false, ref iColumnIndex, ref iColumnIndex2);
                        }
                        else
                        {
                            if (StartRange <= iColumnIndex && iColumnIndex2 <= iEndRange)
                            {
                                iColumnIndex = -1;
                                iColumnIndex2 = -1;
                            }
                            else
                            {
                                DeleteRowColumnIndexDelta(StartRange, iEndRange, -Delta, ref iColumnIndex, ref iColumnIndex2);
                            }
                        }
                    }

                    if (iColumnIndex < 1 || iColumnIndex > SLConstants.ColumnLimit || iColumnIndex2 < 1 || iColumnIndex2 > SLConstants.ColumnLimit)
                    {
                        result = sSheetName + "#REF!";
                    }
                    else
                    {
                        result = string.Format("{0}${1}:{2}${3}", sSheetName, SLTool.ToColumnName(iColumnIndex), sSheetName2, SLTool.ToColumnName(iColumnIndex2));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// A negative StartRowIndex skips sections of row manipulations.
        /// A negative StartColumnIndex skips sections of column manipulations.
        /// RowDelta and ColumnDelta can be positive or negative.
        /// </summary>
        /// <param name="CellReference"></param>
        /// <param name="StartRowIndex"></param>
        /// <param name="RowDelta"></param>
        /// <param name="StartColumnIndex"></param>
        /// <param name="ColumnDelta"></param>
        /// <returns></returns>
        internal string AddDeleteCellReferenceDelta(string CellReference, int StartRowIndex, int RowDelta, int StartColumnIndex, int ColumnDelta)
        {
            string result = string.Empty;
            string sSheetName = string.Empty, sSheetName2 = string.Empty;
            string sCellRef = string.Empty, sCellRef2 = string.Empty;
            bool bIsRange = false;
            int index = 0;
            index = CellReference.LastIndexOf(":");
            if (index < 0)
            {
                bIsRange = false;
                index = CellReference.LastIndexOf("!");
                if (index < 0)
                {
                    sSheetName = string.Empty;
                    sCellRef = CellReference.Trim();
                }
                else
                {
                    sSheetName = CellReference.Substring(0, index).Trim() + "!";
                    sCellRef = CellReference.Substring(index + 1).Trim();
                }
                sSheetName2 = string.Empty;
                sCellRef2 = string.Empty;
            }
            else
            {
                bIsRange = true;
                sCellRef = CellReference.Substring(0, index);
                sCellRef2 = CellReference.Substring(index + 1);

                index = sCellRef.LastIndexOf("!");
                if (index < 0)
                {
                    sSheetName = string.Empty;
                    sCellRef = sCellRef.Trim();
                }
                else
                {
                    sSheetName = sCellRef.Substring(0, index).Trim() + "!";
                    sCellRef = sCellRef.Substring(index + 1).Trim();
                }

                index = sCellRef2.LastIndexOf("!");
                if (index < 0)
                {
                    sSheetName2 = string.Empty;
                    sCellRef2 = sCellRef2.Trim();
                }
                else
                {
                    sSheetName2 = sCellRef2.Substring(0, index).Trim() + "!";
                    sCellRef2 = sCellRef2.Substring(index + 1).Trim();
                }
            }

            bool bIsRowAbsolute = Regex.IsMatch(sCellRef, @"\$[0-9]{1,7}");
            bool bIsColumnAbsolute = Regex.IsMatch(sCellRef, @"\$[a-zA-Z]{1,3}");
            bool bIsRowAbsolute2 = false, bIsColumnAbsolute2 = false;
            sCellRef = sCellRef.Replace("$", "");
            if (bIsRange)
            {
                bIsRowAbsolute2 = Regex.IsMatch(sCellRef2, @"\$[0-9]{1,7}");
                bIsColumnAbsolute2 = Regex.IsMatch(sCellRef2, @"\$[a-zA-Z]{1,3}");
                sCellRef2 = sCellRef2.Replace("$", "");
            }
            int iRowIndex = -1, iColumnIndex = -1;
            int iRowIndex2 = -1, iColumnIndex2 = -1;
            int iEndRowIndex = -1, iEndColumnIndex = -1;

            if (RowDelta >= 0)
            {
                iEndRowIndex = StartRowIndex + RowDelta;
            }
            else
            {
                iEndRowIndex = StartRowIndex - RowDelta - 1;
            }

            if (ColumnDelta >= 0)
            {
                iEndColumnIndex = StartColumnIndex + ColumnDelta;
            }
            else
            {
                iEndColumnIndex = StartColumnIndex - ColumnDelta - 1;
            }

            result = CellReference;
            if (!bIsRange)
            {
                if (SLTool.FormatCellReferenceToRowColumnIndex(sCellRef, out iRowIndex, out iColumnIndex))
                {
                    if (StartRowIndex > 0)
                    {
                        if (RowDelta > 0)
                        {
                            if (iRowIndex >= StartRowIndex)
                            {
                                iRowIndex += RowDelta;
                            }
                        }
                        else
                        {
                            if (StartRowIndex <= iRowIndex && iRowIndex <= iEndRowIndex)
                            {
                                iRowIndex = -1;
                            }
                            else if (iEndRowIndex < iRowIndex)
                            {
                                // the delta is negative, so add it
                                iRowIndex += RowDelta;
                            }
                        }
                    }

                    if (StartColumnIndex > 0)
                    {
                        if (ColumnDelta > 0)
                        {
                            if (iColumnIndex >= StartColumnIndex)
                            {
                                iColumnIndex += ColumnDelta;
                            }
                        }
                        else
                        {
                            if (StartColumnIndex <= iColumnIndex && iColumnIndex <= iEndColumnIndex)
                            {
                                iColumnIndex = -1;
                            }
                            else if (iEndColumnIndex < iColumnIndex)
                            {
                                // the delta is negative, so add it
                                iColumnIndex += ColumnDelta;
                            }
                        }
                    }

                    if (iRowIndex < 1 || iRowIndex > SLConstants.RowLimit || iColumnIndex < 1 || iColumnIndex > SLConstants.ColumnLimit)
                    {
                        result = "#REF!";
                    }
                    else
                    {
                        // would the cell references be independently absolute or relative?
                        // Otherwise we'd use SLTool to form the cell reference...
                        result = sSheetName + (bIsColumnAbsolute ? "$" : "") + SLTool.ToColumnName(iColumnIndex) + (bIsRowAbsolute ? "$" : "") + iRowIndex.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            else
            {
                if (SLTool.FormatCellReferenceToRowColumnIndex(sCellRef, out iRowIndex, out iColumnIndex) && SLTool.FormatCellReferenceToRowColumnIndex(sCellRef2, out iRowIndex2, out iColumnIndex2))
                {
                    if (StartRowIndex > 0)
                    {
                        if (RowDelta > 0)
                        {
                            AddRowColumnIndexDelta(StartRowIndex, RowDelta, true, ref iRowIndex, ref iRowIndex2);
                        }
                        else
                        {
                            if (StartRowIndex <= iRowIndex && iRowIndex2 <= iEndRowIndex)
                            {
                                iRowIndex = -1;
                                iRowIndex2 = -1;
                            }
                            else
                            {
                                DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, -RowDelta, ref iRowIndex, ref iRowIndex2);
                            }
                        }
                    }

                    if (StartColumnIndex > 0)
                    {
                        if (ColumnDelta > 0)
                        {
                            AddRowColumnIndexDelta(StartColumnIndex, ColumnDelta, false, ref iColumnIndex, ref iColumnIndex2);
                        }
                        else
                        {
                            if (StartColumnIndex <= iColumnIndex && iColumnIndex2 <= iEndColumnIndex)
                            {
                                iColumnIndex = -1;
                                iColumnIndex2 = -1;
                            }
                            else
                            {
                                DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, -ColumnDelta, ref iColumnIndex, ref iColumnIndex2);
                            }
                        }
                    }

                    if (iRowIndex < 1 || iRowIndex > SLConstants.RowLimit || iColumnIndex < 1 || iColumnIndex > SLConstants.ColumnLimit || iRowIndex2 < 1 || iRowIndex2 > SLConstants.RowLimit || iColumnIndex2 < 1 || iColumnIndex2 > SLConstants.ColumnLimit)
                    {
                        result = "#REF!";
                    }
                    else
                    {
                        // would the cell references be independently absolute or relative?
                        // Otherwise we'd use SLTool to form the cell reference...
                        result = sSheetName + (bIsColumnAbsolute ? "$" : "") + SLTool.ToColumnName(iColumnIndex) + (bIsRowAbsolute ? "$" : "") + iRowIndex.ToString(CultureInfo.InvariantCulture);
                        result += ":" + sSheetName2 + (bIsColumnAbsolute2 ? "$" : "") + SLTool.ToColumnName(iColumnIndex2) + (bIsRowAbsolute2 ? "$" : "") + iRowIndex2.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            return result;
        }
    }
}
