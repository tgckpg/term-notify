using System;
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

using Net.Astropenguin.Notis;

namespace term_notify
{
    public sealed partial class MainPage : Page
    {
        private NotificationService Service;
        private string SelectedChannel;

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
            IEnumerable<string> Channels = Service.GetChannels();
            if ( Channels.Count() == 0 )
            {
                Service.CreateChannelUri();
            }

            ChannelsView.ItemsSource = Channels;
        }

        private void ShowChannelContext( object sender, RightTappedRoutedEventArgs e )
        {
            TextBlock Text = sender as TextBlock;
            SelectedChannel = Text.DataContext as string;
            FlyoutBase.ShowAttachedFlyout( Text );
        }
    }
}
