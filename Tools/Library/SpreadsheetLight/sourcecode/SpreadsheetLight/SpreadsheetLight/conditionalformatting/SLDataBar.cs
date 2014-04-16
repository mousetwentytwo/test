using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    internal class SLDataBar
    {
        internal SLConditionalFormatValueObject Cfvo1 { get; set; }
        internal SLConditionalFormatValueObject Cfvo2 { get; set; }
        internal SLColor Color { get; set; }
        internal uint MinLength { get; set; }
        internal uint MaxLength { get; set; }
        internal bool ShowValue { get; set; }

        internal SLDataBar()
        {
            this.SetAllNull();
        }

        private void SetAllNull()
        {
            this.Cfvo1 = new SLConditionalFormatValueObject();
            this.Cfvo2 = new SLConditionalFormatValueObject();
            this.Color = new SLColor(new List<System.Drawing.Color>(), new List<System.Drawing.Color>());
            this.MinLength = 10;
            this.MaxLength = 90;
            this.ShowValue = true;
        }

        internal void FromDataBar(DataBar db)
        {
            this.SetAllNull();

            using (OpenXmlReader oxr = OpenXmlReader.Create(db))
            {
                int i = 0;
                while (oxr.Read())
                {
                    if (oxr.ElementType == typeof(ConditionalFormatValueObject))
                    {
                        if (i == 0)
                        {
                            this.Cfvo1.FromConditionalFormatValueObject((ConditionalFormatValueObject)oxr.LoadCurrentElement());
                            ++i;
                        }
                        else if (i == 1)
                        {
                            this.Cfvo2.FromConditionalFormatValueObject((ConditionalFormatValueObject)oxr.LoadCurrentElement());
                            ++i;
                        }
                    }
                    else if (oxr.ElementType == typeof(Color))
                    {
                        this.Color.FromSpreadsheetColor((Color)oxr.LoadCurrentElement());
                    }
                }
            }

            if (db.MinLength != null) this.MinLength = db.MinLength.Value;
            if (db.MaxLength != null) this.MaxLength = db.MaxLength.Value;
            if (db.ShowValue != null) this.ShowValue = db.ShowValue.Value;
        }

        internal DataBar ToDataBar()
        {
            DataBar db = new DataBar();
            if (this.MinLength != 10) db.MinLength = this.MinLength;
            if (this.MaxLength != 90) db.MaxLength = this.MaxLength;
            if (!this.ShowValue) db.ShowValue = this.ShowValue;

            db.Append(this.Cfvo1.ToConditionalFormatValueObject());
            db.Append(this.Cfvo2.ToConditionalFormatValueObject());
            db.Append(this.Color.ToSpreadsheetColor());

            return db;
        }

        internal SLDataBar Clone()
        {
            SLDataBar db = new SLDataBar();
            db.Cfvo1 = this.Cfvo1.Clone();
            db.Cfvo2 = this.Cfvo2.Clone();
            db.Color = this.Color.Clone();
            db.MinLength = this.MinLength;
            db.MaxLength = this.MaxLength;
            db.ShowValue = this.ShowValue;

            return db;
        }
    }
}
