using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;

namespace SpreadsheetLight
{
    internal class SLWorksheet
    {
        internal bool ForceCustomRowColumnDimensionsSplitting { get; set; }
        
        internal List<SLSheetView> SheetViews { get; set; }

        internal SLSheetFormatProperties SheetFormatProperties { get; set; }

        internal Dictionary<int, SLRowProperties> RowProperties { get; set; }
        internal Dictionary<int, SLColumnProperties> ColumnProperties { get; set; }
        internal Dictionary<SLCellPoint, SLCell> Cells { get; set; }

        // note that this doesn't mean that the worksheet is protected,
        // just that the SheetProtection SDK class is present.
        internal bool HasSheetProtection;
        internal SLSheetProtection SheetProtection { get; set; }

        internal bool HasAutoFilter;
        internal SLAutoFilter AutoFilter { get; set; }

        internal List<SLMergeCell> MergeCells { get; set; }

        internal List<SLConditionalFormatting> ConditionalFormattings { get; set; }

        internal SLPageSettings PageSettings { get; set; }

        internal Dictionary<int, SLBreak> RowBreaks { get; set; }
        internal Dictionary<int, SLBreak> ColumnBreaks { get; set; }

        // use the reference ID of the Drawing class directly
        internal string DrawingId { get; set; }

        internal uint NextWorksheetDrawingId { get; set; }

        internal List<Drawing.SLPicture> Pictures { get; set; }

        internal List<Charts.SLChart> Charts { get; set; }

        internal bool ToAppendBackgroundPicture { get; set; }
        internal string BackgroundPictureId { get; set; }
        /// <summary>
        /// if null, then don't have to do anything
        /// </summary>
        internal bool? BackgroundPictureDataIsInFile { get; set; }
        internal string BackgroundPictureFileName { get; set; }
        internal byte[] BackgroundPictureByteData { get; set; }
        internal ImagePartType BackgroundPictureImagePartType { get; set; }

        // for cell comments
        internal string LegacyDrawingId { get; set; }
        internal List<string> Authors { get; set; }
        internal Dictionary<SLCellPoint, SLComment> Comments { get; set; }

        // if a cell has no style, but its corresponding row/column has a style,
        // the cell takes on that style. However, the last applied style takes precendence.
        // So if the row is styled, then the column is styled, the column style is used.
        // This follows Excel behaviour.
        internal List<SLRowColumnStyleHistory> RowColumnStyleHistory { get; set; }

        internal List<SLTable> Tables { get; set; }

        internal List<SLSparklineGroup> SparklineGroups { get; set; }

        internal SLWorksheet(List<System.Drawing.Color> ThemeColors, List<System.Drawing.Color> IndexedColors, double ThemeDefaultColumnWidth, long ThemeDefaultColumnWidthInEMU, int MaxDigitWidth, List<double> ColumnStepSize, double CalculatedDefaultRowHeight)
        {
            this.ForceCustomRowColumnDimensionsSplitting = false;

            this.SheetViews = new List<SLSheetView>();

            this.SheetFormatProperties = new SLSheetFormatProperties(ThemeDefaultColumnWidth, ThemeDefaultColumnWidthInEMU, MaxDigitWidth, ColumnStepSize, CalculatedDefaultRowHeight);

            this.RowProperties = new Dictionary<int, SLRowProperties>();
            this.ColumnProperties = new Dictionary<int, SLColumnProperties>();
            this.Cells = new Dictionary<SLCellPoint, SLCell>();

            this.HasSheetProtection = false;
            this.SheetProtection = new SLSheetProtection();

            this.HasAutoFilter = false;
            this.AutoFilter = new SLAutoFilter();

            this.MergeCells = new List<SLMergeCell>();

            this.ConditionalFormattings = new List<SLConditionalFormatting>();

            this.PageSettings = new SLPageSettings(ThemeColors, IndexedColors);

            this.RowBreaks = new Dictionary<int, SLBreak>();
            this.ColumnBreaks = new Dictionary<int, SLBreak>();

            this.DrawingId = string.Empty;
            this.NextWorksheetDrawingId = 2;
            this.Pictures = new List<Drawing.SLPicture>();
            this.Charts = new List<Charts.SLChart>();

            this.InitializeBackgroundPictureStuff();

            this.LegacyDrawingId = string.Empty;
            this.Authors = new List<string>();
            this.Comments = new Dictionary<SLCellPoint, SLComment>();

            this.RowColumnStyleHistory = new List<SLRowColumnStyleHistory>();

            this.Tables = new List<SLTable>();

            this.SparklineGroups = new List<SLSparklineGroup>();
        }

        internal void InitializeBackgroundPictureStuff()
        {
            this.BackgroundPictureId = string.Empty;
            this.BackgroundPictureDataIsInFile = null;
            this.BackgroundPictureFileName = string.Empty;
            this.BackgroundPictureByteData = new byte[1];
            this.BackgroundPictureImagePartType = ImagePartType.Bmp;
        }

        internal void ToggleCustomRowColumnDimension(bool IsCustom)
        {
            this.SheetFormatProperties.HasDefaultColumnWidth = IsCustom;
            if (IsCustom)
            {
                this.SheetFormatProperties.CustomHeight = IsCustom;
            }
            else
            {
                // default is false
                this.SheetFormatProperties.CustomHeight = null;
            }
        }

        internal uint GetExistingRowColumnStyle(int RowIndex, int ColumnIndex)
        {
            int i;
            bool bFound = false;
            bool IsRow = true;
            for (i = this.RowColumnStyleHistory.Count - 1; i >= 0; --i)
            {
                if (this.RowColumnStyleHistory[i].IsRow)
                {
                    if (this.RowColumnStyleHistory[i].Index == RowIndex)
                    {
                        bFound = true;
                        IsRow = true;
                        break;
                    }
                }
                else
                {
                    if (this.RowColumnStyleHistory[i].Index == ColumnIndex)
                    {
                        bFound = true;
                        IsRow = false;
                        break;
                    }
                }
            }

            uint iStyleIndex = 0;
            if (bFound)
            {
                if (IsRow)
                {
                    if (this.RowProperties.ContainsKey(RowIndex))
                    {
                        iStyleIndex = this.RowProperties[RowIndex].StyleIndex;
                    }
                }
                else
                {
                    if (this.ColumnProperties.ContainsKey(ColumnIndex))
                    {
                        iStyleIndex = this.ColumnProperties[ColumnIndex].StyleIndex;
                    }
                }
            }

            return iStyleIndex;
        }

        internal void RemoveRowColumnStyleHistory(bool IsRow, int StartIndex, int EndIndex)
        {
            for (int i = this.RowColumnStyleHistory.Count - 1; i >= 0; --i)
            {
                if (this.RowColumnStyleHistory[i].IsRow == IsRow)
                {
                    if (StartIndex <= this.RowColumnStyleHistory[i].Index
                        && this.RowColumnStyleHistory[i].Index <= EndIndex)
                    {
                        this.RowColumnStyleHistory.RemoveAt(i);
                    }
                }
            }
        }

        internal void RefreshSparklineGroups()
        {
            for (int i = this.SparklineGroups.Count - 1; i >= 0; --i)
            {
                // in case the group has no sparklines
                if (this.SparklineGroups[i].Sparklines.Count == 0)
                {
                    this.SparklineGroups.RemoveAt(i);
                }
            }
        }
    }
}
