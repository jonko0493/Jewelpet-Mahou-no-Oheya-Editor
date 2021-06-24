using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
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
        [TestCase(TestVariables.NITEN_LOGO_COMPRESSED)]
        public void GfntCanBeParsed(string file)
        {
            GfntFile gfntFile = GfntFile.ParseFromFile(file);
            gfntFile.GetImage();
        }

        // some files fail this test because they have duplicate colors in their palettes
        [Test]
        [TestCase(TestVariables.BCHARM01_A3_DECOMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_00_COMPRESSED)]
        [TestCase(TestVariables.NITEN_LOGO_COMPRESSED)]
        public void GfntParseSetIdempotencyTest(string file)
        {
            GfntFile gfntFile = GfntFile.ParseFromFile(file);
            var pixelData = (byte[])gfntFile.PixelData.Clone();
            var bitmap = gfntFile.GetImage();
            gfntFile.SetImage(bitmap);

            Assert.AreEqual(pixelData, gfntFile.PixelData);
        }
    }
}
