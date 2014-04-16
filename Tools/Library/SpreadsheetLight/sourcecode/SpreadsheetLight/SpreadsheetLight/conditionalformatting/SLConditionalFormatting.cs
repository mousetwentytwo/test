using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    /// <summary>
    /// Built-in data bar types.
    /// </summary>
    public enum SLConditionalFormatDataBarValues
    {
        /// <summary>
        /// Blue data bar
        /// </summary>
        Blue = 0,
        /// <summary>
        /// Green data bar
        /// </summary>
        Green,
        /// <summary>
        /// Red data bar
        /// </summary>
        Red,
        /// <summary>
        /// Orange data bar
        /// </summary>
        Orange,
        /// <summary>
        /// Light blue data bar
        /// </summary>
        LightBlue,
        /// <summary>
        /// Purple data bar
        /// </summary>
        Purple
    }

    /// <summary>
    /// Built-in color scale types.
    /// </summary>
    public enum SLConditionalFormatColorScaleValues
    {
        /// <summary>
        /// Green - Yellow - Red color scale
        /// </summary>
        GreenYellowRed = 0,
        /// <summary>
        /// Red - Yellow - Green color scale
        /// </summary>
        RedYellowGreen,
        /// <summary>
        /// Blue - Yellow - Red color scale
        /// </summary>
        BlueYellowRed,
        /// <summary>
        /// Red - Yellow - Blue color scale
        /// </summary>
        RedYellowBlue,
        /// <summary>
        /// Yellow - Red color scale
        /// </summary>
        YellowRed,
        /// <summary>
        /// Red - Yellow color scale
        /// </summary>
        RedYellow,
        /// <summary>
        /// Green - Yellow color scale
        /// </summary>
        GreenYellow,
        /// <summary>
        /// Yellow - Green color scale
        /// </summary>
        YellowGreen
    }

    /// <summary>
    /// Conditional format type including minimum and maximum types.
    /// </summary>
    public enum SLConditionalFormatMinMaxValues
    {
        /// <summary>
        /// The underlying engine will assign a minimum or maximum depending on the parameter this value is used on.
        /// </summary>
        Value = 0,
        /// <summary>
        /// Number
        /// </summary>
        Number,
        /// <summary>
        /// Percent
        /// </summary>
        Percent,
        /// <summary>
        /// Formula
        /// </summary>
        Formula,
        /// <summary>
        /// Percentile
        /// </summary>
        Percentile
    }

    /// <summary>
    /// Conditional format type excluding minimum and maximum types.
    /// </summary>
    public enum SLConditionalFormatRangeValues
    {
        /// <summary>
        /// Number
        /// </summary>
        Number = 0,
        /// <summary>
        /// Percent
        /// </summary>
        Percent,
        /// <summary>
        /// Formula
        /// </summary>
        Formula,
        /// <summary>
        /// Percentile
        /// </summary>
        Percentile
    }

    /// <summary>
    /// Icon set type for five icons.
    /// </summary>
    public enum SLFiveIconSetValues
    {
        /// <summary>
        /// 5 arrows
        /// </summary>
        FiveArrows = 0,
        /// <summary>
        /// 5 arrows (gray)
        /// </summary>
        FiveArrowsGray,
        /// <summary>
        /// 5 quarters
        /// </summary>
        FiveQuarters,
        /// <summary>
        /// 5 ratings
        /// </summary>
        FiveRating
    }

    /// <summary>
    /// Icon set type for four icons.
    /// </summary>
    public enum SLFourIconSetValues
    {
        /// <summary>
        /// 4 arrows
        /// </summary>
        FourArrows = 0,
        /// <summary>
        /// 4 arrows (gray)
        /// </summary>
        FourArrowsGray,
        /// <summary>
        /// 4 ratings
        /// </summary>
        FourRating,
        /// <summary>
        /// 4 red To black
        /// </summary>
        FourRedToBlack,
        /// <summary>
        /// 4 traffic lights
        /// </summary>
        FourTrafficLights
    }

    /// <summary>
    /// Icon set type for three icons.
    /// </summary>
    public enum SLThreeIconSetValues
    {
        /// <summary>
        /// 3 arrows
        /// </summary>
        ThreeArrows = 0,
        /// <summary>
        /// 3 arrows (gray)
        /// </summary>
        ThreeArrowsGray,
        /// <summary>
        /// 3 flags
        /// </summary>
        ThreeFlags,
        /// <summary>
        /// 3 signs
        /// </summary>
        ThreeSigns,
        /// <summary>
        /// 3 symbols circled
        /// </summary>
        ThreeSymbols,
        /// <summary>
        /// 3 symbols
        /// </summary>
        ThreeSymbols2,
        /// <summary>
        /// 3 traffic lights
        /// </summary>
        ThreeTrafficLights1,
        /// <summary>
        /// 3 traffic lights black
        /// </summary>
        ThreeTrafficLights2
    }

    /// <summary>
    /// Built-in cell highlighting styles
    /// </summary>
    public enum SLHighlightCellsStyleValues
    {
        /// <summary>
        /// Light red background fill with dark red text
        /// </summary>
        LightRedFillWithDarkRedText = 0,
        /// <summary>
        /// Yellow background fill with dark yellow text
        /// </summary>
        YellowFillWithDarkYellowText,
        /// <summary>
        /// Green background fill with dark green text
        /// </summary>
        GreenFillWithDarkGreenText,
        /// <summary>
        /// Light red background fill
        /// </summary>
        LightRedFill,
        /// <summary>
        /// Red text
        /// </summary>
        RedText,
        /// <summary>
        /// Red borders
        /// </summary>
        RedBorder
    }

    /// <summary>
    /// Options on the average value for the selected range.
    /// </summary>
    public enum SLHighlightCellsAboveAverageValues
    {
        /// <summary>
        /// Above the average
        /// </summary>
        Above = 0,
        /// <summary>
        /// Below the average
        /// </summary>
        Below,
        /// <summary>
        /// Equal to or above the average
        /// </summary>
        EqualOrAbove,
        /// <summary>
        /// Equal to or below the average
        /// </summary>
        EqualOrBelow,
        /// <summary>
        /// 1 standard deviation above the average
        /// </summary>
        OneStdDevAbove,
        /// <summary>
        /// 1 standard deviation below the average
        /// </summary>
        OneStdDevBelow,
        /// <summary>
        /// 2 standard deviations above the average
        /// </summary>
        TwoStdDevAbove,
        /// <summary>
        /// 2 standard deviations below the average
        /// </summary>
        TwoStdDevBelow,
        /// <summary>
        /// 3 standard deviations above the average
        /// </summary>
        ThreeStdDevAbove,
        /// <summary>
        /// 3 standard deviations below the average
        /// </summary>
        ThreeStdDevBelow
    }

    /// <summary>
    /// Encapsulates properties and methods for conditional formatting. This simulates the DocumentFormat.OpenXml.Spreadsheet.ConditionalFormatting class.
    /// </summary>
    public class SLConditionalFormatting
    {
        // Conditional formatting doesn't need the theme or indexed colours.

        internal List<SLConditionalFormattingRule> Rules { get; set; }
        internal bool Pivot { get; set; }
        internal List<SLCellPointRange> SequenceOfReferences { get; set; }

        /// <summary>
        /// Initializes an instance of SLConditionalFormatting, given cell references of opposite cells in a cell range.
        /// </summary>
        /// <param name="StartCellReference">The cell reference of the start cell of the cell range to be conditionally formatted, such as "A1". This is typically the top-left cell.</param>
        /// <param name="EndCellReference">The cell reference of the end cell of the cell range to be conditionally formatted, such as "A1". This is typically the bottom-right cell.</param>
        public SLConditionalFormatting(string StartCellReference, string EndCellReference)
        {
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            if (!SLTool.FormatCellReferenceToRowColumnIndex(StartCellReference, out iStartRowIndex, out iStartColumnIndex))
            {
                iStartRowIndex = -1;
                iStartColumnIndex = -1;
            }
            if (!SLTool.FormatCellReferenceToRowColumnIndex(EndCellReference, out iEndRowIndex, out iEndColumnIndex))
            {
                iEndRowIndex = -1;
                iEndColumnIndex = -1;
            }

            this.InitialiseNewConditionalFormatting(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Initializes an instance of SLConditionalFormatting, given row and column indices of opposite cells in a cell range.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the start row. This is typically the top row.</param>
        /// <param name="StartColumnIndex">The column index of the start column. This is typically the left-most column.</param>
        /// <param name="EndRowIndex">The row index of the end row. This is typically the bottom row.</param>
        /// <param name="EndColumnIndex">The column index of the end column. This is typically the right-most column.</param>
        public SLConditionalFormatting(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
        {
            this.InitialiseNewConditionalFormatting(StartRowIndex, StartColumnIndex, EndRowIndex, EndColumnIndex);
        }

        internal SLConditionalFormatting()
        {
            this.SetAllNull();
        }

        private void InitialiseNewConditionalFormatting(int StartRowIndex, int StartColumnIndex, int EndRowIndex, int EndColumnIndex)
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

            if (iStartRowIndex < 1) iStartRowIndex = 1;
            if (iStartColumnIndex < 1) iStartColumnIndex = 1;
            if (iEndRowIndex > SLConstants.RowLimit) iEndRowIndex = SLConstants.RowLimit;
            if (iEndColumnIndex > SLConstants.ColumnLimit) iEndColumnIndex = SLConstants.ColumnLimit;

            this.SetAllNull();

            this.SequenceOfReferences.Add(new SLCellPointRange(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex));
        }

        private void SetAllNull()
        {
            this.Rules = new List<SLConditionalFormattingRule>();
            this.Pivot = false;
            this.SequenceOfReferences = new List<SLCellPointRange>();
        }

        private void AppendRule(SLConditionalFormattingRule cfr)
        {
            //WRONG
/*            if (this.Rules.Count > 0)
            {
                int index = this.Rules.Count - 1;
                // This follows the Excel behaviour.
                // If the last rule is of the same type, then the last rule
                // is overwritten with the newly given rule.
                if (this.Rules[index].Type == cfr.Type)
                {
                    this.Rules[index] = cfr;
                }
                else
                {
                    this.Rules.Add(cfr);
                }
            }
            else */
            {
                this.Rules.Add(cfr);
            }
        }

        /// <summary>
        /// Set a data bar formatting with built-in types.
        /// </summary>
        /// <param name="DataBar">A built-in data bar type.</param>
        public void SetDataBar(SLConditionalFormatDataBarValues DataBar)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.DataBar;
            cfr.DataBar.Cfvo1.Type = ConditionalFormatValueObjectValues.Min;
            cfr.DataBar.Cfvo1.Val = "0";
            cfr.DataBar.Cfvo2.Type = ConditionalFormatValueObjectValues.Max;
            cfr.DataBar.Cfvo2.Val = "0";

            switch (DataBar)
            {
                case SLConditionalFormatDataBarValues.Blue:
                    cfr.DataBar.Color.Color = System.Drawing.Color.FromArgb(0xFF, 0x63, 0x8E, 0xC6);
                    break;
                case SLConditionalFormatDataBarValues.Green:
                    cfr.DataBar.Color.Color = System.Drawing.Color.FromArgb(0xFF, 0x63, 0xC3, 0x84);
                    break;
                case SLConditionalFormatDataBarValues.Red:
                    cfr.DataBar.Color.Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x55, 0x5A);
                    break;
                case SLConditionalFormatDataBarValues.Orange:
                    cfr.DataBar.Color.Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xB6, 0x28);
                    break;
                case SLConditionalFormatDataBarValues.LightBlue:
                    cfr.DataBar.Color.Color = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x8A, 0xEF);
                    break;
                case SLConditionalFormatDataBarValues.Purple:
                    cfr.DataBar.Color.Color = System.Drawing.Color.FromArgb(0xFF, 0xD6, 0x00, 0x7B);
                    break;
            }

            cfr.HasDataBar = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set a custom data bar formatting.
        /// </summary>
        /// <param name="ShowBarOnly">True to show only the data bar. False to show both data bar and value.</param>
        /// <param name="MinLength">The minimum length of the data bar as a percentage of the cell width. The default value is 10.</param>
        /// <param name="MaxLength">The maximum length of the data bar as a percentage of the cell width. The default value is 90.</param>
        /// <param name="ShortestBarType">The conditional format type for the shortest bar.</param>
        /// <param name="ShortestBarValue">The value for the shortest bar. If <paramref name="ShortestBarType"/> is Value, you can just set this to "0".</param>
        /// <param name="LongestBarType">The conditional format type for the longest bar.</param>
        /// <param name="LongestBarValue">The value for the longest bar. If <paramref name="LongestBarType"/> is Value, you can just set this to "0".</param>
        /// <param name="BarColor">The color of the data bar.</param>
        public void SetCustomDataBar(bool ShowBarOnly, uint MinLength, uint MaxLength, SLConditionalFormatMinMaxValues ShortestBarType, string ShortestBarValue, SLConditionalFormatMinMaxValues LongestBarType, string LongestBarValue, System.Drawing.Color BarColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor clr = new SLColor(listempty, listempty);
            clr.Color = BarColor;
            this.SetCustomDataBar(ShowBarOnly, MinLength, MaxLength, ShortestBarType, ShortestBarValue, LongestBarType, LongestBarValue, clr);
        }

        /// <summary>
        /// Set a custom data bar formatting.
        /// </summary>
        /// <param name="ShowBarOnly">True to show only the data bar. False to show both data bar and value.</param>
        /// <param name="MinLength">The minimum length of the data bar as a percentage of the cell width. The default value is 10.</param>
        /// <param name="MaxLength">The maximum length of the data bar as a percentage of the cell width. The default value is 90.</param>
        /// <param name="ShortestBarType">The conditional format type for the shortest bar.</param>
        /// <param name="ShortestBarValue">The value for the shortest bar. If <paramref name="ShortestBarType"/> is Value, you can just set this to "0".</param>
        /// <param name="LongestBarType">The conditional format type for the longest bar.</param>
        /// <param name="LongestBarValue">The value for the longest bar. If <paramref name="LongestBarType"/> is Value, you can just set this to "0".</param>
        /// <param name="BarColor">The theme color to be used for the data bar.</param>
        public void SetCustomDataBar(bool ShowBarOnly, uint MinLength, uint MaxLength, SLConditionalFormatMinMaxValues ShortestBarType, string ShortestBarValue, SLConditionalFormatMinMaxValues LongestBarType, string LongestBarValue, SLThemeColorIndexValues BarColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor clr = new SLColor(listempty, listempty);
            clr.SetThemeColor(BarColor);
            this.SetCustomDataBar(ShowBarOnly, MinLength, MaxLength, ShortestBarType, ShortestBarValue, LongestBarType, LongestBarValue, clr);
        }

        /// <summary>
        /// Set a custom data bar formatting.
        /// </summary>
        /// <param name="ShowBarOnly">True to show only the data bar. False to show both data bar and value.</param>
        /// <param name="MinLength">The minimum length of the data bar as a percentage of the cell width. The default value is 10.</param>
        /// <param name="MaxLength">The maximum length of the data bar as a percentage of the cell width. The default value is 90.</param>
        /// <param name="ShortestBarType">The conditional format type for the shortest bar.</param>
        /// <param name="ShortestBarValue">The value for the shortest bar. If <paramref name="ShortestBarType"/> is Value, you can just set this to "0".</param>
        /// <param name="LongestBarType">The conditional format type for the longest bar.</param>
        /// <param name="LongestBarValue">The value for the longest bar. If <paramref name="LongestBarType"/> is Value, you can just set this to "0".</param>
        /// <param name="BarColor">The theme color to be used for the data bar.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustomDataBar(bool ShowBarOnly, uint MinLength, uint MaxLength, SLConditionalFormatMinMaxValues ShortestBarType, string ShortestBarValue, SLConditionalFormatMinMaxValues LongestBarType, string LongestBarValue, SLThemeColorIndexValues BarColor, double Tint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor clr = new SLColor(listempty, listempty);
            clr.SetThemeColor(BarColor, Tint);
            this.SetCustomDataBar(ShowBarOnly, MinLength, MaxLength, ShortestBarType, ShortestBarValue, LongestBarType, LongestBarValue, clr);
        }

        private void SetCustomDataBar(bool ShowBarOnly, uint MinLength, uint MaxLength, SLConditionalFormatMinMaxValues ShortestBarType, string ShortestBarValue, SLConditionalFormatMinMaxValues LongestBarType, string LongestBarValue, SLColor BarColor)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.DataBar;
            cfr.DataBar.ShowValue = !ShowBarOnly;
            cfr.DataBar.MinLength = MinLength;
            cfr.DataBar.MaxLength = MaxLength;

            switch (ShortestBarType)
            {
                case SLConditionalFormatMinMaxValues.Value:
                    cfr.DataBar.Cfvo1.Type = ConditionalFormatValueObjectValues.Min;
                    break;
                case SLConditionalFormatMinMaxValues.Number:
                    cfr.DataBar.Cfvo1.Type = ConditionalFormatValueObjectValues.Number;
                    break;
                case SLConditionalFormatMinMaxValues.Percent:
                    cfr.DataBar.Cfvo1.Type = ConditionalFormatValueObjectValues.Percent;
                    break;
                case SLConditionalFormatMinMaxValues.Formula:
                    cfr.DataBar.Cfvo1.Type = ConditionalFormatValueObjectValues.Formula;
                    break;
                case SLConditionalFormatMinMaxValues.Percentile:
                    cfr.DataBar.Cfvo1.Type = ConditionalFormatValueObjectValues.Percentile;
                    break;
            }
            cfr.DataBar.Cfvo1.Val = ShortestBarValue;
            switch (LongestBarType)
            {
                case SLConditionalFormatMinMaxValues.Value:
                    cfr.DataBar.Cfvo2.Type = ConditionalFormatValueObjectValues.Max;
                    break;
                case SLConditionalFormatMinMaxValues.Number:
                    cfr.DataBar.Cfvo2.Type = ConditionalFormatValueObjectValues.Number;
                    break;
                case SLConditionalFormatMinMaxValues.Percent:
                    cfr.DataBar.Cfvo2.Type = ConditionalFormatValueObjectValues.Percent;
                    break;
                case SLConditionalFormatMinMaxValues.Formula:
                    cfr.DataBar.Cfvo2.Type = ConditionalFormatValueObjectValues.Formula;
                    break;
                case SLConditionalFormatMinMaxValues.Percentile:
                    cfr.DataBar.Cfvo2.Type = ConditionalFormatValueObjectValues.Percentile;
                    break;
            }
            cfr.DataBar.Cfvo2.Val = LongestBarValue;

            cfr.DataBar.Color = BarColor.Clone();

            cfr.HasDataBar = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set a color scale formatting with built-in types.
        /// </summary>
        /// <param name="ColorScale">A built-in color scale type.</param>
        public void SetColorScale(SLConditionalFormatColorScaleValues ColorScale)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.ColorScale;
            cfr.ColorScale.Cfvos.Add(new SLConditionalFormatValueObject()
            {
                Type = ConditionalFormatValueObjectValues.Min,
                Val = "0"
            });
            if (ColorScale == SLConditionalFormatColorScaleValues.GreenYellowRed
                || ColorScale == SLConditionalFormatColorScaleValues.RedYellowGreen
                || ColorScale == SLConditionalFormatColorScaleValues.BlueYellowRed
                || ColorScale == SLConditionalFormatColorScaleValues.RedYellowBlue)
            {
                cfr.ColorScale.Cfvos.Add(new SLConditionalFormatValueObject()
                {
                    Type = ConditionalFormatValueObjectValues.Percentile,
                    Val = "50"
                });
            }
            cfr.ColorScale.Cfvos.Add(new SLConditionalFormatValueObject()
            {
                Type = ConditionalFormatValueObjectValues.Max,
                Val = "0"
            });

            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            switch (ColorScale)
            {
                case SLConditionalFormatColorScaleValues.GreenYellowRed:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xF8, 0x69, 0x6B) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEB, 0x84) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0x63, 0xBE, 0x7B) });
                    break;
                case SLConditionalFormatColorScaleValues.RedYellowGreen:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0x63, 0xBE, 0x7B) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEB, 0x84) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xF8, 0x69, 0x6B) });
                    break;
                case SLConditionalFormatColorScaleValues.BlueYellowRed:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xF8, 0x69, 0x6B) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEB, 0x84) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0x5A, 0x8A, 0xC6) });
                    break;
                case SLConditionalFormatColorScaleValues.RedYellowBlue:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0x5A, 0x8A, 0xC6) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEB, 0x84) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xF8, 0x69, 0x6B) });
                    break;
                case SLConditionalFormatColorScaleValues.YellowRed:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x71, 0x28) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEF, 0x9C) });
                    break;
                case SLConditionalFormatColorScaleValues.RedYellow:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEF, 0x9C) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x71, 0x28) });
                    break;
                case SLConditionalFormatColorScaleValues.GreenYellow:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEF, 0x9C) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0x63, 0xBE, 0x7B) });
                    break;
                case SLConditionalFormatColorScaleValues.YellowGreen:
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0x63, 0xBE, 0x7B) });
                    cfr.ColorScale.Colors.Add(new SLColor(listempty, listempty) { Color = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEF, 0x9C) });
                    break;
            }
            
            cfr.HasColorScale = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set a custom 2-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The color for the minimum.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The color for the maximum.</param>
        public void SetCustom2ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, System.Drawing.Color MinColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, System.Drawing.Color MaxColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            minclr.Color = MinColor;
            SLColor maxclr = new SLColor(listempty, listempty);
            maxclr.Color = MaxColor;

            SLColor midclr = new SLColor(listempty, listempty);
            this.SetCustomColorScale(MinType, MinValue, minclr, false, SLConditionalFormatRangeValues.Percentile, "", midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 2-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The color for the minimum.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The theme color for the maximum.</param>
        /// <param name="MaxColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustom2ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, System.Drawing.Color MinColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLThemeColorIndexValues MaxColor, double MaxColorTint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            minclr.Color = MinColor;
            SLColor maxclr = new SLColor(listempty, listempty);
            if (MaxColorTint == 0) maxclr.SetThemeColor(MaxColor);
            else maxclr.SetThemeColor(MaxColor, MaxColorTint);

            SLColor midclr = new SLColor(listempty, listempty);
            this.SetCustomColorScale(MinType, MinValue, minclr, false, SLConditionalFormatRangeValues.Percentile, "", midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 2-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The theme color for the minimum.</param>
        /// <param name="MinColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The color for the maximum.</param>
        public void SetCustom2ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLThemeColorIndexValues MinColor, double MinColorTint,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, System.Drawing.Color MaxColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            if (MinColorTint == 0) minclr.SetThemeColor(MinColor);
            else minclr.SetThemeColor(MinColor, MinColorTint);
            SLColor maxclr = new SLColor(listempty, listempty);
            maxclr.Color = MaxColor;

            SLColor midclr = new SLColor(listempty, listempty);
            this.SetCustomColorScale(MinType, MinValue, minclr, false, SLConditionalFormatRangeValues.Percentile, "", midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 2-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The theme color for the minimum.</param>
        /// <param name="MinColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The theme color for the maximum.</param>
        /// <param name="MaxColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustom2ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLThemeColorIndexValues MinColor, double MinColorTint,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLThemeColorIndexValues MaxColor, double MaxColorTint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            if (MinColorTint == 0) minclr.SetThemeColor(MinColor);
            else minclr.SetThemeColor(MinColor, MinColorTint);
            SLColor maxclr = new SLColor(listempty, listempty);
            if (MaxColorTint == 0) maxclr.SetThemeColor(MaxColor);
            else maxclr.SetThemeColor(MaxColor, MaxColorTint);

            SLColor midclr = new SLColor(listempty, listempty);
            this.SetCustomColorScale(MinType, MinValue, minclr, false, SLConditionalFormatRangeValues.Percentile, "", midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The color for the minimum.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The color for the midpoint.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The color for the maximum.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, System.Drawing.Color MinColor,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, System.Drawing.Color MidPointColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, System.Drawing.Color MaxColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            minclr.Color = MinColor;
            SLColor maxclr = new SLColor(listempty, listempty);
            maxclr.Color = MaxColor;

            SLColor midclr = new SLColor(listempty, listempty);
            midclr.Color = MidPointColor;
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The color for the minimum.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The color for the midpoint.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The theme color for the maximum.</param>
        /// <param name="MaxColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, System.Drawing.Color MinColor,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, System.Drawing.Color MidPointColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLThemeColorIndexValues MaxColor, double MaxColorTint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            minclr.Color = MinColor;
            SLColor maxclr = new SLColor(listempty, listempty);
            if (MaxColorTint == 0) maxclr.SetThemeColor(MaxColor);
            else maxclr.SetThemeColor(MaxColor, MaxColorTint);

            SLColor midclr = new SLColor(listempty, listempty);
            midclr.Color = MidPointColor;
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The color for the minimum.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The theme color for the midpoint.</param>
        /// <param name="MidPointColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The color for the maximum.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, System.Drawing.Color MinColor,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, SLThemeColorIndexValues MidPointColor, double MidPointColorTint,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, System.Drawing.Color MaxColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            minclr.Color = MinColor;
            SLColor maxclr = new SLColor(listempty, listempty);
            maxclr.Color = MaxColor;

            SLColor midclr = new SLColor(listempty, listempty);
            if (MidPointColorTint == 0) midclr.SetThemeColor(MidPointColor);
            else midclr.SetThemeColor(MidPointColor, MidPointColorTint);
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The color for the minimum.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The theme color for the midpoint.</param>
        /// <param name="MidPointColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The theme color for the maximum.</param>
        /// <param name="MaxColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, System.Drawing.Color MinColor,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, SLThemeColorIndexValues MidPointColor, double MidPointColorTint,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLThemeColorIndexValues MaxColor, double MaxColorTint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            minclr.Color = MinColor;
            SLColor maxclr = new SLColor(listempty, listempty);
            if (MaxColorTint == 0) maxclr.SetThemeColor(MaxColor);
            else maxclr.SetThemeColor(MaxColor, MaxColorTint);

            SLColor midclr = new SLColor(listempty, listempty);
            if (MidPointColorTint == 0) midclr.SetThemeColor(MidPointColor);
            else midclr.SetThemeColor(MidPointColor, MidPointColorTint);
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The theme color for the minimum.</param>
        /// <param name="MinColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The color for the midpoint.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The color for the maximum.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLThemeColorIndexValues MinColor, double MinColorTint,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, System.Drawing.Color MidPointColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, System.Drawing.Color MaxColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            if (MinColorTint == 0) minclr.SetThemeColor(MinColor);
            else minclr.SetThemeColor(MinColor, MinColorTint);
            SLColor maxclr = new SLColor(listempty, listempty);
            maxclr.Color = MaxColor;

            SLColor midclr = new SLColor(listempty, listempty);
            midclr.Color = MidPointColor;
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The theme color for the minimum.</param>
        /// <param name="MinColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The color for the midpoint.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The theme color for the maximum.</param>
        /// <param name="MaxColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLThemeColorIndexValues MinColor, double MinColorTint,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, System.Drawing.Color MidPointColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLThemeColorIndexValues MaxColor, double MaxColorTint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            if (MinColorTint == 0) minclr.SetThemeColor(MinColor);
            else minclr.SetThemeColor(MinColor, MinColorTint);
            SLColor maxclr = new SLColor(listempty, listempty);
            if (MaxColorTint == 0) maxclr.SetThemeColor(MaxColor);
            else maxclr.SetThemeColor(MaxColor, MaxColorTint);

            SLColor midclr = new SLColor(listempty, listempty);
            midclr.Color = MidPointColor;
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The theme color for the minimum.</param>
        /// <param name="MinColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The theme color for the midpoint.</param>
        /// <param name="MidPointColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The color for the maximum.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLThemeColorIndexValues MinColor, double MinColorTint,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, SLThemeColorIndexValues MidPointColor, double MidPointColorTint,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, System.Drawing.Color MaxColor)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            if (MinColorTint == 0) minclr.SetThemeColor(MinColor);
            else minclr.SetThemeColor(MinColor, MinColorTint);
            SLColor maxclr = new SLColor(listempty, listempty);
            maxclr.Color = MaxColor;

            SLColor midclr = new SLColor(listempty, listempty);
            if (MidPointColorTint == 0) midclr.SetThemeColor(MidPointColor);
            else midclr.SetThemeColor(MidPointColor, MidPointColorTint);
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        /// <summary>
        /// Set a custom 3-color scale.
        /// </summary>
        /// <param name="MinType">The conditional format type for the minimum.</param>
        /// <param name="MinValue">The value for the minimum. If <paramref name="MinType"/> is Value, you can just set this to "0".</param>
        /// <param name="MinColor">The theme color for the minimum.</param>
        /// <param name="MinColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MidPointType">The conditional format type for the midpoint.</param>
        /// <param name="MidPointValue">The value for the midpoint.</param>
        /// <param name="MidPointColor">The theme color for the midpoint.</param>
        /// <param name="MidPointColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="MaxType">The conditional format type for the maximum.</param>
        /// <param name="MaxValue">The value for the maximum. If <paramref name="MaxType"/> is Value, you can just set this to "0".</param>
        /// <param name="MaxColor">The theme color for the maximum.</param>
        /// <param name="MaxColorTint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        public void SetCustom3ColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLThemeColorIndexValues MinColor, double MinColorTint,
            SLConditionalFormatRangeValues MidPointType, string MidPointValue, SLThemeColorIndexValues MidPointColor, double MidPointColorTint,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLThemeColorIndexValues MaxColor, double MaxColorTint)
        {
            List<System.Drawing.Color> listempty = new List<System.Drawing.Color>();
            SLColor minclr = new SLColor(listempty, listempty);
            if (MinColorTint == 0) minclr.SetThemeColor(MinColor);
            else minclr.SetThemeColor(MinColor, MinColorTint);
            SLColor maxclr = new SLColor(listempty, listempty);
            if (MaxColorTint == 0) maxclr.SetThemeColor(MaxColor);
            else maxclr.SetThemeColor(MaxColor, MaxColorTint);

            SLColor midclr = new SLColor(listempty, listempty);
            if (MidPointColorTint == 0) midclr.SetThemeColor(MidPointColor);
            else midclr.SetThemeColor(MidPointColor, MidPointColorTint);
            this.SetCustomColorScale(MinType, MinValue, minclr, true, MidPointType, MidPointValue, midclr, MaxType, MaxValue, maxclr);
        }

        private void SetCustomColorScale(SLConditionalFormatMinMaxValues MinType, string MinValue, SLColor MinColor,
            bool HasMidPoint, SLConditionalFormatRangeValues MidPointType, string MidPointValue, SLColor MidPointColor,
            SLConditionalFormatMinMaxValues MaxType, string MaxValue, SLColor MaxColor)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.ColorScale;

            SLConditionalFormatValueObject cfvo;

            cfvo = new SLConditionalFormatValueObject();
            switch (MinType)
            {
                case SLConditionalFormatMinMaxValues.Value:
                    cfvo.Type = ConditionalFormatValueObjectValues.Min;
                    break;
                case SLConditionalFormatMinMaxValues.Number:
                    cfvo.Type = ConditionalFormatValueObjectValues.Number;
                    break;
                case SLConditionalFormatMinMaxValues.Percent:
                    cfvo.Type = ConditionalFormatValueObjectValues.Percent;
                    break;
                case SLConditionalFormatMinMaxValues.Formula:
                    cfvo.Type = ConditionalFormatValueObjectValues.Formula;
                    break;
                case SLConditionalFormatMinMaxValues.Percentile:
                    cfvo.Type = ConditionalFormatValueObjectValues.Percentile;
                    break;
            }
            cfvo.Val = MinValue;
            cfr.ColorScale.Cfvos.Add(cfvo);
            cfr.ColorScale.Colors.Add(MinColor.Clone());

            if (HasMidPoint)
            {
                cfvo = new SLConditionalFormatValueObject();
                switch (MidPointType)
                {
                    case SLConditionalFormatRangeValues.Number:
                        cfvo.Type = ConditionalFormatValueObjectValues.Number;
                        break;
                    case SLConditionalFormatRangeValues.Percent:
                        cfvo.Type = ConditionalFormatValueObjectValues.Percent;
                        break;
                    case SLConditionalFormatRangeValues.Formula:
                        cfvo.Type = ConditionalFormatValueObjectValues.Formula;
                        break;
                    case SLConditionalFormatRangeValues.Percentile:
                        cfvo.Type = ConditionalFormatValueObjectValues.Percentile;
                        break;
                }
                cfvo.Val = MidPointValue;
                cfr.ColorScale.Cfvos.Add(cfvo);
                cfr.ColorScale.Colors.Add(MidPointColor.Clone());
            }

            cfvo = new SLConditionalFormatValueObject();
            switch (MaxType)
            {
                case SLConditionalFormatMinMaxValues.Value:
                    cfvo.Type = ConditionalFormatValueObjectValues.Max;
                    break;
                case SLConditionalFormatMinMaxValues.Number:
                    cfvo.Type = ConditionalFormatValueObjectValues.Number;
                    break;
                case SLConditionalFormatMinMaxValues.Percent:
                    cfvo.Type = ConditionalFormatValueObjectValues.Percent;
                    break;
                case SLConditionalFormatMinMaxValues.Formula:
                    cfvo.Type = ConditionalFormatValueObjectValues.Formula;
                    break;
                case SLConditionalFormatMinMaxValues.Percentile:
                    cfvo.Type = ConditionalFormatValueObjectValues.Percentile;
                    break;
            }
            cfvo.Val = MaxValue;
            cfr.ColorScale.Cfvos.Add(cfvo);
            cfr.ColorScale.Colors.Add(MaxColor.Clone());

            cfr.HasColorScale = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set an icon set formatting with built-in types.
        /// </summary>
        /// <param name="IconSetType">A built-in icon set type.</param>
        public void SetIconSet(IconSetValues IconSetType)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.IconSet;
            cfr.IconSet.IconSetValue = IconSetType;

            switch (IconSetType)
            {
                case IconSetValues.FiveArrows:
                case IconSetValues.FiveArrowsGray:
                case IconSetValues.FiveQuarters:
                case IconSetValues.FiveRating:
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "0" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "20" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "40" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "60" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "80" });
                    break;
                case IconSetValues.FourArrows:
                case IconSetValues.FourArrowsGray:
                case IconSetValues.FourRating:
                case IconSetValues.FourRedToBlack:
                case IconSetValues.FourTrafficLights:
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "0" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "25" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "50" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "75" });
                    break;
                case IconSetValues.ThreeArrows:
                case IconSetValues.ThreeArrowsGray:
                case IconSetValues.ThreeFlags:
                case IconSetValues.ThreeSigns:
                case IconSetValues.ThreeSymbols:
                case IconSetValues.ThreeSymbols2:
                case IconSetValues.ThreeTrafficLights1:
                case IconSetValues.ThreeTrafficLights2:
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "0" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "33" });
                    cfr.IconSet.Cfvos.Add(new SLConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percent, Val = "67" });
                    break;
            }

            cfr.HasIconSet = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set a custom 3-icon set.
        /// </summary>
        /// <param name="IconSetType">The type of 3-icon set.</param>
        /// <param name="ReverseIconOrder">True to reverse the order of the icons. False to use the default order.</param>
        /// <param name="ShowIconOnly">True to show only icons. False to show both icon and value.</param>
        /// <param name="GreaterThanOrEqual2">True if values are to be greater than or equal to the 2nd range value. False if values are to be strictly greater than.</param>
        /// <param name="Value2">The 2nd range value.</param>
        /// <param name="Type2">The conditional format type for the 2nd range value.</param>
        /// <param name="GreaterThanOrEqual3">True if values are to be greater than or equal to the 3rd range value. False if values are to be strictly greater than.</param>
        /// <param name="Value3">The 3rd range value.</param>
        /// <param name="Type3">The conditional format type for the 3rd range value.</param>
        public void SetCustomIconSet(SLThreeIconSetValues IconSetType, bool ReverseIconOrder, bool ShowIconOnly,
            bool GreaterThanOrEqual2, string Value2, SLConditionalFormatRangeValues Type2,
            bool GreaterThanOrEqual3, string Value3, SLConditionalFormatRangeValues Type3)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.IconSet;
            cfr.IconSet.Reverse = ReverseIconOrder;
            cfr.IconSet.ShowValue = !ShowIconOnly;

            switch (IconSetType)
            {
                case SLThreeIconSetValues.ThreeArrows:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeArrows;
                    break;
                case SLThreeIconSetValues.ThreeArrowsGray:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeArrowsGray;
                    break;
                case SLThreeIconSetValues.ThreeFlags:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeFlags;
                    break;
                case SLThreeIconSetValues.ThreeSigns:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeSigns;
                    break;
                case SLThreeIconSetValues.ThreeSymbols:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeSymbols;
                    break;
                case SLThreeIconSetValues.ThreeSymbols2:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeSymbols2;
                    break;
                case SLThreeIconSetValues.ThreeTrafficLights1:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeTrafficLights1;
                    break;
                case SLThreeIconSetValues.ThreeTrafficLights2:
                    cfr.IconSet.IconSetValue = IconSetValues.ThreeTrafficLights2;
                    break;
            }

            SLConditionalFormatValueObject cfvo;

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = ConditionalFormatValueObjectValues.Percent;
            cfvo.Val = "0";
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type2);
            cfvo.Val = Value2;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual2;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type3);
            cfvo.Val = Value3;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual3;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfr.HasIconSet = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set a custom 4-icon set.
        /// </summary>
        /// <param name="IconSetType">The type of 3-icon set.</param>
        /// <param name="ReverseIconOrder">True to reverse the order of the icons. False to use the default order.</param>
        /// <param name="ShowIconOnly">True to show only icons. False to show both icon and value.</param>
        /// <param name="GreaterThanOrEqual2">True if values are to be greater than or equal to the 2nd range value. False if values are to be strictly greater than.</param>
        /// <param name="Value2">The 2nd range value.</param>
        /// <param name="Type2">The conditional format type for the 2nd range value.</param>
        /// <param name="GreaterThanOrEqual3">True if values are to be greater than or equal to the 3rd range value. False if values are to be strictly greater than.</param>
        /// <param name="Value3">The 3rd range value.</param>
        /// <param name="Type3">The conditional format type for the 3rd range value.</param>
        /// <param name="GreaterThanOrEqual4">True if values are to be greater than or equal to the 4th range value. False if values are to be strictly greater than.</param>
        /// <param name="Value4">The 4th range value.</param>
        /// <param name="Type4">The conditional format type for the 4th range value.</param>
        public void SetCustomIconSet(SLFourIconSetValues IconSetType, bool ReverseIconOrder, bool ShowIconOnly,
            bool GreaterThanOrEqual2, string Value2, SLConditionalFormatRangeValues Type2,
            bool GreaterThanOrEqual3, string Value3, SLConditionalFormatRangeValues Type3,
            bool GreaterThanOrEqual4, string Value4, SLConditionalFormatRangeValues Type4)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.IconSet;
            cfr.IconSet.Reverse = ReverseIconOrder;
            cfr.IconSet.ShowValue = !ShowIconOnly;

            switch (IconSetType)
            {
                case SLFourIconSetValues.FourArrows:
                    cfr.IconSet.IconSetValue = IconSetValues.FourArrows;
                    break;
                case SLFourIconSetValues.FourArrowsGray:
                    cfr.IconSet.IconSetValue = IconSetValues.FourArrowsGray;
                    break;
                case SLFourIconSetValues.FourRating:
                    cfr.IconSet.IconSetValue = IconSetValues.FourRating;
                    break;
                case SLFourIconSetValues.FourRedToBlack:
                    cfr.IconSet.IconSetValue = IconSetValues.FourRedToBlack;
                    break;
                case SLFourIconSetValues.FourTrafficLights:
                    cfr.IconSet.IconSetValue = IconSetValues.FourTrafficLights;
                    break;
            }

            SLConditionalFormatValueObject cfvo;

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = ConditionalFormatValueObjectValues.Percent;
            cfvo.Val = "0";
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type2);
            cfvo.Val = Value2;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual2;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type3);
            cfvo.Val = Value3;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual3;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type4);
            cfvo.Val = Value4;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual4;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfr.HasIconSet = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Set a custom 5-icon set.
        /// </summary>
        /// <param name="IconSetType">The type of 3-icon set.</param>
        /// <param name="ReverseIconOrder">True to reverse the order of the icons. False to use the default order.</param>
        /// <param name="ShowIconOnly">True to show only icons. False to show both icon and value.</param>
        /// <param name="GreaterThanOrEqual2">True if values are to be greater than or equal to the 2nd range value. False if values are to be strictly greater than.</param>
        /// <param name="Value2">The 2nd range value.</param>
        /// <param name="Type2">The conditional format type for the 2nd range value.</param>
        /// <param name="GreaterThanOrEqual3">True if values are to be greater than or equal to the 3rd range value. False if values are to be strictly greater than.</param>
        /// <param name="Value3">The 3rd range value.</param>
        /// <param name="Type3">The conditional format type for the 3rd range value.</param>
        /// <param name="GreaterThanOrEqual4">True if values are to be greater than or equal to the 4th range value. False if values are to be strictly greater than.</param>
        /// <param name="Value4">The 4th range value.</param>
        /// <param name="Type4">The conditional format type for the 4th range value.</param>
        /// <param name="GreaterThanOrEqual5">True if values are to be greater than or equal to the 5th range value. False if values are to be strictly greater than.</param>
        /// <param name="Value5">The 5th range value.</param>
        /// <param name="Type5">The conditional format type for the 5th range value.</param>
        public void SetCustomIconSet(SLFiveIconSetValues IconSetType, bool ReverseIconOrder, bool ShowIconOnly,
            bool GreaterThanOrEqual2, string Value2, SLConditionalFormatRangeValues Type2,
            bool GreaterThanOrEqual3, string Value3, SLConditionalFormatRangeValues Type3,
            bool GreaterThanOrEqual4, string Value4, SLConditionalFormatRangeValues Type4,
            bool GreaterThanOrEqual5, string Value5, SLConditionalFormatRangeValues Type5)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.IconSet;
            cfr.IconSet.Reverse = ReverseIconOrder;
            cfr.IconSet.ShowValue = !ShowIconOnly;

            switch (IconSetType)
            {
                case SLFiveIconSetValues.FiveArrows:
                    cfr.IconSet.IconSetValue = IconSetValues.FiveArrows;
                    break;
                case SLFiveIconSetValues.FiveArrowsGray:
                    cfr.IconSet.IconSetValue = IconSetValues.FiveArrowsGray;
                    break;
                case SLFiveIconSetValues.FiveQuarters:
                    cfr.IconSet.IconSetValue = IconSetValues.FiveQuarters;
                    break;
                case SLFiveIconSetValues.FiveRating:
                    cfr.IconSet.IconSetValue = IconSetValues.FiveRating;
                    break;
            }

            SLConditionalFormatValueObject cfvo;

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = ConditionalFormatValueObjectValues.Percent;
            cfvo.Val = "0";
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type2);
            cfvo.Val = Value2;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual2;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type3);
            cfvo.Val = Value3;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual3;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type4);
            cfvo.Val = Value4;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual4;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.TranslateRangeValues(Type5);
            cfvo.Val = Value5;
            cfvo.GreaterThanOrEqual = GreaterThanOrEqual5;
            cfr.IconSet.Cfvos.Add(cfvo);

            cfr.HasIconSet = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values greater than a given value.
        /// </summary>
        /// <param name="IncludeEquality">True for greater than or equal to. False for strictly greater than.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsGreaterThan(bool IncludeEquality, string Value, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsGreaterThan(IncludeEquality, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values greater than a given value.
        /// </summary>
        /// <param name="IncludeEquality">True for greater than or equal to. False for strictly greater than.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsGreaterThan(bool IncludeEquality, string Value, SLStyle HighlightStyle)
        {
            this.HighlightCellsGreaterThan(IncludeEquality, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsGreaterThan(bool IncludeEquality, string Value, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.CellIs;
            cfr.Operator = IncludeEquality ? ConditionalFormattingOperatorValues.GreaterThanOrEqual : ConditionalFormattingOperatorValues.GreaterThan;
            cfr.HasOperator = true;

            cfr.Formulas.Add(this.GetFormulaFromText(Value));

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values less than a given value.
        /// </summary>
        /// <param name="IncludeEquality">True for less than or equal to. False for strictly less than.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsLessThan(bool IncludeEquality, string Value, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsLessThan(IncludeEquality, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values less than a given value.
        /// </summary>
        /// <param name="IncludeEquality">True for less than or equal to. False for strictly less than.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsLessThan(bool IncludeEquality, string Value, SLStyle HighlightStyle)
        {
            this.HighlightCellsLessThan(IncludeEquality, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsLessThan(bool IncludeEquality, string Value, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.CellIs;
            cfr.Operator = IncludeEquality ? ConditionalFormattingOperatorValues.LessThanOrEqual : ConditionalFormattingOperatorValues.LessThan;
            cfr.HasOperator = true;

            cfr.Formulas.Add(this.GetFormulaFromText(Value));

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values between 2 given values.
        /// </summary>
        /// <param name="IsBetween">True for between the 2 given values. False for not between the 2 given values.</param>
        /// <param name="Value1">The 1st value to be compared with.</param>
        /// <param name="Value2">The 2nd value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsBetween(bool IsBetween, string Value1, string Value2, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsBetween(IsBetween, Value1, Value2, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values between 2 given values.
        /// </summary>
        /// <param name="IsBetween">True for between the 2 given values. False for not between the 2 given values.</param>
        /// <param name="Value1">The 1st value to be compared with.</param>
        /// <param name="Value2">The 2nd value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsBetween(bool IsBetween, string Value1, string Value2, SLStyle HighlightStyle)
        {
            this.HighlightCellsBetween(IsBetween, Value1, Value2, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsBetween(bool IsBetween, string Value1, string Value2, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.CellIs;
            cfr.Operator = IsBetween ? ConditionalFormattingOperatorValues.Between : ConditionalFormattingOperatorValues.NotBetween;
            cfr.HasOperator = true;

            cfr.Formulas.Add(this.GetFormulaFromText(Value1));
            cfr.Formulas.Add(this.GetFormulaFromText(Value2));

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values equal to a given value.
        /// </summary>
        /// <param name="IsEqual">True for equal to given value. False for not equal to given value.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsEqual(bool IsEqual, string Value, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsEqual(IsEqual, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values equal to a given value.
        /// </summary>
        /// <param name="IsEqual">True for equal to given value. False for not equal to given value.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsEqual(bool IsEqual, string Value, SLStyle HighlightStyle)
        {
            this.HighlightCellsEqual(IsEqual, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsEqual(bool IsEqual, string Value, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.CellIs;
            cfr.Operator = IsEqual ? ConditionalFormattingOperatorValues.Equal : ConditionalFormattingOperatorValues.NotEqual;
            cfr.HasOperator = true;

            cfr.Formulas.Add(this.GetFormulaFromText(Value));

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values containing a given value.
        /// </summary>
        /// <param name="IsContaining">True for containing given value. False for not containing given value.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsContainingText(bool IsContaining, string Value, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsContainingText(IsContaining, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values containing a given value.
        /// </summary>
        /// <param name="IsContaining">True for containing given value. False for not containing given value.</param>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsContainingText(bool IsContaining, string Value, SLStyle HighlightStyle)
        {
            this.HighlightCellsContainingText(IsContaining, Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsContainingText(bool IsContaining, string Value, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Text = Value;

            Formula f = new Formula();
            string sRef = string.Empty;
            if (this.SequenceOfReferences.Count > 0)
            {
                sRef = SLTool.ToCellReference(this.SequenceOfReferences[0].StartRowIndex, this.SequenceOfReferences[0].StartColumnIndex);
            }
            if (IsContaining)
            {
                cfr.Type = ConditionalFormatValues.ContainsText;
                cfr.Operator = ConditionalFormattingOperatorValues.ContainsText;
                cfr.HasOperator = true;
                f.Text = string.Format("NOT(ISERROR(SEARCH({0},{1})))", this.GetCleanedStringFromText(Value), sRef);
            }
            else
            {
                cfr.Type = ConditionalFormatValues.NotContainsText;
                cfr.Operator = ConditionalFormattingOperatorValues.NotContains;
                cfr.HasOperator = true;
                f.Text = string.Format("ISERROR(SEARCH({0},{1}))", this.GetCleanedStringFromText(Value), sRef);
            }
            cfr.Formulas.Add(f);

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values beginning with a given value.
        /// </summary>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsBeginningWith(string Value, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsBeginningWith(Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values beginning with a given value.
        /// </summary>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsBeginningWith(string Value, SLStyle HighlightStyle)
        {
            this.HighlightCellsBeginningWith(Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsBeginningWith(string Value, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Text = Value;

            Formula f = new Formula();
            string sRef = string.Empty;
            if (this.SequenceOfReferences.Count > 0)
            {
                sRef = SLTool.ToCellReference(this.SequenceOfReferences[0].StartRowIndex, this.SequenceOfReferences[0].StartColumnIndex);
            }
            cfr.Type = ConditionalFormatValues.BeginsWith;
            cfr.Operator = ConditionalFormattingOperatorValues.BeginsWith;
            cfr.HasOperator = true;
            f.Text = string.Format("LEFT({0},{1})={2}", sRef, Value.Length, this.GetCleanedStringFromText(Value));
            cfr.Formulas.Add(f);

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values ending with a given value.
        /// </summary>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsEndingWith(string Value, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsEndingWith(Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values ending with a given value.
        /// </summary>
        /// <param name="Value">The value to be compared with.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsEndingWith(string Value, SLStyle HighlightStyle)
        {
            this.HighlightCellsEndingWith(Value, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsEndingWith(string Value, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Text = Value;

            Formula f = new Formula();
            string sRef = string.Empty;
            if (this.SequenceOfReferences.Count > 0)
            {
                sRef = SLTool.ToCellReference(this.SequenceOfReferences[0].StartRowIndex, this.SequenceOfReferences[0].StartColumnIndex);
            }
            cfr.Type = ConditionalFormatValues.EndsWith;
            cfr.Operator = ConditionalFormattingOperatorValues.EndsWith;
            cfr.HasOperator = true;
            f.Text = string.Format("RIGHT({0},{1})={2}", sRef, Value.Length, this.GetCleanedStringFromText(Value));
            cfr.Formulas.Add(f);

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells that are blank.
        /// </summary>
        /// <param name="ContainsBlanks">True for containing blanks. False for not containing blanks.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsContainingBlanks(bool ContainsBlanks, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsContainingBlanks(ContainsBlanks, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells that are blank.
        /// </summary>
        /// <param name="ContainsBlanks">True for containing blanks. False for not containing blanks.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsContainingBlanks(bool ContainsBlanks, SLStyle HighlightStyle)
        {
            this.HighlightCellsContainingBlanks(ContainsBlanks, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsContainingBlanks(bool ContainsBlanks, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();

            Formula f = new Formula();
            string sRef = string.Empty;
            if (this.SequenceOfReferences.Count > 0)
            {
                sRef = SLTool.ToCellReference(this.SequenceOfReferences[0].StartRowIndex, this.SequenceOfReferences[0].StartColumnIndex);
            }
            if (ContainsBlanks)
            {
                cfr.Type = ConditionalFormatValues.ContainsBlanks;
                f.Text = string.Format("LEN(TRIM({0}))=0", sRef);
            }
            else
            {
                cfr.Type = ConditionalFormatValues.NotContainsBlanks;
                f.Text = string.Format("LEN(TRIM({0}))>0", sRef);
            }
            cfr.Formulas.Add(f);

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells containing errors.
        /// </summary>
        /// <param name="ContainsErrors">True for containing errors. False for not containing errors.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsContainingErrors(bool ContainsErrors, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsContainingErrors(ContainsErrors, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells containing errors.
        /// </summary>
        /// <param name="ContainsErrors">True for containing errors. False for not containing errors.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsContainingErrors(bool ContainsErrors, SLStyle HighlightStyle)
        {
            this.HighlightCellsContainingErrors(ContainsErrors, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsContainingErrors(bool ContainsErrors, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();

            Formula f = new Formula();
            string sRef = string.Empty;
            if (this.SequenceOfReferences.Count > 0)
            {
                sRef = SLTool.ToCellReference(this.SequenceOfReferences[0].StartRowIndex, this.SequenceOfReferences[0].StartColumnIndex);
            }
            if (ContainsErrors)
            {
                cfr.Type = ConditionalFormatValues.ContainsErrors;
                f.Text = string.Format("ISERROR({0})", sRef);
            }
            else
            {
                cfr.Type = ConditionalFormatValues.NotContainsErrors;
                f.Text = string.Format("NOT(ISERROR({0}))", sRef);
            }
            cfr.Formulas.Add(f);

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with date values occurring according to a given time period.
        /// </summary>
        /// <param name="DatePeriod">A given time period.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsWithDatesOccurring(TimePeriodValues DatePeriod, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsWithDatesOccurring(DatePeriod, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with date values occurring according to a given time period.
        /// </summary>
        /// <param name="DatePeriod">A given time period.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsWithDatesOccurring(TimePeriodValues DatePeriod, SLStyle HighlightStyle)
        {
            this.HighlightCellsWithDatesOccurring(DatePeriod, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsWithDatesOccurring(TimePeriodValues DatePeriod, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.TimePeriod;
            cfr.TimePeriod = DatePeriod;
            cfr.HasTimePeriod = true;

            Formula f = new Formula();
            string sRef = string.Empty;
            if (this.SequenceOfReferences.Count > 0)
            {
                sRef = SLTool.ToCellReference(this.SequenceOfReferences[0].StartRowIndex, this.SequenceOfReferences[0].StartColumnIndex);
            }
            switch (DatePeriod)
            {
                case TimePeriodValues.Yesterday:
                    f.Text = string.Format("FLOOR({0},1)=TODAY()-1", sRef);
                    break;
                case TimePeriodValues.Today:
                    f.Text = string.Format("FLOOR({0},1)=TODAY()", sRef);
                    break;
                case TimePeriodValues.Tomorrow:
                    f.Text = string.Format("FLOOR({0},1)=TODAY()+1", sRef);
                    break;
                case TimePeriodValues.Last7Days:
                    f.Text = string.Format("AND(TODAY()-FLOOR({0},1)<=6,FLOOR({0},1)<=TODAY())", sRef);
                    break;
                case TimePeriodValues.LastWeek:
                    f.Text = string.Format("AND(TODAY()-ROUNDDOWN({0},0)>=(WEEKDAY(TODAY())),TODAY()-ROUNDDOWN({0},0)<(WEEKDAY(TODAY())+7))", sRef);
                    break;
                case TimePeriodValues.ThisWeek:
                    f.Text = string.Format("AND(TODAY()-ROUNDDOWN({0},0)<=WEEKDAY(TODAY())-1,ROUNDDOWN({0},0)-TODAY()<=7-WEEKDAY(TODAY()))", sRef);
                    break;
                case TimePeriodValues.NextWeek:
                    f.Text = string.Format("AND(ROUNDDOWN({0},0)-TODAY()>(7-WEEKDAY(TODAY())),ROUNDDOWN({0},0)-TODAY()<(15-WEEKDAY(TODAY())))", sRef);
                    break;
                case TimePeriodValues.LastMonth:
                    f.Text = string.Format("AND(MONTH({0})=MONTH(EDATE(TODAY(),0-1)),YEAR({0})=YEAR(EDATE(TODAY(),0-1)))", sRef);
                    break;
                case TimePeriodValues.ThisMonth:
                    f.Text = string.Format("AND(MONTH({0})=MONTH(TODAY()),YEAR({0})=YEAR(TODAY()))", sRef);
                    break;
                case TimePeriodValues.NextMonth:
                    f.Text = string.Format("AND(MONTH({0})=MONTH(TODAY())+1,OR(YEAR({0})=YEAR(TODAY()),AND(MONTH({0})=12,YEAR({0})=YEAR(TODAY())+1)))", sRef);
                    break;
            }
            cfr.Formulas.Add(f);

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with duplicate values.
        /// </summary>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsWithDuplicates(SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsWithDuplicates(this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with duplicate values.
        /// </summary>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsWithDuplicates(SLStyle HighlightStyle)
        {
            this.HighlightCellsWithDuplicates(this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsWithDuplicates(SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.DuplicateValues;

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with unique values.
        /// </summary>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsWithUniques(SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsWithUniques(this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with unique values.
        /// </summary>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsWithUniques(SLStyle HighlightStyle)
        {
            this.HighlightCellsWithUniques(this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsWithUniques(SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.UniqueValues;

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values in the top/bottom range.
        /// </summary>
        /// <param name="IsTopRange">True if in the top range. False if in the bottom range.</param>
        /// <param name="Rank">The value of X in "Top/Bottom X". If <paramref name="IsPercent"/> is true, then X refers to X%, otherwise it's X number of items.</param>
        /// <param name="IsPercent">True if referring to percentage. False if referring to number of items.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsInTopRange(bool IsTopRange, uint Rank, bool IsPercent, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsInTopRange(IsTopRange, Rank, IsPercent, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values in the top/bottom range.
        /// </summary>
        /// <param name="IsTopRange">True if in the top range. False if in the bottom range.</param>
        /// <param name="Rank">The value of X in "Top/Bottom X". If <paramref name="IsPercent"/> is true, then X refers to X%, otherwise it's X number of items.</param>
        /// <param name="IsPercent">True if referring to percentage. False if referring to number of items.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsInTopRange(bool IsTopRange, uint Rank, bool IsPercent, SLStyle HighlightStyle)
        {
            this.HighlightCellsInTopRange(IsTopRange, Rank, IsPercent, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsInTopRange(bool IsTopRange, uint Rank, bool IsPercent, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.Top10;
            cfr.Bottom = !IsTopRange;
            cfr.Rank = Rank;
            cfr.Percent = IsPercent;

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values compared to the average.
        /// </summary>
        /// <param name="AverageType">The type of comparison to the average.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsAboveAverage(SLHighlightCellsAboveAverageValues AverageType, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsAboveAverage(AverageType, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values compared to the average.
        /// </summary>
        /// <param name="AverageType">The type of comparison to the average.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsAboveAverage(SLHighlightCellsAboveAverageValues AverageType, SLStyle HighlightStyle)
        {
            this.HighlightCellsAboveAverage(AverageType, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsAboveAverage(SLHighlightCellsAboveAverageValues AverageType, SLDifferentialFormat HighlightStyle)
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.AboveAverage;

            switch (AverageType)
            {
                case SLHighlightCellsAboveAverageValues.Above:
                    // that's all is needed!
                    break;
                case SLHighlightCellsAboveAverageValues.Below:
                    cfr.AboveAverage = false;
                    break;
                case SLHighlightCellsAboveAverageValues.EqualOrAbove:
                    cfr.EqualAverage = true;
                    break;
                case SLHighlightCellsAboveAverageValues.EqualOrBelow:
                    cfr.EqualAverage = true;
                    cfr.AboveAverage = false;
                    break;
                case SLHighlightCellsAboveAverageValues.OneStdDevAbove:
                    cfr.StdDev = 1;
                    break;
                case SLHighlightCellsAboveAverageValues.OneStdDevBelow:
                    cfr.AboveAverage = false;
                    cfr.StdDev = 1;
                    break;
                case SLHighlightCellsAboveAverageValues.TwoStdDevAbove:
                    cfr.StdDev = 2;
                    break;
                case SLHighlightCellsAboveAverageValues.TwoStdDevBelow:
                    cfr.AboveAverage = false;
                    cfr.StdDev = 2;
                    break;
                case SLHighlightCellsAboveAverageValues.ThreeStdDevAbove:
                    cfr.StdDev = 3;
                    break;
                case SLHighlightCellsAboveAverageValues.ThreeStdDevBelow:
                    cfr.AboveAverage = false;
                    cfr.StdDev = 3;
                    break;
            }

            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        /// <summary>
        /// Highlight cells with values according to a formula.
        /// </summary>
        /// <param name="Formula">The formula to apply.</param>
        /// <param name="HighlightStyle">A built-in highlight style.</param>
        public void HighlightCellsWithFormula(string Formula, SLHighlightCellsStyleValues HighlightStyle)
        {
            this.HighlightCellsWithFormula(Formula, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        /// <summary>
        /// Highlight cells with values according to a formula.
        /// </summary>
        /// <param name="Formula">The formula to apply.</param>
        /// <param name="HighlightStyle">A custom formatted style. Note that only number formats, fonts, borders and fills are used. Note further that for fonts, only italic/bold, underline, color and strikethrough settings are used.</param>
        public void HighlightCellsWithFormula(string Formula, SLStyle HighlightStyle)
        {
            this.HighlightCellsWithFormula(Formula, this.TranslateToDifferentialFormat(HighlightStyle));
        }

        private void HighlightCellsWithFormula(string Formula, SLDifferentialFormat HighlightStyle)
        {
            if (Formula.StartsWith("=")) Formula = Formula.Substring(1);

            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();
            cfr.Type = ConditionalFormatValues.Expression;
            cfr.Formulas.Add(new DocumentFormat.OpenXml.Spreadsheet.Formula(Formula));
            cfr.DifferentialFormat = HighlightStyle.Clone();
            cfr.HasDifferentialFormat = true;

            this.AppendRule(cfr);
        }

        internal Formula GetFormulaFromText(string Text)
        {
            Formula f = new Formula();
            double fTemp = 0.0;
            if (double.TryParse(Text, out fTemp))
            {
                f.Text = Text;
            }
            else
            {
                // double quotes are doubled
                Text = Text.Replace("\"", "\"\"");
                if (SLTool.ToPreserveSpace(Text))
                {
                    f.Space = SpaceProcessingModeValues.Preserve;
                }
                // double quotes are placed at the ends of the given value
                f.Text = string.Format("\"{0}\"", Text);
            }
            return f;
        }

        internal string GetCleanedStringFromText(string Text)
        {
            // double quotes are doubled
            Text = Text.Replace("\"", "\"\"");
            // double quotes are placed at the ends of the given value
            Text = string.Format("\"{0}\"", Text);
            return Text;
        }

        internal ConditionalFormatValueObjectValues TranslateRangeValues(SLConditionalFormatRangeValues RangeValue)
        {
            ConditionalFormatValueObjectValues cfvov = ConditionalFormatValueObjectValues.Number;
            switch (RangeValue)
            {
                case SLConditionalFormatRangeValues.Number:
                    cfvov = ConditionalFormatValueObjectValues.Number;
                    break;
                case SLConditionalFormatRangeValues.Percent:
                    cfvov = ConditionalFormatValueObjectValues.Percent;
                    break;
                case SLConditionalFormatRangeValues.Formula:
                    cfvov = ConditionalFormatValueObjectValues.Formula;
                    break;
                case SLConditionalFormatRangeValues.Percentile:
                    cfvov = ConditionalFormatValueObjectValues.Percentile;
                    break;
            }
            return cfvov;
        }

        internal SLDifferentialFormat TranslateToDifferentialFormat(SLHighlightCellsStyleValues style)
        {
            SLDifferentialFormat df = new SLDifferentialFormat();
            switch (style)
            {
                case SLHighlightCellsStyleValues.LightRedFillWithDarkRedText:
                    df.Font.Condense = false;
                    df.Font.Extend = false;
                    df.Font.FontColor = System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x00, 0x06);
                    df.Fill.SetPatternBackgroundColor(System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xC7, 0xCE));
                    break;
                case SLHighlightCellsStyleValues.YellowFillWithDarkYellowText:
                    df.Font.Condense = false;
                    df.Font.Extend = false;
                    df.Font.FontColor = System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x65, 0x00);
                    df.Fill.SetPatternBackgroundColor(System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xEB, 0x9C));
                    break;
                case SLHighlightCellsStyleValues.GreenFillWithDarkGreenText:
                    df.Font.Condense = false;
                    df.Font.Extend = false;
                    df.Font.FontColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x61, 0x00);
                    df.Fill.SetPatternBackgroundColor(System.Drawing.Color.FromArgb(0xFF, 0xC6, 0xEF, 0xCE));
                    break;
                case SLHighlightCellsStyleValues.LightRedFill:
                    df.Fill.SetPatternBackgroundColor(System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xC7, 0xCE));
                    break;
                case SLHighlightCellsStyleValues.RedText:
                    df.Font.Condense = false;
                    df.Font.Extend = false;
                    df.Font.FontColor = System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x00, 0x06);
                    break;
                case SLHighlightCellsStyleValues.RedBorder:
                    df.Border.SetLeftBorder(BorderStyleValues.Thin, System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x00, 0x06));
                    df.Border.SetRightBorder(BorderStyleValues.Thin, System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x00, 0x06));
                    df.Border.SetTopBorder(BorderStyleValues.Thin, System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x00, 0x06));
                    df.Border.SetBottomBorder(BorderStyleValues.Thin, System.Drawing.Color.FromArgb(0xFF, 0x9C, 0x00, 0x06));
                    break;
            }

            df.Sync();
            return df;
        }

        internal SLDifferentialFormat TranslateToDifferentialFormat(SLStyle style)
        {
            style.Sync();
            SLDifferentialFormat df = new SLDifferentialFormat();
            if (style.HasNumberingFormat) df.FormatCode = style.FormatCode;

            if (style.Font.Italic != null && style.Font.Italic.Value) df.Font.Italic = true;
            if (style.Font.Bold != null && style.Font.Bold.Value) df.Font.Bold = true;
            if (style.Font.HasUnderline) df.Font.Underline = style.Font.Underline;
            if (style.Font.HasFontColor)
            {
                df.Font.clrFontColor = style.Font.clrFontColor.Clone();
                df.Font.HasFontColor = true;
            }
            if (style.Font.Strike != null && style.Font.Strike.Value) df.Font.Strike = true;

            if (style.HasBorder) df.Border = style.Border.Clone();
            if (style.HasFill) df.Fill = style.Fill.Clone();

            df.Sync();
            return df;
        }

        internal void FromConditionalFormatting(ConditionalFormatting cf)
        {
            this.SetAllNull();

            if (cf.Pivot != null) this.Pivot = cf.Pivot.Value;

            SLCellPointRange pt;
            int index;
            int iStartRowIndex = -1;
            int iStartColumnIndex = -1;
            int iEndRowIndex = -1;
            int iEndColumnIndex = -1;
            foreach (var s in cf.SequenceOfReferences.Items)
            {
                index = s.Value.IndexOf(":");
                if (index > -1)
                {
                    if (SLTool.FormatCellReferenceRangeToRowColumnIndex(s.Value, out iStartRowIndex, out iStartColumnIndex, out iEndRowIndex, out iEndColumnIndex))
                    {
                        pt = new SLCellPointRange(iStartRowIndex, iStartColumnIndex, iEndRowIndex, iEndColumnIndex);
                        this.SequenceOfReferences.Add(pt);
                    }
                }
                else
                {
                    if (SLTool.FormatCellReferenceToRowColumnIndex(s.Value, out iStartRowIndex, out iStartColumnIndex))
                    {
                        pt = new SLCellPointRange(iStartRowIndex, iStartColumnIndex, iStartRowIndex, iStartColumnIndex);
                        this.SequenceOfReferences.Add(pt);
                    }
                }
            }

            foreach (var rule in cf.Elements<ConditionalFormattingRule>())
            {
                var cfr = new SLConditionalFormattingRule();
                cfr.FromConditionalFormattingRule(rule);
                this.Rules.Add(cfr);
            }
        }

        internal ConditionalFormatting ToConditionalFormatting()
        {
            ConditionalFormatting cf = new ConditionalFormatting();
            if (this.Pivot) cf.Pivot = this.Pivot;
            if (this.SequenceOfReferences.Count > 0)
            {
                string sRef = string.Empty;
                cf.SequenceOfReferences = new ListValue<StringValue>();
                foreach (SLCellPointRange pt in this.SequenceOfReferences)
                {
                    if (pt.StartRowIndex == pt.EndRowIndex && pt.StartColumnIndex == pt.EndColumnIndex)
                    {
                        sRef = SLTool.ToCellReference(pt.StartRowIndex, pt.StartColumnIndex);
                    }
                    else
                    {
                        sRef = string.Format("{0}:{1}", SLTool.ToCellReference(pt.StartRowIndex, pt.StartColumnIndex), SLTool.ToCellReference(pt.EndRowIndex, pt.EndColumnIndex));
                    }
                    cf.SequenceOfReferences.Items.Add(new StringValue(sRef));
                }
            }

            foreach (SLConditionalFormattingRule cfr in this.Rules)
            {
                cf.Append(cfr.ToConditionalFormattingRule());
            }

            return cf;
        }

        internal SLConditionalFormatting Clone()
        {
            SLConditionalFormatting cf = new SLConditionalFormatting();

            int i;
            cf.Rules = new List<SLConditionalFormattingRule>();
            for (i = 0; i < this.Rules.Count; ++i)
            {
                cf.Rules.Add(this.Rules[i].Clone());
            }

            cf.Pivot = this.Pivot;

            cf.SequenceOfReferences = new List<SLCellPointRange>();
            SLCellPointRange cpr;
            for (i = 0; i < this.SequenceOfReferences.Count; ++i)
            {
                cpr = new SLCellPointRange(this.SequenceOfReferences[i].StartRowIndex, this.SequenceOfReferences[i].StartColumnIndex, this.SequenceOfReferences[i].EndRowIndex, this.SequenceOfReferences[i].EndColumnIndex);
                cf.SequenceOfReferences.Add(cpr);
            }

            return cf;
        }
    }
}
