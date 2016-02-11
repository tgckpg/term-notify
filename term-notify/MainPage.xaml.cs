using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.Notis;

namespace term_notify
{
    public sealed partial class MainPage : Page
    {
        private NotificationService Service;
        private NotisChannel SelectedChannel;

        public MainPage()
        {
            this.InitializeComponent();

            Bootstrap();
            SetTemplate();
        }

        private void Bootstrap()
        {
            Net.Astropenguin.IO.XRegistry.AStorage = new Net.Astropenguin.IO.AppStorage();
        }

        private void SetTemplate()
        {
            Service = new NotificationService();
            LayoutRoot.DataContext = Service;
        }

        private void ShowChannelContext( object sender, RightTappedRoutedEventArgs e )
        {
            StackPanel Panel = sender as StackPanel;
            SelectedChannel = Panel.DataContext as NotisChannel;
            FlyoutBase.ShowAttachedFlyout( Panel );
        }

        private void NewChannel( object sender, RoutedEventArgs e )
        {
            Service.CreateChannelUri();
        }

        private void CopyCurl( object sender, RoutedEventArgs e )
        {
            if ( SelectedChannel == null ) return;
            DataPackage Data = new DataPackage();

            Data.SetText( SelectedChannel.CmdCurl );
            Clipboard.SetContent( Data );
        }

        private void CopyWget( object sender, RoutedEventArgs e )
        {
            if ( SelectedChannel == null ) return;
            DataPackage Data = new DataPackage();

            Data.SetText( SelectedChannel.CmdWget );
            Clipboard.SetContent( Data );
        }

        private void RemoveChannel( object sender, RoutedEventArgs e )
        {
            if ( SelectedChannel == null ) return;
            Service.Remove( SelectedChannel );
        }

        private async void ShowPrivacy( object sender, RoutedEventArgs e )
        {
            string title = "Of course we concern about privacy!";
            string message = "No, I do not store your message. But every notification is sent via WNS ( Windows Push Notification Server ), they are hosted by Microsoft. So that part is none of my concern.";
            await Popups.ShowDialog( new MessageDialog( message, title ) );
        }

        private async void ShowAbout( object sender, RoutedEventArgs e )
        {
            await Popups.ShowDialog( new Pages.Dialogs.About() );
        }
    }
}
