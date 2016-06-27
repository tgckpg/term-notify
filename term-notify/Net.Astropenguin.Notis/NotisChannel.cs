using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tasks;

namespace Net.Astropenguin.Notis
{
    class NotisChannel
    {
        public string uuid { get; private set; }

        public string CmdCurl 
        {
            get
            {
                return "curl " + Provider.Protocol + " --data \"" + Helloworld + "\"";
            }
        }

        public string CmdWget
        {
            get
            {
                return "wget -qO- " + Provider.Protocol + " --post-data=\"" + Helloworld + "\"";
            }
        }

        public string Helloworld
        {
            get
            {
                return string.Format(
                    "id={0}&action=deliver&title={1}&message={2}"
                    , uuid, Uri.EscapeDataString( "Hello world" )
                    , Uri.EscapeDataString( "Test notification message" )
                );
            }

        }

        public string ServiceName
        {
            get { return Provider.Name; }
        }

        public ServiceInfo Provider
        {
            get; private set;
        }

        public NotisChannel( string uuid, ServiceInfo provider )
        {
            this.uuid = uuid;
            Provider = provider;
        }
    }
}
