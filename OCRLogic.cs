using IronOcr;
using IronSoftware.Drawing;
using System;
using System.IO;

namespace LidlPriceStats
{
    public static class OCRLogic
    {
        public static void ProcessImageFilesUsingOCRAndSaveToTextFiles()
        {
            //Create the OCR and setup languages
            IronTesseract ocr = new IronTesseract();
            ocr.Language = OcrLanguage.RomanianBest;
            ocr.AddSecondaryLanguage(OcrLanguage.EnglishBest);

            foreach (string filePath in Directory.EnumerateFiles("img"))
            {
                Console.WriteLine($"Processing {filePath} ...");
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();

                //Create a new OCR input
                using (OcrInput input = new OcrInput())
                {
                    //Load the image
                    using (AnyBitmap bmp = new AnyBitmap(filePath))
                    {
                        //Crop the header and the calculated price from the receipt
                        input.Add(bmp, new CropRectangle(0, 390, bmp.Width - 250, bmp.Height));

                        //input.SelectTextColor(Color.FromArgb(34, 34, 34), 20);

                        //Replace the blue used for discounts with white
                        input.ReplaceColor(new Color("6dbcf0"), new Color("ffffff"), 64);
                        foreach (var page in input.GetPages())
                        {
                            page.SaveAsImage($"flt/flt_{filePath.Substring(4, 10)}_{page.Index}.bmp");
                        }


                        //Get the result of the OCR and save it to a text file
                        OcrResult result = ocr.Read(input);
                        File.WriteAllText($"txt/{Path.GetFileNameWithoutExtension(filePath)}.txt", result.Text);
                    }
                }
                watch.Stop();
                Console.WriteLine($"Done processing {filePath} (took {watch.ElapsedMilliseconds} ms)");
            }

            Console.WriteLine("Done");
        }
    }
}
