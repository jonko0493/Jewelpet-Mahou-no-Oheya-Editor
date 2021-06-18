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
    }

    public class MessageTextBox : TextBox
    {
        public Message Message { get; set; }
        public ListBox MessageListBox { get; set; }
    }
}
