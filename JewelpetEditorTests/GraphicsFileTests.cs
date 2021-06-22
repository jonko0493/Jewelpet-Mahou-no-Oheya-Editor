using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
using System.IO;
namespace JewelpetEditorTests
{
    public class GraphicsFileTests
    {
        [Test]
        [TestCase(TestVariables.BCHARM01_A3_DECOMPRESSED)]
        [TestCase(TestVariables.DATE_DECOMPRESSED)]
        [TestCase(TestVariables.MTO_LOGO_00_COMPRESSED)]
        public void GfntCanBeParsed(string file)
        {
            GfntFile.ParseFromFile(file);
        }
    }
}
