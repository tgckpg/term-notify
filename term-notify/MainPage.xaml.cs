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
using Tasks;

namespace term_notify
{
    public sealed partial class MainPage : Page
    {
        private NotificationService Service;
        private NotisChannel SelectedChannel;
        private ServiceInfo SelectedService;

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

        private void ShowServiceContext( object sender, RightTappedRoutedEventArgs e )
        {
            StackPanel Panel = sender as StackPanel;
            SelectedService = Panel.DataContext as ServiceInfo;
            FlyoutBase.ShowAttachedFlyout( Panel );
        }

        private void CopyId( object sender, RoutedEventArgs e )
        {
            if ( SelectedChannel == null ) return;
            DataPackage Data = new DataPackage();

            Data.SetText( SelectedChannel.uuid );
            Clipboard.SetContent( Data );
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

        private void NewChannel( object sender, RoutedEventArgs e )
        {
            if ( SelectedService == null ) return;
            Service.CreateChannelUri( SelectedService );
        }

        private void TestMessage( object sender, RoutedEventArgs e )
        {
            if ( SelectedChannel == null ) return;
            Service.TestMessage( SelectedChannel );
        }

        private void RemoveChannel( object sender, RoutedEventArgs e )
        {
            if ( SelectedChannel == null ) return;
            Service.Remove( SelectedChannel );
        }

        private async void EditService( object sender, RoutedEventArgs e )
        {
            if ( !( SelectedService == null || SelectedService.CanEdit ) )
            {
                SelectedService = null;
            }

            Pages.Dialogs.EditServiceProvider ESProvider = new Pages.Dialogs.EditServiceProvider( SelectedService );
            await Popups.ShowDialog( ESProvider );

            if ( ESProvider.Canceled ) return;

            Service.AddService( ESProvider.NameEx, ESProvider.ServiceEx, ESProvider.ParamEx );
        }

        private async void RemoveService( object sender, RoutedEventArgs e )
        {
            if ( SelectedService == null ) return;

            bool Confirmed = true;

            if ( Service.HasChannel( SelectedService.Name ) )
            {
                Confirmed = false;

                MessageDialog MsgBox = new MessageDialog(
                    "All associated channels by this services will also be removed. Continue?"
                    , "Remove \"" + SelectedService.Name + "\"" );
                MsgBox.Commands.Add( new UICommand( "Yes", ( x ) => { Confirmed = true; } ) );
                MsgBox.Commands.Add( new UICommand( "No" ) );

                await Popups.ShowDialog( MsgBox );
            }

            if ( Confirmed ) Service.Remove( SelectedService );
        }

        private void Help( object sender, RoutedEventArgs e )
        {
            var j = Windows.System.Launcher.LaunchUriAsync( new Uri( "https://github.com/tgckpg/term-notify/wiki" ) );
        }

        private async void ShowPrivacy( object sender, RoutedEventArgs e )
        {
            string title = "Having a trust issue?";
            string message = "Although I do not store your messages. You can create your own server by grabbing the service-side source code ( ~1000 lines of code ) at github under the help section..";
            await Popups.ShowDialog( new MessageDialog( message, title ) );
        }

        private async void ShowAbout( object sender, RoutedEventArgs e )
        {
            await Popups.ShowDialog( new Pages.Dialogs.About() );
        }
    }
}
