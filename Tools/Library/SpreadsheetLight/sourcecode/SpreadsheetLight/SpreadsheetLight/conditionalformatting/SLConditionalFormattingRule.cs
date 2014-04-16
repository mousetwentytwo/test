using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    internal class SLConditionalFormattingRule
    {
        internal List<Formula> Formulas { get; set; }

        internal bool HasColorScale;
        internal SLColorScale ColorScale { get; set; }
        internal bool HasDataBar;
        internal SLDataBar DataBar { get; set; }
        internal bool HasIconSet;
        internal SLIconSet IconSet { get; set; }

        internal ConditionalFormatValues Type { get; set; }

        internal uint? FormatId { get; set; }
        internal bool HasDifferentialFormat;
        internal SLDifferentialFormat DifferentialFormat { get; set; }

        internal int Priority { get; set; }
        internal bool StopIfTrue { get; set; }
        internal bool AboveAverage { get; set; }
        internal bool Percent { get; set; }
        internal bool Bottom { get; set; }

        internal bool HasOperator;
        internal ConditionalFormattingOperatorValues Operator { get; set; }

        internal string Text { get; set; }

        internal bool HasTimePeriod;
        internal TimePeriodValues TimePeriod { get; set; }

        internal uint? Rank { get; set; }
        internal int? StdDev { get; set; }
        internal bool EqualAverage { get; set; }

        internal SLConditionalFormattingRule()
        {
            this.SetAllNull();
        }

        private void SetAllNull()
        {
            this.Formulas = new List<Formula>();
            this.ColorScale = new SLColorScale();
            this.HasColorScale = false;
            this.DataBar = new SLDataBar();
            this.HasDataBar = false;
            this.IconSet = new SLIconSet();
            this.HasIconSet = false;

            this.Type = ConditionalFormatValues.DataBar;
            this.FormatId = null;
            this.Priority = 1;
            this.StopIfTrue = false;
            this.AboveAverage = true;
            this.Percent = false;
            this.Bottom = false;
            this.Operator = ConditionalFormattingOperatorValues.Equal;
            this.HasOperator = false;
            this.Text = null;
            this.TimePeriod = TimePeriodValues.Today;
            this.HasTimePeriod = false;
            this.Rank = null;
            this.StdDev = null;
            this.EqualAverage = false;

            this.DifferentialFormat = new SLDifferentialFormat();
            this.HasDifferentialFormat = false;
        }

        internal void FromConditionalFormattingRule(ConditionalFormattingRule cfr)
        {
            this.SetAllNull();

            if (cfr.Type != null) this.Type = cfr.Type.Value;
            if (cfr.FormatId != null) this.FormatId = cfr.FormatId.Value;
            this.Priority = cfr.Priority.Value;
            if (cfr.StopIfTrue != null) this.StopIfTrue = cfr.StopIfTrue.Value;
            if (cfr.AboveAverage != null) this.AboveAverage = cfr.AboveAverage.Value;
            if (cfr.Percent != null) this.Percent = cfr.Percent.Value;
            if (cfr.Bottom != null) this.Bottom = cfr.Bottom.Value;
            if (cfr.Operator != null)
            {
                this.Operator = cfr.Operator.Value;
                this.HasOperator = true;
            }
            if (cfr.Text != null) this.Text = cfr.Text.Value;
            if (cfr.TimePeriod != null)
            {
                this.TimePeriod = cfr.TimePeriod.Value;
                this.HasTimePeriod = true;
            }
            if (cfr.Rank != null) this.Rank = cfr.Rank.Value;
            if (cfr.StdDev != null) this.StdDev = cfr.StdDev.Value;
            if (cfr.EqualAverage != null) this.EqualAverage = cfr.EqualAverage.Value;

            foreach (var c in cfr)
            {
                if (c is Formula)
                    this.Formulas.Add((Formula)c.CloneNode(true));

                if (c is ColorScale)
                {
                    this.ColorScale = new SLColorScale();
                    this.ColorScale.FromColorScale((ColorScale)c);
                }
                if (c is DataBar)
                {
                    this.DataBar = new SLDataBar();
                    this.DataBar.FromDataBar((DataBar)c);
                }
                if (c is IconSet)
                {
                    this.IconSet = new SLIconSet();
                    this.IconSet.FromIconSet((IconSet)c);
                }
            }
        }

        internal ConditionalFormattingRule ToConditionalFormattingRule()
        {
            ConditionalFormattingRule cfr = new ConditionalFormattingRule();
            cfr.Type = this.Type;
            if (this.FormatId != null) cfr.FormatId = this.FormatId.Value;
            cfr.Priority = this.Priority;
            if (this.StopIfTrue) cfr.StopIfTrue = this.StopIfTrue;
            if (!this.AboveAverage) cfr.AboveAverage = this.AboveAverage;
            if (this.Percent) cfr.Percent = this.Percent;
            if (this.Bottom) cfr.Bottom = this.Bottom;
            if (HasOperator) cfr.Operator = this.Operator;
            if (this.Text != null) cfr.Text = this.Text;
            if (HasTimePeriod) cfr.TimePeriod = this.TimePeriod;
            if (this.Rank != null) cfr.Rank = this.Rank.Value;
            if (this.StdDev != null) cfr.StdDev = this.StdDev.Value;
            if (this.EqualAverage) cfr.EqualAverage = this.EqualAverage;

            foreach (Formula f in this.Formulas)
            {
                cfr.Append((Formula)f.CloneNode(true));
            }
            if (HasColorScale) cfr.Append(this.ColorScale.ToColorScale());
            if (HasDataBar) cfr.Append(this.DataBar.ToDataBar());
            if (HasIconSet) cfr.Append(this.IconSet.ToIconSet());

            return cfr;
        }

        internal SLConditionalFormattingRule Clone()
        {
            SLConditionalFormattingRule cfr = new SLConditionalFormattingRule();

            cfr.Formulas = new List<Formula>();
            for (int i = 0; i < this.Formulas.Count; ++i)
            {
                cfr.Formulas.Add((Formula)this.Formulas[i].CloneNode(true));
            }

            cfr.HasColorScale = this.HasColorScale;
            cfr.ColorScale = this.ColorScale.Clone();
            cfr.HasDataBar = this.HasDataBar;
            cfr.DataBar = this.DataBar.Clone();
            cfr.HasIconSet = this.HasIconSet;
            cfr.IconSet = this.IconSet.Clone();

            cfr.Type = this.Type;
            cfr.FormatId = this.FormatId;
            cfr.HasDifferentialFormat = this.HasDifferentialFormat;
            cfr.DifferentialFormat = this.DifferentialFormat.Clone();

            cfr.Priority = this.Priority;
            cfr.StopIfTrue = this.StopIfTrue;
            cfr.AboveAverage = this.AboveAverage;
            cfr.Percent = this.Percent;
            cfr.Bottom = this.Bottom;

            cfr.HasOperator = this.HasOperator;
            cfr.Operator = this.Operator;
            cfr.Text = this.Text;
            cfr.HasTimePeriod = this.HasTimePeriod;
            cfr.TimePeriod = this.TimePeriod;
            cfr.Rank = this.Rank;
            cfr.StdDev = this.StdDev;
            cfr.EqualAverage = this.EqualAverage;

            return cfr;
        }
    }
}
