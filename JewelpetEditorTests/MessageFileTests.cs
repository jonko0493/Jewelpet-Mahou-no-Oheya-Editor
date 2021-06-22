using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
using System.IO;

namespace JewelpetEditorTests
{
    public class MessageFileTests
    {
        [Test]
        [TestCase(TestVariables.SYS_MESS_DECOMPRESSED)]
        [TestCase(TestVariables.COMM_TALK_DECOMPRESSED)]
        [TestCase(TestVariables.EVENT_500_599_DECOMPRESSED)]
        [TestCase(TestVariables.EVENT_600_699_DECOMPRESSED)]
        [TestCase(TestVariables.RESPONSE_DECOMPRESSED)]
        [TestCase(TestVariables.GARDEN_MESS_DECOMPRESSED)]
        [TestCase(TestVariables.ITEM_MESS_DECOMPRESSED)]
        [TestCase(TestVariables.RESPONSE_TITANA_DECOMPRESSED)]
        public void MessageParseWriteMatch(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            MessageFile messageFile = MessageFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = messageFile.GetBytes();
            File.WriteAllBytes("test.cmp.decompressed", dataInMemory);
            Assert.AreEqual(dataOnDisk, dataInMemory, $"File: {file}");
        }
    }
}