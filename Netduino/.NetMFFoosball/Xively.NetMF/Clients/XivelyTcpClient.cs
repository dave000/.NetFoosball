using System;
using Microsoft.SPOT;
using Xively.NetMF.Interface;
using Xively.NetMF.Entities;
using System.Collections;
using System.Threading;

namespace Xively.NetMF.Clients
{
    public class XivelyTcpClient : XivelyClient
    {
        private bool _keepAlive;
        private AutoResetEvent _pendingResWait = new AutoResetEvent(false);
        private Thread readThread;

        private void receiveStart()
        {
            while (true)
            {
                if (_pendingResWait.WaitOne())
                {
                    lock (socketHelper.SocketLock)
                    {
                        var response = _socket.Receive(true);

                        pendingReqWait.Set();
                        var resultObject = (PutResult)serializer.Deserialize(response, typeof(PutResult));

                        RaiseOnResponse(resultObject.Status, resultObject.Token, resultObject.Resource);

                        if (!IsConnected)
                        {
                            RaiseOnLostConnection();
                        }
                        else
                        {
                            CloseIfNeeded(true);
                        }

                    }
                }
            }
        }

        public override bool KeepAlive
        {
            get
            {
                return _keepAlive;
            }
        }

        protected override void OnConnected()
        {
            if (readThread == null)
            {
                readThread = new Thread(receiveStart);
                readThread.Start();
            }
            base.OnConnected();
        }

        protected override void SendData(string json, string waitForToken = null)
        {
            EnsureConnection();    
            _socket.Send(json);
            _pendingResWait.Set();
        }

        public XivelyTcpClient(Uri xivelyUrl, ISocketHelper socketHelper, bool keepAlive = true)
            : base(xivelyUrl, socketHelper)
        {
            _keepAlive = keepAlive;
        }

        protected override void PutWorker(Feed feed, DataStreamValue[] values, string token, Hashtable headers)
        {
            var request = PutPostRequestEntity.CreatePutRequest(feed.AsResource(), token, headers);
            //TODO: place values in the base
            
            request.AddDataStreamValues(values);
            SendData(request, token);
        }
    }
}
