using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Xml;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;

namespace ResourceTool
{
    class Program
    {
        private const string RootPath = @"..\..\..\..\..\Neurotoxin.Godspeed\Neurotoxin.Godspeed.Shell";

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ResourceTool.exe -i/e sample.xlsx");
                return;
            }

            switch (args[0])
            {
                case "-i":
                    {
                        var doc = new SLDocument(args[1]);
                        doc.SelectWorksheet("Translations");
                        var stat = doc.GetWorksheetStatistics();
                        var languages = new List<string>();
                        for (var i = 3; i <= stat.EndColumnIndex; i++)
                        {
                            var lang = doc.GetCellValueAsString(2, i);
                            var resxName = lang == "en-US" ? "Resources.resx" : string.Format("Resources.{0}.resx", lang);
                            languages.Add(resxName);
                        }

                        for (var j = 0; j < languages.Count; j++)
                        {
                            var language = languages[j];
                            var resxFile = Path.Combine(RootPath, "Properties", language);
                            var resx = new XmlDocument();
                            resx.Load(resxFile);

                            for (var i = 3; i <= stat.EndRowIndex; i++)
                            {
                                var key = doc.GetCellValueAsString(i, 1);
                                Console.WriteLine("{0} {1} {2}", i, key, language);

                                var value = doc.GetCellValueAsString(i, j + 3);

                                var node = resx.SelectSingleNode(string.Format(".//data[@name='{0}']", key));
                                if (node == null)
                                {
                                    node = resx.CreateElement("data");
                                    var name = resx.CreateAttribute("name");
                                    name.Value = key;
                                    node.Attributes.Append(name);
                                    var xmlspace = resx.CreateAttribute("xml:space");
                                    xmlspace.Value = "preserve";
                                    node.Attributes.Append(xmlspace);
                                    resx.DocumentElement.AppendChild(node);
                                    var valueNode = resx.CreateElement("value");
                                    valueNode.InnerText = value;
                                    node.AppendChild(valueNode);
                                }
                                else
                                {
                                    var valueNode = node.SelectSingleNode("value");
                                    if (valueNode == null)
                                    {
                                        valueNode = resx.CreateElement("value");
                                        node.AppendChild(valueNode);
                                    }
                                    valueNode.InnerText = value;
                                }
                            }
                            resx.Save(resxFile);
                        }
                    }
                    break;
                case "-e":
                    {
                        if (File.Exists(args[1])) File.Delete(args[1]);
                        var r = new Regex(@"Resources\.([a-z]{2}-[A-Z]{2})?\.?resx");
                        var doc = new SLDocument();
                        doc.AddWorksheet("Translations");
                        doc.DeleteWorksheet("Sheet1");
                        doc.SelectWorksheet("Translations");
                        doc.SetCellValue(1, 1, "Key");
                        doc.SetCellValue(1, 2, "Comment");

                        var headerStyle = doc.CreateStyle();
                        headerStyle.Font.Bold = true;
                        headerStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue,
                            System.Drawing.Color.LightBlue);
                        doc.SetCellStyle(1, 1, headerStyle);
                        doc.SetCellStyle(1, 2, headerStyle);

                        var resxFiles = Directory.GetFiles(Path.Combine(RootPath, "Properties"), "*.resx");
                        var columnIndex = 3;
                        var keys = new List<string>();
                        var files = resxFiles.OrderBy(n =>
                        {
                            var l = r.Match(Path.GetFileName(n)).Groups[1].Value;
                            return string.IsNullOrEmpty(l) ? null : n;
                        });
                        foreach (var resx in files)
                        {
                            var lang = r.Match(Path.GetFileName(resx)).Groups[1].Value;

                            var english = false;
                            if (string.IsNullOrEmpty(lang))
                            {
                                lang = "en-US";
                                english = true;
                            }

                            var ci = CultureInfo.GetCultureInfo(lang);
                            doc.SetCellValue(1, columnIndex, ci.EnglishName);
                            doc.SetCellValue(2, columnIndex, ci.Name);
                            doc.SetCellStyle(1, columnIndex, headerStyle);
                            doc.SetColumnWidth(columnIndex, 100);

                            var rr = new ResXResourceReader(resx);
                            if (english)
                            {
                                rr.UseResXDataNodes = true;
                                var rowIndex = 3;
                                foreach (DictionaryEntry entry in rr)
                                {
                                    var key = (string) entry.Key;
                                    var node = (ResXDataNode) entry.Value;
                                    keys.Add(key);
                                    doc.SetCellValue(rowIndex, 1, key);
                                    doc.SetCellValue(rowIndex, 2, node.Comment);
                                    doc.SetCellValue(rowIndex, columnIndex,
                                        node.GetValue((ITypeResolutionService) null).ToString());
                                    rowIndex++;
                                }
                            }
                            else
                            {
                                rr.UseResXDataNodes = true;
                                foreach (DictionaryEntry entry in rr)
                                {
                                    var key = (string) entry.Key;
                                    var node = (ResXDataNode) entry.Value;
                                    var index = keys.IndexOf(key);
                                    int rowIndex;
                                    if (index == -1)
                                    {
                                        rowIndex = keys.Count;
                                        keys.Add(key);
                                        doc.SetCellValue(rowIndex, 1, key);
                                    }
                                    else
                                    {
                                        rowIndex = index + 3;
                                    }
                                    doc.SetCellValue(rowIndex, columnIndex,
                                        node.GetValue((ITypeResolutionService) null).ToString());
                                }
                            }

                            columnIndex++;
                        }
                        doc.AutoFitColumn(1, 2);
                        doc.SaveAs(args[1]);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown switch {0}", args[0]);
                    return;
            }

        }
    }
}