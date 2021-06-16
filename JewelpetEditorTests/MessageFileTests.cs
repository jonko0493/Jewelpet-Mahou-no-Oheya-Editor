using Jewelpet_Mahou_no_Oheya_Editor;
using NUnit.Framework;
using System.IO;

namespace JewelpetEditorTests
{
    public class MessageFileTests
    {
        private const string SYS_MESS_DECOMPRESSED = ".\\inputs\\SysMess.cmp.decompressed";

        [Test]
        [TestCase(SYS_MESS_DECOMPRESSED)]
        public void MessageParseWriteMatch(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            MessageFile messageFile = MessageFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = messageFile.GetBytes();
            Assert.AreEqual(dataOnDisk, dataInMemory);
        }
    }
}