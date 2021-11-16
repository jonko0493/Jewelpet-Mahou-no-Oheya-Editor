using FolderBrowserEx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private GtpcFile _gtpcFile;
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
                Filter = "MBB file|*.mbb.enus"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    editStackPanel.Children.Clear();
                    _messageFile = MessageFile.ParseFromDecompressedFile(openFileDialog.FileName);
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
                Filter = "Graphics files|*.gtx"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string fileType = Helpers.IdentifyFileType(openFileDialog.FileName);

                if (fileType.Contains("Graphics Tile File"))
                {
                    _gfntFile = GfntFile.ParseFromFile(openFileDialog.FileName);

                    graphicsStackPanel.Children.Clear();
                    graphicsEditPanel.Children.Clear();
                    graphicsTabControl.Items.Clear();
                    graphicsTabControl.Items.Add(new TabItem { Header = "GFNT" });
                    graphicsTabControl.SelectedIndex = 0;
                    graphicsListBox.ItemsSource = new List<object>();

                    var image = _gfntFile.GetImage();
                    graphicsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(image), MaxWidth = image.Width * 2, MaxHeight = image.Height * 2 });
                }
                else if (fileType.Contains("Graphics Map/Animation File"))
                {
                    _gtpcFile = new GtpcFile(openFileDialog.FileName);

                    graphicsTabControl.Items.Clear();
                    graphicsTabControl.Items.Add(new TabItem { Header = "GTSF" });
                    graphicsTabControl.Items.Add(new TabItem { Header = "GTSH" });
                    graphicsTabControl.SelectedIndex = 0;

                    graphicsListBox.ItemsSource = _gtpcFile.Gtsf.GfuvFiles;
                }
                else
                {
                    MessageBox.Show("Not a valid graphics file.");
                }
            }
        }

        private void SaveGraphicsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new();
            switch (((TabItem)graphicsTabControl.SelectedItem).Header)
            {
                case "GFNT":
                    saveFileDialog.Filter = "GFNT files|*.cmp;*.gfnt|Compressed GFNT file|*.cmp|Decompressed GFNT file|*.gfnt";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        _gfntFile.SaveToFile(saveFileDialog.FileName);
                        MessageBox.Show("Saved successfully!");
                    }
                    break;

                case "GTSF":
                case "GTSH":
                    saveFileDialog.Filter = "GTPC files|*.cmp;*.gtpc|Compressed GTPC file|*.cmp|Decompressed GTPC file|*.gtpc";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        _gtpcFile.Save(saveFileDialog.FileName);
                        MessageBox.Show("Saved successfully!");
                    }
                    break;

                default:
                    MessageBox.Show("Somehow you've gotten me into a weird state where I don't recognize what graphics file I have open!");
                    break;
            }
        }

        private void ChangeGtsfTileImages(GfuvFile gfuvFile)
        {
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
                MaxHeight = gfuvImage.Height * 2,
            });
        }

        private void ChangeGtsfDetails()
        {
            var gfuvFile = (GfuvFile)graphicsListBox.SelectedItem;

            ChangeGtsfTileImages(gfuvFile);

            for (int i = 0; i < gfuvFile.TilesCount; i++)
            {
                var xTextBox = new GfuvTileTextBox
                {
                    Gfuv = gfuvFile,
                    TileIndex = i,
                    TileImage = (Image)graphicsStackPanel.Children[i + 1], // plus 1 to get past the TextBlock
                    Property = GfuvTileTextBox.BoundingBoxProperty.X,
                    Text = gfuvFile.BoundingBoxes[i].X.ToString(),
                };
                var yTextBox = new GfuvTileTextBox
                {
                    Gfuv = gfuvFile,
                    TileIndex = i,
                    TileImage = (Image)graphicsStackPanel.Children[i + 1], // plus 1 to get past the TextBlock
                    Property = GfuvTileTextBox.BoundingBoxProperty.Y,
                    Text = gfuvFile.BoundingBoxes[i].Y.ToString(),
                };
                var widthTextBox = new GfuvTileTextBox
                {
                    Gfuv = gfuvFile,
                    TileIndex = i,
                    TileImage = (Image)graphicsStackPanel.Children[i + 1], // plus 1 to get past the TextBlock
                    Property = GfuvTileTextBox.BoundingBoxProperty.Width,
                    Text = gfuvFile.BoundingBoxes[i].Width.ToString(),
                };
                var heightTextBox = new GfuvTileTextBox
                {
                    Gfuv = gfuvFile,
                    TileIndex = i,
                    TileImage = (Image)graphicsStackPanel.Children[i + 1], // plus 1 to get past the TextBlock
                    Property = GfuvTileTextBox.BoundingBoxProperty.Height,
                    Text = gfuvFile.BoundingBoxes[i].Height.ToString(),
                };

                xTextBox.TextChanged += GfuvTextBox_TextChanged;
                yTextBox.TextChanged += GfuvTextBox_TextChanged;
                widthTextBox.TextChanged += GfuvTextBox_TextChanged;
                heightTextBox.TextChanged += GfuvTextBox_TextChanged;

                StackPanel tileEditStackPanel = new() { Orientation = Orientation.Horizontal };
                tileEditStackPanel.Children.Add(xTextBox);
                tileEditStackPanel.Children.Add(yTextBox);
                tileEditStackPanel.Children.Add(widthTextBox);
                tileEditStackPanel.Children.Add(heightTextBox);

                graphicsEditPanel.Children.Add(tileEditStackPanel);
            }
        }

        private void GfuvTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (GfuvTileTextBox)sender;
            if (short.TryParse(textBox.Text, out short value))
            {
                var rect = textBox.Gfuv.BoundingBoxes[textBox.TileIndex];
                switch (textBox.Property)
                {
                    case GfuvTileTextBox.BoundingBoxProperty.X:
                        rect.X = value;
                        break;
                    case GfuvTileTextBox.BoundingBoxProperty.Y:
                        rect.Y = value;
                        break;
                    case GfuvTileTextBox.BoundingBoxProperty.Width:
                        rect.Width = value;
                        break;
                    case GfuvTileTextBox.BoundingBoxProperty.Height:
                        rect.Height = value;
                        break;
                }
                textBox.Gfuv.BoundingBoxes[textBox.TileIndex] = rect;
                graphicsStackPanel.Children.Clear();
                ChangeGtsfTileImages(textBox.Gfuv);
            }
        }

        private void GraphicsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                try
                {
                    graphicsStackPanel.Children.Clear();
                    graphicsEditPanel.Children.Clear();

                    switch (((TabItem)graphicsTabControl.SelectedItem).Header)
                    {
                        case "GTSF":
                            ChangeGtsfDetails();
                            break;

                        case "GTSH":
                            var spriteDef = (GtshFile.SpriteDef)graphicsListBox.SelectedItem;

                            graphicsStackPanel.Children.Add(new TextBlock { Text = "Sprite" });
                            var spriteImage = spriteDef.GetImage(_gtpcFile.Gtsf.GfuvFiles);
                            graphicsStackPanel.Children.Add(new Image
                            {
                                Source = Helpers.GetBitmapImageFromBitmap(spriteImage),
                                MaxWidth = spriteImage.Width,
                                MaxHeight = spriteImage.Height
                            });
                            foreach (var tileInstance in spriteDef.TileInstances)
                            {
                                graphicsEditPanel.Children.Add(new Separator());
                                graphicsEditPanel.Children.Add(new TextBlock { Text = $"File Index: {tileInstance.GfuvIndex}" +
                                    $"({_gtpcFile.Gtsf.GfuvFiles.ElementAtOrDefault(tileInstance.GfuvIndex)?.FileName})," +
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
                graphicsEditPanel.Children.Clear();
                switch (((TabItem)graphicsTabControl.SelectedItem).Header)
                {
                    case "GFNT":
                        exportPaletteButton.IsEnabled = true;
                        exportTileBitmapButton.IsEnabled = true;
                        importTileBitmapButton.IsEnabled = true;
                        break;

                    case "GTSF":
                        exportPaletteButton.IsEnabled = true;
                        exportTileBitmapButton.IsEnabled = true;
                        importTileBitmapButton.IsEnabled = true;
                        graphicsListBox.ItemsSource = _gtpcFile.Gtsf.GfuvFiles;
                        break;

                    case "GTSH":
                        exportPaletteButton.IsEnabled = false;
                        exportTileBitmapButton.IsEnabled = false;
                        importTileBitmapButton.IsEnabled = false;
                        graphicsListBox.ItemsSource = _gtpcFile.Gtsf.GtshFile.SpriteDefs;
                        break;
                }
                graphicsListBox.Items.Refresh();
            }
        }

        private void ExportPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "RIFF palette|*.pal",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                GfntFile gfntFile = ((TabItem)graphicsTabControl.SelectedItem).Header switch
                {
                    "GFNT" => _gfntFile,
                    "GTSF" => ((GfuvFile)graphicsListBox.SelectedItem).AssociatedGfntFile,
                    _ => new GfntFile(),
                };
                gfntFile.SaveRiffPaletteToFile(saveFileDialog.FileName);
                MessageBox.Show("Palette exported!");
            }
        }

        private void ExportTileBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG file|*.png"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                GfntFile gfntFile = ((TabItem)graphicsTabControl.SelectedItem).Header switch
                {
                    "GFNT" => _gfntFile,
                    "GTSF" => ((GfuvFile)graphicsListBox.SelectedItem).AssociatedGfntFile,
                    _ => new GfntFile(),
                };
                gfntFile.GetImage().Save(saveFileDialog.FileName, ImageFormat.Png);
                MessageBox.Show("Tiles exported!");
            }
        }

        private void ImportTileBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG file|*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                GfntFile gfntFile = ((TabItem)graphicsTabControl.SelectedItem).Header switch
                {
                    "GFNT" => _gfntFile,
                    "GTSF" => ((GfuvFile)graphicsListBox.SelectedItem).AssociatedGfntFile,
                    _ => new GfntFile(),
                };
                gfntFile.SetImage(openFileDialog.FileName);

                if ((string)((TabItem)graphicsTabControl.SelectedItem).Header == "GTSF")
                {
                    ChangeGtsfDetails();
                }
                else
                {
                    var image = _gfntFile.GetImage();
                    graphicsStackPanel.Children.Clear();
                    graphicsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(image), MaxWidth = image.Width * 2, MaxHeight = image.Height * 2 });
                }
            }
        }

        private void BgColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graphicsStackPanel is not null)
            {
                switch (((ComboBoxItem)bgColorComboBox.SelectedItem).Content)
                {
                    case "White":
                        graphicsStackPanel.Background = Brushes.White;
                        break;

                    case "Black":
                        graphicsStackPanel.Background = Brushes.Black;
                        break;
                }
            }
        }
    }
}
