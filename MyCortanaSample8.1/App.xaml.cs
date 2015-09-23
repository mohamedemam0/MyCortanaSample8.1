using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace MyCortanaSample8._1
{
    public sealed partial class App : Application
    {
        public static string InstallationMessage;
        private TransitionCollection transitions;
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                //Step Two: Registering VCD file.
                try
                {
                    Uri uri = new Uri("ms-appx:///VoiceCommandDefinition1.xml");
                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                    await VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(file);

                    App.InstallationMessage = "Installation Successfull";
                }
                catch(Exception ex)
                {
                    //new MessageDialog(ex.Message).ShowAsync();
                    App.InstallationMessage = ex.Message;
                }

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
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            Window.Current.Activate();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;
                Window.Current.Content = rootFrame;
                rootFrame.Navigate(typeof(MainPage));
            }

            Window.Current.Activate();

            // For VoiceCommand activations, the activation Kind is ActivationKind.VoiceCommand
            if (args.Kind == ActivationKind.VoiceCommand)
            {
                // since we know this is the kind, a cast will work fine
                VoiceCommandActivatedEventArgs vcArgs = (VoiceCommandActivatedEventArgs)args;

                // The NavigationTarget retrieved here is the value of the Target attribute in the
                // Voice Command Definition xml Navigate node
                string target = vcArgs.Result.SemanticInterpretation.Properties["NavigationTarget"][0];

                // since the target is option, we check for its presence
                if (!String.IsNullOrEmpty(target))
                {
                    // This GetType is a way get the type corresponding to target page
                    // in order to navigate to the page.
                    Type pageType = Type.GetType(typeof(MainPage).Namespace + "." + target);

                    // If the Target in the xml does not correspond to a page the pageType
                    // will be null. Such issue would be caught in app development.
                    // The two targets in the xml correspond to a page
                    if (pageType != null)
                    {
                        // Navigate to the page passing the speech result for further processing
                        // in the page. In Silverlight the navigation happens under the hood.
                        // For Phone Store Apps, it is done explicitly like this:
                        rootFrame.Navigate(pageType, vcArgs.Result);
                    }
                }
            }
        }

        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}