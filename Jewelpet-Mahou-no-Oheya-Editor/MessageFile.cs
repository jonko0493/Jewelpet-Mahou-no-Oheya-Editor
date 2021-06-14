using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    public class MessageFile
    {
        public string FileName { get; set; }
        public List<MessageSection> MessageSections { get; internal set; } = new List<MessageSection>();
        public int UnknownInt { get; set; }
        public int[] UnknownHeaderSecondLine { get; set; }
        public int MtblLength { get; set; }
        public int UnknownMtblInt { get; set; }
        public static int HEADER_LENGTH = 0x30;

        public static MessageFile ParseFromFile(string fileName)
        {
            var messageFile = ParseFromData(File.ReadAllBytes(fileName));
            messageFile.FileName = Path.GetFileName(fileName);
            return messageFile;
        }

        public static MessageFile ParseFromData(byte[] data)
        {
            var messageFile = new MessageFile
            {
                UnknownInt = BitConverter.ToInt32(data.Skip(8).Take(4).ToArray()),
                UnknownHeaderSecondLine = new int[4],
            };
            for (int i = 0; i < messageFile.UnknownHeaderSecondLine.Length; i++)
            {
                messageFile.UnknownHeaderSecondLine[i] = BitConverter.ToInt32(data.Skip(i * 4 + 0x10).Take(4).ToArray());
            }
            messageFile.MtblLength = BitConverter.ToInt32(data.Skip(0x28).Take(4).ToArray());
            messageFile.UnknownMtblInt = BitConverter.ToInt32(data.Skip(0x2C).Take(4).ToArray());

            int firstPointer = BitConverter.ToInt32(data.Skip(HEADER_LENGTH + 0x04).Take(4).ToArray());
            for (int i = HEADER_LENGTH; i < firstPointer + HEADER_LENGTH; i += MessageSection.LENGTH) // message sections
            {
                var messageSection = new MessageSection();
                for (int j = 0x04; j < 0x190; j += 0x04) // message section pointers
                {
                    int pointer = BitConverter.ToInt32(data.Skip(i + j).Take(4).ToArray());
                    if (pointer == 0x0000)
                    {
                        break;
                    }
                    messageSection.Pointers.Add(pointer);
                    messageSection.Messages.Add(new Message(data.TakeLast(data.Length - (pointer + HEADER_LENGTH)).ToArray()));
                }

                messageFile.MessageSections.Add(messageSection);
            }

            return messageFile;
        }

        public void SaveToFile(string file)
        {
            File.WriteAllBytes(file, GetBytes());
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();

            // File Header
            bytes.AddRange(Encoding.ASCII.GetBytes("TBB1"));
            bytes.AddRange(BitConverter.GetBytes(0x00000010));
            bytes.AddRange(BitConverter.GetBytes(UnknownInt));
            bytes.AddRange(BitConverter.GetBytes(0xFFFFFFFF)); // Length placeholder, will be replaced later
            foreach (int unknown in UnknownHeaderSecondLine)
            {
                bytes.AddRange(BitConverter.GetBytes(unknown));
            }
            // Message Table Header
            bytes.AddRange(Encoding.ASCII.GetBytes("MTBL"));
            bytes.AddRange(BitConverter.GetBytes(0x00000010));
            bytes.AddRange(BitConverter.GetBytes(MtblLength)); // Will be replaced later
            bytes.AddRange(BitConverter.GetBytes(UnknownMtblInt));

            

            return bytes.ToArray();
        }

        public static Dictionary<ushort, string> ShortToCharMap = new Dictionary<ushort, string>
        {
            { 0x0000, "[end]" },
            { 0x0001, " " },
            { 0x0002, "!" },
            { 0x0003, "♪" },
            { 0x0004, "?" },
            { 0x0005, "0" },
            { 0x0006, "1" },
            { 0x0007, "2" },
            { 0x0008, "3" },
            { 0x0009, "4" },
            { 0x000A, "5" },
            { 0x000B, "6" },
            { 0x000C, "7" },
            { 0x000D, "8" },
            { 0x000E, "9" },
            { 0x000F, "A" },
            { 0x0010, "B" },
            { 0x0011, "C" },
            { 0x0012, "D" },
            { 0x0013, "E" },
            { 0x0014, "F" },
            { 0x0015, "G" },
            { 0x0016, "H" },
            { 0x0017, "I" },
            { 0x0018, "J" },
            { 0x0019, "K" },
            { 0x001A, "L" },
            { 0x001B, "M" },
            { 0x001C, "N" },
            { 0x001D, "O" },
            { 0x001E, "P" },
            { 0x001F, "Q" },
            { 0x0020, "R" },
            { 0x0021, "S" },
            { 0x0022, "T" },
            { 0x0023, "U" },
            { 0x0024, "V" },
            { 0x0025, "W" },
            { 0x0026, "X" },
            { 0x0027, "Y" },
            { 0x0028, "Z" },
            { 0x0029, "a" },
            { 0x002A, "b" },
            { 0x002B, "c" },
            { 0x002C, "d" },
            { 0x002D, "e" },
            { 0x002E, "f" },
            { 0x002F, "g" },
            { 0x0030, "h" },
            { 0x0031, "i" },
            { 0x0032, "j" },
            { 0x0033, "k" },
            { 0x0034, "l" },
            { 0x0035, "m" },
            { 0x0036, "n" },
            { 0x0037, "o" },
            { 0x0038, "p" },
            { 0x0039, "q" },
            { 0x003A, "r" },
            { 0x003B, "s" },
            { 0x003C, "t" },
            { 0x003D, "u" },
            { 0x003E, "v" },
            { 0x003F, "w" },
            { 0x0040, "x" },
            { 0x0041, "y" },
            { 0x0042, "z" },
            { 0x0043, "ぁ" },
            { 0x0044, "あ" },
            { 0x0045, "ぃ" },
            { 0x0046, "い" },
            { 0x0047, "ぅ" },
            { 0x0048, "う" },
            { 0x0049, "ぇ" },
            { 0x004A, "え" },
            { 0x004B, "ぉ" },
            { 0x004C, "お" },
            { 0x004D, "か" },
            { 0x004E, "が" },
            { 0x004F, "き" },
            { 0x0050, "ぎ" },
            { 0x0051, "く" },
            { 0x0052, "ぐ" },
            { 0x0053, "け" },
            { 0x0054, "げ" },
            { 0x0055, "こ" },
            { 0x0056, "ご" },
            { 0x0057, "さ" },
            { 0x0058, "ざ" },
            { 0x0059, "し" },
            { 0x005A, "じ" },
            { 0x005B, "す" },
            { 0x005C, "ず" },
            { 0x005D, "せ" },
            { 0x005E, "ぜ" },
            { 0x005F, "そ" },
            { 0x0060, "ぞ" },
            { 0x0061, "た" },
            { 0x0062, "だ" },
            { 0x0063, "ち" },
            { 0x0064, "ぢ" },
            { 0x0065, "っ" },
            { 0x0066, "つ" },
            { 0x0067, "づ" },
            { 0x0068, "て" },
            { 0x0069, "で" },
            { 0x006A, "と" },
            { 0x006B, "ど" },
            { 0x006C, "な" },
            { 0x006D, "に" },
            { 0x006E, "ぬ" },
            { 0x006F, "ね" },
            { 0x0070, "の" },
            { 0x0071, "は" },
            { 0x0072, "ば" },
            { 0x0073, "ぱ" },
            { 0x0074, "ひ" },
            { 0x0075, "び" },
            { 0x0076, "ぴ" },
            { 0x0077, "ふ" },
            { 0x0078, "ぶ" },
            { 0x0079, "ぷ" },
            { 0x007A, "へ" },
            { 0x007B, "べ" },
            { 0x007C, "ぺ" },
            { 0x007D, "ほ" },
            { 0x007E, "ぼ" },
            { 0x007F, "ぽ" },
            { 0x0080, "ま" },
            { 0x0081, "み" },
            { 0x0082, "む" },
            { 0x0083, "め" },
            { 0x0084, "も" },
            { 0x0085, "ゃ" },
            { 0x0086, "や" },
            { 0x0087, "ゅ" },
            { 0x0088, "ゆ" },
            { 0x0089, "ょ" },
            { 0x008A, "よ" },
            { 0x008B, "ら" },
            { 0x008C, "り" },
            { 0x008D, "る" },
            { 0x008E, "れ" },
            { 0x008F, "ろ" },
            { 0x0090, "ゎ" },
            { 0x0091, "わ" },
            { 0x0092, "ゐ" },
            { 0x0093, "ゑ" },
            { 0x0094, "を" },
            { 0x0095, "ん" },
            { 0x0096, "ァ" },
            { 0x0097, "ア" },
            { 0x0098, "ィ" },
            { 0x0099, "イ" },
            { 0x009A, "ゥ" },
            { 0x009B, "ウ" },
            { 0x009C, "ヴ" },
            { 0x009D, "ェ" },
            { 0x009E, "エ" },
            { 0x009F, "ォ" },
            { 0x00A0, "オ" },
            { 0x00A1, "カ" },
            { 0x00A2, "ガ" },
            { 0x00A3, "キ" },
            { 0x00A4, "ギ" },
            { 0x00A5, "ク" },
            { 0x00A6, "グ" },
            { 0x00A7, "ケ" },
            { 0x00A8, "ゲ" },
            { 0x00A9, "コ" },
            { 0x00AA, "ゴ" },
            { 0x00AB, "サ" },
            { 0x00AC, "ザ" },
            { 0x00AD, "シ" },
            { 0x00AE, "ジ" },
            { 0x00AF, "ス" },
            { 0x00B0, "ズ" },
            { 0x00B1, "セ" },
            { 0x00B2, "ゼ" },
            { 0x00B3, "ソ" },
            { 0x00B4, "ゾ" },
            { 0x00B5, "タ" },
            { 0x00B6, "ダ" },
            { 0x00B7, "チ" },
            { 0x00B8, "ヂ" },
            { 0x00B9, "ッ" },
            { 0x00BA, "ツ" },
            { 0x00BB, "ヅ" },
            { 0x00BC, "テ" },
            { 0x00BD, "デ" },
            { 0x00BE, "ト" },
            { 0x00BF, "ド" },
            { 0x00C0, "ナ" },
            { 0x00C1, "ニ" },
            { 0x00C2, "ヌ" },
            { 0x00C3, "ネ" },
            { 0x00C4, "ノ" },
            { 0x00C5, "ハ" },
            { 0x00C6, "バ" },
            { 0x00C7, "パ" },
            { 0x00C8, "ヒ" },
            { 0x00C9, "ビ" },
            { 0x00CA, "ピ" },
            { 0x00CB, "フ" },
            { 0x00CC, "ブ" },
            { 0x00CD, "プ" },
            { 0x00CE, "ヘ" },
            { 0x00CF, "ベ" },
            { 0x00D0, "ペ" },
            { 0x00D1, "ホ" },
            { 0x00D2, "ボ" },
            { 0x00D3, "ポ" },
            { 0x00D4, "マ" },
            { 0x00D5, "ミ" },
            { 0x00D6, "ム" },
            { 0x00D7, "メ" },
            { 0x00D8, "モ" },
            { 0x00D9, "ャ" },
            { 0x00DA, "ヤ" },
            { 0x00DB, "ュ" },
            { 0x00DC, "ユ" },
            { 0x00DD, "ョ" },
            { 0x00DE, "ヨ" },
            { 0x00DF, "ラ" },
            { 0x00E0, "リ" },
            { 0x00E1, "ル" },
            { 0x00E2, "レ" },
            { 0x00E3, "ロ" },
            { 0x00E4, "ヮ" },
            { 0x00E5, "ワ" },
            { 0x00E6, "ヰ" },
            { 0x00E7, "ヱ" },
            { 0x00E8, "ヲ" },
            { 0x00E9, "ン" },
            { 0x00EA, "ヵ" },
            { 0x00EB, "ヶ" },
            { 0x00EC, "…" },
            { 0x00ED, "、" },
            { 0x00EE, "。" },
            { 0x00EF, "ー" },
            { 0x00F0, "/" },
            { 0x00F1, "\\" },
            { 0x00F2, "~" },
            { 0x00F3, "|" },
            { 0x00F4, "「" },
            { 0x00F5, "」" },
            { 0x00F6, "±" },
            { 0x00F7, "×" },
            { 0x00F8, "÷" },
            { 0x00F9, "∞" },
            { 0x00FA, "∴" },
            { 0x00FB, "\"" },
            { 0x00FC, "☆" },
            { 0x00FD, "★" },
            { 0x00FE, "○" },
            { 0x00FF, "●" },
            { 0x0100, "⦾" },
            { 0x0101, "◇" },
            { 0x0102, "◆" },
            { 0x0103, "□" },
            { 0x0104, "■" },
            { 0x0105, "△" },
            { 0x0106, "▲" },
            { 0x0107, "▽" },
            { 0x0108, "▼" },
            { 0x0109, "※" },
            { 0x010A, "[t_with_line]" },
            { 0x010B, "[right_arrow]" },
            { 0x010C, "[left_arrow]" },
            { 0x010D, "[up_arrow]" },
            { 0x010E, "[down_arrow]" },
            { 0x010F, "♭" },
            { 0x0110, "(" },
            { 0x0111, ")" },
            { 0x0112, "『" },
            { 0x0113, "』" },
            { 0x0114, "<" },
            { 0x0115, ">" },
            { 0x0116, "^" },
            { 0x0117, "≥" },
            { 0x0118, "≤" },
            { 0x0119, "+" },
            { 0x011A, "-" },
            { 0x011B, "=" },
            { 0x011C, "#" },
            { 0x011D, "·" },
            { 0x011E, "*" },
            { 0x011F, "%" },
            { 0x0120, "&" },
            { 0x0121, "," },
            { 0x0122, "." },
            { 0x0123, "読" },
            { 0x0124, "込" },
            { 0x0125, "失" },
            { 0x0126, "敗" },
            { 0x0127, "電" },
            { 0x0128, "源" },
            { 0x0129, "切" },
            { 0x012A, "差" },
            { 0x012B, "直" },
            { 0x012C, "書" },
            { 0x012D, "破" },
            { 0x012E, "損" },
            { 0x012F, "初" },
            { 0x0130, "期" },
            { 0x0131, "化" },
            { 0x0170, "[unknown0170]" },
            { 0x01D0, "[unknown01D0]" },
            { 0x01E0, "[unknown01E0]" },
            { 0x0200, "[unknown0200]" },
            { 0x03B0, "[unknown03B0]" },
            { 0x09B0, "[unknown09B0]" },
            { 0x1000, "[unknown1000]" },
            { 0x1360, "[unknown1360]" },
            { 0x13A0, "[unknown13A0]" },
            { 0x2000, "[unknown2000]" },
            { 0x3000, "[unknown3000]" },
            { 0x3142, "[unknown3142]" },
            { 0x4000, "[unknown4000]" },
            { 0x4254, "[unknown4254]" },
            { 0x4C42, "[unknown4C42]" },
            { 0x5000, "[unknown5000]" },
            { 0x544D, "[unknown544D]" },
            { 0x6000, "[unknown6000]" },
            { 0x7000, "[unknown7000]" },
            { 0x8000, "[unknown8000]" },
            { 0x8001, "\n" },
            { 0x8004, "[unknown8004]" },
            { 0x8064, "[player_name]" },
            { 0x8065, "[unknown8065]" },
            { 0x8066, "[ルビ]" },
            { 0x8068, "[24]" },
            { 0x8069, "[ラブラ]" },
            { 0x9000, "[unknown9000]" },
            { 0xA000, "[unknownA000]" },
            { 0xB000, "[unknownB000]" },
            { 0xC000, "[unknownC000]" },
            { 0xD000, "[unknownD000]" },
            { 0xE000, "[unknownE000]" },
            { 0xF000, "[unknownF000]" },
        };

        public static Dictionary<ushort, string> X8067ShortToCharMap = new Dictionary<ushort, string>
        {
            { 0x0000, "[なにもつけない]" },
            { 0x000D, "[なにもつけない02]" },
            { 0x00EF, "アクセ３９" },
        };
    }

    public class MessageSection
    {
        public static int LENGTH = 0x190;
        public List<Message> Messages { get; set; } = new List<Message>();
        public List<int> Pointers { get; set; } = new List<int>();

        public override string ToString()
        {
            if (Messages.Count > 0)
            {
                return Messages[0].ToString();
            }
            else
            {
                return "";
            }
        }

    }

    public class Message
    {
        public string Text { get; set; }

        public Message(byte[] data)
        {
            Text = "";
            for (int i = 0; i < data.Length - 1; i += 2)
            {
                ushort nextShort = BitConverter.ToUInt16(data.Skip(i).Take(2).ToArray());
                if (nextShort == 0x8067)
                {
                    ushort next8067Operator = BitConverter.ToUInt16(data.Skip(i + 2).Take(2).ToArray());
                    if (MessageFile.X8067ShortToCharMap.ContainsKey(next8067Operator))
                    {
                        Text += MessageFile.X8067ShortToCharMap[next8067Operator];
                        i += 2;
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown 8067 operator 0x{next8067Operator:X4}");
                    }
                }
                else if (nextShort == 0x0000)
                {
                    break;
                }
                else if (MessageFile.ShortToCharMap.ContainsKey(nextShort))
                {
                    Text += MessageFile.ShortToCharMap[nextShort];
                }
                else
                {
                    throw new ArgumentException($"Unknown opcode 0x{nextShort:X4} encountered");
                    //Text += $"[unknown{nextShort:X4}]";
                }
            }
        }

        public byte[] GetBytes()
        {
            var bytes = new List<byte>();

            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '[')
                {
                    string op = string.Concat(Text.Substring(i).TakeWhile(c => c != ']'));
                    if (MessageFile.ShortToCharMap.ContainsValue($"[{op}"))
                    {
                        bytes.AddRange(BitConverter.GetBytes(MessageFile.ShortToCharMap.First(c => c.Value == $"[{op}").Key));
                        i += op.Length;
                    }
                    else if (MessageFile.X8067ShortToCharMap.ContainsValue($"[{op}"))
                    {
                        bytes.AddRange(BitConverter.GetBytes((ushort)0x8067));
                        bytes.AddRange(BitConverter.GetBytes(MessageFile.X8067ShortToCharMap.First(c => c.Value == $"[{op}").Key));
                        i += op.Length;
                    }
                    else
                    {
                        throw new ArgumentException($"Unrecognized op '[{op}'");
                    }
                }
                else
                {
                    bytes.AddRange(BitConverter.GetBytes(MessageFile.ShortToCharMap.First(c => c.Value == Text[i].ToString()).Key));
                }
            }

            return bytes.ToArray();
        }

        public override string ToString()
        {
            return Text.Replace("\n", "\\n");
        }
    }
}
