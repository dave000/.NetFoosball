using System;
using Microsoft.SPOT;
using Toolbox.NETMF.NET;

namespace Xively.NetMF.Interface
{
    public interface ISocketHelper
    {
        object SocketLock { get; }

        SimpleSocket CreateSocket(string address, ushort port);
    }
}
