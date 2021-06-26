using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace JewelpetEditorTests
{
    public class GraphicsFileTests
    {
        [Test]
        [TestCase(TestVariables.BCHARM01_A3_DECOMPRESSED)]
        [TestCase(TestVariables.DATE_DECOMPRESSED)]
        [TestCase(TestVariables.FONT_11P_00_COMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_00_COMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_01_COMPRESSED)]
        [TestCase(TestVariables.NITEN_LOGO_COMPRESSED)]
        [TestCase(TestVariables.SANRIO_LOGO_COMPRESSED)]
        [TestCase(TestVariables.SEGATOYS_LOGO_COMPRESSED)]
        [TestCase(TestVariables.COMMON_NEW_COMPRESSED)]
        public void GfntCanBeParsed(string file)
        {
            GfntFile gfntFile = GfntFile.ParseFromFile(file);
            gfntFile.GetImage();
        }

        // some files fail this test because they have duplicate colors in their palettes
        [Test]
        [TestCase(TestVariables.BCHARM01_A3_DECOMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_00_COMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_01_COMPRESSED)]
        [TestCase(TestVariables.NITEN_LOGO_COMPRESSED)]
        [TestCase(TestVariables.SANRIO_LOGO_COMPRESSED)]
        [TestCase(TestVariables.SEGATOYS_LOGO_COMPRESSED)]
        [TestCase(TestVariables.COMMON_NEW_COMPRESSED)]
        public void GfntParseSetImageInverseFunctionsTest(string file)
        {
            GfntFile gfntFile = GfntFile.ParseFromFile(file);
            var pixelData = (byte[])gfntFile.PixelData.Clone();
            var bitmap = gfntFile.GetImage();
            gfntFile.SetImage(bitmap);

            Assert.AreEqual(pixelData, gfntFile.PixelData);
        }

        [Test]
        [TestCase(TestVariables.BCHARM01_A3_DECOMPRESSED)]
        [TestCase(TestVariables.DATE_DECOMPRESSED)]
        [TestCase(TestVariables.FONT_11P_00_COMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_00_COMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_01_COMPRESSED)]
        [TestCase(TestVariables.NITEN_LOGO_COMPRESSED)]
        [TestCase(TestVariables.SANRIO_LOGO_COMPRESSED)]
        [TestCase(TestVariables.SEGATOYS_LOGO_COMPRESSED)]
        [TestCase(TestVariables.COMMON_NEW_COMPRESSED)]
        public void GfntParseSaveInverseFunctionsTest(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            if (dataOnDisk.IdentifyDataType().Contains("Compressed"))
            {
                dataOnDisk = Helpers.DecompressFile(file);
            }
            GfntFile gfntFile = GfntFile.ParseFromData(dataOnDisk);
            byte[] dataInMemory = gfntFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(TestVariables.GT_LOGO_COMPRESSED)]
        [TestCase(TestVariables.GT_COMMON_NEW_COMPRESSED)]
        public void GtpcParseGetBytesInverseFunctionsTest(string file)
        {
            GtpcFile gtpcFile = new(file);
            byte[] dataOnDisk = Helpers.DecompressFile(file);
            byte[] dataInMemory = gtpcFile.GetBytes();

            File.WriteAllBytes("test.cmp.decompressed", dataInMemory);
            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(TestVariables.GT_LOGO_COMPRESSED)]
        [TestCase(TestVariables.GT_COMMON_NEW_COMPRESSED)]
        public void GtpcFullSaveTest(string file)
        {
            file = Path.GetFullPath(file);
            GtpcFile gtpcFile = new(file);
            var tempDirectory = Directory.CreateDirectory("temp");
            gtpcFile.Save(Path.Combine(tempDirectory.FullName, Path.GetFileName(file)));

            string[] originalFiles = Directory.GetFiles(Path.GetDirectoryName(file));
            string[] currentFiles = Directory.GetFiles(tempDirectory.FullName);

            try
            {
                Assert.AreEqual(originalFiles.Select(f => Path.GetFileName(f)), currentFiles.Select(f => Path.GetFileName(f)));

                foreach (string fileName in originalFiles)
                {
                    Assert.AreEqual(Helpers.DecompressFile(fileName), Helpers.DecompressFile(Path.Combine(tempDirectory.FullName, Path.GetFileName(fileName))),
                        $"{Path.GetFileName(fileName)} not equal");
                }
            }
            finally
            {
                Directory.Delete(tempDirectory.FullName, true);
            }
        }
    }
}
