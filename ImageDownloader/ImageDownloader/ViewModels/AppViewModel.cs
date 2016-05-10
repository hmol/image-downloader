using ImageDownloader.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
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

        public Utils.Helpers.DownloadHelper DownloadHelper { get; set; }

        public AppViewModel()
        {
            DownloadHelper = new Utils.Helpers.DownloadHelper();

            ExecuteDownload = ReactiveCommand.CreateAsyncTask(parameter => DownloadHelper.DownloadImages(this.SourceUrl, this.DestinationPath));
            ExecuteDownload.ThrownExceptions.Subscribe(ex => {/* Handle errors here */});

            _DownloadResults = ExecuteDownload.ToProperty(this, x => x.DownloadResults, new List<DownloadResult>());

            _SpinnerVisibility = ExecuteDownload.IsExecuting
                .Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, x => x.SpinnerVisibility, Visibility.Hidden);

            _ButtonEnabled = ExecuteDownload.IsExecuting.Select(x => !x);
        }
    }
}
