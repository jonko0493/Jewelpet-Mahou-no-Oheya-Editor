using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    public class GfntFile
    {
        public int IntContainingPaletteLength { get; set; }
        public int PaletteLength 
        { 
            get 
            {
                int lsbAdjusted = (IntContainingPaletteLength & 0x7F) - 0x1C;
                if (lsbAdjusted == 0)
                {
                    return 0x200;
                }
                else
                {
                    return lsbAdjusted * PaletteMultiplier;
                }
            }
        }
        public TileForm ImageTileForm { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int PaletteMultiplier { get; set; }
        public byte[] PixelData { get; set; }
        public byte[] PaletteData { get; set; }

        public List<Color> Palette { get; set; } = new();

        public enum TileForm
        {
            A3 = 0x01,
            GBA_4BPP = 0x03,
            GBA_8BPP = 0x04,
            A5 = 0x06,
        }

        public static GfntFile ParseFromData(byte[] data)
        {
            GfntFile gfntFile = new GfntFile
            {
                IntContainingPaletteLength = BitConverter.ToInt32(data.Skip(4).Take(4).ToArray()),
                ImageTileForm = (TileForm)BitConverter.ToInt32(data.Skip(0x0C).Take(4).ToArray()),
                TileWidth = (int)Math.Pow(2, 3 + BitConverter.ToInt32(data.Skip(0x10).Take(4).ToArray())),
                TileHeight = (int)Math.Pow(2, 3 + BitConverter.ToInt32(data.Skip(0x14).Take(4).ToArray())),
                PaletteMultiplier = BitConverter.ToInt32(data.Skip(0x18).Take(4).ToArray()),
            };

            gfntFile.PixelData = data.Skip(0x1C).Take(data.Length - 0x1C - gfntFile.PaletteLength).ToArray(); // minus header, minus palette
            gfntFile.PaletteData = data.TakeLast(gfntFile.PaletteLength).ToArray();

            if (gfntFile.ImageTileForm == TileForm.A3 || gfntFile.ImageTileForm == TileForm.A5)
            {
                int alphaStep = gfntFile.PaletteLength / 2;
                int alphaStart = 255;

                for (int alpha = alphaStart; alpha >= 0 && gfntFile.Palette.Count < 256; alpha -= alphaStep)
                {
                    for (int i = gfntFile.PaletteData.Length - 2; i >= 0 ; i -= 2)
                    {
                        short color = BitConverter.ToInt16(gfntFile.PaletteData.Skip(i).Take(2).ToArray());
                        gfntFile.Palette.Insert(0, Color.FromArgb(alpha, (color & 0x1F) << 3, ((color >> 5) & 0x1F) << 3, ((color >> 10) & 0x1F) << 3));
                    }
                }
            }
            else
            {
                for (int i = 0; i < gfntFile.PaletteData.Length; i += 2)
                {
                    short color = BitConverter.ToInt16(gfntFile.PaletteData.Skip(i).Take(2).ToArray());
                    gfntFile.Palette.Add(Color.FromArgb((color & 0x1F) << 3, ((color >> 5) & 0x1F) << 3, ((color >> 10) & 0x1F) << 3));
                }
            }

            return gfntFile;
        }

        public static GfntFile ParseFromFile(string file)
        {
            byte[] data = File.ReadAllBytes(file);

            if (data.IdentifyDataType().Contains("Compressed"))
            {
                data = Helpers.DecompressFile(file);
            }

            return ParseFromData(data);
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new();

            bytes.AddRange(Encoding.ASCII.GetBytes("GFNT"));
            bytes.AddRange(BitConverter.GetBytes(IntContainingPaletteLength));
            bytes.AddRange(Encoding.ASCII.GetBytes("1.02"));
            bytes.AddRange(BitConverter.GetBytes((int)ImageTileForm));
            bytes.AddRange(BitConverter.GetBytes((int)Math.Log2(TileWidth) - 3));
            bytes.AddRange(BitConverter.GetBytes((int)Math.Log2(TileHeight) - 3));
            bytes.AddRange(BitConverter.GetBytes(PaletteMultiplier));
            bytes.AddRange(PixelData);
            bytes.AddRange(PaletteData);

            return bytes.ToArray();
        }

        public void SaveToFile(string file)
        {
            if (Path.GetExtension(file) == ".cmp")
            {
                Helpers.SaveToCompressedFile(file, GetBytes());
            }
            else
            {
                File.WriteAllBytes(file, GetBytes());
            }
        }

        public Bitmap GetImage()
        {
            int imageWidth = TileWidth * 2, imageHeight = TileHeight * 2;
            var bitmap = new Bitmap(imageWidth, imageHeight);
            int pixelIndex = 0;

            for (int row = 0; row < imageWidth / 8 && pixelIndex < PixelData.Length; row++)
            {
                for (int col = 0; col < imageHeight / 8 && pixelIndex < PixelData.Length; col++)
                {
                    for (int ypix = 0; ypix < TileHeight && pixelIndex < PixelData.Length; ypix++)
                    {
                        if (ImageTileForm == TileForm.GBA_4BPP)
                        {
                            for (int xpix = 0; xpix < TileWidth / 2 && pixelIndex < PixelData.Length; xpix++)
                            {
                                for (int xypix = 0; xypix < 2 && pixelIndex < PixelData.Length; xypix++)
                                {
                                    bitmap.SetPixel((col * 8) + (xpix * 2) + xypix, (row * 8) + ypix,
                                        Palette[PixelData[pixelIndex] >> (xypix * 4) & 0xF]);
                                }
                                pixelIndex++;
                            }
                        }
                        else
                        {
                            for (int xpix = 0; xpix < TileWidth && pixelIndex < PixelData.Length; xpix++)
                            {
                                bitmap.SetPixel((col * 8) + xpix, (row * 8) + ypix,
                                    Palette[PixelData[pixelIndex++]]);
                            }
                        }
                    }
                }
            }
            return bitmap;
        }
        public void SetImage(string bitmapFile)
        {
            SetImage(new Bitmap(bitmapFile));
        }

        public void SetImage(Bitmap bitmap)
        {
            // at least for now we're going to enforce same tile size
            if (bitmap.Width != TileWidth * 2 && bitmap.Height != TileHeight * 2)
            {
                throw new ArgumentException($"Bitmap size does not match expected size of {TileWidth * 2} x {TileHeight * 2}");
            }

            List<byte> pixelData = new();

            for (int row = 0; row < bitmap.Width / 8 && pixelData.Count < PixelData.Length; row++)
            {
                for (int col = 0; col < bitmap.Height / 8 && pixelData.Count < PixelData.Length; col++)
                {
                    for (int ypix = 0; ypix < TileHeight && pixelData.Count < PixelData.Length; ypix++)
                    {
                        if (ImageTileForm == TileForm.GBA_4BPP)
                        {
                            for (int xpix = 0; xpix < TileWidth / 2 && pixelData.Count < PixelData.Length; xpix++)
                            {
                                int color1 = Helpers.ClosestColorIndex(Palette, bitmap.GetPixel((col * 8) + (xpix * 2), (row * 8) + ypix));
                                int color2 = Helpers.ClosestColorIndex(Palette, bitmap.GetPixel((col * 8) + (xpix * 2) + 1, (row * 8) + ypix));

                                pixelData.Add((byte)(color1 + (color2 << 4)));
                            }
                        }
                        else
                        {
                            for (int xpix = 0; xpix < TileWidth && pixelData.Count < PixelData.Length; xpix++)
                            {
                                pixelData.Add((byte)Helpers.ClosestColorIndex(Palette, bitmap.GetPixel((col * 8) + xpix, (row * 8) + ypix)));
                            }
                        }
                    }
                }
            }

            PixelData = pixelData.ToArray();
        }

        public byte[] GetRiffPaletteBytes()
        {
            List<byte> riffBytes = new List<byte>();

            Color[] tempPaletteArray = new Color[Palette.Count];
            Palette.CopyTo(tempPaletteArray);
            var tempPalette = tempPaletteArray.ToList();

            while (tempPalette.Count < 256)
            {
                tempPalette.Add(Color.Black);
            }

            int documentSize = 16 + tempPalette.Count * 4;
            ushort count = (ushort)tempPalette.Count;

            riffBytes.AddRange(Encoding.ASCII.GetBytes("RIFF"));
            riffBytes.AddRange(BitConverter.GetBytes(documentSize));
            riffBytes.AddRange(Encoding.ASCII.GetBytes("PAL data"));
            riffBytes.AddRange(BitConverter.GetBytes(0));
            riffBytes.AddRange(new byte[] { 0, 3 }); // version
            riffBytes.AddRange(BitConverter.GetBytes(count));

            foreach (Color color in tempPalette)
            {
                riffBytes.Add(color.R);
                riffBytes.Add(color.G);
                riffBytes.Add(color.B);
                riffBytes.Add(0); // leave flags unset
            }

            return riffBytes.ToArray();
        }

        public void SaveRiffPaletteToFile(string file)
        {
            File.WriteAllBytes(file, GetRiffPaletteBytes());
        }

    }
}
