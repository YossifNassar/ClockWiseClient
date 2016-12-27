using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using TryClock.Logic;
using System.Diagnostics;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Microsoft.Toolkit.Uwp.Notifications;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TryClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<String> Metrics { get; private set; }
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            LoadSettings();
            this.Metrics = new ObservableCollection<String>();
            // Subscribe to the NetworkAvailabilityChanged event
            NetworkInformation.NetworkStatusChanged += new NetworkStatusChangedEventHandler(Connection_NetworkStatusChanged);
            FillMetricsList();
            this.DataContext = this;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            SetAlarm();
        }

        private void LoadSettings()
        {
            if (!localSettings.Values.ContainsKey("alarm_time"))
            {
                localSettings.Values["alarm_time"] = "07:00";
            }
            textBlock.Text = localSettings.Values["alarm_time"].ToString();
        }


        private void SetAlarm()
        {
            ToastContent content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "Clock Wise Alarm"
                                },

                                new AdaptiveText()
                                {
                                    Text = "Wake Up!"
                                }
                            }
                    }
                },
                Scenario = ToastScenario.Alarm,
                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-winsoundevent:Notification.Looping.Alarm")
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                        {
                           new ToastButtonSnooze(),
                           new ToastButtonDismiss()
                        }
                }
            };
            XmlDocument xmlContent = new XmlDocument();
            xmlContent.LoadXml(content.GetContent());
            ToastNotification notification = new ToastNotification(xmlContent);
            string iString = localSettings.Values["alarm_time"].ToString();
            DateTime oDate = DateTime.ParseExact(iString, "HH:mm", null);
            Debug.WriteLine(oDate);
            DateTime dueTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, oDate.Hour, oDate.Minute, oDate.Second);
            ToastNotificationManager.History.Clear();
            if(dueTime > DateTime.Now)
            {
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(new ScheduledToastNotification(xmlContent, dueTime));
            }
            else
            {
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(new ScheduledToastNotification(xmlContent, dueTime.AddDays(1)));
            }
        }

        private void Connection_NetworkStatusChanged(object sender)
        {
            Debug.WriteLine("Internet Connection changed");
        }

        private async void FillMetricsList()
        {
            this.Metrics.Clear();
            if (NetworkInformation.GetInternetConnectionProfile() != null)
            {
                IEnumerable<Metric> list = await ClockClient.GetMetricsAsync("http://clockapi.azurewebsites.net/metrics");
             
                foreach (Metric m in list)
                {
                    this.Metrics.Add("Heart rate is: " + m.HeartRate);
                }
            }
            else
            {
                Metrics.Add("No metrics to display! check internet connectivity.");
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
            Debug.WriteLine("time: " +e.Parameter.ToString());
            if (!e.Parameter.ToString().Equals(""))
            {
                localSettings.Values["alarm_time"] = e.Parameter.ToString();
                textBlock.Text = e.Parameter.ToString();
                SetAlarm();
            }    
        }

        private void textBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SetAlarmPage),"Set Alarm");
        }
    }
}
