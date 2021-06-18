using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    public class GtsfFile
    {
        public int UnknownInt1 { get; set; }
        public int Length { get; set; }
        public int FilesCount { get; set; }
        public int UnknownInt2 { get; set; }
        public List<GfuvFile> GfuvFiles { get; set; } = new();
        public GtshFile GtshFile { get; set; }

        public static GtsfFile ParseFromFile(string file)
        {
            string[] filesInDirectory = Directory.GetFiles(Path.GetDirectoryName(file));

            byte[] data;
            if (Path.GetExtension(file) == ".cmp")
            {
                data = Helpers.DecompressFile(file);
            }
            else
            {
                data = File.ReadAllBytes(file);
            }

            GtsfFile gtsfFile = new GtsfFile
            {
                UnknownInt1 = BitConverter.ToInt32(data.Skip(0x04).Take(4).ToArray()),
                Length = BitConverter.ToInt32(data.Skip(0x0C).Take(4).ToArray()),
                FilesCount = BitConverter.ToInt32(data.Skip(0x10).Take(4).ToArray()),
                UnknownInt2 = BitConverter.ToInt32(data.Skip(0x14).Take(4).ToArray()),
            };

            int index = 0x1C;
            for (int i = 0; i < gtsfFile.FilesCount; i++)
            {
                GfuvFile gfuvFile = new GfuvFile
                {
                    Length = BitConverter.ToInt32(data.Skip(index).Take(4).ToArray()),
                    TilesCount = BitConverter.ToInt32(data.Skip(index + 8).Take(4).ToArray()),
                };
                index += 0x0C;

                gfuvFile.FileName = Encoding.ASCII.GetString(data.Skip(index).Take(0x24).ToArray()).Replace("\0", "");
                index += 0x24;

                for (int j = 0; j < gfuvFile.TilesCount; j++)
                {
                    gfuvFile.BoundingBoxes.Add(new Rectangle(
                        BitConverter.ToInt16(data.Skip(index).Take(2).ToArray()),
                        BitConverter.ToInt16(data.Skip(index + 2).Take(2).ToArray()),
                        BitConverter.ToInt16(data.Skip(index + 4).Take(2).ToArray()),
                        BitConverter.ToInt16(data.Skip(index + 6).Take(2).ToArray())));
                    index += 8;
                }

                string associatedFile = filesInDirectory.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == gfuvFile.FileName);
                if (string.IsNullOrEmpty(associatedFile))
                {
                    gfuvFile.AssociatedGfntFile = null;
                }
                else if (Path.GetExtension(associatedFile) == ".cmp")
                {
                    gfuvFile.AssociatedGfntFile = GfntFile.ParseFromCompressedFile(associatedFile);
                }
                else
                {
                    gfuvFile.AssociatedGfntFile = GfntFile.ParseFromDecompressedFile(associatedFile);
                }

                gtsfFile.GfuvFiles.Add(gfuvFile);
                index += 4; // next file header;
            }

            var gtshFile = new GtshFile
            {
                Length = BitConverter.ToInt32(data.Skip(index).Take(4).ToArray()),
                NumSpriteDefs = BitConverter.ToInt32(data.Skip(index + 0x08).Take(4).ToArray()),
                UnknownInt1 = BitConverter.ToInt32(data.Skip(index + 0x0C).Take(4).ToArray()),
                UnknownInt2 = BitConverter.ToInt32(data.Skip(index + 0x10).Take(4).ToArray()),
            };
            index += 0x14;

            for (int i = 0; i < gtshFile.NumSpriteDefs; i++)
            {
                gtshFile.SpriteDefs.Add(new GtshFile.SpriteDef
                {
                    NumTiles = BitConverter.ToInt32(data.Skip(index).Take(4).ToArray()),
                    FirstTileInstanceOffset = BitConverter.ToInt32(data.Skip(index + 0x04).Take(4).ToArray()),
                    UnknownShort1 = BitConverter.ToInt16(data.Skip(index + 0x08).Take(2).ToArray()),
                    UnknownShort2 = BitConverter.ToInt16(data.Skip(index + 0x0A).Take(2).ToArray()),
                });
                index += 0x0C;
            }

            foreach (var spriteDef in gtshFile.SpriteDefs)
            {
                for (int i = 0; i < spriteDef.NumTiles; i++)
                {
                    var tileInstance = new GtshFile.TileInstance
                    {
                        GfuvIndex = BitConverter.ToInt32(data.Skip(index).Take(4).ToArray()),
                        TileIndex = BitConverter.ToInt32(data.Skip(index + 0x04).Take(4).ToArray()),
                        TilePosition = (BitConverter.ToInt16(data.Skip(index + 0x08).Take(2).ToArray()),
                                       BitConverter.ToInt16(data.Skip(index + 0x0A).Take(2).ToArray())),
                        TileSize = (BitConverter.ToInt16(data.Skip(index + 0x0C).Take(2).ToArray()),
                                   BitConverter.ToInt16(data.Skip(index + 0x0E).Take(2).ToArray())),
                        Rotation = data[index + 0x10],
                        UnknownByte = data[index + 0x11],
                        UnknownShort2 = BitConverter.ToInt16(data.Skip(index + 0x12).Take(2).ToArray()),
                        UnknownInt1 = BitConverter.ToInt32(data.Skip(index + 0x14).Take(4).ToArray()),
                        UnknownInt2 = BitConverter.ToInt32(data.Skip(index + 0x18).Take(4).ToArray()),
                        UnknownInt3 = BitConverter.ToInt32(data.Skip(index + 0x1C).Take(4).ToArray()),
                    };
                    index += 0x20;
                    gtshFile.TileInstances.Add(tileInstance);
                    spriteDef.TileInstances.Add(tileInstance);
                }
            }

            gtsfFile.GtshFile = gtshFile;

            return gtsfFile;
        }
    }

    public class GfuvFile
    {
        public int Length { get; set; }
        public string FileName { get; set; }
        public int TilesCount { get; set; }
        public List<Rectangle> BoundingBoxes { get; set; } = new();

        public GfntFile AssociatedGfntFile { get; set; }

        public override string ToString()
        {
            return FileName;
        }

        public List<Bitmap> GetTileImages()
        {
            if (AssociatedGfntFile is null)
            {
                return new List<Bitmap>();
            }
            List<Bitmap> tiles = new();
            using var associatedImage = AssociatedGfntFile.GetImage();
            foreach (var boundingBox in BoundingBoxes)
            {
                if ((boundingBox.Width == 0 && boundingBox.Height == 0) || boundingBox.Width > associatedImage.Width || boundingBox.Height > associatedImage.Height)
                {
                    tiles.Add((Bitmap)associatedImage.Clone());
                }
                else
                {
                    tiles.Add(associatedImage.Clone(boundingBox, System.Drawing.Imaging.PixelFormat.DontCare));
                }
            }
            return tiles;
        }
    }

    public class GtshFile
    {
        public int Length { get; set; }
        public int NumSpriteDefs { get; set; } // num sprite defs ?
        public int UnknownInt1 { get; set; } // num tile references / total entries ?
        public int UnknownInt2 { get; set; }
        public List<SpriteDef> SpriteDefs { get; set; } = new();
        public List<TileInstance> TileInstances { get; set; } = new();

        public class SpriteDef
        {
            public int NumTiles { get; set; }
            public int FirstTileInstanceOffset { get; set; }
            public short UnknownShort1 { get; set; }
            public short UnknownShort2 { get; set; }

            public List<TileInstance> TileInstances { get; set; } = new();

            public override string ToString()
            {
                return $"0x{FirstTileInstanceOffset:X4}";
            }

            public Bitmap GetImage(List<GfuvFile> gfuvFiles)
            {
                var bitmap = new Bitmap(256, 384);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    foreach (TileInstance tile in TileInstances)
                    {
                        if (tile.GfuvIndex < 0 || gfuvFiles[tile.GfuvIndex].AssociatedGfntFile is null)
                        {
                            continue;
                        }
                        var image = gfuvFiles[tile.GfuvIndex].GetTileImages()[tile.TileIndex];
                        image.MakeTransparent(gfuvFiles[tile.GfuvIndex].AssociatedGfntFile.Palette[0]);
                        switch (tile.Rotation)
                        {
                            case 0x65:
                                break;
                            case 0xC8:
                                break;
                            case 0xC9:
                                break;
                            case 0xFC:
                                // do not rotate or flip
                                break;
                            case 0xFD:
                                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                break;
                            case 0xFE:
                                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                break;
                            case 0xFF:
                                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                        }
                        g.DrawImage(image,
                            new Rectangle
                            {
                                X = tile.TilePosition.x,
                                Y = tile.TilePosition.y,
                                Width = tile.TileSize.width,
                                Height = tile.TileSize.height
                            });
                    }
                }
                return bitmap;
            }
        }

        public class TileInstance
        {
            public int GfuvIndex { get; set; }
            public int TileIndex { get; set; }
            public (short x, short y) TilePosition { get; set; }
            public (short width, short height) TileSize { get; set; }

            public byte Rotation { get; set; }
            public byte UnknownByte { get; set; }
            public short UnknownShort2 { get; set; }
            public int UnknownInt1 { get; set; }
            public int UnknownInt2 { get; set; }
            public int UnknownInt3 { get; set; }
        }
    }
}
