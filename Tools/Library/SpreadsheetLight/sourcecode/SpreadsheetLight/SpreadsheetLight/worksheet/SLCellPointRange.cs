using System;

namespace SpreadsheetLight
{
    internal struct SLCellPointRange
    {
        internal int StartRowIndex;
        internal int StartColumnIndex;
        internal int EndRowIndex;
        internal int EndColumnIndex;

        internal SLCellPointRange(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
        {
            this.StartRowIndex = StartRowIndex;
            this.StartColumnIndex = StartColumnIndex;
            this.EndRowIndex = EndRowIndex;
            this.EndColumnIndex = EndColumnIndex;
        }
    }
}
