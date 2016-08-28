using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks
{
    public sealed class ServiceInfo
    {
        public string Name { get; internal set; }
        public string Protocol { get; internal set; }
        public string Param { get; internal set; }
        public bool CanEdit { get; internal set; }
    }

    public sealed class ServiceProvider
    {
        XRegistry Settings;

        private readonly ServiceInfo DefaultService = new ServiceInfo() {
            Name = "Default ( Astropenguin )"
            , Protocol = Channel.Info.NOTIS_PROTO
            , Param = Channel.Info.SERVICE_AUTH
            , CanEdit = false
        };

        public ServiceProvider()
        {
            Settings = new XRegistry( "<services />", "services.xml" );
        }

        public void SetService( string Name, string Proto, string Auth )
        {
            XParameter Param = new XParameter( Name );
            Param.SetXValue( new XKey[]
            {
                new XKey( "proto", Proto )
                , new XKey( "auth", Auth )
            } );

            Settings.SetParameter( Param );
            Settings.Save();
        }

        public void RemoveService( string name )
        {
            Settings.RemoveParameter( name );
            Settings.Save();
        }

        public ServiceInfo GetService( string Name )
        {
            XParameter Param = Settings.Parameter( Name );
            if ( Param != null )
            {
                return new ServiceInfo() { Name = Param.Id, Protocol = Param.GetValue( "proto" ), Param = Param.GetValue( "auth" ) };
            }

            return DefaultService;
        }

        public IEnumerable<ServiceInfo> GetServices()
        {
            List<ServiceInfo> Services = new List<ServiceInfo>();

            XParameter[] Params = Settings.Parameters( "proto" );

            foreach( XParameter P in Params )
            {
                Services.Add( new ServiceInfo() {
                    Name = P.Id
                    , Protocol = P.GetValue( "proto" )
                    , Param = P.GetValue( "auth" )
                    , CanEdit = true
                } );
            }

            Services.Add( DefaultService );
            return Services;
        }
    }
}