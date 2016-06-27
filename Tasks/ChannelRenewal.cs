using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;

using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;

namespace Tasks
{
    public sealed class ChannelRenewal : IBackgroundTask
    {
        BackgroundTaskDeferral Deferral;

        XRegistry TaskLog;
        XRegistry SavedChannels;
        XParameter CurrentTask;

        ServiceProvider SProvider;

        public ChannelRenewal()
        {
            // Bootstrap
            XRegistry.AStorage = new AppStorage();
            SProvider = new ServiceProvider();

            TaskLog = new XRegistry( "<tasklog />", "tasklog.xml" );
            SavedChannels = new XRegistry( "<channels />", "channels.xml" );
        }

        public async void Run( IBackgroundTaskInstance taskInstance )
        {
            Deferral = taskInstance.GetDeferral();

            XParameter[] Params = SavedChannels.GetParametersWithKey( "channel" );

            if ( Params.Length == 0 )
            {
                Deferral.Complete();
                return;
            }

            // Associate a cancellation handler with the background task.
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler( OnCanceled );

            foreach ( XParameter Param in Params )
            {
                CurrentTask = new XParameter( DateTime.Now.ToFileTime() + "" );
                CurrentTask.SetValue( new XKey[] {
                    new XKey( "name", taskInstance.Task.Name )
                    , new XKey( "start", true )
                    , new XKey( "end", false )
                } );

                PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                if ( channel.Uri != Param.GetValue( "uri" ) )
                {
                    await RenewChannel( Param.GetValue( "provider" ), Param.ID, Uri.EscapeDataString( channel.Uri ) );
                }

            }

            Deferral.Complete();
        }

        private async Task RenewChannel( string provider, string uuid, string uri )
        {
            ServiceInfo Provider = SProvider.GetService( provider );
            TaskCompletionSource<int> TCS = new TaskCompletionSource<int>();

            HttpRequest Request = new HttpRequest( new Uri( Provider.Protocol ) );
            Request.EN_UITHREAD = false;
            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";

            Request.OnRequestComplete += e =>
            {
                try
                {
                    CurrentTask.SetValue( new XKey( "Response", e.ResponseString ) );
                }
                catch ( Exception ex )
                {
                    CurrentTask.SetValue( new XKey( "Error", ex.Message ) );
                }

                TCS.SetResult( 1 );
            };

            Request.OpenWriteAsync( Provider.Param + "action=register&id=" + uuid + "&uri=" + uri );

            await TCS.Task;
        }

        // Handles background task cancellation.
        private void OnCanceled( IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason )
        {
            if ( CurrentTask != null )
            {
                CurrentTask.SetValue( new XKey( "canceled", true ) );
                TaskLog.SetParameter( CurrentTask );
                TaskLog.Save();
            }

            if ( Deferral != null ) Deferral.Complete();
        }
    }
}
