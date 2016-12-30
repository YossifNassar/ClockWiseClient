using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace TryClock
{
    public class bluetoothConnectionParams
    {
        public RfcommDeviceService chatService;
        public StreamSocket chatSocket;
        public DataWriter chatWriter;
        public DataReader chatReader;
        public bool isConnectedToBluetooth;
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif
        public static bluetoothConnectionParams connectionParams;
        public static int num;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            connectionParams = new bluetoothConnectionParams();
            connectionParams.chatReader = null;
            connectionParams.chatService = null;
            connectionParams.chatSocket = null;
            connectionParams.chatWriter = null;
            connectionParams.isConnectedToBluetooth = false;
        }

        public static string makeString(int[] arr)
        {
            string s = "";
            int i = 0;
            foreach (int a in arr)
            {
                s += a.ToString();
                if (i != 15)
                {
                    s += " ";
                }
                else
                {
                    s += "]";
                }
                i++;
            }
            return s;
        }

        public static async void SendBTSignal(int[] arr)
        {
            string s = App.makeString(arr);
            App.connectionParams.chatWriter.WriteString(s);
            await App.connectionParams.chatWriter.StoreAsync();
        }

        public static async void RecieveBTSignal()
        {
            char ch = '\0';
            int fit = 0;
            while (ch != '\n')
            {
                uint sizeFieldCount;
                IAsyncOperation<uint> taskLoad = App.connectionParams.chatReader.LoadAsync(1);
                taskLoad.AsTask().Wait();
                sizeFieldCount = taskLoad.GetResults();
                if (sizeFieldCount != 1)
                {
                    App.connectionParams.isConnectedToBluetooth = false;
                    return; // the socket was closed before reading.
                }
                byte b = App.connectionParams.chatReader.ReadByte();
                ch = Convert.ToChar(b);
                if (ch != '\r' && ch != '\n')
                {
                    fit *= 10;
                    fit += Convert.ToInt32(b) - '0';
                }
            }
            App.num = fit;
        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}