﻿using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using TryClock.Logic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TryClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<String> Metrics { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.Metrics = new ObservableCollection<String>();
            Metrics.Add("hey");
            FillMetricsList();
            this.DataContext = this;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            
        }

        private async void FillMetricsList()
        {
            IEnumerable<Metric> list = await ClockClient.GetMetricsAsync("http://clockapi.azurewebsites.net/metrics");
            foreach(Metric m in list)
            {
                this.Metrics.Add("Heart rate is: " + m.HeartRate);
            }
            
        }


    /// <summary>
    /// Invoked when this page is about to be displayed in a Frame.
    /// </summary>
    /// <param name="e">Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.</param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
