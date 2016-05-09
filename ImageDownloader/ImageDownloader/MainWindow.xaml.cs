using ImageDownloader.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReactiveUI;
using System.Windows.Controls;

namespace ImageDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IViewFor<AppViewModel>
    {
        public AppViewModel ViewModel { get; private set; }

        AppViewModel IViewFor<AppViewModel>.ViewModel
        {
            get
            {
                return ViewModel;
            }

            set
            {
                //throw new NotImplementedException();
            }
        }

        object IViewFor.ViewModel
        {
            get
            {
                return ViewModel;
            }

            set
            {
                //throw new NotImplementedException();
            }
        }

        public MainWindow()
        {
            ViewModel = new AppViewModel();

            InitializeComponent();
            WindowGrid.DataContext = ViewModel;
            this.Bind(ViewModel, x => x.DestinationPath, x => x.TxtDestination.Text);
        }

        private void DestinationTextBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                TxtDestination.Text = dialog.FileName;
            }
        }
    }
}
////Get full path to selected folder, and display it in the destination textbox


//private bool ValidateUrl()
//{
//    try
//    {
//        var url = new Uri(TxtSourceUrl.Text);
//        return true;
//    }
//    catch (UriFormatException)
//    {
//        ErrorToaster.Toast("Url malformed", ToasterPosition.ApplicationTopRight);
//    }
//    return false;
//}

//private void Download_BtnClick(object sender, System.Windows.RoutedEventArgs e)
//{
//    var isValid = ValidateUrl();
//    if (isValid)
//    {
//        var client = new RestClient(TxtSourceUrl.Text);
//        var request = new RestRequest();
//        var result = client.Execute(request);

//        var htmlDoc = new HtmlDocument();
//        htmlDoc.LoadHtml(result.Content);
//        var nodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
//        var imgSrcList = nodes.Select(x => x.GetAttributeValue("src", string.Empty).TrimEnd('/')).ToList();

//        foreach (var imgSrc in imgSrcList)
//        {
//            var name = Path.GetFileName(imgSrc);
//            var rgx = new Regex("[^a-zA-Z0-9 - .]");
//            var newName = rgx.Replace(name, "_");

//            var localFilename = string.Format("{0}\\{1}", TxtDestination.Text, newName);

//            using (var webClient = new WebClient())
//            {
//                webClient.DownloadFile(imgSrc, localFilename);
//            }

//            var downloadResult = new DownloadResult() { Status = "Ok", Url = imgSrc };
//            MyList.Add(downloadResult);
//        }

//    }
// }

