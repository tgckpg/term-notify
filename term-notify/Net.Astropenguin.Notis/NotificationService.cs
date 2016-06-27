using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

using Tasks;

namespace Net.Astropenguin.Notis
{
    class NotificationService : ActiveData
    {
        public static readonly string ID = typeof( NotificationService ).Name;

        private XRegistry SavedChannels;
        private ServiceProvider SProvider = new ServiceProvider();

        public IEnumerable<NotisChannel> Channels
        {
            get
            {
                return SavedChannels.GetParametersWithKey( "channel" ).Remap(
                    x => new NotisChannel( x.ID, SProvider.GetService( x.GetValue( "provider" ) ) )
                );
            }
        }

        public IEnumerable<ServiceInfo> Services
        {
            get
            {
                return SProvider.GetServices();
            }
        }


        public NotificationService()
        {
            SavedChannels = new XRegistry( "<channels />", "channels.xml" );
            CreateChannelRenewalTrigger();
        }

        private void CreateChannelRenewalTrigger()
        {
            foreach ( KeyValuePair<Guid, IBackgroundTaskRegistration> BTask in BackgroundTaskRegistration.AllTasks )
            {
                if ( BTask.Value.Name == "ChannelRenewalTrigger" ) return;
            }

            TimeTrigger OneDayTrigger = new TimeTrigger( 1440, false );
            BackgroundTaskBuilder Builder = new BackgroundTaskBuilder();

            Builder.Name = "ChannelRenewalTrigger";
            Builder.TaskEntryPoint = "Tasks.ChannelRenewal";
            Builder.SetTrigger( OneDayTrigger );

            BackgroundTaskRegistration task = Builder.Register();
        }

        public void ShowNotification( string Title, string Message )
        {
            XmlDocument XDoc = ToastNotificationManager.GetTemplateContent( ToastTemplateType.ToastText02 );
            ToastNotifier Notifier = ToastNotificationManager.CreateToastNotifier();

            XmlNodeList List = XDoc.GetElementsByTagName( "text" );
            List.First().InnerText = Title;
            List.Last().InnerText = Message;

            Notifier.Show( new ToastNotification( XDoc ) );
        }

        public void Remove( NotisChannel Channel )
        {
            SavedChannels.RemoveParameter( Channel.uuid );
            SavedChannels.Save();

            NotifyChanged( "Channels" );
            RequestRemove( Channel.Provider, Channel.uuid );
        }

        public void Remove( ServiceInfo Service )
        {
            NotisChannel[] RmChannels = Channels.Where( x => x.Provider.Name == Service.Name ).ToArray();
            foreach( NotisChannel RmChannel in RmChannels ) Remove( RmChannel );

            SProvider.RemoveService( Service.Name );
            NotifyChanged( "Services" );
        }

        internal void TestMessage( NotisChannel Channel )
        {
            HttpRequest Request = new HttpRequest( new Uri( Channel.Provider.Protocol ) );

            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";

            Request.OnRequestComplete += ( e ) =>
            {
            };

            Request.OpenWriteAsync( Channel.Provider.Param + Channel.Helloworld );
        }

        private void RequestRemove( ServiceInfo Info, string uuid )
        {
            HttpRequest Request = new HttpRequest( new Uri( Info.Protocol ) );

            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";

            Request.OnRequestComplete += e =>
            {
                try
                {
                    Logger.Log( ID, e.ResponseString, LogType.DEBUG );
                }
                catch ( Exception ex )
                {
                    Logger.Log( ID, ex.Message, LogType.ERROR );
                }
            };

            Request.OpenWriteAsync( Info.Param + "action=remove&id=" + uuid );
        }

        internal bool HasChannel( string ServiceName )
        {
            return SavedChannels.GetParametersWithKey( "channel" ).Any( x => x.GetValue( "provider" ) == ServiceName );
        }

        internal void AddService( string nameEx, string serviceEx, string paramEx )
        {
            SProvider.SetService( nameEx, serviceEx, paramEx );
            NotifyChanged( "Services" );
        }

        public async void CreateChannelUri( ServiceInfo Info )
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            HttpRequest Request = new HttpRequest( new Uri( Info.Protocol ) );

            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.OnRequestComplete += e => Request_OnRequestComplete( Info, channel, e );
            Request.OpenWriteAsync( Info.Param + "action=register&uri=" + Uri.EscapeDataString( channel.Uri ) );
        }

        private void Request_OnRequestComplete( ServiceInfo Info, PushNotificationChannel Channel, DRequestCompletedEventArgs DArgs )
        {
            try
            {
                string Res = DArgs.ResponseString;

                Match m = new Regex( "^[\\da-f]{8}-[\\da-f]{4}-[\\da-f]{4}-[\\da-f]{4}-[\\da-f]{12}$" ).Match( Res );
                if ( m.Success )
                {
                    XParameter Param = new XParameter( Res );
                    Param.SetValue(
                        new XKey[] {
                            new XKey( "provider", Info.Name )
                            , new XKey( "channel", 1 )
                            , new XKey( "uri", Channel.Uri )
                        } );

                    SavedChannels.SetParameter( Param );

                    SavedChannels.Save();
                    NotifyChanged( "Channels" );
                    return;
                }
            }
            catch ( Exception ) { }
            Channel.Close();
        }
    }
}
