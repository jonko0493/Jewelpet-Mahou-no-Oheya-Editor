using PuyoTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    public static class Helpers
    {
        public static BitmapImage GetBitmapImageFromBitmap(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public static byte[] DecompressFile(string file)
        {
            byte[] decompressedData;
            using FileStream fileStream = File.OpenRead(file);
            using MemoryStream memoryStream = new MemoryStream();
            Lz10Compression.Decompress(fileStream, memoryStream);
            decompressedData = new byte[memoryStream.Length];
            memoryStream.Position = 0;
            while (memoryStream.Position < memoryStream.Length)
            {
                memoryStream.Read(decompressedData);
            }

            return decompressedData;
        }

        public static async Task<byte[]> DecompressFileAsync(string file)
        {
            byte[] decompressedData;
            using FileStream fileStream = File.OpenRead(file);
            using MemoryStream memoryStream = new MemoryStream();
            Lz10Compression.Decompress(fileStream, memoryStream);
            decompressedData = new byte[memoryStream.Length];
            memoryStream.Position = 0;
            while (memoryStream.Position < memoryStream.Length)
            {
                await memoryStream.ReadAsync(decompressedData);
            }

            return decompressedData;
        }

        public static string IdentifyFileType(string file)
        {
            return File.ReadAllBytes(file).IdentifyDataType();
        }

        public static string IdentifyDataType(this byte[] dataRef)
        {
            byte[] data = new byte[dataRef.Length];
            dataRef.CopyTo(data, 0);

            string compressionStatus;

            using MemoryStream memoryStream = new MemoryStream(data);
            if (Lz10Compression.Identify(memoryStream))
            {
                compressionStatus = "Compressed";
                using MemoryStream decompressedStream = new MemoryStream();
                Lz10Compression.Decompress(memoryStream, decompressedStream);
                data = new byte[decompressedStream.Length];
                decompressedStream.Position = 0;
                while (decompressedStream.Position < decompressedStream.Length)
                {
                    decompressedStream.Read(data);
                }
            }
            else
            {
                compressionStatus = "Decompressed";
            }

            if (data.StartsWith("TBB1"))
            {
                for (int i = 0x20; i < data.Length; i += 0x10)
                {
                    var header = data.Skip(i).Take(4);
                    if (header.StartsWith("MTBL"))
                    {
                        return $"{compressionStatus} Message File";
                    }
                    else if (header.StartsWith("TBB1"))
                    {
                        return $"{compressionStatus} Table File";
                    }
                }
            }
            else if (data.StartsWith("GFNT"))
            {
                return $"{compressionStatus} Graphics Tile File";
            }
            else if (data.StartsWith("GTSF"))
            {
                return $"{compressionStatus} Graphics Map/Animation File";
            }

            return "Unknown File";
        }

        public static bool StartsWith(this IEnumerable<byte> sequence, string asciiByteSequence)
        {
            return sequence.StartsWith(Encoding.ASCII.GetBytes(asciiByteSequence));
        }
        public static bool StartsWith(this IEnumerable<byte> sequence, IEnumerable<byte> startsWithSequence)
        {
            if (sequence.Count() < startsWithSequence.Count())
            {
                return false;
            }

            bool startsWith = true;
            int startsWithLength = startsWithSequence.Count();

            for (int i = 0; startsWith && i < startsWithLength; i++)
            {
                startsWith = startsWith && (sequence.ElementAt(i) == startsWithSequence.ElementAt(i));
            }

            return startsWith;
        }

        // redmean color distance formula with alpha term
        public static double ColorDistance(Color color1, Color color2)
        {
            double redmean = (color1.R + color2.R) / 2.0;

            return Math.Sqrt((2 + redmean / 256) * Math.Pow(color1.R - color2.R, 2)
                + 4 * Math.Pow(color1.G - color2.G, 2)
                + (2 + (255 - redmean) / 256) * Math.Pow(color1.B - color2.B, 2)
                + Math.Pow(color1.A - color2.A, 2));
        }

        public static int ClosestColorIndex(List<Color> colors, Color color)
        {
            var colorDistances = colors.Select(c => ColorDistance(c, color)).ToList();

            return colorDistances.IndexOf(colorDistances.Min());
        }
    }

    public class MessageTextBox : TextBox
    {
        public Message Message { get; set; }
        public ListBox MessageListBox { get; set; }
    }
}
