using Xively.NetMF.Interface;
using Toolbox.NETMF.Hardware;
using Toolbox.NETMF.NET;

namespace NetMFFoosball.Helpers
{
    public class WiflyHelper : ISocketHelper
    {
        WiFlyGSX wifi;

        public object SocketLock
        {
            get { return wifi ?? new object(); }
        }

        public Toolbox.NETMF.NET.SimpleSocket CreateSocket(string address, ushort port)
        {
            return new WiFlySocket(address, port, wifi);
        }

        public WiflyHelper(WiFlyGSX pWifi)
        {
            wifi = pWifi;
        }
    }

    public class NetduinoSocketHelper : ISocketHelper
    {

        public object SocketLock
        {
            get { return new object(); }
        }

        public Toolbox.NETMF.NET.SimpleSocket CreateSocket(string address, ushort port)
        {
            return new IntegratedSocket(address, port);
        }

    }
}

