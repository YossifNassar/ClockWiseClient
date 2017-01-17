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
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;
using Newtonsoft.Json.Linq;
using Windows.Networking.Proximity;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.UI.WebUI;

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
        private const string BT_NAME = "HC-06";
        private static DispatcherTimer dispatcherTimer;
        private static ObservableCollection<double> cartesianChartData = new ObservableCollection<double>() {0 };

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
            SetChartData();
            //FillCombobox();
            this.myChart.Series[0].ItemsSource = CreateData();
            //timer 
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            try
            {
                Random rnd = new Random();
                int heartRate = ((rnd.Next()) % 100);
                heartTextBlock.Text = heartRate.ToString();
                cartesianChartData.Add(heartRate);
                String btSignal = App.RecieveBTSignal();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            
        }

        public class Data
        {
            public double Value { get; set; }
        }

        public List<Data> CreateData()
        {
            List<Data> data = new List<Data>();
            data.Add(new Data() { Value = 15.5 });
            data.Add(new Data() { Value = 8.5 });
            return data;
        }


        private async void FillCombobox()
        {
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();
            if (pairedDevices.Count == 0)
            {
                return;
            }
            foreach(var device in pairedDevices)
            {
                comboDevice.Items.Add(device.DisplayName);
            }
        }

        private void SetChartData()
        {
            this.radChart.DataContext = cartesianChartData;
            this.radChart2.DataContext = cartesianChartData;
        }
        

        private void LoadSettings()
        {
            if (!localSettings.Values.ContainsKey("alarm_time"))
            {
                localSettings.Values["alarm_time"] = "07:00";
            }
            textBlock.Text = localSettings.Values["alarm_time"].ToString();
            App.connectionParams.isConnectedToBluetooth = false;
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
            if (dueTime > DateTime.Now)
            {
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(new ScheduledToastNotification(xmlContent, dueTime));
            }
            else
            {
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(new ScheduledToastNotification(xmlContent, dueTime.AddDays(1)));
            }
        }

        private async void Connection_NetworkStatusChanged(object sender)
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

        private async void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                try
                {
                    if (toggleSwitch.IsOn == true && App.connectionParams.isConnectedToBluetooth == false)
                    {
                        await connectToBT();
                    }
                    if (toggleSwitch.IsOn == false && App.connectionParams.isConnectedToBluetooth == true)
                    {
                        disconnectFromBT();
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }

        }

        private async Task connectToBT()
        {
            bluetoothStatus.Text = "Trying to connect...";
            bool deviceFound = false;
            if (App.connectionParams.isConnectedToBluetooth)
            {
                return;
            }
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();
            if (pairedDevices.Count == 0)
            {
                Debug.WriteLine("No paired devices found.");
            }
            else
            {
                foreach (var device in pairedDevices)
                {
                    Debug.WriteLine("current = " + device.DisplayName);
                    if (device.DisplayName == BT_NAME) 
                    {
                        App.connectionParams.chatSocket = new StreamSocket();
                        try
                        {
                            Debug.WriteLine("device host name: "+ device.HostName);
                            await App.connectionParams.chatSocket.ConnectAsync(device.HostName, "1");
                            App.connectionParams.chatWriter = new DataWriter(App.connectionParams.chatSocket.OutputStream);
                            App.connectionParams.chatReader = new DataReader(App.connectionParams.chatSocket.InputStream);
                            bluetoothStatus.Text = "Device is connected.";
                            deviceFound = true;
                            App.connectionParams.isConnectedToBluetooth = true;
                            
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine("Cannot connect to "+BT_NAME);
                            toggleSwitch.IsOn = false;
                        }
                    }
                }
                if (App.connectionParams.chatSocket == null)
                {
                    Debug.WriteLine("Device is Not Paired");
                }
                if (!deviceFound)
                {
                    bluetoothStatus.Text = "Device is not connected!";
                }
            }
        }

        private void disconnectFromBT()
        {
            App.connectionParams.chatSocket.Dispose();
            App.connectionParams.chatReader.DetachStream();
            App.connectionParams.chatReader.Dispose();
            App.connectionParams.chatWriter.DetachStream();
            App.connectionParams.chatWriter.Dispose();
            App.connectionParams.chatReader = null;
            App.connectionParams.chatService = null;
            App.connectionParams.chatSocket = null;
            App.connectionParams.chatWriter = null;
            App.connectionParams.isConnectedToBluetooth = false;
            bluetoothStatus.Text = "Not connected";
        }


    }
}
