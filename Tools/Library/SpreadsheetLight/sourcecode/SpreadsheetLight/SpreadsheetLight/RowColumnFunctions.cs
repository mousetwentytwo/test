using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    public partial class SLDocument
    {
        /// <summary>
        /// Indicates if the row has an existing style.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row has an existing style. False otherwise.</returns>
        public bool HasRowStyle(int RowIndex)
        {
            bool result = false;
            if (slws.RowProperties.ContainsKey(RowIndex))
            {
                SLRowProperties rp = slws.RowProperties[RowIndex];
                if (rp.StyleIndex > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the row height. If the row doesn't have a height explicitly set, the default row height for the current worksheet is returned.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>The row height in points.</returns>
        public double GetRowHeight(int RowIndex)
        {
            double fHeight = slws.SheetFormatProperties.DefaultRowHeight;
            if (slws.RowProperties.ContainsKey(RowIndex))
            {
                SLRowProperties rp = slws.RowProperties[RowIndex];
                if (rp.HasHeight)
                {
                    fHeight = rp.Height;
                }
            }

            return fHeight;
        }

        /// <summary>
        /// Set the row height.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="RowHeight">The row height in points.</param>
        /// <returns>True if the row index is valid. False otherwise.</returns>
        public bool SetRowHeight(int RowIndex, double RowHeight)
        {
            return SetRowHeight(RowIndex, RowIndex, RowHeight);
        }

        /// <summary>
        /// Set the row height for a range of rows.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <param name="RowHeight">The row height in points.</param>
        /// <returns>True if the row indices are valid. False otherwise.</returns>
        public bool SetRowHeight(int StartRowIndex, int EndRowIndex, double RowHeight)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0;
                SLRowProperties rp;
                for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
                {
                    if (slws.RowProperties.ContainsKey(i))
                    {
                        rp = slws.RowProperties[i];
                        rp.Height = RowHeight;
                        slws.RowProperties[i] = rp;
                    }
                    else
                    {
                        rp = new SLRowProperties(SimpleTheme.ThemeRowHeight);
                        rp.Height = RowHeight;
                        slws.RowProperties.Add(i, rp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Automatically fit row height according to cell contents.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        public void AutoFitRow(int RowIndex)
        {
            this.AutoFitRow(RowIndex, RowIndex);
        }

        /// <summary>
        /// Automatically fit row height according to cell contents.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        public void AutoFitRow(int StartRowIndex, int EndRowIndex)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            if (iStartRowIndex < 1) iStartRowIndex = 1;
            if (iStartRowIndex > SLConstants.RowLimit) iStartRowIndex = SLConstants.RowLimit;
            if (iEndRowIndex < 1) iEndRowIndex = 1;
            if (iEndRowIndex > SLConstants.RowLimit) iEndRowIndex = SLConstants.RowLimit;

            Dictionary<int, int> pixellength = this.AutoFitRowColumn(true, iStartRowIndex, iEndRowIndex);

            double fResolution = 96.0;
            using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(32, 32))
            {
                fResolution = (double)bm.VerticalResolution;
            }

            double fDefaultRowHeight = slws.SheetFormatProperties.DefaultRowHeight;
            double fMinimumHeight = 0;

            SLStyle style;
            string sFontName;
            double fFontSize;
            bool bBold;
            bool bItalic;
            bool bStrike;
            bool bUnderline;
            System.Drawing.FontStyle drawstyle = System.Drawing.FontStyle.Regular;
            System.Drawing.SizeF szf;

            SLRowProperties rp;
            double fRowHeight;
            int iPixelLength;
            foreach (int pixlenpt in pixellength.Keys)
            {
                iPixelLength = pixellength[pixlenpt];
                if (iPixelLength > 0)
                {
                    // height in points = number of pixels * 72 (points per inch) / resolution (DPI)
                    fRowHeight = (double)iPixelLength * 72.0 / fResolution;
                    if (slws.RowProperties.ContainsKey(pixlenpt))
                    {
                        rp = slws.RowProperties[pixlenpt];

                        // if the row has a style, we have to check if the resulting row height
                        // based on the typeface, boldness, italicise-ness and whatnot will change.
                        // Basically we're calculating a "default" row height based on the typeface
                        // set on the entire row.
                        style = new SLStyle();
                        style.FromHash(listStyle[(int)rp.StyleIndex]);
                        if (style.HasFont)
                        {
                            sFontName = SimpleTheme.MinorLatinFont;
                            fFontSize = SLConstants.DefaultFontSize;
                            bBold = false;
                            bItalic = false;
                            bStrike = false;
                            bUnderline = false;
                            drawstyle = System.Drawing.FontStyle.Regular;
                            if (style.fontReal.HasFontScheme)
                            {
                                if (style.fontReal.FontScheme == FontSchemeValues.Major) sFontName = SimpleTheme.MajorLatinFont;
                                else if (style.fontReal.FontScheme == FontSchemeValues.Minor) sFontName = SimpleTheme.MinorLatinFont;
                                else if (style.fontReal.FontName.Length > 0) sFontName = style.fontReal.FontName;
                            }
                            else
                            {
                                if (style.fontReal.FontName.Length > 0) sFontName = style.fontReal.FontName;
                            }

                            if (style.fontReal.FontSize != null) fFontSize = style.fontReal.FontSize.Value;
                            if (style.fontReal.Bold != null && style.fontReal.Bold.Value) bBold = true;
                            if (style.fontReal.Italic != null && style.fontReal.Italic.Value) bItalic = true;
                            if (style.fontReal.Strike != null && style.fontReal.Strike.Value) bStrike = true;
                            if (style.fontReal.HasUnderline) bUnderline = true;

                            if (bBold) drawstyle |= System.Drawing.FontStyle.Bold;
                            if (bItalic) drawstyle |= System.Drawing.FontStyle.Italic;
                            if (bStrike) drawstyle |= System.Drawing.FontStyle.Strikeout;
                            if (bUnderline) drawstyle |= System.Drawing.FontStyle.Underline;

                            // any text will do. Apparently the height is the same regardless.
                            szf = SLTool.MeasureText("0123456789", sFontName, fFontSize, drawstyle);

                            fMinimumHeight = Math.Min(szf.Height, fDefaultRowHeight);
                        }
                        else
                        {
                            fMinimumHeight = fDefaultRowHeight;
                        }

                        if (fRowHeight > fMinimumHeight)
                        {
                            rp.Height = fRowHeight;
                            rp.CustomHeight = false;
                            slws.RowProperties[pixlenpt] = rp.Clone();
                        }
                    }
                    else
                    {
                        rp = new SLRowProperties(SimpleTheme.ThemeRowHeight);
                        rp.Height = fRowHeight;
                        rp.CustomHeight = false;
                        slws.RowProperties[pixlenpt] = rp.Clone();
                    }
                }
                else
                {
                    // else we set autoheight. Meaning we set the default height for any
                    // existing rows.

                    if (slws.RowProperties.ContainsKey(pixlenpt))
                    {
                        rp = slws.RowProperties[pixlenpt];
                        rp.Height = SimpleTheme.ThemeRowHeight;
                        rp.CustomHeight = false;
                        slws.RowProperties[pixlenpt] = rp.Clone();
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if the row is hidden.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row is hidden. False otherwise.</returns>
        public bool IsRowHidden(int RowIndex)
        {
            bool result = false;
            if (slws.RowProperties.ContainsKey(RowIndex))
            {
                SLRowProperties rp = slws.RowProperties[RowIndex];
                result = rp.Hidden;
            }

            return result;
        }

        private bool ToggleRowHidden(int StartRowIndex, int EndRowIndex, bool Hidden)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0;
                SLRowProperties rp;
                for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
                {
                    if (slws.RowProperties.ContainsKey(i))
                    {
                        rp = slws.RowProperties[i];
                        rp.Hidden = Hidden;
                        slws.RowProperties[i] = rp;
                    }
                    else
                    {
                        rp = new SLRowProperties(SimpleTheme.ThemeRowHeight);
                        rp.Hidden = Hidden;
                        slws.RowProperties.Add(i, rp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hide the row.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row index is valid. False otherwise.</returns>
        public bool HideRow(int RowIndex)
        {
            return ToggleRowHidden(RowIndex, RowIndex, true);
        }

        /// <summary>
        /// Hide a range of rows.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <returns>True if the row indices are valid. False otherwise.</returns>
        public bool HideRow(int StartRowIndex, int EndRowIndex)
        {
            return ToggleRowHidden(StartRowIndex, EndRowIndex, true);
        }

        /// <summary>
        /// Unhide the row.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row index is valid. False otherwise.</returns>
        public bool UnhideRow(int RowIndex)
        {
            return ToggleRowHidden(RowIndex, RowIndex, false);
        }

        /// <summary>
        /// Unhide a range of rows.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <returns>True if the row indices are valid. False otherwise.</returns>
        public bool UnhideRow(int StartRowIndex, int EndRowIndex)
        {
            return ToggleRowHidden(StartRowIndex, EndRowIndex, false);
        }

        // not supporting row grouping
        // The following algorithm is wrong!!

        //public bool GroupRows(int StartRowIndex, int EndRowIndex)
        //{
        //    int iStartRowIndex = 1, iEndRowIndex = 1;
        //    if (StartRowIndex < EndRowIndex)
        //    {
        //        iStartRowIndex = StartRowIndex;
        //        iEndRowIndex = EndRowIndex;
        //    }
        //    else
        //    {
        //        iStartRowIndex = EndRowIndex;
        //        iEndRowIndex = StartRowIndex;
        //    }

        //    bool result = false;
        //    if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
        //    {
        //        result = true;
        //        int i = 0;
        //        SLRowProperties rp;
        //        for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
        //        {
        //            if (slws.RowProperties.ContainsKey(i))
        //            {
        //                rp = slws.RowProperties[i];
        //                rp.byOutlineLevel = (byte)(rp.byOutlineLevel + 1);
        //                if (rp.byOutlineLevel > 8) rp.byOutlineLevel = 8;
        //                slws.RowProperties[i] = rp;
        //            }
        //            else
        //            {
        //                rp = new SLRowProperties();
        //                rp.byOutlineLevel = (byte)(rp.byOutlineLevel + 1);
        //                if (rp.byOutlineLevel > 8) rp.byOutlineLevel = 8;
        //                slws.RowProperties.Add(i, rp);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public bool UngroupRows(int StartRowIndex, int EndRowIndex)
        //{
        //    int iStartRowIndex = 1, iEndRowIndex = 1;
        //    if (StartRowIndex < EndRowIndex)
        //    {
        //        iStartRowIndex = StartRowIndex;
        //        iEndRowIndex = EndRowIndex;
        //    }
        //    else
        //    {
        //        iStartRowIndex = EndRowIndex;
        //        iEndRowIndex = StartRowIndex;
        //    }

        //    bool result = false;
        //    if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
        //    {
        //        result = true;
        //        int i = 0;
        //        int iOutlineLevel = 0;
        //        SLRowProperties rp;
        //        for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
        //        {
        //            if (slws.RowProperties.ContainsKey(i))
        //            {
        //                rp = slws.RowProperties[i];
        //                iOutlineLevel = rp.byOutlineLevel - 1;
        //                if (iOutlineLevel < 0) iOutlineLevel = 0;
        //                rp.byOutlineLevel = (byte)iOutlineLevel;
        //                slws.RowProperties[i] = rp;
        //            }
        //            else
        //            {
        //                rp = new SLRowProperties();
        //                iOutlineLevel = rp.byOutlineLevel - 1;
        //                if (iOutlineLevel < 0) iOutlineLevel = 0;
        //                rp.byOutlineLevel = (byte)iOutlineLevel;
        //                slws.RowProperties.Add(i, rp);
        //            }
        //        }
        //    }

        //    return result;
        //}

        // not supporting row collapsing

        /// <summary>
        /// Indicates if the row has a thick top.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row has a thick top. False otherwise.</returns>
        public bool IsRowThickTopped(int RowIndex)
        {
            bool result = false;
            if (slws.RowProperties.ContainsKey(RowIndex))
            {
                SLRowProperties rp = slws.RowProperties[RowIndex];
                result = rp.ThickTop;
            }

            return result;
        }

        /// <summary>
        /// Set the thick top property of the row.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="IsThickTopped">True if the row should have a thick top. False otherwise.</param>
        /// <returns>True if the row index is valid. False otherwise.</returns>
        public bool SetRowThickTop(int RowIndex, bool IsThickTopped)
        {
            return SetRowThickTop(RowIndex, RowIndex, IsThickTopped);
        }

        /// <summary>
        /// Set the thick top property of a range of rows.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <param name="IsThickTopped">True if the rows should have a thick top. False otherwise.</param>
        /// <returns>True if the row indices are valid. False otherwise.</returns>
        public bool SetRowThickTop(int StartRowIndex, int EndRowIndex, bool IsThickTopped)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0;
                SLRowProperties rp;
                for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
                {
                    if (slws.RowProperties.ContainsKey(i))
                    {
                        rp = slws.RowProperties[i];
                        rp.ThickTop = IsThickTopped;
                        slws.RowProperties[i] = rp;
                    }
                    else
                    {
                        rp = new SLRowProperties(SimpleTheme.ThemeRowHeight);
                        rp.ThickTop = IsThickTopped;
                        slws.RowProperties.Add(i, rp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Indicates if the row has a thick bottom.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row has a thick bottom. False otherwise.</returns>
        public bool IsRowThickBottomed(int RowIndex)
        {
            bool result = false;
            if (slws.RowProperties.ContainsKey(RowIndex))
            {
                SLRowProperties rp = slws.RowProperties[RowIndex];
                result = rp.ThickBottom;
            }

            return result;
        }

        /// <summary>
        /// Set the thick bottom property of the row.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="IsThickBottomed">True if the row should have a thick bottom. False otherwise.</param>
        /// <returns>True if the row index is valid. False otherwise.</returns>
        public bool SetRowThickBottomed(int RowIndex, bool IsThickBottomed)
        {
            return SetRowThickBottomed(RowIndex, RowIndex, IsThickBottomed);
        }

        /// <summary>
        /// Set the thick bottom property of a range of rows.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <param name="IsThickBottomed">True if the rows should have a thick bottom. False otherwise.</param>
        /// <returns>True if the row indices are valid. False otherwise.</returns>
        public bool SetRowThickBottomed(int StartRowIndex, int EndRowIndex, bool IsThickBottomed)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0;
                SLRowProperties rp;
                for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
                {
                    if (slws.RowProperties.ContainsKey(i))
                    {
                        rp = slws.RowProperties[i];
                        rp.ThickBottom = IsThickBottomed;
                        slws.RowProperties[i] = rp;
                    }
                    else
                    {
                        rp = new SLRowProperties(SimpleTheme.ThemeRowHeight);
                        rp.ThickBottom = IsThickBottomed;
                        slws.RowProperties.Add(i, rp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Indicates if the row is showing phonetic information.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <returns>True if the row is showing phonetic information. False otherwise.</returns>
        public bool IsRowShowingPhonetic(int RowIndex)
        {
            bool result = false;
            if (slws.RowProperties.ContainsKey(RowIndex))
            {
                SLRowProperties rp = slws.RowProperties[RowIndex];
                result = rp.ShowPhonetic;
            }

            return result;
        }

        /// <summary>
        /// Set the show phonetic property for the row.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ShowPhonetic">True if the row should show phonetic information. False otherwise.</param>
        /// <returns>True if the row index is valid. False otherwise.</returns>
        public bool SetRowShowPhonetic(int RowIndex, bool ShowPhonetic)
        {
            return SetRowShowPhonetic(RowIndex, RowIndex, ShowPhonetic);
        }

        /// <summary>
        /// Set the show phonetic property for a range of rows.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <param name="ShowPhonetic">True if the rows should show phonetic information. False otherwise.</param>
        /// <returns>True if the row indices are valid. False otherwise.</returns>
        public bool SetRowShowPhonetic(int StartRowIndex, int EndRowIndex, bool ShowPhonetic)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0;
                SLRowProperties rp;
                for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
                {
                    if (slws.RowProperties.ContainsKey(i))
                    {
                        rp = slws.RowProperties[i];
                        rp.ShowPhonetic = ShowPhonetic;
                        slws.RowProperties[i] = rp;
                    }
                    else
                    {
                        rp = new SLRowProperties(SimpleTheme.ThemeRowHeight);
                        rp.ShowPhonetic = ShowPhonetic;
                        slws.RowProperties.Add(i, rp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Copy one row to another row.
        /// </summary>
        /// <param name="RowIndex">The row index of the row to be copied from.</param>
        /// <param name="AnchorRowIndex">The row index of the row to be copied to.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyRow(int RowIndex, int AnchorRowIndex)
        {
            return CopyRow(RowIndex, RowIndex, AnchorRowIndex, false);
        }

        /// <summary>
        /// Copy one row to another row.
        /// </summary>
        /// <param name="RowIndex">The row index of the row to be copied from.</param>
        /// <param name="AnchorRowIndex">The row index of the row to be copied to.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyRow(int RowIndex, int AnchorRowIndex, bool ToCut)
        {
            return CopyRow(RowIndex, RowIndex, AnchorRowIndex, ToCut);
        }

        /// <summary>
        /// Copy a range of rows to another range, given the anchor row of the destination range (top row).
        /// </summary>
        /// <param name="StartRowIndex">The row index of the start row of the row range. This is typically the top row.</param>
        /// <param name="EndRowIndex">The row index of the end row of the row range. This is typically the bottom row.</param>
        /// <param name="AnchorRowIndex">The row index of the anchor row.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyRow(int StartRowIndex, int EndRowIndex, int AnchorRowIndex)
        {
            return CopyRow(StartRowIndex, EndRowIndex, AnchorRowIndex, false);
        }

        /// <summary>
        /// Copy a range of rows to another range, given the anchor row of the destination range (top row).
        /// </summary>
        /// <param name="StartRowIndex">The row index of the start row of the row range. This is typically the top row.</param>
        /// <param name="EndRowIndex">The row index of the end row of the row range. This is typically the bottom row.</param>
        /// <param name="AnchorRowIndex">The row index of the anchor row.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyRow(int StartRowIndex, int EndRowIndex, int AnchorRowIndex, bool ToCut)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            bool result = false;
            if (iStartRowIndex >= 1 && iStartRowIndex <= SLConstants.RowLimit
                && iEndRowIndex >= 1 && iEndRowIndex <= SLConstants.RowLimit
                && AnchorRowIndex >= 1 && AnchorRowIndex <= SLConstants.RowLimit
                && iStartRowIndex != AnchorRowIndex)
            {
                result = true;

                int diff = AnchorRowIndex - iStartRowIndex;
                int i = 0;
                Dictionary<int, SLRowProperties> rows = new Dictionary<int, SLRowProperties>();
                for (i = iStartRowIndex; i <= iEndRowIndex; ++i)
                {
                    if (slws.RowProperties.ContainsKey(i))
                    {
                        rows[i + diff] = slws.RowProperties[i].Clone();
                        if (ToCut)
                        {
                            slws.RowProperties.Remove(i);
                        }
                    }
                }

                if (ToCut) slws.RemoveRowColumnStyleHistory(true, iStartRowIndex, iEndRowIndex);

                int AnchorEndRowIndex = AnchorRowIndex + iEndRowIndex - iStartRowIndex;
                // removing rows within destination "paste" operation
                foreach (var r in slws.RowProperties)
                {
                    if (r.Key >= AnchorRowIndex && r.Key <= AnchorEndRowIndex)
                    {
                        slws.RowProperties.Remove(r.Key);
                    }
                }

                foreach (var key in rows.Keys)
                {
                    slws.RowProperties[key] = rows[key];
                    if (rows[key].StyleIndex > 0)
                    {
                        slws.RowColumnStyleHistory.Add(new SLRowColumnStyleHistory(true, key));
                    }
                }

                Dictionary<SLCellPoint, SLCell> cells = new Dictionary<SLCellPoint, SLCell>();
                List<SLCellPoint> listCellKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                foreach (SLCellPoint pt in listCellKeys)
                {
                    if (pt.RowIndex >= iStartRowIndex && pt.RowIndex <= iEndRowIndex)
                    {
                        cells[new SLCellPoint(pt.RowIndex + diff, pt.ColumnIndex)] = slws.Cells[pt].Clone();
                        if (ToCut)
                        {
                            slws.Cells.Remove(pt);
                        }
                    }
                }

                listCellKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                foreach (SLCellPoint pt in listCellKeys)
                {
                    // any cell within destination "paste" operation is taken out
                    if (pt.RowIndex >= AnchorRowIndex && pt.RowIndex <= AnchorEndRowIndex)
                    {
                        slws.Cells.Remove(pt);
                    }
                }

                int iNumberOfRows = iEndRowIndex - iStartRowIndex + 1;
                if (AnchorRowIndex <= iStartRowIndex) iNumberOfRows = -iNumberOfRows;

                SLCell c;
                foreach (var key in cells.Keys)
                {
                    c = cells[key];
                    this.ProcessCellFormulaDelta(ref c, AnchorRowIndex, iNumberOfRows, -1, 0);
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
                    if (mc.StartRowIndex >= iStartRowIndex && mc.EndRowIndex <= iEndRowIndex)
                    {
                        if (ToCut)
                        {
                            slws.MergeCells.Remove(mc);
                        }
                        this.MergeWorksheetCells(mc.StartRowIndex + diff, mc.StartColumnIndex, mc.EndRowIndex + diff, mc.EndColumnIndex);
                    }
                }

                #region Calculation cells
                if (slwb.CalculationCells.Count > 0)
                {
                    List<int> listToDelete = new List<int>();
                    int iRowLimit = AnchorRowIndex + iStartRowIndex - iEndRowIndex;
                    for (i = 0; i < slwb.CalculationCells.Count; ++i)
                    {
                        if (slwb.CalculationCells[i].SheetId == giSelectedWorksheetID)
                        {
                            if (ToCut && slwb.CalculationCells[i].RowIndex >= iStartRowIndex && slwb.CalculationCells[i].RowIndex <= iEndRowIndex)
                            {
                                // just remove because recalculation of cell references is too complicated...
                                if (!listToDelete.Contains(i)) listToDelete.Add(i);
                            }

                            if (slwb.CalculationCells[i].RowIndex >= AnchorRowIndex && slwb.CalculationCells[i].RowIndex <= iRowLimit)
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
        /// Insert one or more rows.
        /// </summary>
        /// <param name="StartRowIndex">Additional rows are inserted at this row index.</param>
        /// <param name="NumberOfRows">The number of rows to insert.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool InsertRow(int StartRowIndex, int NumberOfRows)
        {
            if (NumberOfRows < 1) return false;

            bool result = false;
            if (StartRowIndex >= 1 && StartRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0, iNewIndex = 0;

                int index = 0;
                int iRowIndex = -1;
                int iRowIndex2 = -1;

                #region Tables
                if (slws.Tables.Count > 0)
                {
                    foreach (SLTable t in slws.Tables)
                    {
                        iRowIndex = t.StartRowIndex;
                        iRowIndex2 = t.EndRowIndex;
                        this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref iRowIndex, ref iRowIndex2);
                        if (iRowIndex != t.StartRowIndex || iRowIndex2 != t.EndRowIndex) t.IsNewTable = true;
                        t.StartRowIndex = iRowIndex;
                        t.EndRowIndex = iRowIndex2;

                        if (t.HasAutoFilter)
                        {
                            iRowIndex = t.AutoFilter.StartRowIndex;
                            iRowIndex2 = t.AutoFilter.EndRowIndex;
                            this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref iRowIndex, ref iRowIndex2);
                            if (iRowIndex != t.AutoFilter.StartRowIndex || iRowIndex2 != t.AutoFilter.EndRowIndex) t.IsNewTable = true;
                            t.AutoFilter.StartRowIndex = iRowIndex;
                            t.AutoFilter.EndRowIndex = iRowIndex2;

                            if (t.AutoFilter.HasSortState)
                            {
                                iRowIndex = t.AutoFilter.SortState.StartRowIndex;
                                iRowIndex2 = t.AutoFilter.SortState.EndRowIndex;
                                this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref iRowIndex, ref iRowIndex2);
                                if (iRowIndex != t.AutoFilter.SortState.StartRowIndex || iRowIndex2 != t.AutoFilter.SortState.EndRowIndex) t.IsNewTable = true;
                                t.AutoFilter.SortState.StartRowIndex = iRowIndex;
                                t.AutoFilter.SortState.EndRowIndex = iRowIndex2;
                            }
                        }

                        if (t.HasSortState)
                        {
                            iRowIndex = t.SortState.StartRowIndex;
                            iRowIndex2 = t.SortState.EndRowIndex;
                            this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref iRowIndex, ref iRowIndex2);
                            if (iRowIndex != t.SortState.StartRowIndex || iRowIndex2 != t.SortState.EndRowIndex) t.IsNewTable = true;
                            t.SortState.StartRowIndex = iRowIndex;
                            t.SortState.EndRowIndex = iRowIndex2;
                        }
                    }
                }
                #endregion

                #region Row properties
                SLRowProperties rp;
                List<int> listRowIndex = slws.RowProperties.Keys.ToList<int>();
                // this sorting in descending order is crucial!
                // we move the data from after the insert range to their new reference keys
                // first, then we put in the new data, which will then have no data
                // key collision.
                listRowIndex.Sort();
                listRowIndex.Reverse();

                for (i = 0; i < listRowIndex.Count; ++i)
                {
                    index = listRowIndex[i];
                    if (index >= StartRowIndex)
                    {
                        rp = slws.RowProperties[index];
                        slws.RowProperties.Remove(index);
                        slws.RemoveRowColumnStyleHistory(true, index, index);
                        iNewIndex = index + NumberOfRows;
                        // if the new row is below the bottom limit of the worksheet,
                        // then it disappears into the ether...
                        if (iNewIndex <= SLConstants.RowLimit)
                        {
                            slws.RowProperties[iNewIndex] = rp;
                            if (rp.StyleIndex > 0)
                            {
                                slws.RowColumnStyleHistory.Add(new SLRowColumnStyleHistory(true, iNewIndex));
                            }
                        }
                    }
                    else
                    {
                        // the rows before the start row are unaffected by the insertion.
                        // Because it's sorted in descending order, we can just break out.
                        break;
                    }
                }
                #endregion

                #region Cell data
                List<SLCellPoint> listCellRefKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                // this sorting in descending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());
                listCellRefKeys.Reverse();

                SLCell c;
                SLCellPoint pt;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    c = slws.Cells[pt];
                    this.ProcessCellFormulaDelta(ref c, StartRowIndex, NumberOfRows, -1, 0);

                    if (pt.RowIndex >= StartRowIndex)
                    {
                        slws.Cells.Remove(pt);
                        iNewIndex = pt.RowIndex + NumberOfRows;
                        if (iNewIndex <= SLConstants.RowLimit)
                        {
                            slws.Cells[new SLCellPoint(iNewIndex, pt.ColumnIndex)] = c;
                        }
                    }
                    else
                    {
                        slws.Cells[pt] = c;
                    }
                }

                #region Cell comments
                listCellRefKeys = slws.Comments.Keys.ToList<SLCellPoint>();
                // this sorting in descending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());
                listCellRefKeys.Reverse();

                SLComment comm;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    comm = slws.Comments[pt];
                    if (pt.RowIndex >= StartRowIndex)
                    {
                        slws.Comments.Remove(pt);
                        iNewIndex = pt.RowIndex + NumberOfRows;
                        if (iNewIndex <= SLConstants.RowLimit)
                        {
                            slws.Comments[new SLCellPoint(iNewIndex, pt.ColumnIndex)] = comm;
                        }
                    }
                    // no else because there's nothing done
                }
                #endregion

                #endregion

                #region Merge cells
                if (slws.MergeCells.Count > 0)
                {
                    SLMergeCell mc;
                    for (i = 0; i < slws.MergeCells.Count; ++i)
                    {
                        mc = slws.MergeCells[i];
                        this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref mc.iStartRowIndex, ref mc.iEndRowIndex);
                        slws.MergeCells[i] = mc;
                    }
                }
                #endregion

                // TODO picture/worksheet drawings
                //WorksheetPart wsp = (WorksheetPart)wbp.GetPartById(gsSelectedWorksheetRelationshipID);
                //if (wsp.DrawingsPart != null)
                //{
                //    DrawingsPart dp = wsp.DrawingsPart;
                //}

                #region Calculation chain
                if (slwb.CalculationCells.Count > 0)
                {
                    foreach (SLCalculationCell cc in slwb.CalculationCells)
                    {
                        if (cc.SheetId == giSelectedWorksheetID)
                        {
                            iRowIndex = cc.RowIndex;
                            // don't need this but assign something anyway...
                            iRowIndex2 = SLConstants.RowLimit;

                            this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref iRowIndex, ref iRowIndex2);
                            cc.RowIndex = iRowIndex;
                        }
                    }
                }
                #endregion

                #region Defined names
                if (slwb.DefinedNames.Count > 0)
                {
                    string sDefinedNameText = string.Empty;
                    foreach (SLDefinedName d in slwb.DefinedNames)
                    {
                        sDefinedNameText = d.Text;
                        sDefinedNameText = AddDeleteCellFormulaDelta(sDefinedNameText, StartRowIndex, NumberOfRows, -1, 0);
                        sDefinedNameText = AddDeleteDefinedNameRowColumnRangeDelta(sDefinedNameText, true, StartRowIndex, NumberOfRows);
                        d.Text = sDefinedNameText;
                    }
                }
                #endregion

                #region Sparklines
                if (slws.SparklineGroups.Count > 0)
                {
                    SLSparkline spk;
                    foreach (SLSparklineGroup spkgrp in slws.SparklineGroups)
                    {
                        if (spkgrp.DateAxis && spkgrp.DateWorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref spkgrp.DateStartRowIndex, ref spkgrp.DateEndRowIndex);
                        }

                        // starting from the end is important because we might be deleting!
                        for (i = spkgrp.Sparklines.Count - 1; i >= 0; --i)
                        {
                            spk = spkgrp.Sparklines[i];

                            if (spk.LocationRowIndex >= StartRowIndex)
                            {
                                iNewIndex = spk.LocationRowIndex + NumberOfRows;
                                if (iNewIndex <= SLConstants.RowLimit)
                                {
                                    spk.LocationRowIndex = iNewIndex;
                                }
                                else
                                {
                                    // out of range!
                                    spkgrp.Sparklines.RemoveAt(i);
                                    continue;
                                }
                            }
                            // else the location is before the start row so don't have to do anything

                            // process only if the data source is on the currently selected worksheet
                            if (spk.WorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.AddRowColumnIndexDelta(StartRowIndex, NumberOfRows, true, ref spk.StartRowIndex, ref spk.EndRowIndex);
                            }

                            spkgrp.Sparklines[i] = spk;
                        }
                    }
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Delete one or more rows.
        /// </summary>
        /// <param name="StartRowIndex">Rows will be deleted from this row index, including this row itself.</param>
        /// <param name="NumberOfRows">Number of rows to delete.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool DeleteRow(int StartRowIndex, int NumberOfRows)
        {
            if (NumberOfRows < 1) return false;

            bool result = false;
            if (StartRowIndex >= 1 && StartRowIndex <= SLConstants.RowLimit)
            {
                result = true;
                int i = 0, iNewIndex = 0;
                int iEndRowIndex = StartRowIndex + NumberOfRows - 1;
                if (iEndRowIndex > SLConstants.RowLimit) iEndRowIndex = SLConstants.RowLimit;
                // this autocorrects in the case of overshooting the row limit
                int iNumberOfRows = iEndRowIndex - StartRowIndex + 1;

                int index = 0;
                int iRowIndex = -1;
                int iRowIndex2 = -1;

                // tables part has to be at the beginning because we need to check if
                // the header row of any table is within delete range, BUT the whole of that
                // table is NOT within delete range (meaning we're deleting the header row
                // without deleting the whole table), and we exit the function (because
                // Excel doesn't allow this behaviour).
                #region Tables
                if (slws.Tables.Count > 0)
                {
                    SLTable t;
                    #region Table header check
                    for (i = 0; i < slws.Tables.Count; ++i)
                    {
                        t = slws.Tables[i];
                        if (t.HeaderRowCount > 0)
                        {
                            if (StartRowIndex <= t.StartRowIndex && t.StartRowIndex <= iEndRowIndex && iEndRowIndex < t.EndRowIndex)
                            {
                                // the delete range includes a header row, BUT does not
                                // delete the whole table.
                                return false;
                            }
                        }

                        // check if the delete range contains the body of the table
                        // Excel allows this, but keeps an empty row afterwards.
                        // This means even though 6 rows are to be deleted, Excel only deletes
                        // 5 rows, leaving an empty row after that.
                        // Without visual feedback, this is difficult to keep track from the calling
                        // program, so we'll just disallow this.
                        if (t.HeaderRowCount > 0 && t.TotalsRowCount > 0)
                        {
                            if ((StartRowIndex == (t.StartRowIndex + 1)) && (iEndRowIndex == (t.EndRowIndex - 1)))
                            {
                                return false;
                            }
                        }
                        else if (t.HeaderRowCount > 0 && t.TotalsRowCount == 0)
                        {
                            if ((StartRowIndex == (t.StartRowIndex + 1)) && (iEndRowIndex >= t.EndRowIndex))
                            {
                                return false;
                            }
                        }
                        else if (t.HeaderRowCount == 0 && t.TotalsRowCount > 0)
                        {
                            if ((StartRowIndex <= t.StartRowIndex) && (iEndRowIndex == (t.EndRowIndex - 1)))
                            {
                                return false;
                            }
                        }

                        // else there are no header rows or totals row.
                        // and if the body of the table is within delete range,
                        // then it's taken care of below.
                    }
                    #endregion

                    List<int> listTablesToDelete = new List<int>();
                    for (i = 0; i < slws.Tables.Count; ++i)
                    {
                        t = slws.Tables[i];
                        if (StartRowIndex <= t.StartRowIndex && t.EndRowIndex <= iEndRowIndex)
                        {
                            // table is completely within delete range, so delete the whole table
                            listTablesToDelete.Add(i);
                            continue;
                        }
                        else
                        {
                            if (t.TotalsRowCount > 0)
                            {
                                // the totals row is within delete range
                                if (StartRowIndex <= t.EndRowIndex && t.EndRowIndex <= iEndRowIndex)
                                {
                                    // should be just minus 1, but we'll just do this instead...
                                    t.EndRowIndex -= (int)t.TotalsRowCount;
                                    t.TotalsRowCount = 0;
                                    t.IsNewTable = true;
                                }
                            }

                            iRowIndex = t.StartRowIndex;
                            iRowIndex2 = t.EndRowIndex;
                            this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref iRowIndex, ref iRowIndex2);
                            if (iRowIndex != t.StartRowIndex || iRowIndex2 != t.EndRowIndex) t.IsNewTable = true;
                            t.StartRowIndex = iRowIndex;
                            t.EndRowIndex = iRowIndex2;
                        }

                        if (t.HasAutoFilter)
                        {
                            // if the autofilter range is completely within delete range,
                            // then it's already taken care off above.
                            iRowIndex = t.AutoFilter.StartRowIndex;
                            iRowIndex2 = t.AutoFilter.EndRowIndex;
                            this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref iRowIndex, ref iRowIndex2);
                            if (iRowIndex != t.AutoFilter.StartRowIndex || iRowIndex2 != t.AutoFilter.EndRowIndex) t.IsNewTable = true;
                            t.AutoFilter.StartRowIndex = iRowIndex;
                            t.AutoFilter.EndRowIndex = iRowIndex2;

                            if (t.AutoFilter.HasSortState)
                            {
                                // if the sort state range is completely within delete range,
                                // then it's already taken care off above.
                                iRowIndex = t.AutoFilter.SortState.StartRowIndex;
                                iRowIndex2 = t.AutoFilter.SortState.EndRowIndex;
                                this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref iRowIndex, ref iRowIndex2);
                                if (iRowIndex != t.AutoFilter.SortState.StartRowIndex || iRowIndex2 != t.AutoFilter.SortState.EndRowIndex) t.IsNewTable = true;
                                t.AutoFilter.SortState.StartRowIndex = iRowIndex;
                                t.AutoFilter.SortState.EndRowIndex = iRowIndex2;
                            }
                        }

                        if (t.HasSortState)
                        {
                            // if the sort state range is completely within delete range,
                            // then it's already taken care off above.
                            iRowIndex = t.SortState.StartRowIndex;
                            iRowIndex2 = t.SortState.EndRowIndex;
                            this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref iRowIndex, ref iRowIndex2);
                            if (iRowIndex != t.SortState.StartRowIndex || iRowIndex2 != t.SortState.EndRowIndex) t.IsNewTable = true;
                            t.SortState.StartRowIndex = iRowIndex;
                            t.SortState.EndRowIndex = iRowIndex2;
                        }
                    }

                    if (listTablesToDelete.Count > 0)
                    {
                        WorksheetPart wsp = (WorksheetPart)wbp.GetPartById(gsSelectedWorksheetRelationshipID);
                        string sTableRelID = string.Empty;
                        string sTableName = string.Empty;
                        uint iTableID = 0;
                        for (i = listTablesToDelete.Count - 1; i >= 0; --i)
                        {
                            // remove IDs and table names from the spreadsheet unique lists
                            iTableID = slws.Tables[listTablesToDelete[i]].Id;
                            if (slwb.TableIds.Contains(iTableID)) slwb.TableIds.Remove(iTableID);
                            
                            sTableName = slws.Tables[listTablesToDelete[i]].DisplayName;
                            if (slwb.TableNames.Contains(sTableName)) slwb.TableNames.Remove(sTableName);

                            sTableRelID = slws.Tables[listTablesToDelete[i]].RelationshipID;
                            if (sTableRelID.Length > 0)
                            {
                                wsp.DeletePart(sTableRelID);
                            }
                            slws.Tables.RemoveAt(listTablesToDelete[i]);
                        }
                    }
                }
                #endregion

                #region Row properties
                SLRowProperties rp;
                List<int> listRowIndex = slws.RowProperties.Keys.ToList<int>();
                // this sorting in ascending order is crucial!
                // we're removing data within delete range, and if the data is after the delete range,
                // then the key is modified. Since the data within delete range is removed,
                // there won't be data key collisions.
                listRowIndex.Sort();

                for (i = 0; i < listRowIndex.Count; ++i)
                {
                    index = listRowIndex[i];
                    if (index >= StartRowIndex && index <= iEndRowIndex)
                    {
                        slws.RowProperties.Remove(index);
                    }
                    else if (index > iEndRowIndex)
                    {
                        rp = slws.RowProperties[index];
                        slws.RowProperties.Remove(index);
                        iNewIndex = index - iNumberOfRows;
                        slws.RowProperties[iNewIndex] = rp;
                    }

                    // the rows before the start row are unaffected by the deleting.
                }
                slws.RemoveRowColumnStyleHistory(true, StartRowIndex, iEndRowIndex);
                #endregion

                #region Cell data
                List<SLCellPoint> listCellRefKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                // this sorting in ascending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());

                SLCell c;
                SLCellPoint pt;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    c = slws.Cells[pt];
                    this.ProcessCellFormulaDelta(ref c, StartRowIndex, -NumberOfRows, -1, 0);

                    if (StartRowIndex <= pt.RowIndex && pt.RowIndex <= iEndRowIndex)
                    {
                        slws.Cells.Remove(pt);
                    }
                    else if (pt.RowIndex > iEndRowIndex)
                    {
                        slws.Cells.Remove(pt);
                        iNewIndex = pt.RowIndex - iNumberOfRows;
                        slws.Cells[new SLCellPoint(iNewIndex, pt.ColumnIndex)] = c;
                    }
                    else
                    {
                        slws.Cells[pt] = c;
                    }
                }

                #region Cell comments
                listCellRefKeys = slws.Comments.Keys.ToList<SLCellPoint>();
                // this sorting in ascending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());

                SLComment comm;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    comm = slws.Comments[pt];
                    if (StartRowIndex <= pt.RowIndex && pt.RowIndex <= iEndRowIndex)
                    {
                        slws.Comments.Remove(pt);
                    }
                    else if (pt.RowIndex > iEndRowIndex)
                    {
                        slws.Comments.Remove(pt);
                        iNewIndex = pt.RowIndex - iNumberOfRows;
                        slws.Comments[new SLCellPoint(iNewIndex, pt.ColumnIndex)] = comm;
                    }
                    // no else because there's nothing done
                }
                #endregion

                #endregion

                #region Merge cells
                if (slws.MergeCells.Count > 0)
                {
                    SLMergeCell mc;
                    // starting from the end is crucial because we might be removing items
                    for (i = slws.MergeCells.Count - 1; i >= 0; --i)
                    {
                        mc = slws.MergeCells[i];
                        if (mc.iStartRowIndex >= StartRowIndex && mc.iEndRowIndex <= iEndRowIndex)
                        {
                            // merge cell is completely within delete range
                            slws.MergeCells.RemoveAt(i);
                        }
                        else
                        {
                            this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref mc.iStartRowIndex, ref mc.iEndRowIndex);
                            slws.MergeCells[i] = mc;
                        }
                    }
                }
                #endregion

                // TODO pictures/charts!

                #region Calculation chain
                if (slwb.CalculationCells.Count > 0)
                {
                    List<int> listToDelete = new List<int>();
                    for (i = 0; i < slwb.CalculationCells.Count; ++i)
                    {
                        if (slwb.CalculationCells[i].SheetId == giSelectedWorksheetID)
                        {
                            if (StartRowIndex <= slwb.CalculationCells[i].RowIndex && slwb.CalculationCells[i].RowIndex <= iEndRowIndex)
                            {
                                listToDelete.Add(i);
                            }
                            else if (iEndRowIndex < slwb.CalculationCells[i].RowIndex)
                            {
                                slwb.CalculationCells[i].RowIndex -= iNumberOfRows;
                            }
                        }
                    }

                    // start from the back because we're deleting elements and we don't want
                    // the indices to get messed up.
                    for (i = listToDelete.Count - 1; i >= 0; --i)
                    {
                        slwb.CalculationCells.RemoveAt(listToDelete[i]);
                    }
                }
                #endregion

                #region Defined names
                if (slwb.DefinedNames.Count > 0)
                {
                    string sDefinedNameText = string.Empty;
                    foreach (SLDefinedName d in slwb.DefinedNames)
                    {
                        sDefinedNameText = d.Text;
                        sDefinedNameText = AddDeleteCellFormulaDelta(sDefinedNameText, StartRowIndex, -NumberOfRows, -1, 0);
                        sDefinedNameText = AddDeleteDefinedNameRowColumnRangeDelta(sDefinedNameText, true, StartRowIndex, -NumberOfRows);
                        d.Text = sDefinedNameText;
                    }
                }
                #endregion

                #region Sparklines
                if (slws.SparklineGroups.Count > 0)
                {
                    SLSparkline spk;
                    foreach (SLSparklineGroup spkgrp in slws.SparklineGroups)
                    {
                        if (spkgrp.DateAxis && spkgrp.DateWorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (StartRowIndex <= spkgrp.DateStartRowIndex && spkgrp.DateEndRowIndex <= iEndRowIndex)
                            {
                                // the whole date range is completely within delete range
                                spkgrp.DateAxis = false;
                            }
                            else
                            {
                                this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref spkgrp.DateStartRowIndex, ref spkgrp.DateEndRowIndex);
                            }
                        }

                        // starting from the end is important because we might be deleting!
                        for (i = spkgrp.Sparklines.Count - 1; i >= 0; --i)
                        {
                            spk = spkgrp.Sparklines[i];

                            if (StartRowIndex <= spk.LocationRowIndex && spk.LocationRowIndex <= iEndRowIndex)
                            {
                                spkgrp.Sparklines.RemoveAt(i);
                                continue;
                            }
                            else if (spk.LocationRowIndex > iEndRowIndex)
                            {
                                iNewIndex = spk.LocationRowIndex - iNumberOfRows;
                                spk.LocationRowIndex = iNewIndex;
                            }
                            // no else because there's nothing done

                            // process only if the data source is on the currently selected worksheet
                            if (spk.WorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (StartRowIndex <= spk.StartRowIndex && spk.EndRowIndex <= iEndRowIndex)
                                {
                                    // the data source is completely within delete range
                                    // Excel 2010 keeps the WorksheetExtension, but I'm gonna just delete the whole thing.
                                    spkgrp.Sparklines.RemoveAt(i);
                                    continue;
                                }
                                else
                                {
                                    this.DeleteRowColumnIndexDelta(StartRowIndex, iEndRowIndex, iNumberOfRows, ref spk.StartRowIndex, ref spk.EndRowIndex);
                                }
                            }

                            spkgrp.Sparklines[i] = spk;
                        }
                    }
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Clear all cell content within specified rows. If the top-left cell of a merged cell is within specified rows, the merged cell content is also cleared.
        /// </summary>
        /// <param name="StartRowIndex">The row index of the starting row.</param>
        /// <param name="EndRowIndex">The row index of the ending row.</param>
        /// <returns>True if content has been cleared. False otherwise. If there are no content within specified rows, false is also returned.</returns>
        public bool ClearRowContent(int StartRowIndex, int EndRowIndex)
        {
            int iStartRowIndex = 1, iEndRowIndex = 1;
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

            if (iStartRowIndex < 1) iStartRowIndex = 1;
            if (iEndRowIndex > SLConstants.RowLimit) iEndRowIndex = SLConstants.RowLimit;

            bool result = false;
            foreach (SLCellPoint pt in slws.Cells.Keys)
            {
                if (iStartRowIndex <= pt.RowIndex && pt.RowIndex <= iEndRowIndex)
                {
                    this.ClearCellContentData(pt);
                }
            }

            return result;
        }

        /// <summary>
        /// Indicates if the column has an existing style.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <returns>True if the column has an existing style. False otherwise.</returns>
        public bool HasColumnStyle(string ColumnName)
        {
            bool result = false;
            result = HasColumnStyle(SLTool.ToColumnIndex(ColumnName));

            return result;
        }

        /// <summary>
        /// Indicates if the column has an existing style.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>True if the column has an existing style. False otherwise.</returns>
        public bool HasColumnStyle(int ColumnIndex)
        {
            bool result = false;
            if (slws.ColumnProperties.ContainsKey(ColumnIndex))
            {
                SLColumnProperties cp = slws.ColumnProperties[ColumnIndex];
                if (cp.StyleIndex > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the column width. If the column doesn't have a width explicitly set, the default column width for the current worksheet is returned.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <returns>The column width.</returns>
        public double GetColumnWidth(string ColumnName)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return GetColumnWidth(iColumnIndex);
        }

        /// <summary>
        /// Get the column width. If the column doesn't have a width explicitly set, the default column width for the current worksheet is returned.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>The column width.</returns>
        public double GetColumnWidth(int ColumnIndex)
        {
            double fWidth = slws.SheetFormatProperties.DefaultColumnWidth;
            if (slws.ColumnProperties.ContainsKey(ColumnIndex))
            {
                SLColumnProperties cp = slws.ColumnProperties[ColumnIndex];
                if (cp.HasWidth)
                {
                    fWidth = cp.Width;
                }
            }

            return fWidth;
        }

        /// <summary>
        /// Set the column width.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <param name="ColumnWidth">The column width.</param>
        /// <returns>True if the column name is valid. False otherwise.</returns>
        public bool SetColumnWidth(string ColumnName, double ColumnWidth)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return SetColumnWidth(iColumnIndex, iColumnIndex, ColumnWidth);
        }

        /// <summary>
        /// Set the column width.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="ColumnWidth">The column width.</param>
        /// <returns>True if the column index is valid. False otherwise.</returns>
        public bool SetColumnWidth(int ColumnIndex, double ColumnWidth)
        {
            return SetColumnWidth(ColumnIndex, ColumnIndex, ColumnWidth);
        }

        /// <summary>
        /// Set the column width for a range of columns.
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column.</param>
        /// <param name="EndColumnName">The column name of the end column.</param>
        /// <param name="ColumnWidth">The column width.</param>
        /// <returns>True if the column names are valid. False otherwise.</returns>
        public bool SetColumnWidth(string StartColumnName, string EndColumnName, double ColumnWidth)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);

            return SetColumnWidth(iStartColumnIndex, iEndColumnIndex, ColumnWidth);
        }

        /// <summary>
        /// Set the column width for a range of columns.
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column.</param>
        /// <param name="EndColumnIndex">The column index of the end column.</param>
        /// <param name="ColumnWidth">The column width.</param>
        /// <returns>True if the column indices are valid. False otherwise.</returns>
        public bool SetColumnWidth(int StartColumnIndex, int EndColumnIndex, double ColumnWidth)
        {
            int iStartColumnIndex = 1, iEndColumnIndex = 1;
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
            if (iStartColumnIndex >= 1 && iStartColumnIndex <= SLConstants.ColumnLimit && iEndColumnIndex >= 1 && iEndColumnIndex <= SLConstants.ColumnLimit)
            {
                result = true;
                int i = 0;
                SLColumnProperties cp;
                for (i = iStartColumnIndex; i <= iEndColumnIndex; ++i)
                {
                    if (slws.ColumnProperties.ContainsKey(i))
                    {
                        cp = slws.ColumnProperties[i];
                        cp.Width = ColumnWidth;
                        slws.ColumnProperties[i] = cp;
                    }
                    else
                    {
                        cp = new SLColumnProperties(SimpleTheme.ThemeColumnWidth, SimpleTheme.ThemeColumnWidthInEMU, SimpleTheme.ThemeMaxDigitWidth, SimpleTheme.listColumnStepSize);
                        cp.Width = ColumnWidth;
                        slws.ColumnProperties.Add(i, cp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Automatically fit column width according to cell contents.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        public void AutoFitColumn(string ColumnName)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            this.AutoFitColumn(iColumnIndex, iColumnIndex);
        }

        /// <summary>
        /// Automatically fit column width according to cell contents.
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column.</param>
        /// <param name="EndColumnName">The column name of the end column.</param>
        public void AutoFitColumn(string StartColumnName, string EndColumnName)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);

            this.AutoFitColumn(iStartColumnIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Automatically fit column width according to cell contents.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        public void AutoFitColumn(int ColumnIndex)
        {
            this.AutoFitColumn(ColumnIndex, ColumnIndex);
        }

        /// <summary>
        /// Automatically fit column width according to cell contents.
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column.</param>
        /// <param name="EndColumnIndex">The column index of the end column.</param>
        public void AutoFitColumn(int StartColumnIndex, int EndColumnIndex)
        {
            int iStartColumnIndex = 1, iEndColumnIndex = 1;
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

            if (iStartColumnIndex < 1) iStartColumnIndex = 1;
            if (iStartColumnIndex > SLConstants.ColumnLimit) iStartColumnIndex = SLConstants.ColumnLimit;
            if (iEndColumnIndex < 1) iEndColumnIndex = 1;
            if (iEndColumnIndex > SLConstants.ColumnLimit) iEndColumnIndex = SLConstants.ColumnLimit;

            Dictionary<int, int> pixellength = this.AutoFitRowColumn(false, iStartColumnIndex, iEndColumnIndex);

            SLColumnProperties cp;
            double fColumnWidth;
            int iPixelLength;
            double fWholeNumber;
            double fRemainder;
            foreach (int pixlenpt in pixellength.Keys)
            {
                iPixelLength = pixellength[pixlenpt];
                if (iPixelLength > 0)
                {
                    fWholeNumber = (double)(iPixelLength / (SimpleTheme.ThemeMaxDigitWidth - 1));
                    fRemainder = (double)(iPixelLength % (SimpleTheme.ThemeMaxDigitWidth - 1));
                    fRemainder = fRemainder / (double)(SimpleTheme.ThemeMaxDigitWidth - 1);
                    // we'll leave it to the algorithm within SLColumnProperties.Width to handle
                    // the actual column width refitting.
                    fColumnWidth = fWholeNumber + fRemainder;
                    if (slws.ColumnProperties.ContainsKey(pixlenpt))
                    {
                        cp = slws.ColumnProperties[pixlenpt];
                        cp.Width = fColumnWidth;
                        cp.BestFit = true;
                        slws.ColumnProperties[pixlenpt] = cp.Clone();
                    }
                    else
                    {
                        cp = new SLColumnProperties(SimpleTheme.ThemeColumnWidth, SimpleTheme.ThemeColumnWidthInEMU, SimpleTheme.ThemeMaxDigitWidth, SimpleTheme.listColumnStepSize);
                        cp.Width = fColumnWidth;
                        cp.BestFit = true;
                        slws.ColumnProperties[pixlenpt] = cp.Clone();
                    }
                }
                // else we don't have to do anything
            }
        }

        /// <summary>
        /// Indicates if the column is hidden.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <returns>True if the column is hidden. False otherwise.</returns>
        public bool IsColumnHidden(string ColumnName)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return IsColumnHidden(iColumnIndex);
        }

        /// <summary>
        /// Indicates if the column is hidden.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>True if the column is hidden. False otherwise.</returns>
        public bool IsColumnHidden(int ColumnIndex)
        {
            bool result = false;
            if (slws.ColumnProperties.ContainsKey(ColumnIndex))
            {
                SLColumnProperties cp = slws.ColumnProperties[ColumnIndex];
                result = cp.Hidden;
            }

            return result;
        }

        private bool ToggleColumnHidden(int StartColumnIndex, int EndColumnIndex, bool Hidden)
        {
            int iStartColumnIndex = 1, iEndColumnIndex = 1;
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
            if (iStartColumnIndex >= 1 && iStartColumnIndex <= SLConstants.ColumnLimit && iEndColumnIndex >= 1 && iEndColumnIndex <= SLConstants.ColumnLimit)
            {
                result = true;
                int i = 0;
                SLColumnProperties cp;
                for (i = iStartColumnIndex; i <= iEndColumnIndex; ++i)
                {
                    if (slws.ColumnProperties.ContainsKey(i))
                    {
                        cp = slws.ColumnProperties[i];
                        cp.Hidden = Hidden;
                        slws.ColumnProperties[i] = cp;
                    }
                    else
                    {
                        cp = new SLColumnProperties(SimpleTheme.ThemeColumnWidth, SimpleTheme.ThemeColumnWidthInEMU, SimpleTheme.ThemeMaxDigitWidth, SimpleTheme.listColumnStepSize);
                        cp.Hidden = Hidden;
                        slws.ColumnProperties.Add(i, cp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hide the column.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <returns>True if the column name is valid. False otherwise.</returns>
        public bool HideColumn(string ColumnName)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return HideColumn(iColumnIndex);
        }

        /// <summary>
        /// Hide the column.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>True if the column index is valid. False otherwise.</returns>
        public bool HideColumn(int ColumnIndex)
        {
            return ToggleColumnHidden(ColumnIndex, ColumnIndex, true);
        }

        /// <summary>
        /// Hide a range of columns.
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column.</param>
        /// <param name="EndColumnName">The column name of the end column.</param>
        /// <returns>True if the column names are valid. False otherwise.</returns>
        public bool HideColumn(string StartColumnName, string EndColumnName)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);

            return HideColumn(iStartColumnIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Hide a range of columns.
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column.</param>
        /// <param name="EndColumnIndex">The column index of the end column.</param>
        /// <returns>True if the column indices are valid. False otherwise.</returns>
        public bool HideColumn(int StartColumnIndex, int EndColumnIndex)
        {
            return ToggleColumnHidden(StartColumnIndex, EndColumnIndex, true);
        }

        /// <summary>
        /// Unhide the column.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <returns>True if the column name is valid. False otherwise.</returns>
        public bool UnhideColumn(string ColumnName)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return UnhideColumn(iColumnIndex);
        }

        /// <summary>
        /// Unhide the column.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>True if the column index is valid. False otherwise.</returns>
        public bool UnhideColumn(int ColumnIndex)
        {
            return ToggleColumnHidden(ColumnIndex, ColumnIndex, false);
        }

        /// <summary>
        /// Unhide a range of columns.
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column.</param>
        /// <param name="EndColumnName">The column name of the end column.</param>
        /// <returns>True if the column names are valid. False otherwise.</returns>
        public bool UnhideColumn(string StartColumnName, string EndColumnName)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);

            return UnhideColumn(iStartColumnIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Unhide a range of columns.
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column.</param>
        /// <param name="EndColumnIndex">The column index of the end column.</param>
        /// <returns>True if the column indices are valid. False otherwise.</returns>
        public bool UnhideColumn(int StartColumnIndex, int EndColumnIndex)
        {
            return ToggleColumnHidden(StartColumnIndex, EndColumnIndex, false);
        }

        /// <summary>
        /// Indicates if the column is showing phonetic information.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <returns>True if the column is showing phonetic information. False otherwise.</returns>
        public bool IsColumnShowingPhonetic(string ColumnName)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return IsColumnShowingPhonetic(iColumnIndex);
        }

        /// <summary>
        /// Indicates if the column is showing phonetic information.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>True if the column is showing phonetic information. False otherwise.</returns>
        public bool IsColumnShowingPhonetic(int ColumnIndex)
        {
            bool result = false;
            if (slws.ColumnProperties.ContainsKey(ColumnIndex))
            {
                SLColumnProperties cp = slws.ColumnProperties[ColumnIndex];
                result = cp.Phonetic;
            }

            return result;
        }

        /// <summary>
        /// Set the show phonetic property for the column.
        /// </summary>
        /// <param name="ColumnName">The column name, such as "A".</param>
        /// <param name="ShowPhonetic">True if the column should show phonetic information. False otherwise.</param>
        /// <returns>True if the column name is valid. False otherwise.</returns>
        public bool SetColumnShowPhonetic(string ColumnName, bool ShowPhonetic)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);

            return SetColumnShowPhonetic(iColumnIndex, iColumnIndex, ShowPhonetic);
        }

        /// <summary>
        /// Set the show phonetic property for the column.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <param name="ShowPhonetic">True if the column should show phonetic information. False otherwise.</param>
        /// <returns>True if the column index is valid. False otherwise.</returns>
        public bool SetColumnShowPhonetic(int ColumnIndex, bool ShowPhonetic)
        {
            return SetColumnShowPhonetic(ColumnIndex, ColumnIndex, ShowPhonetic);
        }

        /// <summary>
        /// Set the show phonetic property for a range of columns.
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column.</param>
        /// <param name="EndColumnName">The column name of the end column.</param>
        /// <param name="ShowPhonetic">True if the columns should show phonetic information. False otherwise.</param>
        /// <returns>True if the column names are valid. False otherwise.</returns>
        public bool SetColumnShowPhonetic(string StartColumnName, string EndColumnName, bool ShowPhonetic)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);

            return SetColumnShowPhonetic(iStartColumnIndex, iEndColumnIndex, ShowPhonetic);
        }

        /// <summary>
        /// Set the show phonetic property for a range of columns.
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column.</param>
        /// <param name="EndColumnIndex">The column index of the end column.</param>
        /// <param name="ShowPhonetic">True if the columns should show phonetic information. False otherwise.</param>
        /// <returns>True if the column indices are valid. False otherwise.</returns>
        public bool SetColumnShowPhonetic(int StartColumnIndex, int EndColumnIndex, bool ShowPhonetic)
        {
            int iStartColumnIndex = 1, iEndColumnIndex = 1;
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
            if (iStartColumnIndex >= 1 && iStartColumnIndex <= SLConstants.ColumnLimit && iEndColumnIndex >= 1 && iEndColumnIndex <= SLConstants.ColumnLimit)
            {
                result = true;
                int i = 0;
                SLColumnProperties cp;
                for (i = iStartColumnIndex; i <= iEndColumnIndex; ++i)
                {
                    if (slws.ColumnProperties.ContainsKey(i))
                    {
                        cp = slws.ColumnProperties[i];
                        cp.Phonetic = ShowPhonetic;
                        slws.ColumnProperties[i] = cp;
                    }
                    else
                    {
                        cp = new SLColumnProperties(SimpleTheme.ThemeColumnWidth, SimpleTheme.ThemeColumnWidthInEMU, SimpleTheme.ThemeMaxDigitWidth, SimpleTheme.listColumnStepSize);
                        cp.Phonetic = ShowPhonetic;
                        slws.ColumnProperties.Add(i, cp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Copy one column to another column.
        /// </summary>
        /// <param name="ColumnName">The column name of the column to be copied from.</param>
        /// <param name="AnchorColumnName">The column name of the column to be copied to.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(string ColumnName, string AnchorColumnName)
        {
            int iColumnIndex = -1;
            int iAnchorColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);
            iAnchorColumnIndex = SLTool.ToColumnIndex(AnchorColumnName);

            return CopyColumn(iColumnIndex, iColumnIndex, iAnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy one column to another column.
        /// </summary>
        /// <param name="ColumnIndex">The column index of the column to be copied from.</param>
        /// <param name="AnchorColumnIndex">The column index of the column to be copied to.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(int ColumnIndex, int AnchorColumnIndex)
        {
            return CopyColumn(ColumnIndex, ColumnIndex, AnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy one column to another column.
        /// </summary>
        /// <param name="ColumnName">The column name of the column to be copied from.</param>
        /// <param name="AnchorColumnName">The column name of the column to be copied to.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(string ColumnName, string AnchorColumnName, bool ToCut)
        {
            int iColumnIndex = -1;
            int iAnchorColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(ColumnName);
            iAnchorColumnIndex = SLTool.ToColumnIndex(AnchorColumnName);

            return CopyColumn(iColumnIndex, iColumnIndex, iAnchorColumnIndex, ToCut);
        }

        /// <summary>
        /// Copy one column to another column.
        /// </summary>
        /// <param name="ColumnIndex">The column index of the column to be copied from.</param>
        /// <param name="AnchorColumnIndex">The column index of the column to be copied to.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(int ColumnIndex, int AnchorColumnIndex, bool ToCut)
        {
            return CopyColumn(ColumnIndex, ColumnIndex, AnchorColumnIndex, ToCut);
        }

        /// <summary>
        /// Copy a range of columns to another range, given the anchor column of the destination range (left-most column).
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column of the column range. This is typically the left-most column.</param>
        /// <param name="EndColumnName">The column name of the end column of the column range. This is typically the right-most column.</param>
        /// <param name="AnchorColumnName">The column name of the anchor column.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(string StartColumnName, string EndColumnName, string AnchorColumnName)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            int iAnchorColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);
            iAnchorColumnIndex = SLTool.ToColumnIndex(AnchorColumnName);

            return CopyColumn(iStartColumnIndex, iEndColumnIndex, iAnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy a range of columns to another range, given the anchor column of the destination range (left-most column).
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column of the column range. This is typically the left-most column.</param>
        /// <param name="EndColumnIndex">The column index of the end column of the column range. This is typically the right-most column.</param>
        /// <param name="AnchorColumnIndex">The column index of the anchor column.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(int StartColumnIndex, int EndColumnIndex, int AnchorColumnIndex)
        {
            return CopyColumn(StartColumnIndex, EndColumnIndex, AnchorColumnIndex, false);
        }

        /// <summary>
        /// Copy a range of columns to another range, given the anchor column of the destination range (left-most column).
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column of the column range. This is typically the left-most column.</param>
        /// <param name="EndColumnName">The column name of the end column of the column range. This is typically the right-most column.</param>
        /// <param name="AnchorColumnName">The column name of the anchor column.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(string StartColumnName, string EndColumnName, string AnchorColumnName, bool ToCut)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            int iAnchorColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);
            iAnchorColumnIndex = SLTool.ToColumnIndex(AnchorColumnName);

            return CopyColumn(iStartColumnIndex, iEndColumnIndex, iAnchorColumnIndex, ToCut);
        }

        /// <summary>
        /// Copy a range of columns to another range, given the anchor column of the destination range (left-most column).
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column of the column range. This is typically the left-most column.</param>
        /// <param name="EndColumnIndex">The column index of the end column of the column range. This is typically the right-most column.</param>
        /// <param name="AnchorColumnIndex">The column index of the anchor column.</param>
        /// <param name="ToCut">True for cut-and-paste. False for copy-and-paste.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool CopyColumn(int StartColumnIndex, int EndColumnIndex, int AnchorColumnIndex, bool ToCut)
        {
            int iStartColumnIndex = 1, iEndColumnIndex = 1;
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
            if (iStartColumnIndex >= 1 && iStartColumnIndex <= SLConstants.ColumnLimit
                && iEndColumnIndex >= 1 && iEndColumnIndex <= SLConstants.ColumnLimit
                && AnchorColumnIndex >= 1 && AnchorColumnIndex <= SLConstants.ColumnLimit
                && iStartColumnIndex != AnchorColumnIndex)
            {
                result = true;

                int diff = AnchorColumnIndex - iStartColumnIndex;
                int i = 0;
                Dictionary<int, SLColumnProperties> cols = new Dictionary<int, SLColumnProperties>();
                for (i = iStartColumnIndex; i <= iEndColumnIndex; ++i)
                {
                    if (slws.ColumnProperties.ContainsKey(i))
                    {
                        cols[i + diff] = slws.ColumnProperties[i].Clone();
                        if (ToCut)
                        {
                            slws.ColumnProperties.Remove(i);
                        }
                    }
                }

                if (ToCut) slws.RemoveRowColumnStyleHistory(false, iStartColumnIndex, iEndColumnIndex);

                int AnchorEndColumnIndex = AnchorColumnIndex + iEndColumnIndex - iStartColumnIndex;
                // removing columns within destination "paste" operation
                foreach (var col in slws.ColumnProperties)
                {
                    if (col.Key >= AnchorColumnIndex && col.Key <= AnchorEndColumnIndex)
                    {
                        slws.ColumnProperties.Remove(col.Key);
                    }
                }

                foreach (var key in cols.Keys)
                {
                    slws.ColumnProperties[key] = cols[key];
                    if (cols[key].StyleIndex > 0)
                    {
                        slws.RowColumnStyleHistory.Add(new SLRowColumnStyleHistory(false, key));
                    }
                }

                Dictionary<SLCellPoint, SLCell> cells = new Dictionary<SLCellPoint, SLCell>();
                List<SLCellPoint> listCellKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                foreach (SLCellPoint pt in listCellKeys)
                {
                    if (pt.ColumnIndex >= iStartColumnIndex && pt.ColumnIndex <= iEndColumnIndex)
                    {
                        cells[new SLCellPoint(pt.RowIndex, pt.ColumnIndex + diff)] = slws.Cells[pt].Clone();
                        if (ToCut)
                        {
                            slws.Cells.Remove(pt);
                        }
                    }
                }

                listCellKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                foreach (SLCellPoint pt in listCellKeys)
                {
                    // any cell within destination "paste" operation is taken out
                    if (pt.ColumnIndex >= AnchorColumnIndex && pt.ColumnIndex <= AnchorEndColumnIndex)
                    {
                        slws.Cells.Remove(pt);
                    }
                }

                int iNumberOfColumns = iEndColumnIndex - iStartColumnIndex + 1;
                if (AnchorColumnIndex <= iStartColumnIndex) iNumberOfColumns = -iNumberOfColumns;

                SLCell c;
                foreach (var key in cells.Keys)
                {
                    c = cells[key];
                    this.ProcessCellFormulaDelta(ref c, -1, 0, AnchorColumnIndex, iNumberOfColumns);
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
                    if (mc.StartColumnIndex >= iStartColumnIndex && mc.EndColumnIndex <= iEndColumnIndex)
                    {
                        if (ToCut)
                        {
                            slws.MergeCells.Remove(mc);
                        }
                        this.MergeWorksheetCells(mc.StartRowIndex, mc.StartColumnIndex + diff, mc.EndRowIndex, mc.EndColumnIndex + diff);
                    }
                }

                #region Calculation cells
                if (slwb.CalculationCells.Count > 0)
                {
                    List<int> listToDelete = new List<int>();
                    int iColumnLimit = AnchorColumnIndex + iStartColumnIndex - iEndColumnIndex;
                    for (i = 0; i < slwb.CalculationCells.Count; ++i)
                    {
                        if (slwb.CalculationCells[i].SheetId == giSelectedWorksheetID)
                        {
                            if (ToCut && slwb.CalculationCells[i].ColumnIndex >= iStartColumnIndex && slwb.CalculationCells[i].ColumnIndex <= iEndColumnIndex)
                            {
                                // just remove because recalculation of cell references is too complicated...
                                if (!listToDelete.Contains(i)) listToDelete.Add(i);
                            }

                            if (slwb.CalculationCells[i].ColumnIndex >= AnchorColumnIndex && slwb.CalculationCells[i].ColumnIndex <= iColumnLimit)
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

        // not supporting column grouping
        // not supporting column collapsing

        // Autofitting columns is tricky because need to take care of the "display" version
        // of the contents, rather than the contents themselves.
        // For example, the date value is stored as a floating point number, but if the
        // style to display it makes the display string length long, then we have to calculate that.
        // Also, 1234567890 can be display as 1,234,567,890.00
        // So yeah, fun...
        // Also, remember to take care of inline strings.
        //public void AutofitColumnWidth(int ColumnIndex)
        //{
        //}

        /// <summary>
        /// Insert one or more columns.
        /// </summary>
        /// <param name="StartColumnName">Additional columns will be inserted at this column.</param>
        /// <param name="NumberOfColumns">Number of columns to insert.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool InsertColumn(string StartColumnName, int NumberOfColumns)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(StartColumnName);

            return InsertColumn(iColumnIndex, NumberOfColumns);
        }

        /// <summary>
        /// Insert one or more columns.
        /// </summary>
        /// <param name="StartColumnIndex">Additional columns will be inserted at this column index.</param>
        /// <param name="NumberOfColumns">Number of columns to insert.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool InsertColumn(int StartColumnIndex, int NumberOfColumns)
        {
            if (NumberOfColumns < 1) return false;

            bool result = false;
            if (StartColumnIndex >= 1 && StartColumnIndex <= SLConstants.ColumnLimit)
            {
                result = true;
                int i = 0, iNewIndex = 0;

                int index = 0;
                //int iRowIndex = -1, iColumnIndex = -1;
                //int iRowIndex2 = -1, iColumnIndex2 = -1;
                int iColumnIndex = -1;
                int iColumnIndex2 = -1;

                #region Column properties
                SLColumnProperties cp;
                List<int> listColumnIndex = slws.ColumnProperties.Keys.ToList<int>();
                // this sorting in descending order is crucial!
                // we move the data from after the insert range to their new reference keys
                // first, then we put in the new data, which will then have no data
                // key collision.
                listColumnIndex.Sort();
                listColumnIndex.Reverse();

                for (i = 0; i < listColumnIndex.Count; ++i)
                {
                    index = listColumnIndex[i];
                    if (index >= StartColumnIndex)
                    {
                        cp = slws.ColumnProperties[index];
                        slws.ColumnProperties.Remove(index);
                        slws.RemoveRowColumnStyleHistory(false, index, index);
                        iNewIndex = index + NumberOfColumns;
                        // if the new column is right of right-side limit of the worksheet,
                        // then it disappears into the ether...
                        if (iNewIndex <= SLConstants.ColumnLimit)
                        {
                            slws.ColumnProperties[iNewIndex] = cp;
                            if (cp.StyleIndex > 0)
                            {
                                slws.RowColumnStyleHistory.Add(new SLRowColumnStyleHistory(false, iNewIndex));
                            }
                        }
                    }
                    else
                    {
                        // the columns before the start column are unaffected by the insertion.
                        // Because it's sorted in descending order, we can just break out.
                        break;
                    }
                }
                #endregion

                #region Cell data
                List<SLCellPoint> listCellRefKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                // this sorting in descending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());
                listCellRefKeys.Reverse();

                SLCell c;
                SLCellPoint pt;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    c = slws.Cells[pt];
                    this.ProcessCellFormulaDelta(ref c, -1, 0, StartColumnIndex, NumberOfColumns);

                    if (pt.ColumnIndex >= StartColumnIndex)
                    {
                        slws.Cells.Remove(pt);
                        iNewIndex = pt.ColumnIndex + NumberOfColumns;
                        if (iNewIndex <= SLConstants.ColumnLimit)
                        {
                            slws.Cells[new SLCellPoint(pt.RowIndex, iNewIndex)] = c;
                        }
                    }
                    else
                    {
                        slws.Cells[pt] = c;
                    }
                }

                #region Cell comments
                listCellRefKeys = slws.Comments.Keys.ToList<SLCellPoint>();
                // this sorting in descending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());
                listCellRefKeys.Reverse();

                SLComment comm;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    comm = slws.Comments[pt];
                    if (pt.ColumnIndex >= StartColumnIndex)
                    {
                        slws.Comments.Remove(pt);
                        iNewIndex = pt.ColumnIndex + NumberOfColumns;
                        if (iNewIndex <= SLConstants.ColumnLimit)
                        {
                            slws.Comments[new SLCellPoint(pt.RowIndex, iNewIndex)] = comm;
                        }
                    }
                    // no else because there's nothing done
                }
                #endregion

                #endregion

                // the tables part has to be after the cell data part because we need
                // the cells to be correctly adjusted first. The insertion of new columns
                // also means updating some cells for the column header names.

                // Excel doesn't seem to allow inserting/deleting columns in certain cases
                // when 2 (or more) tables overlap each other vertically (meaning one above the other).
                // In particular, when the insert/delete range overlaps an existing column
                // in 2 (or more) tables.
                // The algorithm below works fine, meaning the resulting spreadsheet doesn't
                // have errors, so not sure why Excel doesn't allow it.
                #region Tables
                if (slws.Tables.Count > 0)
                {
                    int iNewID = 0;
                    string sNewColumnName = string.Empty;
                    int iCount = 0;

                    foreach (SLTable t in slws.Tables)
                    {
                        iColumnIndex = t.StartColumnIndex;
                        iColumnIndex2 = t.EndColumnIndex;
                        // need to modify table columns if the start column index is between the
                        // table reference columns, inclusive of the end column. If the start column index
                        // is the same as (or before) the start column of the table, then the whole
                        // table is shifted, so no modification is needed.
                        if (iColumnIndex < StartColumnIndex && StartColumnIndex <= iColumnIndex2)
                        {
                            for (i = 0; i < NumberOfColumns; ++i)
                            {
                                // the new ID and column name should be found long before the
                                // column limit is hit. Unless the table is unusually large...
                                for (iNewID = 1; iNewID <= SLConstants.ColumnLimit; ++iNewID)
                                {
                                    sNewColumnName = string.Format("Column{0}", iNewID);
                                    iCount = t.TableColumns.Count(n => n.Name.Equals(sNewColumnName, StringComparison.InvariantCultureIgnoreCase));
                                    if (iCount == 0) break;
                                }

                                for (iNewID = 1; iNewID <= SLConstants.ColumnLimit; ++iNewID)
                                {
                                    iCount = t.TableColumns.Count(n => n.Id == iNewID);
                                    if (iCount == 0) break;
                                }

                                if (t.HeaderRowCount > 0)
                                {
                                    iNewIndex = StartColumnIndex + i;
                                    if (iNewIndex > SLConstants.ColumnLimit) iNewIndex = SLConstants.ColumnLimit;
                                    this.SetCellValue(t.StartRowIndex, iNewIndex, sNewColumnName);
                                }

                                t.TableColumns.Insert(StartColumnIndex - iColumnIndex + i, new SLTableColumn() { Id = (uint)iNewID, Name = sNewColumnName });
                            }

                            // remove any extra columns that hang outside the worksheet after insertion
                            iCount = StartColumnIndex + NumberOfColumns - SLConstants.ColumnLimit;
                            for (i = 0; i < iCount; ++i)
                            {
                                // keep removing the last one
                                t.TableColumns.RemoveAt(t.TableColumns.Count - 1);
                            }
                        }

                        this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref iColumnIndex, ref iColumnIndex2);
                        if (iColumnIndex != t.StartColumnIndex || iColumnIndex2 != t.EndColumnIndex) t.IsNewTable = true;
                        t.StartColumnIndex = iColumnIndex;
                        t.EndColumnIndex = iColumnIndex2;

                        if (t.HasAutoFilter)
                        {
                            iColumnIndex = t.AutoFilter.StartColumnIndex;
                            iColumnIndex2 = t.AutoFilter.EndColumnIndex;
                            this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref iColumnIndex, ref iColumnIndex2);
                            if (iColumnIndex != t.AutoFilter.StartColumnIndex || iColumnIndex2 != t.AutoFilter.EndColumnIndex) t.IsNewTable = true;
                            t.AutoFilter.StartColumnIndex = iColumnIndex;
                            t.AutoFilter.EndColumnIndex = iColumnIndex2;

                            if (t.AutoFilter.HasSortState)
                            {
                                iColumnIndex = t.AutoFilter.SortState.StartColumnIndex;
                                iColumnIndex2 = t.AutoFilter.SortState.EndColumnIndex;
                                this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref iColumnIndex, ref iColumnIndex2);
                                if (iColumnIndex != t.AutoFilter.SortState.StartColumnIndex || iColumnIndex2 != t.AutoFilter.SortState.EndColumnIndex) t.IsNewTable = true;
                                t.AutoFilter.SortState.StartColumnIndex = iColumnIndex;
                                t.AutoFilter.SortState.EndColumnIndex = iColumnIndex2;
                            }
                        }

                        if (t.HasSortState)
                        {
                            iColumnIndex = t.SortState.StartColumnIndex;
                            iColumnIndex2 = t.SortState.EndColumnIndex;
                            this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref iColumnIndex, ref iColumnIndex2);
                            if (iColumnIndex != t.SortState.StartColumnIndex || iColumnIndex2 != t.SortState.EndColumnIndex) t.IsNewTable = true;
                            t.SortState.StartColumnIndex = iColumnIndex;
                            t.SortState.EndColumnIndex = iColumnIndex2;
                        }
                    }
                }
                #endregion

                #region Merge cells
                if (slws.MergeCells.Count > 0)
                {
                    SLMergeCell mc;
                    for (i = 0; i < slws.MergeCells.Count; ++i)
                    {
                        mc = slws.MergeCells[i];
                        this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref mc.iStartColumnIndex, ref mc.iEndColumnIndex);
                        slws.MergeCells[i] = mc;
                    }
                }
                #endregion

                // TODO picture/worksheet drawings
                //WorksheetPart wsp = (WorksheetPart)wbp.GetPartById(gsSelectedWorksheetRelationshipID);
                //if (wsp.DrawingsPart != null)
                //{
                //    DrawingsPart dp = wsp.DrawingsPart;
                //}

                #region Calculation chain
                if (slwb.CalculationCells.Count > 0)
                {
                    foreach (SLCalculationCell cc in slwb.CalculationCells)
                    {
                        if (cc.SheetId == giSelectedWorksheetID)
                        {
                            iColumnIndex = cc.ColumnIndex;
                            // don't need this but assign something anyway...
                            iColumnIndex2 = SLConstants.ColumnLimit;

                            this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref iColumnIndex, ref iColumnIndex2);
                            cc.ColumnIndex = iColumnIndex;
                        }
                    }
                }
                #endregion

                #region Defined names
                if (slwb.DefinedNames.Count > 0)
                {
                    string sDefinedNameText = string.Empty;
                    foreach (SLDefinedName d in slwb.DefinedNames)
                    {
                        sDefinedNameText = d.Text;
                        sDefinedNameText = AddDeleteCellFormulaDelta(sDefinedNameText, -1, 0, StartColumnIndex, NumberOfColumns);
                        sDefinedNameText = AddDeleteDefinedNameRowColumnRangeDelta(sDefinedNameText, false, StartColumnIndex, NumberOfColumns);
                        d.Text = sDefinedNameText;
                    }
                }
                #endregion

                #region Sparklines
                if (slws.SparklineGroups.Count > 0)
                {
                    SLSparkline spk;
                    foreach (SLSparklineGroup spkgrp in slws.SparklineGroups)
                    {
                        if (spkgrp.DateAxis && spkgrp.DateWorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref spkgrp.DateStartColumnIndex, ref spkgrp.DateEndColumnIndex);
                        }

                        // starting from the end is important because we might be deleting!
                        for (i = spkgrp.Sparklines.Count - 1; i >= 0; --i)
                        {
                            spk = spkgrp.Sparklines[i];

                            if (spk.LocationColumnIndex >= StartColumnIndex)
                            {
                                iNewIndex = spk.LocationColumnIndex + NumberOfColumns;
                                if (iNewIndex <= SLConstants.ColumnLimit)
                                {
                                    spk.LocationColumnIndex = iNewIndex;
                                }
                                else
                                {
                                    // out of range!
                                    spkgrp.Sparklines.RemoveAt(i);
                                    continue;
                                }
                            }
                            // else the location is before the start column so don't have to do anything

                            // process only if the data source is on the currently selected worksheet
                            if (spk.WorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.AddRowColumnIndexDelta(StartColumnIndex, NumberOfColumns, false, ref spk.StartColumnIndex, ref spk.EndColumnIndex);
                            }

                            spkgrp.Sparklines[i] = spk;
                        }
                    }
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Delete one or more columns.
        /// </summary>
        /// <param name="StartColumnName">Columns will deleted from this column, including this column itself.</param>
        /// <param name="NumberOfColumns">Number of columns to delete.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool DeleteColumn(string StartColumnName, int NumberOfColumns)
        {
            int iColumnIndex = -1;
            iColumnIndex = SLTool.ToColumnIndex(StartColumnName);

            return DeleteColumn(iColumnIndex, NumberOfColumns);
        }

        /// <summary>
        /// Delete one or more columns.
        /// </summary>
        /// <param name="StartColumnIndex">Columns will be deleted from this column index, including this column itself.</param>
        /// <param name="NumberOfColumns">Number of columns to delete.</param>
        /// <returns>True if successful. False otherwise.</returns>
        public bool DeleteColumn(int StartColumnIndex, int NumberOfColumns)
        {
            if (NumberOfColumns < 1) return false;

            bool result = false;
            if (StartColumnIndex >= 1 && StartColumnIndex <= SLConstants.ColumnLimit)
            {
                result = true;
                int i = 0, iNewIndex = 0;
                int iEndColumnIndex = StartColumnIndex + NumberOfColumns - 1;
                if (iEndColumnIndex > SLConstants.ColumnLimit) iEndColumnIndex = SLConstants.ColumnLimit;
                // this autocorrects in the case of overshooting the column limit
                int iNumberOfColumns = iEndColumnIndex - StartColumnIndex + 1;

                int index = 0;
                int iColumnIndex = -1;
                int iColumnIndex2 = -1;

                #region Column properties
                SLColumnProperties cp;
                List<int> listColumnIndex = slws.ColumnProperties.Keys.ToList<int>();
                // this sorting in ascending order is crucial!
                // we're removing data within delete range, and if the data is after the delete range,
                // then the key is modified. Since the data within delete range is removed,
                // there won't be data key collisions.
                listColumnIndex.Sort();

                for (i = 0; i < listColumnIndex.Count; ++i)
                {
                    index = listColumnIndex[i];
                    if (StartColumnIndex <= index && index <= iEndColumnIndex)
                    {
                        slws.ColumnProperties.Remove(index);
                    }
                    else if (index > iEndColumnIndex)
                    {
                        cp = slws.ColumnProperties[index];
                        slws.ColumnProperties.Remove(index);
                        iNewIndex = index - iNumberOfColumns;
                        slws.ColumnProperties[iNewIndex] = cp;
                    }

                    // the columns before the start column are unaffected by the deleting.
                }
                slws.RemoveRowColumnStyleHistory(false, StartColumnIndex, iEndColumnIndex);
                #endregion

                #region Cell data
                List<SLCellPoint> listCellRefKeys = slws.Cells.Keys.ToList<SLCellPoint>();
                // this sorting in ascending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());

                SLCell c;
                SLCellPoint pt;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    c = slws.Cells[pt];
                    this.ProcessCellFormulaDelta(ref c, -1, 0, StartColumnIndex, -NumberOfColumns);

                    if (StartColumnIndex <= pt.ColumnIndex && pt.ColumnIndex <= iEndColumnIndex)
                    {
                        slws.Cells.Remove(pt);
                    }
                    else if (pt.ColumnIndex > iEndColumnIndex)
                    {
                        slws.Cells.Remove(pt);
                        iNewIndex = pt.ColumnIndex - iNumberOfColumns;
                        slws.Cells[new SLCellPoint(pt.RowIndex, iNewIndex)] = c;
                    }
                    else
                    {
                        slws.Cells[pt] = c;
                    }
                }

                #region Cell comments
                listCellRefKeys = slws.Comments.Keys.ToList<SLCellPoint>();
                // this sorting in ascending order is crucial!
                listCellRefKeys.Sort(new SLCellReferencePointComparer());

                SLComment comm;
                for (i = 0; i < listCellRefKeys.Count; ++i)
                {
                    pt = listCellRefKeys[i];
                    comm = slws.Comments[pt];
                    if (StartColumnIndex <= pt.ColumnIndex && pt.ColumnIndex <= iEndColumnIndex)
                    {
                        slws.Comments.Remove(pt);
                    }
                    else if (pt.ColumnIndex > iEndColumnIndex)
                    {
                        slws.Comments.Remove(pt);
                        iNewIndex = pt.ColumnIndex - iNumberOfColumns;
                        slws.Comments[new SLCellPoint(pt.RowIndex, iNewIndex)] = comm;
                    }
                    // no else because there's nothing done
                }
                #endregion

                #endregion

                // Excel doesn't seem to allow inserting/deleting columns in certain cases
                // when 2 (or more) tables overlap each other vertically (meaning one above the other).
                // In particular, when the insert/delete range overlaps an existing column
                // in 2 (or more) tables.
                // The algorithm below works fine, meaning the resulting spreadsheet doesn't
                // have errors, so not sure why Excel doesn't allow it.
                #region Tables
                if (slws.Tables.Count > 0)
                {
                    SLTable t;
                    List<int> listTablesToDelete = new List<int>();
                    for (i = 0; i < slws.Tables.Count; ++i)
                    {
                        t = slws.Tables[i];
                        iColumnIndex = t.StartColumnIndex;
                        iColumnIndex2 = t.EndColumnIndex;
                        if (StartColumnIndex <= iColumnIndex && iColumnIndex2 <= iEndColumnIndex)
                        {
                            // table is completely within delete range, so delete the whole table
                            listTablesToDelete.Add(i);
                            continue;
                        }
                        else
                        {
                            int iTableStartIndex = 0, iNumberOfTableColumnsToDelete = 0;
                            if (StartColumnIndex <= iColumnIndex && iColumnIndex <= iEndColumnIndex && iEndColumnIndex < iColumnIndex2)
                            {
                                // the left part of the table columns are deleted
                                iTableStartIndex = 0;
                                iNumberOfTableColumnsToDelete = iEndColumnIndex - iColumnIndex + 1;
                            }
                            else if (iColumnIndex < StartColumnIndex && iEndColumnIndex < iColumnIndex2)
                            {
                                // the middle part of the table columns are deleted
                                iTableStartIndex = StartColumnIndex - iColumnIndex;
                                iNumberOfTableColumnsToDelete = iEndColumnIndex - StartColumnIndex + 1;
                            }
                            else if (iColumnIndex < StartColumnIndex && StartColumnIndex <= iColumnIndex2 && iColumnIndex2 <= iEndColumnIndex)
                            {
                                // the right part of the table columns are deleted
                                iTableStartIndex = StartColumnIndex - iColumnIndex;
                                iNumberOfTableColumnsToDelete = iColumnIndex2 - StartColumnIndex + 1;
                            }

                            // this assumes that TableColumns only has TableColumn as children
                            // and that the number of table columns corresponds correctly to the
                            // table reference range (because it might be different, but that
                            // means the spreadsheet has an error).
                            // We start from the back because we're deleting so we don't want to
                            // mess up the indices.
                            for (i = iTableStartIndex + iNumberOfTableColumnsToDelete - 1; i >= iTableStartIndex; --i)
                            {
                                t.TableColumns.RemoveAt(i);
                            }

                            this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref iColumnIndex, ref iColumnIndex2);
                            if (iColumnIndex != t.StartColumnIndex || iColumnIndex2 != t.EndColumnIndex) t.IsNewTable = true;
                            t.StartColumnIndex = iColumnIndex;
                            t.EndColumnIndex = iColumnIndex2;
                        }

                        if (t.HasAutoFilter)
                        {
                            // if the autofilter range is completely within delete range,
                            // then it's already taken care off above.
                            iColumnIndex = t.AutoFilter.StartColumnIndex;
                            iColumnIndex2 = t.AutoFilter.EndColumnIndex;
                            this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref iColumnIndex, ref iColumnIndex2);
                            if (iColumnIndex != t.AutoFilter.StartColumnIndex || iColumnIndex2 != t.AutoFilter.EndColumnIndex) t.IsNewTable = true;
                            t.AutoFilter.StartColumnIndex = iColumnIndex;
                            t.AutoFilter.EndColumnIndex = iColumnIndex2;

                            if (t.AutoFilter.HasSortState)
                            {
                                // if the sort state range is completely within delete range,
                                // then it's already taken care off above.
                                iColumnIndex = t.AutoFilter.SortState.StartColumnIndex;
                                iColumnIndex2 = t.AutoFilter.SortState.EndColumnIndex;
                                this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref iColumnIndex, ref iColumnIndex2);
                                if (iColumnIndex != t.AutoFilter.SortState.StartColumnIndex || iColumnIndex2 != t.AutoFilter.SortState.EndColumnIndex) t.IsNewTable = true;
                                t.AutoFilter.SortState.StartColumnIndex = iColumnIndex;
                                t.AutoFilter.SortState.EndColumnIndex = iColumnIndex2;
                            }
                        }

                        if (t.HasSortState)
                        {
                            // if the sort state range is completely within delete range,
                            // then it's already taken care off above.
                            iColumnIndex = t.SortState.StartColumnIndex;
                            iColumnIndex2 = t.SortState.EndColumnIndex;
                            this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref iColumnIndex, ref iColumnIndex2);
                            if (iColumnIndex != t.SortState.StartColumnIndex || iColumnIndex2 != t.SortState.EndColumnIndex) t.IsNewTable = true;
                            t.SortState.StartColumnIndex = iColumnIndex;
                            t.SortState.EndColumnIndex = iColumnIndex2;
                        }
                    }

                    if (listTablesToDelete.Count > 0)
                    {
                        WorksheetPart wsp = (WorksheetPart)wbp.GetPartById(gsSelectedWorksheetRelationshipID);
                        string sTableRelID = string.Empty;
                        string sTableName = string.Empty;
                        uint iTableID = 0;
                        for (i = listTablesToDelete.Count - 1; i >= 0; --i)
                        {
                            // remove IDs and table names from the spreadsheet unique lists
                            iTableID = slws.Tables[listTablesToDelete[i]].Id;
                            if (slwb.TableIds.Contains(iTableID)) slwb.TableIds.Remove(iTableID);

                            sTableName = slws.Tables[listTablesToDelete[i]].DisplayName;
                            if (slwb.TableNames.Contains(sTableName)) slwb.TableNames.Remove(sTableName);

                            sTableRelID = slws.Tables[listTablesToDelete[i]].RelationshipID;
                            if (sTableRelID.Length > 0)
                            {
                                wsp.DeletePart(sTableRelID);
                            }
                            slws.Tables.RemoveAt(listTablesToDelete[i]);
                        }
                    }
                }
                #endregion

                #region Merge cells
                if (slws.MergeCells.Count > 0)
                {
                    SLMergeCell mc;
                    // starting from the end is crucial because we might be removing items
                    for (i = slws.MergeCells.Count - 1; i >= 0; --i)
                    {
                        mc = slws.MergeCells[i];
                        if (StartColumnIndex <= mc.iStartColumnIndex && mc.iEndColumnIndex <= iEndColumnIndex)
                        {
                            // merge cell is completely within delete range
                            slws.MergeCells.RemoveAt(i);
                        }
                        else
                        {
                            this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref mc.iStartColumnIndex, ref mc.iEndColumnIndex);
                            slws.MergeCells[i] = mc;
                        }
                    }
                }
                #endregion

                // TODO pictures/charts!

                #region Calculation chain
                if (slwb.CalculationCells.Count > 0)
                {
                    List<int> listToDelete = new List<int>();
                    for (i = 0; i < slwb.CalculationCells.Count; ++i)
                    {
                        if (slwb.CalculationCells[i].SheetId == giSelectedWorksheetID)
                        {
                            if (StartColumnIndex <= slwb.CalculationCells[i].ColumnIndex && slwb.CalculationCells[i].ColumnIndex <= iEndColumnIndex)
                            {
                                listToDelete.Add(i);
                            }
                            else if (iEndColumnIndex < slwb.CalculationCells[i].ColumnIndex)
                            {
                                slwb.CalculationCells[i].ColumnIndex -= iNumberOfColumns;
                            }
                        }
                    }

                    // start from the back because we're deleting elements and we don't want
                    // the indices to get messed up.
                    for (i = listToDelete.Count - 1; i >= 0; --i)
                    {
                        slwb.CalculationCells.RemoveAt(listToDelete[i]);
                    }
                }
                #endregion

                #region Defined names
                if (slwb.DefinedNames.Count > 0)
                {
                    string sDefinedNameText = string.Empty;
                    foreach (SLDefinedName d in slwb.DefinedNames)
                    {
                        sDefinedNameText = d.Text;
                        sDefinedNameText = AddDeleteCellFormulaDelta(sDefinedNameText, -1, 0, StartColumnIndex, -NumberOfColumns);
                        sDefinedNameText = AddDeleteDefinedNameRowColumnRangeDelta(sDefinedNameText, false, StartColumnIndex, -NumberOfColumns);
                        d.Text = sDefinedNameText;
                    }
                }
                #endregion

                #region Sparklines
                if (slws.SparklineGroups.Count > 0)
                {
                    SLSparkline spk;
                    foreach (SLSparklineGroup spkgrp in slws.SparklineGroups)
                    {
                        if (spkgrp.DateAxis && spkgrp.DateWorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (StartColumnIndex <= spkgrp.DateStartColumnIndex && spkgrp.DateEndColumnIndex <= iEndColumnIndex)
                            {
                                // the whole date range is completely within delete range
                                spkgrp.DateAxis = false;
                            }
                            else
                            {
                                this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref spkgrp.DateStartColumnIndex, ref spkgrp.DateEndColumnIndex);
                            }
                        }

                        // starting from the end is important because we might be deleting!
                        for (i = spkgrp.Sparklines.Count - 1; i >= 0; --i)
                        {
                            spk = spkgrp.Sparklines[i];

                            if (StartColumnIndex <= spk.LocationColumnIndex && spk.LocationColumnIndex <= iEndColumnIndex)
                            {
                                spkgrp.Sparklines.RemoveAt(i);
                                continue;
                            }
                            else if (spk.LocationColumnIndex > iEndColumnIndex)
                            {
                                iNewIndex = spk.LocationColumnIndex - iNumberOfColumns;
                                spk.LocationColumnIndex = iNewIndex;
                            }
                            // no else because there's nothing done

                            // process only if the data source is on the currently selected worksheet
                            if (spk.WorksheetName.Equals(gsSelectedWorksheetName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (StartColumnIndex <= spk.StartColumnIndex && spk.EndColumnIndex <= iEndColumnIndex)
                                {
                                    // the data source is completely within delete range
                                    // Excel 2010 keeps the WorksheetExtension, but I'm gonna just delete the whole thing.
                                    spkgrp.Sparklines.RemoveAt(i);
                                    continue;
                                }
                                else
                                {
                                    this.DeleteRowColumnIndexDelta(StartColumnIndex, iEndColumnIndex, iNumberOfColumns, ref spk.StartColumnIndex, ref spk.EndColumnIndex);
                                }
                            }

                            spkgrp.Sparklines[i] = spk;
                        }
                    }
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Clear all cell content within specified columns. If the top-left cell of a merged cell is within specified columns, the merged cell content is also cleared.
        /// </summary>
        /// <param name="StartColumnName">The column name of the start column.</param>
        /// <param name="EndColumnName">The column name of the end column.</param>
        /// <returns>True if content has been cleared. False otherwise. If there are no content within specified rows, false is also returned.</returns>
        public bool ClearColumnContent(string StartColumnName, string EndColumnName)
        {
            int iStartColumnIndex = -1;
            int iEndColumnIndex = -1;
            iStartColumnIndex = SLTool.ToColumnIndex(StartColumnName);
            iEndColumnIndex = SLTool.ToColumnIndex(EndColumnName);

            return ClearColumnContent(iStartColumnIndex, iEndColumnIndex);
        }

        /// <summary>
        /// Clear all cell content within specified columns. If the top-left cell of a merged cell is within specified columns, the merged cell content is also cleared.
        /// </summary>
        /// <param name="StartColumnIndex">The column index of the start column.</param>
        /// <param name="EndColumnIndex">The column index of the end column.</param>
        /// <returns>True if content has been cleared. False otherwise. If there are no content within specified rows, false is also returned.</returns>
        public bool ClearColumnContent(int StartColumnIndex, int EndColumnIndex)
        {
            int iStartColumnIndex = 1, iEndColumnIndex = 1;
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

            if (iStartColumnIndex < 1) iStartColumnIndex = 1;
            if (iEndColumnIndex > SLConstants.ColumnLimit) iEndColumnIndex = SLConstants.ColumnLimit;

            bool result = false;
            int i = 0;
            for (i = iStartColumnIndex; i <= iEndColumnIndex; ++i)
            {
                if (slws.ColumnProperties.ContainsKey(i))
                {
                    if (slws.ColumnProperties[i].IsEmpty)
                    {
                        slws.ColumnProperties.Remove(i);
                        result = true;
                    }
                }
            }

            foreach (SLCellPoint pt in slws.Cells.Keys)
            {
                if (iStartColumnIndex <= pt.ColumnIndex && pt.ColumnIndex <= iEndColumnIndex)
                {
                    this.ClearCellContentData(pt);
                }
            }

            return result;
        }

        /// <summary>
        /// Delta is >= 0
        /// </summary>
        /// <param name="GivenStartIndex"></param>
        /// <param name="Delta">Delta is >= 0</param>
        /// <param name="IsRow"></param>
        /// <param name="CurrentStartIndex"></param>
        /// <param name="CurrentEndIndex"></param>
        internal void AddRowColumnIndexDelta(int GivenStartIndex, int Delta, bool IsRow, ref int CurrentStartIndex, ref int CurrentEndIndex)
        {
            if (CurrentStartIndex >= GivenStartIndex)
            {
                CurrentStartIndex += Delta;
                if (IsRow)
                {
                    if (CurrentStartIndex > SLConstants.RowLimit) CurrentStartIndex = SLConstants.RowLimit;
                }
                else
                {
                    if (CurrentStartIndex > SLConstants.ColumnLimit) CurrentStartIndex = SLConstants.ColumnLimit;
                }
            }

            if (CurrentEndIndex >= GivenStartIndex)
            {
                CurrentEndIndex += Delta;
                if (IsRow)
                {
                    if (CurrentEndIndex > SLConstants.RowLimit) CurrentEndIndex = SLConstants.RowLimit;
                }
                else
                {
                    if (CurrentEndIndex > SLConstants.ColumnLimit) CurrentEndIndex = SLConstants.ColumnLimit;
                }
            }
        }

        /// <summary>
        /// Delta is >= 0
        /// </summary>
        /// <param name="GivenStartIndex"></param>
        /// <param name="GivenEndIndex"></param>
        /// <param name="Delta">Delta is >= 0</param>
        /// <param name="CurrentStartIndex"></param>
        /// <param name="CurrentEndIndex"></param>
        internal void DeleteRowColumnIndexDelta(int GivenStartIndex, int GivenEndIndex, int Delta, ref int CurrentStartIndex, ref int CurrentEndIndex)
        {
            // the case where the current range is completely within the delete range
            // should already be handled by the calling function.

            if (GivenEndIndex < CurrentStartIndex)
            {
                // current range is completely below/right-of delete range
                CurrentStartIndex -= Delta;
                CurrentEndIndex -= Delta;
            }
            else if ((GivenStartIndex <= CurrentStartIndex && CurrentStartIndex <= GivenEndIndex) && GivenEndIndex < CurrentEndIndex)
            {
                // top/left part of current range is within delete range
                CurrentStartIndex = GivenEndIndex + 1;
                CurrentStartIndex -= Delta;
                CurrentEndIndex -= Delta;
            }
            else if (CurrentStartIndex < GivenStartIndex && GivenEndIndex < CurrentEndIndex)
            {
                // current range strictly covers the delete range
                CurrentEndIndex -= Delta;
            }
            else if (CurrentStartIndex < GivenStartIndex && (GivenStartIndex <= CurrentEndIndex && CurrentEndIndex <= GivenEndIndex))
            {
                // bottom/right part of current range is within delete range
                // That part is gone, so move the end index to 1 level before the given start index
                CurrentEndIndex = GivenStartIndex - 1;
            }

            // else the delete range is complete below/right-of the current range
            // so don't have to do anything
        }

        /// <summary>
        /// This returns a list of index with pixel lengths. Depending on the type,
        /// the pixel length is for row heights or column widths
        /// </summary>
        /// <param name="IsRow"></param>
        /// <param name="StartIndex"></param>
        /// <param name="EndIndex"></param>
        /// <returns></returns>
        internal Dictionary<int, int> AutoFitRowColumn(bool IsRow, int StartIndex, int EndIndex)
        {
            int i;
            Dictionary<int, int> pixellength = new Dictionary<int, int>();
            // initialise all to zero first. This also ensures the existence of a dictionary entry.
            for (i = StartIndex; i <= EndIndex; ++i)
            {
                pixellength[i] = 0;
            }

            List<SLCellPoint> ptkeys = slws.Cells.Keys.ToList<SLCellPoint>();
            SLCell c;
            SLRstType rst;
            Text txt;
            Run run;
            FontSchemeValues vFontScheme;
            int index;
            SLStyle style;
            string sFontName;
            double fFontSize;
            bool bBold;
            bool bItalic;
            bool bStrike;
            bool bUnderline;
            System.Drawing.FontStyle drawstyle;
            System.Drawing.SizeF szf;
            string sText;
            float fWidth;
            float fHeight;
            int iPointIndex;

            foreach (SLCellPoint pt in ptkeys)
            {
                if (IsRow) iPointIndex = pt.RowIndex;
                else iPointIndex = pt.ColumnIndex;

                if (StartIndex <= iPointIndex && iPointIndex <= EndIndex)
                {
                    c = slws.Cells[pt];
                    style = this.GetCellStyle(pt.RowIndex, pt.ColumnIndex);
                    sText = string.Empty;

                    fWidth = 0;
                    fHeight = 0;

                    if (c.DataType == CellValues.SharedString)
                    {
                        index = Convert.ToInt32(c.NumericValue);
                        if (index >= 0 && index < listSharedString.Count)
                        {
                            rst = new SLRstType();
                            rst.FromHash(listSharedString[index]);
                            i = 0;
                            foreach (var child in rst.istrReal.ChildElements.Reverse())
                            {
                                sText = string.Empty;
                                sFontName = SimpleTheme.MinorLatinFont;
                                fFontSize = SLConstants.DefaultFontSize;
                                bBold = false;
                                bItalic = false;
                                bStrike = false;
                                bUnderline = false;
                                drawstyle = System.Drawing.FontStyle.Regular;
                                if (style.HasFont)
                                {
                                    if (style.fontReal.HasFontScheme)
                                    {
                                        if (style.fontReal.FontScheme == FontSchemeValues.Major) sFontName = SimpleTheme.MajorLatinFont;
                                        else if (style.fontReal.FontScheme == FontSchemeValues.Minor) sFontName = SimpleTheme.MinorLatinFont;
                                        else if (style.fontReal.FontName.Length > 0) sFontName = style.fontReal.FontName;
                                    }
                                    else if (!string.IsNullOrEmpty(style.fontReal.FontName)) 
                                        sFontName = style.fontReal.FontName;

                                    if (style.fontReal.FontSize != null) fFontSize = style.fontReal.FontSize.Value;
                                    if (style.fontReal.Bold != null && style.fontReal.Bold.Value) bBold = true;
                                    if (style.fontReal.Italic != null && style.fontReal.Italic.Value) bItalic = true;
                                    if (style.fontReal.Strike != null && style.fontReal.Strike.Value) bStrike = true;
                                    if (style.fontReal.HasUnderline) bUnderline = true;
                                }

                                if (child is Text)
                                {
                                    txt = (Text)child;
                                    sText = txt.Text;
                                }
                                else if (child is Run)
                                {
                                    run = (Run)child;
                                    sText = run.Text.Text;
                                    vFontScheme = FontSchemeValues.None;
                                    using (OpenXmlReader oxr = OpenXmlReader.Create(run))
                                    {
                                        while (oxr.Read())
                                        {
                                            if (oxr.ElementType == typeof(RunFont))
                                            {
                                                sFontName = ((RunFont)oxr.LoadCurrentElement()).Val;
                                            }
                                            else if (oxr.ElementType == typeof(FontSize))
                                            {
                                                fFontSize = ((FontSize)oxr.LoadCurrentElement()).Val;
                                            }
                                            else if (oxr.ElementType == typeof(Bold))
                                            {
                                                Bold b = (Bold)oxr.LoadCurrentElement();
                                                if (b.Val == null) bBold = true;
                                                else bBold = b.Val.Value;
                                            }
                                            else if (oxr.ElementType == typeof(Italic))
                                            {
                                                Italic itlc = (Italic)oxr.LoadCurrentElement();
                                                if (itlc.Val == null) bItalic = true;
                                                else bItalic = itlc.Val.Value;
                                            }
                                            else if (oxr.ElementType == typeof(Strike))
                                            {
                                                Strike strk = (Strike)oxr.LoadCurrentElement();
                                                if (strk.Val == null) bStrike = true;
                                                else bStrike = strk.Val.Value;
                                            }
                                            else if (oxr.ElementType == typeof(Underline))
                                            {
                                                Underline und = (Underline)oxr.LoadCurrentElement();
                                                if (und.Val == null)
                                                {
                                                    bUnderline = true;
                                                }
                                                else
                                                {
                                                    if (und.Val.Value != UnderlineValues.None) bUnderline = true;
                                                    else bUnderline = false;
                                                }
                                            }
                                            else if (oxr.ElementType == typeof(FontScheme))
                                            {
                                                vFontScheme = ((FontScheme)oxr.LoadCurrentElement()).Val;
                                            }
                                        }
                                    }

                                    if (vFontScheme == FontSchemeValues.Major) sFontName = SimpleTheme.MajorLatinFont;
                                    else if (vFontScheme == FontSchemeValues.Minor) sFontName = SimpleTheme.MinorLatinFont;
                                }

                                // the last element has the trailing spaces ignored. Hence the Reverse() above.
                                if (i == 0)
                                {
                                    sText = sText.TrimEnd();
                                }

                                if (bBold) drawstyle |= System.Drawing.FontStyle.Bold;
                                if (bItalic) drawstyle |= System.Drawing.FontStyle.Italic;
                                if (bStrike) drawstyle |= System.Drawing.FontStyle.Strikeout;
                                if (bUnderline) drawstyle |= System.Drawing.FontStyle.Underline;

                                szf = SLTool.MeasureText(sText, sFontName, fFontSize, drawstyle);
                                if (szf.Height > fHeight) fHeight = szf.Height;
                                fWidth += szf.Width;

                                ++i;
                            }
                        }
                    }
                    else
                    {
                        if (c.DataType == CellValues.Number)
                        {
                            if (style.FormatCode.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(c.CellText))
                                {
                                    sText = SLTool.ToSampleDisplayFormat(Convert.ToDouble(c.CellText), style.FormatCode);
                                }
                                else
                                {
                                    sText = SLTool.ToSampleDisplayFormat(c.NumericValue, style.FormatCode);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(c.CellText))
                                {
                                    sText = SLTool.ToSampleDisplayFormat(Convert.ToDouble(c.CellText), "General");
                                }
                                else
                                {
                                    sText = SLTool.ToSampleDisplayFormat(c.NumericValue, "General");
                                }
                            }
                        }
                        else if (c.DataType == CellValues.Boolean)
                        {
                            if (c.NumericValue > 0.5) sText = "TRUE";
                            else sText = "FALSE";
                        }
                        else
                        {
                            if (c.CellText != null) sText = c.CellText;
                            else sText = string.Empty;
                        }

                        sFontName = SimpleTheme.MinorLatinFont;
                        fFontSize = SLConstants.DefaultFontSize;
                        bBold = false;
                        bItalic = false;
                        bStrike = false;
                        bUnderline = false;
                        drawstyle = System.Drawing.FontStyle.Regular;
                        if (style.HasFont)
                        {
                            if (style.fontReal.HasFontScheme)
                            {
                                if (style.fontReal.FontScheme == FontSchemeValues.Major) sFontName = SimpleTheme.MajorLatinFont;
                                else if (style.fontReal.FontScheme == FontSchemeValues.Minor) sFontName = SimpleTheme.MinorLatinFont;
                                else if (style.fontReal.FontName.Length > 0) sFontName = style.fontReal.FontName;
                            }
                            else if (!string.IsNullOrEmpty(style.fontReal.FontName))
                                sFontName = style.fontReal.FontName;

                            if (style.fontReal.FontSize != null) fFontSize = style.fontReal.FontSize.Value;
                            if (style.fontReal.Bold != null && style.fontReal.Bold.Value) bBold = true;
                            if (style.fontReal.Italic != null && style.fontReal.Italic.Value) bItalic = true;
                            if (style.fontReal.Strike != null && style.fontReal.Strike.Value) bStrike = true;
                            if (style.fontReal.HasUnderline) bUnderline = true;
                        }

                        if (bBold) drawstyle |= System.Drawing.FontStyle.Bold;
                        if (bItalic) drawstyle |= System.Drawing.FontStyle.Italic;
                        if (bStrike) drawstyle |= System.Drawing.FontStyle.Strikeout;
                        if (bUnderline) drawstyle |= System.Drawing.FontStyle.Underline;

                        szf = SLTool.MeasureText(sText, sFontName, fFontSize, drawstyle);
                        fWidth = szf.Width;
                        fHeight = szf.Height;
                    }

                    // Through empirical experimental data, it appears that there's still a bit of padding
                    // at the end of the column when autofitting column widths. I don't know how to
                    // calculate this padding. So I guess. I experimented with the widths of obvious
                    // characters such as a space, an exclamation mark, a period.

                    // Then I remember there's the documentation on the Open XML class property
                    // Column.Width, which says there's an extra 5 pixels, 2 pixels on the left/right
                    // and a pixel for the gridlines.

                    // Note that this padding appears to change depending on the font/typeface and 
                    // font size used. (Haha... where have I seen this before...) So 5 pixels doesn't
                    // seem to work exactly. Or maybe it's wrong because the method of measuring isn't
                    // what Excel actually uses to measure the text.

                    // Since we're autofitting, it seems fitting (haha) that the column width is slightly
                    // larger to accomodate the text. So it's best to err on the larger side.
                    // Thus we add 7 instead of the "recommended" or "documented" 5 pixels, 1 extra pixel
                    // on the left and right.
                    fWidth += 7;
                    // I could also have used 8, but it might have been too much of an extra padding.
                    // The number 8 is a lucky number in Chinese culture. Goodness knows I need some
                    // luck figuring out what Excel is doing...

                    if (style.HasAlignment && style.alignReal.TextRotation != null)
                    {
                        szf = SLTool.CalculateOuterBoundsOfRotatedRectangle(fWidth, fHeight, style.alignReal.TextRotation.Value);
                        fHeight = szf.Height;
                        fWidth = szf.Width;
                    }

                    if (IsRow)
                    {
                        if (pixellength[iPointIndex] < fHeight)
                        {
                            pixellength[iPointIndex] = Convert.ToInt32(Math.Ceiling(fHeight));
                        }
                    }
                    else
                    {
                        if (pixellength[iPointIndex] < fWidth)
                        {
                            pixellength[iPointIndex] = Convert.ToInt32(Math.Ceiling(fWidth));
                        }
                    }
                }
            }

            return pixellength;
        }
    }
}
