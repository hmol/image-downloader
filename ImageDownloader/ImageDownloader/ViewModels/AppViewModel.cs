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

        //A Command managing a task running in the background.
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

        IObservable<bool> _ButtonEnabled;
        public IObservable<bool> ButtonEnabled => _ButtonEnabled;

        public AppViewModel()
        {
            ExecuteDownload = ReactiveCommand.CreateAsyncTask(parameter => GetDownloadResults(this.SourceUrl, this.DestinationPath));
            ExecuteDownload.ThrownExceptions.Subscribe(ex => {/* Handle errors here */});

            _DownloadResults = ExecuteDownload.ToProperty(this, x => x.DownloadResults, new List<DownloadResult>());

            _SpinnerVisibility = ExecuteDownload.IsExecuting
                .Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, x => x.SpinnerVisibility, Visibility.Hidden);

            _ButtonEnabled = ExecuteDownload.IsExecuting.Select(x => !x);
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
