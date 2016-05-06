using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows.Controls;
using netoaster;

namespace ImageDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Get full path to selected folder, and display it in the destination textbox
        private void DestinationTextBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                textBox.Text = dialog.FileName;
            }
        }

        private void ValidateUrl()
        {
            try
            {
                var url = new Uri(TxtSourceUrl.Text);
            }
            catch (UriFormatException)
            {
                ErrorToaster.Toast("Url malformed", ToasterPosition.ApplicationTopRight);
            }
        }

        private void Download_BtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ValidateUrl();
        }
    }
}
