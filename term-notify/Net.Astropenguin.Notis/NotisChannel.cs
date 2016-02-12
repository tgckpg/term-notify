using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Notis
{
    class NotisChannel
    {
        public string uuid { get; private set; }

        public string CmdCurl 
        {
            get
            {
                return "curl " + Channel.Info.NOTIS_PROTO
                    + string.Format(
                        " --data \"id={0}&action=deliver&title={1}&message={2}\""
                        , uuid, Uri.EscapeDataString( "Hello world" )
                        , Uri.EscapeDataString( "Test notification message" )
                    );
            }
        }

        public string CmdWget
        {
            get
            {
                return "wget -qO- " + Channel.Info.NOTIS_PROTO
                    + string.Format(
                        " --post-data=\"id={0}&action=deliver&title={1}&message={2}\""
                        , uuid, Uri.EscapeDataString( "Hello world" )
                        , Uri.EscapeDataString( "Test notification message" )
                    );
            }
        }

        public NotisChannel( string uuid )
        {
            this.uuid = uuid;
        }
    }
}
