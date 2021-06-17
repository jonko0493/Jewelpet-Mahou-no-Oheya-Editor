using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    public class GfntFile
    {
        public int PaletteLengthContainingInt { get; set; }
        public int PaletteLength 
        { 
            get 
            {
                int lsbAdjusted = (PaletteLengthContainingInt & 0x7F) - 0x1C;
                if (lsbAdjusted == 0)
                {
                    return 0x200;
                }
                else
                {
                    return lsbAdjusted * PaletteNumber;
                }
            }
        }
        public TileForm ImageTileForm { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int PaletteNumber { get; set; }
        public byte[] PixelData { get; set; }
        public byte[] PaletteData { get; set; }

        List<Color> Palette { get; set; } = new();

        public enum TileForm
        {
            GBA_4BPP = 0x03,
            GBA_8BPP = 0x04,
            RGBA_8BPP = 0x06,
        }

        public static GfntFile ParseFromData(byte[] data)
        {
            GfntFile gfntFile = new GfntFile
            {
                PaletteLengthContainingInt = BitConverter.ToInt32(data.Skip(4).Take(4).ToArray()),
                ImageTileForm = (TileForm)BitConverter.ToInt32(data.Skip(0x0C).Take(4).ToArray()),
                TileWidth = (int)Math.Pow(2, 3 + BitConverter.ToInt32(data.Skip(0x10).Take(4).ToArray())),
                TileHeight = (int)Math.Pow(2, 3 + BitConverter.ToInt32(data.Skip(0x14).Take(4).ToArray())),
                PaletteNumber = BitConverter.ToInt32(data.Skip(0x18).Take(4).ToArray()),
            };

            gfntFile.PixelData = data.Skip(0x1C).Take(data.Length - 0x1C - gfntFile.PaletteLength).ToArray(); // minus header, minus palette
            gfntFile.PaletteData = data.TakeLast(gfntFile.PaletteLength).ToArray();

            for (int i = 0; i < gfntFile.PaletteData.Length; i += 2)
            {
                short color = BitConverter.ToInt16(gfntFile.PaletteData.Skip(i).Take(2).ToArray());
                gfntFile.Palette.Add(Color.FromArgb((color & 0x1F) << 3, ((color >> 5) & 0x1F) << 3, ((color >> 10) & 0x1F) << 3));
            }

            while (gfntFile.Palette.Count < 256)
            {
                gfntFile.Palette.Add(Color.FromArgb(0, 0, 0));
            }

            return gfntFile;
        }

        public static GfntFile ParseFromDecompressedFile(string file)
        {
            return ParseFromData(File.ReadAllBytes(file));
        }

        public static GfntFile ParseFromCompressedFile(string file)
        {
            return ParseFromData(Helpers.DecompressFile(file));
        }

        public Bitmap GetImage()
        {
            var bitmap = new Bitmap(TileWidth * 2, TileHeight * 2);
            int pixelIndex = 0;

            for (int row = 0; row < 32 && pixelIndex < PixelData.Length; row++)
            {
                for (int col = 0; col < 32 && pixelIndex < PixelData.Length; col++)
                {
                    for (int ypix = 0; ypix < TileHeight && pixelIndex < PixelData.Length; ypix++)
                    {
                        if (ImageTileForm == TileForm.GBA_4BPP)
                        {
                            for (int xpix = 0; xpix < TileWidth / 2 && pixelIndex < PixelData.Length; xpix++)
                            {
                                for (int xypix = 0; xypix < 2 && pixelIndex < PixelData.Length; xypix++)
                                {
                                    bitmap.SetPixel((col << 3) + (xpix << 1) + xypix, (row << 3) + ypix,
                                        Palette[PixelData[pixelIndex] >> (xypix << 2) & 0xF]);
                                }
                                pixelIndex++;
                            }
                        }
                        else
                        {
                            for (int xpix = 0; xpix < TileWidth && pixelIndex < PixelData.Length; xpix++)
                            {
                                bitmap.SetPixel((col << 3) + xpix, (row << 3) + ypix,
                                    Palette[PixelData[pixelIndex++]]);
                            }
                        }
                    }
                }
            }
            return bitmap;
        }

        public byte[] GetRiffPaletteBytes()
        {
            List<byte> riffBytes = new List<byte>();

            int documentSize = 16 + Palette.Count * 4;
            ushort count = (ushort)Palette.Count;

            riffBytes.AddRange(Encoding.ASCII.GetBytes("RIFF"));
            riffBytes.AddRange(BitConverter.GetBytes(documentSize));
            riffBytes.AddRange(Encoding.ASCII.GetBytes("PAL data"));
            riffBytes.AddRange(BitConverter.GetBytes(0));
            riffBytes.AddRange(new byte[] { 0, 3 }); // version
            riffBytes.AddRange(BitConverter.GetBytes(count));

            foreach (Color color in Palette)
            {
                riffBytes.Add(color.R);
                riffBytes.Add(color.G);
                riffBytes.Add(color.B);
                riffBytes.Add(0); // leave flags unset
            }

            return riffBytes.ToArray();
        }

    }
}
