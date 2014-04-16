using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    internal class SLIconSet
    {
        internal List<SLConditionalFormatValueObject> Cfvos { get; set; }
        internal IconSetValues IconSetValue { get; set; }
        internal bool ShowValue { get; set; }
        internal bool Percent { get; set; }
        internal bool Reverse { get; set; }

        internal SLIconSet()
        {
            this.SetAllNull();
        }

        private void SetAllNull()
        {
            this.Cfvos = new List<SLConditionalFormatValueObject>();
            this.IconSetValue = IconSetValues.ThreeTrafficLights1;
            this.ShowValue = true;
            this.Percent = true;
            this.Reverse = false;
        }

        internal void FromIconSet(IconSet ics)
        {
            this.SetAllNull();

            if (ics.IconSetValue != null) this.IconSetValue = ics.IconSetValue.Value;
            if (ics.ShowValue != null) this.ShowValue = ics.ShowValue.Value;
            if (ics.Percent != null) this.Percent = ics.Percent.Value;
            if (ics.Reverse != null) this.Reverse = ics.Reverse.Value;

            using (OpenXmlReader oxr = OpenXmlReader.Create(ics))
            {
                SLConditionalFormatValueObject cfvo;
                while (oxr.Read())
                {
                    if (oxr.ElementType == typeof(ConditionalFormatValueObject))
                    {
                        cfvo = new SLConditionalFormatValueObject();
                        cfvo.FromConditionalFormatValueObject((ConditionalFormatValueObject)oxr.LoadCurrentElement());
                        this.Cfvos.Add(cfvo);
                    }
                }
            }
        }

        internal IconSet ToIconSet()
        {
            IconSet ics = new IconSet();
            if (this.IconSetValue != IconSetValues.ThreeTrafficLights1) ics.IconSetValue = this.IconSetValue;
            if (!this.ShowValue) ics.ShowValue = this.ShowValue;
            if (!this.Percent) ics.Percent = this.Percent;
            if (this.Reverse) ics.Reverse = this.Reverse;

            foreach (SLConditionalFormatValueObject cfvo in this.Cfvos)
            {
                ics.Append(cfvo.ToConditionalFormatValueObject());
            }

            return ics;
        }

        internal SLIconSet Clone()
        {
            SLIconSet ics = new SLIconSet();
            ics.Cfvos = new List<SLConditionalFormatValueObject>();
            for (int i = 0; i < this.Cfvos.Count; ++i)
            {
                ics.Cfvos.Add(this.Cfvos[i].Clone());
            }

            ics.IconSetValue = this.IconSetValue;
            ics.ShowValue = this.ShowValue;
            ics.Percent = this.Percent;
            ics.Reverse = this.Reverse;

            return ics;
        }
    }
}
