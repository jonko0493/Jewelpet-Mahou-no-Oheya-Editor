using FolderBrowserEx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
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
using Path = System.IO.Path;

namespace Jewelpet_Mahou_no_Oheya_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessageFile _messageFile;
        private GfntFile _gfntFile;
        private GtsfFile _gtsfFile;
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
                    var messageTextBox = new MessageTextBox { Text = message.Text, Height = 60, Message = message, MessageListBox = (ListBox)sender, AcceptsReturn = true };
                    messageTextBox.TextChanged += MessageTextBox_TextChanged;
                    editStackPanel.Children.Add(messageTextBox);
                }
            }
        }

        private void OpenMessagesFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CMP file|*.cmp"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    editStackPanel.Children.Clear();
                    _messageFile = MessageFile.ParseFromCompressedFile(openFileDialog.FileName).GetAwaiter().GetResult();
                    messageFileTabControl.Items.Clear();
                    foreach (MessageTable messageTable in _messageFile.MessageTables)
                    {
                        TabItem messageTableTabItem = new TabItem
                        {
                            Header = $"Table 0x{messageTable.Offset:X4}",
                            Width = messageFileTabControl.Width,
                        };
                        ListBox messageTableListBox = new ListBox
                        {
                            Margin = new Thickness(0, 0, 0, 0),
                            Height = messageFileTabControl.Height - 30,
                        };

                        messageTableListBox.ItemsSource = messageTable.MessageSections;
                        messageTableListBox.SelectionChanged += MessageListBox_SelectionChanged;
                        messageTableTabItem.Content = messageTableListBox;

                        messageFileTabControl.Items.Add(messageTableTabItem);
                    }

                    messageFileTabControl.SelectedIndex = 0;
                    Title = $"{BaseWindowTitle} - {_messageFile.FileName}";
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Failed to load message file - {exc.Message}");
                }
            }
        }

        private void SaveMessagesFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CMP file|*.cmp"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _messageFile.SaveToCompressedFile(saveFileDialog.FileName);
            }
        }

        private void ExtractMessagesFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file|*.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _messageFile.SaveToCompressedFile(saveFileDialog.FileName);
            }
        }

        private void ExtractMessagesDirectoryFileButton_Click(object sender, RoutedEventArgs e)
        {
            ExtractMessagesDirectoryFileButtonAsync().GetAwaiter().GetResult();
        }

        private async Task ExtractMessagesDirectoryFileButtonAsync()
        {
            FolderBrowserDialog openFolderBrowserDialog = new FolderBrowserDialog
            {
                Title = "Select folder to extract files from",
            };
            if (openFolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var files = Directory.GetFiles(openFolderBrowserDialog.SelectedFolder, "*.cmp", SearchOption.AllDirectories);
                await Task.WhenAll(files.Select(f => MessageFile.ExtractStringsFromFileToFile(f,
                    Path.Combine(Path.GetDirectoryName(f), $"{Path.GetFileNameWithoutExtension(f)}.txt"))));
                MessageBox.Show("Extraction complete!");
            }
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var messageTextBox = (MessageTextBox)sender;
            messageTextBox.Message.Text = messageTextBox.Text.Replace("\r", "");
            messageTextBox.MessageListBox.Items.Refresh();
        }

        private void LoadGraphicsFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Graphics files|*.cmp;*.gfnt;*.gtpc|Compressed graphics file|*.cmp|Decompressed GFNT file|*.gfnt|Decompressed GTPC file|*.gtpc"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string fileType = Helpers.IdentifyFileType(openFileDialog.FileName);

                if (fileType.Contains("Graphics Tile File"))
                {
                    if (fileType.Contains("Decompressed"))
                    {
                        _gfntFile = GfntFile.ParseFromDecompressedFile(openFileDialog.FileName);
                    }
                    else
                    {
                        _gfntFile = GfntFile.ParseFromCompressedFile(openFileDialog.FileName);
                    }

                    graphicsStackPanel.Children.Clear();
                    graphicsTabControl.Items.Clear();
                    graphicsTabControl.Items.Add(new TabItem { Header = "GFNT" });
                    graphicsTabControl.SelectedIndex = 0;
                    graphicsListBox.ItemsSource = new List<object>();

                    var image = _gfntFile.GetImage();
                    graphicsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(image), MaxWidth = image.Width * 2, MaxHeight = image.Height * 2 });
                }
                else if (fileType.Contains("Graphics Map/Animation File"))
                {
                    _gtsfFile = GtsfFile.ParseFromFile(openFileDialog.FileName);

                    graphicsTabControl.Items.Clear();
                    graphicsTabControl.Items.Add(new TabItem { Header = "GTSF" });
                    graphicsTabControl.Items.Add(new TabItem { Header = "GTSH" });
                    graphicsTabControl.SelectedIndex = 0;

                    graphicsListBox.ItemsSource = _gtsfFile.GfuvFiles;
                }
                else
                {
                    MessageBox.Show("Not a valid graphics file.");
                }
            }
        }

        private void GraphicsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                try
                {
                    switch (((TabItem)graphicsTabControl.SelectedItem).Header)
                    {
                        case "GTSF":
                            var gfuvFile = (GfuvFile)graphicsListBox.SelectedItem;

                            graphicsStackPanel.Children.Clear();
                            graphicsStackPanel.Children.Add(new TextBlock { Text = "Tiles" });
                            var tiles = gfuvFile.GetTileImages();
                            foreach (var tile in tiles)
                            {
                                graphicsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(tile), MaxWidth = tile.Width, MaxHeight = tile.Height });
                            }

                            graphicsStackPanel.Children.Add(new TextBlock { Text = "GFNT Tile File" });
                            var gfuvImage = gfuvFile.AssociatedGfntFile?.GetImage() ?? new System.Drawing.Bitmap(256, 394);
                            graphicsStackPanel.Children.Add(new Image
                            {
                                Source = Helpers.GetBitmapImageFromBitmap(gfuvImage),
                                MaxWidth = gfuvImage.Width * 2,
                                MaxHeight = gfuvImage.Height * 2
                            });
                            break;

                        case "GTSH":
                            var spriteDef = (GtshFile.SpriteDef)graphicsListBox.SelectedItem;

                            graphicsStackPanel.Children.Clear();
                            graphicsStackPanel.Children.Add(new TextBlock { Text = "Sprite" });
                            var spriteImage = spriteDef.GetImage(_gtsfFile.GfuvFiles);
                            graphicsStackPanel.Children.Add(new Image
                            {
                                Source = Helpers.GetBitmapImageFromBitmap(spriteImage),
                                MaxWidth = spriteImage.Width,
                                MaxHeight = spriteImage.Height
                            });
                            foreach (var tileInstance in spriteDef.TileInstances)
                            {
                                graphicsStackPanel.Children.Add(new Separator());
                                graphicsStackPanel.Children.Add(new TextBlock { Text = $"File Index: {tileInstance.GfuvIndex}" +
                                    $"({_gtsfFile.GfuvFiles.ElementAtOrDefault(tileInstance.GfuvIndex)?.FileName})," +
                                    $"Tile Index: {tileInstance.TileIndex}, Rotation: {tileInstance.Rotation:X2}, Unknown Byte: {tileInstance.UnknownByte:X2}" });
                            }
                            break;
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Failed to parse image - {exc.Message}");
                }
            }
        }

        private void GraphicsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                graphicsListBox.ItemsSource = null;
                graphicsListBox.Items.Refresh();
                graphicsStackPanel.Children.Clear();
                switch (((TabItem)graphicsTabControl.SelectedItem).Header)
                {
                    case "GFNT":
                        exportTileBitmapButton.IsEnabled = true;
                        break;

                    case "GTSF":
                        exportTileBitmapButton.IsEnabled = true;
                        graphicsListBox.ItemsSource = _gtsfFile.GfuvFiles;
                        break;

                    case "GTSH":
                        exportTileBitmapButton.IsEnabled = false;
                        graphicsListBox.ItemsSource = _gtsfFile.GtshFile.SpriteDefs;
                        break;
                }
                graphicsListBox.Items.Refresh();
            }
        }

        private void ExportTileBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "PNG file|*.png"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                GfntFile gfntFile;
                switch (((TabItem)graphicsTabControl.SelectedItem).Header)
                {
                    case "GFNT":
                        gfntFile = _gfntFile;
                        break;

                    case "GTSF":
                        gfntFile = ((GfuvFile)graphicsListBox.SelectedItem).AssociatedGfntFile;
                        break;

                    default:
                        gfntFile = new GfntFile();
                        break;
                }
                gfntFile.GetImage().Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }
    }
}
