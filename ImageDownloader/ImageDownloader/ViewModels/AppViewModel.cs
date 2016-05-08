using HtmlAgilityPack;
using ImageDownloader.Models;
using ReactiveUI;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ImageDownloader.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        public string DestinationPath { get; set; }
        public string SourceUrl { get; set; }

        // We will describe this later, but ReactiveCommand is a Command
        // (like "Open", "Copy", "Delete", etc), that manages a task running
        // in the background.

        public ReactiveCommand<List<DownloadResult>> ExecuteDownload { get; protected set; }

        /* ObservableAsPropertyHelper
         * 
         * Here's the interesting part: In ReactiveUI, we can take IObservables
         * and "pipe" them to a Property - whenever the Observable yields a new
         * value, we will notify ReactiveObject that the property has changed.
         * 
         * To do this, we have a class called ObservableAsPropertyHelper - this
         * class subscribes to an Observable and stores a copy of the latest value.
         * It also runs an action whenever the property changes, usually calling
         * ReactiveObject's RaisePropertyChanged.
         */

        ObservableAsPropertyHelper<List<DownloadResult>> _DownloadResults;
        public List<DownloadResult> DownloadResults => _DownloadResults.Value;

        // Here, we want to create a property to represent when the application 
        // is performing a search (i.e. when to show the "spinner" control that 
        // lets the user know that the app is busy). We also declare this property
        // to be the result of an Observable (i.e. its value is derived from 
        // some other property)

        ObservableAsPropertyHelper<Visibility> _SpinnerVisibility;
        public Visibility SpinnerVisibility => _SpinnerVisibility.Value;
        public ReactiveCommand<object> DownloadCommand { get; private set; }
        public AppViewModel()
        {

            var canClickMeObservable = this.WhenAny(vm => vm.SourceUrl,
                                s => !string.IsNullOrWhiteSpace(s.Value));
            ExecuteDownload = ReactiveCommand.CreateAsyncTask(parameter => GetDownloadResults(this.SourceUrl, this.DestinationPath));

            DownloadCommand = new ReactiveCommand<object>(canClickMeObservable, null, null);
            DownloadCommand.Subscribe(param => MessageBox.Show("I was clicked"));

            /* Creating our UI declaratively
             * 
             * The Properties in this ViewModel are related to each other in different 
             * ways - with other frameworks, it is difficult to describe each relation
             * succinctly; the code to implement "The UI spinner spins while the search 
             * is live" usually ends up spread out over several event handlers.
             *
             * However, with RxUI, we can describe how properties are related in a very 
             * organized clear way. Let's describe the workflow of what the user does in
             * this application, in the order they do it.
             */

            // We're going to take a Property and turn it into an Observable here - this
            // Observable will yield a value every time the Search term changes (which in
            // the XAML, is connected to the TextBox). 
            //
            // We're going to use the Throttle operator to ignore changes that 
            // happen too quickly, since we don't want to issue a search for each 
            // key pressed! We then pull the Value of the change, then filter 
            // out changes that are identical, as well as strings that are empty.
            //
            // Finally, we use RxUI's InvokeCommand operator, which takes the String 
            // and calls the Execute method on the ExecuteSearch Command, after 
            // making sure the Command can be executed via calling CanExecute.
            /*
            this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(800), RxApp.MainThreadScheduler)
                .Select(x => x?.Trim())
                .DistinctUntilChanged()
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .InvokeCommand(ExecuteSearch);
                */
            // How would we describe when to show the spinner in English? We 
            // might say something like, "The spinner's visibility is whether
            // the search is running". RxUI lets us write these kinds of 
            // statements in code.
            //
            // ExecuteSearch has an IObservable<bool> called IsExecuting that
            // fires every time the command changes execution state. We Select() that into
            // a Visibility then we will use RxUI's
            // ToProperty operator, which is a helper to create an 
            // ObservableAsPropertyHelper object.

            _SpinnerVisibility = ExecuteDownload.IsExecuting
                .Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, x => x.SpinnerVisibility, Visibility.Hidden);

            // We subscribe to the "ThrownExceptions" property of our ReactiveCommand,
            // where ReactiveUI pipes any exceptions that are thrown in 
            // "GetSearchResultsFromFlickr" into. See the "Error Handling" section
            // for more information about this.
            ExecuteDownload.ThrownExceptions.Subscribe(ex => {/* Handle errors here */});

            // Here, we're going to actually describe what happens when the Command
            // gets invoked - we're going to run the GetSearchResultsFromFlickr every
            // time the Command is executed. 
            //
            // The important bit here is the return value - an Observable. We're going
            // to end up here with a Stream of FlickrPhoto Lists: every time someone 
            // calls Execute, we eventually end up with a new list which we then 
            // immediately put into the SearchResults property, that will then 
            // automatically fire INotifyPropertyChanged.
            _DownloadResults = ExecuteDownload.ToProperty(this, x => x.DownloadResults, new List<DownloadResult>());
        }

        public static async Task<List<DownloadResult>> GetDownloadResults(string sourceUrl, string destinationPath)
        {
            if (!IsValid(sourceUrl, destinationPath))
                return null;

            var client = new RestClient(sourceUrl);
            var request = new RestRequest();

            var response = await Task.Run(() => client.Execute(request));
            if (response == null)
                return null;
            var downloadList = new List<DownloadResult>();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response.Content);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
            var imgSrcList = nodes.Select(x => x.GetAttributeValue("src", string.Empty).TrimEnd('/')).ToList();

            foreach (var imgSrc in imgSrcList)
            {
                var name = Path.GetFileName(imgSrc);
                var rgx = new Regex("[^a-zA-Z0-9 - .]");
                var newName = rgx.Replace(name, "_");

                var localFilename = string.Format("{0}\\{1}", destinationPath, newName);
                var status = "Ok";
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(imgSrc, localFilename);
                    }


                }
                catch (Exception ex)
                {
                    status = ex.Message;
                }
                var downloadResult = new DownloadResult() { Status = status, Url = imgSrc };
                downloadList.Add(downloadResult);
            }
            return downloadList;
        }

        private static bool IsValid(string sourceUrl, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourceUrl) || string.IsNullOrEmpty(destinationPath))
                return false;
            return true;
        }
    }
}
