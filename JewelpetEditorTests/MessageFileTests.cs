using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
using System.IO;

namespace JewelpetEditorTests
{
    public class MessageFileTests
    {
        private const string SYS_MESS_DECOMPRESSED = ".\\inputs\\SysMess.cmp.decompressed";
        private const string COMM_TALK_DECOMPRESSED = ".\\inputs\\CommTalk.cmp.decompressed";
        private const string RESPONSE_DECOMPRESSED = ".\\inputs\\Response.cmp.decompressed";
        private const string GARDEN_MESS_DECOMPRESSED = ".\\inputs\\GardenMess.cmp.decompressed";
        private const string ITEM_MESS_DECOMPRESSED = ".\\inputs\\ItemMes.cmp.decompressed";

        [Test]
        [TestCase(SYS_MESS_DECOMPRESSED)]
        [TestCase(COMM_TALK_DECOMPRESSED)]
        [TestCase(RESPONSE_DECOMPRESSED)]
        [TestCase(GARDEN_MESS_DECOMPRESSED)]
        [TestCase(ITEM_MESS_DECOMPRESSED)]
        public void MessageParseWriteMatch(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            MessageFile messageFile = MessageFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = messageFile.GetBytes();
            File.WriteAllBytes("test.cmp.decompressed", dataInMemory);
            Assert.AreEqual(dataOnDisk, dataInMemory);
        }
    }
}