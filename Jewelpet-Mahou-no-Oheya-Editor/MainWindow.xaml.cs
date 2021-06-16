using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessageFile _messageFile;
        private string BaseWindowTitle = "Jewelpet Mahou no Oheya Editor";

        public MainWindow()
        {
            InitializeComponent();
            Title = BaseWindowTitle;
        }

        private void MessageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editStackPanel.Children.Clear();
            if (e.AddedItems.Count > 0)
            {
                MessageSection messageSection = (MessageSection)e.AddedItems[0];
                foreach (Message message in messageSection.Messages)
                {
                    editStackPanel.Children.Add(new TextBox { Text = message.Text, Height = 40 });
                }
            }
        }

        private void OpenDecompressedMessagesFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CMP file|*.cmp"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _messageFile = MessageFile.ParseFromCompressedFile(openFileDialog.FileName);
                messageFileTabControl.Items.Clear();
                foreach (MessageTable messageTable in _messageFile.MessageTables)
                {
                    TabItem messageTableTabItem = new TabItem
                    {
                        Header = $"Table 0x{messageTable.Offset}",
                        Width = messageFileTabControl.Width,
                    };
                    ListBox messageTableListBox = new ListBox
                    {
                        Margin = new Thickness(0, 0, 0, 0),
                        Height = messageFileTabControl.Height - 30
                    };

                    messageTableListBox.ItemsSource = messageTable.MessageSections;
                    messageTableListBox.SelectionChanged += MessageListBox_SelectionChanged;
                    messageTableTabItem.Content = messageTableListBox;

                    messageFileTabControl.Items.Add(messageTableTabItem);
                }

                Title = $"{BaseWindowTitle} - {_messageFile.FileName}";
            }
        }

        private void SaveMessagesFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExtractMessagesFileButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
