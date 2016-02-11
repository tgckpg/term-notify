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

using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace Net.Astropenguin.Notis
{
    class NotificationService
    {
        public static readonly string ID = typeof( NotificationService ).Name;

        private XRegistry SavedChannels;

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

        public async void CreateChannelUri()
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            HttpRequest Request = new HttpRequest( new Uri( "http://botanical.astropenguin.net/" ) );
            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.OnRequestComplete += Request_OnRequestComplete;
            Request.OpenWriteAsync( "action=register&uri=" + Uri.EscapeDataString( channel.Uri ) );
        }

        public IEnumerable<string> GetChannels()
        {
            return SavedChannels.GetParametersWithKey( "channel" ).Remap( x => x.ID );
        }

        private void Request_OnRequestComplete( DRequestCompletedEventArgs DArgs )
        {
            string Res = DArgs.ResponseString;

            Match m = new Regex( "^[\\da-f]{8}-[\\da-f]{4}-[\\da-f]{4}-[\\da-f]{4}-[\\da-f]{12}$" ).Match( Res );
            if ( m != null )
            {
                XParameter Param = new XParameter( Res );
                Param.SetValue( new XKey( "channel", 1 ) );
                SavedChannels.SetParameter( Param );

                SavedChannels.Save();
            }
        }
    }
}
