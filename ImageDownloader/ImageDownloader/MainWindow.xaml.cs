using ImageDownloader.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReactiveUI;
using System.Windows.Controls;

namespace ImageDownloader
{
    public partial class MainWindow : IViewFor<AppViewModel>
    {
        public AppViewModel ViewModel { get; private set; }

        AppViewModel IViewFor<AppViewModel>.ViewModel
        {
            get { return ViewModel; }
            set { }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { }
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
