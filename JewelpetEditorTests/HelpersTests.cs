using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace JewelpetEditorTests
{
    public class HelpersTests
    {

        [Test]
        public void ByteEnumerableStartsWithTest()
        {
            List<byte> sequence = new() { 0x00, 0x0A, 0x14, 0x1E, 0x28, 0x32, 0x3C, 0x46, 0x50 };
            byte[] startsWithSequence = new byte[] { 0x00, 0x0A, 0x14 };
            List<byte> doesntStartWithSequence = new() { 0x00, 0x0A, 0x0B };

            Assert.IsTrue(sequence.StartsWith(startsWithSequence));
            Assert.IsFalse(sequence.StartsWith(doesntStartWithSequence));
        }

        [Test]
        [TestCase(TestVariables.COMM_TALK_DECOMPRESSED, "Decompressed Message File")]
        [TestCase(TestVariables.ITEM_MESS_DECOMPRESSED, "Decompressed Message File")]
        [TestCase(TestVariables.DATE_DECOMPRESSED, "Decompressed Graphics Tile File")]
        [TestCase(TestVariables.MTO_LOGO_00_COMPRESSED, "Compressed Graphics Tile File")]
        public void IdentifyFilesTest(string file, string expectedResult)
        {
            var data = File.ReadAllBytes(file);

            Assert.AreEqual(expectedResult, data.IdentifyDataType());
        }
    }
}
