using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    public class MessageTextBox : TextBox
    {
        public Message Message { get; set; }
        public ListBox MessageListBox { get; set; }
    }
}
