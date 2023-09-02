using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using static LidlPriceStats.TUI;

namespace LidlPriceStats
{
    internal class Program
    {
        public static Dictionary<string, List<PricePoint>> PPD;

        static void Main(string[] args)
        {
            TUI ui = new TUI();
            ui.Init("Lidl Price Analyser");
            MenuNode menu = new MenuNode("Lidl Price Analyser", "Analiza preturilor de la lidl", () => { },
                new MenuNode("Obtine bonuri", "Bonurile se obtin din aplicatia lidl plus sub forma de poze. Pozele se pun in folderul /img. TODO obtinere automata a bonurilor folosind contul lidl?", () =>
                {
                    Console.WriteLine($"Folderul /img contine {Directory.EnumerateFiles("img", "*.jpg.png").Count()} poze cu bonuri.");
                }),
                new MenuNode("Transforma bonurile in text cu ajutorul OCR", "Bonurile se transforma in text. Pozele sunt optimizate pentru a obtine rezultate cat mai corecte. Rezultatele apar in folderul /txt", () =>
                {
                    Console.WriteLine($"Folderul /txt contine {Directory.EnumerateFiles("txt", "*.jpg.txt").Count()} texte extrase din bonuri.");
                    OCRLogic.ProcessImageFilesUsingOCRAndSaveToTextFiles();
                }),
                new MenuNode("Analiza textului", "Textul este analizat si se genereaza un dictionar cu toate produsele si pretul acestora.", () =>
                {
                    PPD = TextLogic.ProcessTextIntoProductDictionary();
                }),
                new MenuNode("Generarea rapoartelor", "Pe urma datelor din analiza se genereaza rapoarte care arata cele mai cumparate produse etc.", () =>
                {
                    if (PPD == null)
                    {
                        Console.WriteLine("Ruleaza analiza textului mai intai...");
                        if (ui.BackOrQuit()) return;

                    }

                    //Generate the datatable with the following columns:
                    // Product name, [... date]
                    // and rows:
                    // [product name], [... price_on_date or empty]

                    SortedSet<DateTime> dateTimes = new SortedSet<DateTime>();

                    foreach (var pd in PPD)
                    {
                        foreach (var pp in pd.Value)
                        {
                            dateTimes.Add(pp.Date);
                        }
                    }

                    Console.WriteLine(dateTimes.Count);

                    //Create the table
                    var dataTable = new DataTable();
                    //Add the table header
                    dataTable.Columns.Add("Name", typeof(string));
                    foreach (var v in dateTimes)
                    {
                        dataTable.Columns.Add(v.ToString("dd.MM.yyyy"), typeof(string));
                    }

                    dataTable.Rows.Add(GetHeaders(dataTable));

                    //Add the table rows
                    foreach (var pd in PPD)
                    {
                        //Build the row
                        //We know dateTimes contains all unique datetime values, so we will make an array the same size as that.
                        var rowData = new object[dateTimes.Count];

                        //Build a dictionary
                        var priceAndDates = new Dictionary<DateTime, decimal>();
                        foreach (var dvp in pd.Value)
                        {
                            priceAndDates[dvp.Date] = dvp.Price;
                        }

                        List<object> rowContents = new List<object>
                        {
                            pd.Key
                        };

                        for (int i = 0; i < rowData.Length; i++)
                        {
                            bool hasData = priceAndDates.ContainsKey(dateTimes.ElementAt(i));
                            if(hasData)
                            {
                                rowData[i] = priceAndDates[dateTimes.ElementAt(i)];
                            } else
                            {
                                rowData[i] = "=NA()";
                            }

                            rowContents.Add(rowData[i]);
                        }

                        object[] array = new object[] { pd.Key, rowData };
                        dataTable.Rows.Add(rowContents.ToArray());
                    }

                    var wb = new ClosedXML.Excel.XLWorkbook();
                    var ws = wb.AddWorksheet();

                    ws.Cell("A1").InsertData(dataTable);

                    wb.SaveAs("txt/excel.xlsx");

                    wb.Dispose();
                })
            );
            ui.Run(menu);

            Console.ReadKey();
        }

        public static DataRow GetHeaders(DataTable dt)
        {
            DataRow dataRow = dt.NewRow();
            string[] columnNames = dt.Columns.Cast<DataColumn>()
                                         .Select(x => x.ColumnName)
                                         .ToArray();
            dataRow.ItemArray = columnNames;
            return dataRow;
        }
    }
}
