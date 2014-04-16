using System;
using System.Globalization;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    public partial class SLDocument
    {
        /// <summary>
        /// <strong>Obsolete. </strong>Get the column name given the column index.
        /// </summary>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>The column name.</returns>
        [Obsolete("Use SLConvert.ToColumnName() instead.")]
        public static string WhatIsColumnName(int ColumnIndex)
        {
            return SLTool.ToColumnName(ColumnIndex);
        }

        /// <summary>
        /// <strong>Obsolete. </strong>Get the column index given a cell reference or column name.
        /// </summary>
        /// <param name="Input">A cell reference such as "A1" or column name such as "A". If the input is invalid, then -1 is returned.</param>
        /// <returns>The column index.</returns>
        [Obsolete("Use SLConvert.ToColumnIndex() instead.")]
        public static int WhatIsColumnIndex(string Input)
        {
            return SLTool.ToColumnIndex(Input);
        }

        /// <summary>
        /// <strong>Obsolete. </strong>Get the cell reference given the row index and column index.
        /// </summary>
        /// <param name="RowIndex">The row index.</param>
        /// <param name="ColumnIndex">The column index.</param>
        /// <returns>The cell reference.</returns>
        [Obsolete("Use SLConvert.ToCellReference() instead.")]
        public static string WhatIsCellReference(int RowIndex, int ColumnIndex)
        {
            return SLTool.ToCellReference(RowIndex, ColumnIndex);
        }

        /// <summary>
        /// Get the row and column indices given a cell reference such as "C5". A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="CellReference">The cell reference in A1 format, such as "C5".</param>
        /// <param name="RowIndex">When this method returns, this contains the row index of the given cell reference if the conversion succeeded.</param>
        /// <param name="ColumnIndex">When this method returns, this contains the column index of the given cell reference if the conversion succeeded.</param>
        /// <returns>True if the conversion succeeded. False otherwise.</returns>
        public static bool WhatIsRowColumnIndex(string CellReference, out int RowIndex, out int ColumnIndex)
        {
            RowIndex = -1;
            ColumnIndex = -1;
            return SLTool.FormatCellReferenceToRowColumnIndex(CellReference, out RowIndex, out ColumnIndex);
        }

        /// <summary>
        /// Indicates if there's an existing defined name given a name.
        /// </summary>
        /// <param name="Name">Name of defined name to check.</param>
        /// <returns>True if the defined name exists. False otherwise.</returns>
        public bool HasDefinedName(string Name)
        {
            bool result = false;
            if (wbp.Workbook.DefinedNames != null)
            {
                foreach (var child in wbp.Workbook.DefinedNames.Elements<DefinedName>())
                {
                    if (child.Name != null && child.Name.Value.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Set a given defined name. If it doesn't exist, a new defined name is created. If it exists, then the existing defined name is overwritten.
        /// </summary>
        /// <param name="Name">Name of defined name. Note that it cannot be a valid cell reference such as A1.</param>
        /// <param name="Text">The reference/content text of the defined name. For example, Sheet1!$A$1:$C$3</param>
        /// <returns>True if the given defined name is created or an existing defined name is overwritten. False otherwise.</returns>
        public bool SetDefinedName(string Name, string Text)
        {
            return SetDefinedName(Name, Text, string.Empty);
        }

        /// <summary>
        /// Set a given defined name. If it doesn't exist, a new defined name is created. If it exists, then the existing defined name is overwritten.
        /// </summary>
        /// <param name="Name">Name of defined name. Note that it cannot be a valid cell reference such as A1.</param>
        /// <param name="Text">The reference/content text of the defined name. For example, Sheet1!$A$1:$C$3</param>
        /// <param name="Comment">Comment for the defined name.</param>
        /// <returns>True if the given defined name is created or an existing defined name is overwritten. False otherwise.</returns>
        public bool SetDefinedName(string Name, string Text, string Comment)
        {
            Name = Name.Trim();
            if (SLTool.IsCellReference(Name))
            {
                return false;
            }

            bool bFound = false;
            SLDefinedName dn = new SLDefinedName(Name);
            dn.Text = Text;
            if (Comment != null && Comment.Length > 0) dn.Comment = Comment;
            foreach (SLDefinedName d in slwb.DefinedNames)
            {
                if (d.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    bFound = true;
                    d.Text = Text;
                    if (Comment != null && Comment.Length > 0) d.Comment = Comment;
                    break;
                }
            }

            if (!bFound)
            {
                slwb.DefinedNames.Add(dn);
            }

            return true;
        }

        /// <summary>
        /// Get reference/content text of existing defined name.
        /// </summary>
        /// <param name="Name">Name of existing defined name.</param>
        /// <returns>Reference/content text of defined name. An empty string is returned if the given defined name doesn't exist.</returns>
        public string GetDefinedNameText(string Name)
        {
            string result = string.Empty;
            foreach (SLDefinedName d in slwb.DefinedNames)
            {
                if (d.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = d.Text;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the comment of existing defined name.
        /// </summary>
        /// <param name="Name">Name of existing defined name.</param>
        /// <returns>The comment of the defined name. An empty string is returned if the given defined name doesn't exist, or there's no comment.</returns>
        public string GetDefinedNameComment(string Name)
        {
            string result = string.Empty;
            foreach (SLDefinedName d in slwb.DefinedNames)
            {
                if (d.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = d.Comment ?? string.Empty;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Delete a defined name if it exists.
        /// </summary>
        /// <param name="Name">Name of defined name.</param>
        /// <returns>True if specified name is deleted. False otherwise.</returns>
        public bool DeleteDefinedName(string Name)
        {
            bool result = false;
            for (int i = 0; i < slwb.DefinedNames.Count; ++i)
            {
                if (slwb.DefinedNames[i].Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = true;
                    slwb.DefinedNames.RemoveAt(i);
                    break;
                }
            }

            return result;
        }
    }
}
