using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace Net.Astropenguin.Notis
{
    class NotificationService : ActiveData
    {
        public static readonly string ID = typeof( NotificationService ).Name;

        private XRegistry SavedChannels;

        public const string NOTIS_PROTO = "http://beta.blog.astropenguin.net/";

        public IEnumerable<NotisChannel> Channels
        {
            get
            {
                return SavedChannels.GetParametersWithKey( "channel" ).Remap( x => new NotisChannel( x.ID ) );
            }
        }

        public NotificationService()
        {
            SavedChannels = new XRegistry( "<channels />", "channels.xml" );
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

            RequestRemove( Channel.uuid );
        }

        public void RequestRemove( string uuid )
        {
            HttpRequest Request = new HttpRequest( new Uri( NOTIS_PROTO ) );
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

            Request.OpenWriteAsync( ServiceAuth.Auth + "&action=remove&id=" + uuid );
        }


        public async void CreateChannelUri()
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            HttpRequest Request = new HttpRequest( new Uri( NOTIS_PROTO ) );
            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.OnRequestComplete += e => Request_OnRequestComplete( channel, e );
            Request.OpenWriteAsync( ServiceAuth.Auth + "&action=register&uri=" + Uri.EscapeDataString( channel.Uri ) );
        }

        private void Request_OnRequestComplete( PushNotificationChannel Channel, DRequestCompletedEventArgs DArgs )
        {
            try
            {
                string Res = DArgs.ResponseString;

                Match m = new Regex( "^[\\da-f]{8}-[\\da-f]{4}-[\\da-f]{4}-[\\da-f]{4}-[\\da-f]{12}$" ).Match( Res );
                if ( m.Success )
                {
                    XParameter Param = new XParameter( Res );
                    Param.SetValue( new XKey( "channel", 1 ) );
                    SavedChannels.SetParameter( Param );

                    SavedChannels.Save();
                    NotifyChanged( "Channels" );
                }
            }
            catch ( Exception )
            {
                Channel.Close();
            }
        }
    }
}
