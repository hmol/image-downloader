using HtmlAgilityPack;
using ImageDownloader.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using netoaster;
using RestSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Controls;

namespace ImageDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<DownloadResult> MyList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MyList = new ObservableCollection<DownloadResult>();
            InfoDataGrid.ItemsSource = MyList;
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

        private bool ValidateUrl()
        {
            try
            {
                var url = new Uri(TxtSourceUrl.Text);
                return true;
            }
            catch (UriFormatException)
            {
                ErrorToaster.Toast("Url malformed", ToasterPosition.ApplicationTopRight);
            }
            return false;
        }

        private void Download_BtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var isValid = ValidateUrl();
            if (isValid)
            {
                var client = new RestClient(TxtSourceUrl.Text);
                var request = new RestRequest();
                var result = client.Execute(request);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(result.Content);
                var nodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
                var imgSrcList = nodes.Select(x => x.GetAttributeValue("src", string.Empty).TrimEnd('/')).ToList();

                foreach (var imgSrc in imgSrcList)
                {
                    var name = Path.GetFileName(imgSrc);
                    var localFilename = string.Format("{0}\\{1}", TxtDestination.Text, name);

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(imgSrc, localFilename);
                    }

                    var downloadResult = new DownloadResult() { Status = "Ok", Url = imgSrc };
                    MyList.Add(downloadResult);
                }

            }
        }
    }
}
