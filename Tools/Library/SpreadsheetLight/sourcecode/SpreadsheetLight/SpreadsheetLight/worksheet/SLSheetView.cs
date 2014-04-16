using System;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    internal class SLSheetView
    {
        // Part of the set of properties is also in SLPageSettings.
        // Short answer is that it's easier for the programmer to access all page-like
        // properties in one class. That's why this class isn't exposed.
        // So remember to sync whenever relevant.

        internal SLPane Pane { get; set; }

        internal string SelectionAndPivotSelectionXml { get; set; }

        internal bool WindowProtection { get; set; }
        internal bool ShowFormulas { get; set; }
        internal bool ShowGridLines { get; set; }
        internal bool ShowRowColHeaders { get; set; }
        internal bool ShowZeros { get; set; }
        internal bool RightToLeft { get; set; }
        internal bool TabSelected { get; set; }
        internal bool ShowRuler { get; set; }
        internal bool ShowOutlineSymbols { get; set; }
        internal bool DefaultGridColor { get; set; }
        internal bool ShowWhiteSpace { get; set; }
        internal SheetViewValues View { get; set; }
        internal string TopLeftCell { get; set; }
        internal uint ColorId { get; set; }

        internal uint iZoomScale;
        internal uint ZoomScale
        {
            get { return iZoomScale; }
            set
            {
                iZoomScale = value;
                if (iZoomScale < 10) iZoomScale = 10;
                if (iZoomScale > 400) iZoomScale = 400;
            }
        }

        internal uint iZoomScaleNormal;
        internal uint ZoomScaleNormal
        {
            get { return iZoomScaleNormal; }
            set
            {
                iZoomScaleNormal = value;
                if (iZoomScaleNormal < 10) iZoomScaleNormal = 10;
                if (iZoomScaleNormal > 400) iZoomScaleNormal = 400;
            }
        }

        internal uint iZoomScaleSheetLayoutView;
        internal uint ZoomScaleSheetLayoutView
        {
            get { return iZoomScaleSheetLayoutView; }
            set
            {
                iZoomScaleSheetLayoutView = value;
                if (iZoomScaleSheetLayoutView < 10) iZoomScaleSheetLayoutView = 10;
                if (iZoomScaleSheetLayoutView > 400) iZoomScaleSheetLayoutView = 400;
            }
        }

        internal uint iZoomScalePageLayoutView;
        internal uint ZoomScalePageLayoutView
        {
            get { return iZoomScalePageLayoutView; }
            set
            {
                iZoomScalePageLayoutView = value;
                if (iZoomScalePageLayoutView < 10) iZoomScalePageLayoutView = 10;
                if (iZoomScalePageLayoutView > 400) iZoomScalePageLayoutView = 400;
            }
        }

        internal uint WorkbookViewId { get; set; }

        internal SLSheetView()
        {
            this.Pane = new SLPane();

            this.SelectionAndPivotSelectionXml = string.Empty;

            this.WindowProtection = false;
            this.ShowFormulas = false;
            this.ShowGridLines = true;
            this.ShowRowColHeaders = true;
            this.ShowZeros = true;
            this.RightToLeft = false;
            this.TabSelected = false;
            this.ShowRuler = true;
            this.ShowOutlineSymbols = true;
            this.DefaultGridColor = true;
            this.ShowWhiteSpace = true;
            this.View = SheetViewValues.Normal;
            this.TopLeftCell = string.Empty;
            this.ColorId = 64;
            this.iZoomScale = 100;
            this.iZoomScaleNormal = 0;
            this.iZoomScaleSheetLayoutView = 0;
            this.iZoomScalePageLayoutView = 0;
            this.WorkbookViewId = 0;
        }

        internal SheetView ToSheetView()
        {
            SheetView sv = new SheetView();
            if (this.WindowProtection != false) sv.WindowProtection = this.WindowProtection;
            if (this.ShowFormulas != false) sv.ShowFormulas = this.ShowFormulas;
            if (this.ShowGridLines != true) sv.ShowGridLines = this.ShowGridLines;
            if (this.ShowRowColHeaders != true) sv.ShowRowColHeaders = this.ShowRowColHeaders;
            if (this.ShowZeros != true) sv.ShowZeros = this.ShowZeros;
            if (this.RightToLeft != false) sv.RightToLeft = this.RightToLeft;
            if (this.TabSelected != false) sv.TabSelected = this.TabSelected;
            if (this.ShowRuler != true) sv.ShowRuler = this.ShowRuler;
            if (this.ShowOutlineSymbols != true) sv.ShowOutlineSymbols = this.ShowOutlineSymbols;
            if (this.DefaultGridColor != true) sv.DefaultGridColor = this.DefaultGridColor;
            if (this.ShowWhiteSpace != true) sv.ShowWhiteSpace = this.ShowWhiteSpace;
            if (this.View != SheetViewValues.Normal) sv.View = this.View;
            if (this.TopLeftCell != null) sv.TopLeftCell = this.TopLeftCell;
            if (this.ColorId != 64) sv.ColorId = this.ColorId;
            if (this.ZoomScale != 100) sv.ZoomScale = this.ZoomScale;
            if (this.ZoomScaleNormal != 0) sv.ZoomScaleNormal = this.ZoomScaleNormal;
            if (this.ZoomScaleSheetLayoutView != 0) sv.ZoomScaleSheetLayoutView = this.ZoomScaleSheetLayoutView;
            if (this.ZoomScalePageLayoutView != 0) sv.ZoomScalePageLayoutView = this.ZoomScalePageLayoutView;
            sv.WorkbookViewId = this.WorkbookViewId;

            if (this.Pane.HorizontalSplit != 0 || this.Pane.VerticalSplit != 0 || this.Pane.TopLeftCell != null || this.Pane.ActivePane != PaneValues.TopLeft || this.Pane.State != PaneStateValues.Split)
            {
                sv.InnerXml = this.Pane.ToPane().OuterXml;
            }
            sv.InnerXml += this.SelectionAndPivotSelectionXml;
            sv.InnerXml = SLTool.RemoveNamespaceDeclaration(sv.InnerXml);

            return sv;
        }

        internal SLSheetView Clone()
        {
            SLSheetView sv = new SLSheetView();
            sv.Pane = this.Pane.Clone();
            sv.SelectionAndPivotSelectionXml = this.SelectionAndPivotSelectionXml;
            sv.WindowProtection = this.WindowProtection;
            sv.ShowFormulas = this.ShowFormulas;
            sv.ShowGridLines = this.ShowGridLines;
            sv.ShowRowColHeaders = this.ShowRowColHeaders;
            sv.ShowZeros = this.ShowZeros;
            sv.RightToLeft = this.RightToLeft;
            sv.TabSelected = this.TabSelected;
            sv.ShowRuler = this.ShowRuler;
            sv.ShowOutlineSymbols = this.ShowOutlineSymbols;
            sv.DefaultGridColor = this.DefaultGridColor;
            sv.ShowWhiteSpace = this.ShowWhiteSpace;
            sv.View = this.View;
            sv.TopLeftCell = this.TopLeftCell;
            sv.ColorId = this.ColorId;
            sv.iZoomScale = this.iZoomScale;
            sv.iZoomScaleNormal = this.iZoomScaleNormal;
            sv.iZoomScaleSheetLayoutView = this.iZoomScaleSheetLayoutView;
            sv.iZoomScalePageLayoutView = this.iZoomScalePageLayoutView;
            sv.WorkbookViewId = this.WorkbookViewId;

            return sv;
        }

        internal static string GetSheetViewValuesAttribute(SheetViewValues svv)
        {
            string result = "normal";
            switch (svv)
            {
                case SheetViewValues.Normal:
                    result = "normal";
                    break;
                case SheetViewValues.PageBreakPreview:
                    result = "pageBreakPreview";
                    break;
                case SheetViewValues.PageLayout:
                    result = "pageLayout";
                    break;
            }

            return result;
        }
    }
}
