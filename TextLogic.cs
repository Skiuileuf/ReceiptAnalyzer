using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LidlPriceStats
{
    public static class TextLogic
    {
        public static Dictionary<string, List<PricePoint>> ProcessTextIntoProductDictionary()
        {
            Dictionary<string, List<PricePoint>> ProductPricePoints = new Dictionary<string, List<PricePoint>>();
            string fileconcat = "";
            string filepaths = "";

            foreach (string filePath in Directory.EnumerateFiles("txt"))
            {
                if (!filePath.EndsWith(".jpg.txt")) continue;

                string file = File.ReadAllText(filePath).Replace("\n", " ").Replace("\r", " ");

                Regex subtotalToEOF = new Regex(@"\bSUBTOTAL\b([\s\S]*)");
                file = subtotalToEOF.Replace(file, "");

                Regex header = new Regex(@"S.C.[\s\S]*C.I.F.:");
                file = header.Replace(file, "");

                Regex residualPrice = new Regex(@"\d+\.\d{2}\s(A|B)");
                file = residualPrice.Replace(file, "");

                fileconcat += file + '\n';
                filepaths += filePath + '\n';

                DateTime date = GetDateFromRegexMatch(Path.GetFileName(filePath));
                Console.WriteLine(date);

                Regex productRegex = new Regex(@"(\d+[.,]\d{3})\s(BUC|KG)\sx\s(\d+[.]\d{2})\s(.*?)(?=\s\d{1,3}(?:,\d{3})*(?:\.\d{3})?\s(?:BUC|KG)|\n|$)", RegexOptions.Singleline);
                var matches = productRegex.Matches(file);

                foreach (Match match in matches)
                {
                    var groups = match.Groups;

                    decimal count = decimal.Parse(groups[1].ToString().Replace(',', '.'));
                    string unit = groups[2].ToString().Trim();
                    decimal price = decimal.Parse(groups[3].ToString().Replace(',', '.'));
                    string name = groups[4].ToString().Trim();

                    if (!ProductPricePoints.ContainsKey(name))
                        ProductPricePoints[name] = new List<PricePoint>();

                    ProductPricePoints[name].Add(new PricePoint(date, price));
                }
            }

            var PPPCopy = new Dictionary<string, List<PricePoint>>(ProductPricePoints);

            //foreach (var kvp in ProductPricePoints)
            //{
            //    if (kvp.Value.Count() < 2)
            //    {
            //        PPPCopy.Remove(kvp.Key);
            //    }
            //}

            var a = from entry in PPPCopy orderby entry.Value.Count() descending select entry;


            File.WriteAllText("txt/concat.txt", fileconcat);
            File.WriteAllText("txt/paths.txt", filepaths);

            File.WriteAllText("txt/dict.json", JsonSerializer.Serialize(a));

            return a.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value // Extract the inner list
            );
        }

        public static DateTime GetDateFromRegexMatch(string file)
        {
            Regex dateRegex = new Regex(@"(\d{4})\.(\d{2})\.(\d{2})");
            var dateMatch = dateRegex.Match(file);


            int year = dateMatch.Groups[1].ToString().IntTryParse() ?? 2000;
            int month = dateMatch.Groups[2].ToString().IntTryParse() ?? 1;
            int day = dateMatch.Groups[3].ToString().IntTryParse() ?? 1;

            DateTime date = new DateTime(year, month, day);
            return date;
        }
    }
}
