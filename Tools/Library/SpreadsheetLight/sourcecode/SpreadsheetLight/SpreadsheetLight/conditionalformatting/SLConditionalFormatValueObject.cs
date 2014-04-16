using System;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    internal class SLConditionalFormatValueObject
    {
        internal ConditionalFormatValueObjectValues Type { get; set; }
        internal string Val { get; set; }
        internal bool GreaterThanOrEqual { get; set; }

        internal SLConditionalFormatValueObject()
        {
            this.SetAllNull();
        }

        private void SetAllNull()
        {
            this.Type = ConditionalFormatValueObjectValues.Percentile;
            this.Val = string.Empty;
            this.GreaterThanOrEqual = true;
        }

        internal void FromConditionalFormatValueObject(ConditionalFormatValueObject cfvo)
        {
            this.SetAllNull();

            this.Type = cfvo.Type.Value;
            if (cfvo.Val != null) this.Val = cfvo.Val.Value;
            if (cfvo.GreaterThanOrEqual != null) this.GreaterThanOrEqual = cfvo.GreaterThanOrEqual.Value;
        }

        internal ConditionalFormatValueObject ToConditionalFormatValueObject()
        {
            ConditionalFormatValueObject cfvo = new ConditionalFormatValueObject();
            cfvo.Type = this.Type;
            cfvo.Val = this.Val;
            if (!this.GreaterThanOrEqual) cfvo.GreaterThanOrEqual = false;

            return cfvo;
        }

        internal SLConditionalFormatValueObject Clone()
        {
            SLConditionalFormatValueObject cfvo = new SLConditionalFormatValueObject();
            cfvo.Type = this.Type;
            cfvo.Val = this.Val;
            cfvo.GreaterThanOrEqual = this.GreaterThanOrEqual;

            return cfvo;
        }
    }
}
