using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    public partial class SLDocument
    {
        internal void LoadSharedStringTable()
        {
            countSharedString = 0;
            listSharedString = new List<string>();
            dictSharedStringHash = new Dictionary<string, int>();

            if (wbp.SharedStringTablePart != null)
            {
                OpenXmlReader oxr = OpenXmlReader.Create(wbp.SharedStringTablePart);
                while (oxr.Read())
                {
                    if (oxr.ElementType == typeof(SharedStringItem))
                    {
                        this.ForceSaveToSharedStringTable((SharedStringItem)oxr.LoadCurrentElement());
                    }
                }
                oxr.Dispose();

                countSharedString = listSharedString.Count;
            }
        }

        internal void WriteSharedStringTable()
        {
            if (wbp.SharedStringTablePart != null)
            {
                if (listSharedString.Count > countSharedString)
                {
                    wbp.SharedStringTablePart.SharedStringTable.Count = (uint)listSharedString.Count;
                    wbp.SharedStringTablePart.SharedStringTable.UniqueCount = (uint)dictSharedStringHash.Count;

                    int diff = listSharedString.Count - countSharedString;
                    for (int i = 0; i < diff; ++i)
                    {
                        wbp.SharedStringTablePart.SharedStringTable.Append(new SharedStringItem()
                        {
                            InnerXml = listSharedString[i + countSharedString]
                        });
                    }
                }
            }
            else
            {
                if (listSharedString.Count > 0)
                {
                    SharedStringTablePart sstp = wbp.AddNewPart<SharedStringTablePart>();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (StreamWriter sw = new StreamWriter(ms))
                        {
                            sw.Write("<x:sst count=\"{0}\" uniqueCount=\"{1}\" xmlns:x=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">", listSharedString.Count, dictSharedStringHash.Count);
                            for (int i = 0; i < listSharedString.Count; ++i)
                            {
                                sw.Write("<x:si>{0}</x:si>", listSharedString[i]);
                            }
                            sw.Write("</x:sst>");
                            sw.Flush();
                            ms.Position = 0;
                            sstp.FeedData(ms);
                        }
                    }
                }
            }
        }
    }
}
