using System;
using Microsoft.SPOT;
using Xively.NetMF.Interface;
using Toolbox.NETMF.NET;
using Json.NETMF;
using System.Collections;
using System.Threading;
using Xively.NetMF.Entities;

namespace Xively.NetMF
{
    public enum RequestMethod
    {
        Get = 1,
        Post = 2,
        Put = 3,
        Subscribe = 4
    }

    public enum ConnectResult
    {
        AlreadyConnected = 0,
        Connected = 1,
        Error = 2
    }

    public delegate bool XivelyResponse(long statusCode, string token, object response);
    public delegate void ConnectionClosed();


    public abstract class XivelyClient
    {
        protected ISocketHelper socketHelper;
        protected SimpleSocket _socket;
        private Uri address;
        protected JsonSerializer serializer = new JsonSerializer();
        protected ManualResetEvent pendingReqWait = new ManualResetEvent(true);
        

        public event XivelyResponse OnResponse;
        public event ConnectionClosed OnLostConnection;
        public event ConnectionClosed OnCloseConnection;

        public Feed CurrentFeed { get; set; }

        protected void RaiseOnResponse(long statusCode, string token, object response)
        {
            if (OnResponse != null)
            {
                OnResponse(statusCode, token, response);
            }
        }

        protected void RaiseOnLostConnection()
        {
            if (OnLostConnection != null)
            {
                OnLostConnection();
            }
        }

        public Uri Address
        {
            get
            {
                return address;
            }
        }

        public static bool EndsWith(string s, string value)
        {
            return s.IndexOf(value) == s.Length - value.Length;
        }




        

        protected bool IsConnected
        {
            get
            {
                return _socket != null && _socket.IsConnected;
            }
        }

        protected ConnectResult Connect()
        {
            if (IsConnected)
            {
                return ConnectResult.AlreadyConnected;
            }

            _socket = socketHelper.CreateSocket(address.Host, (ushort)address.Port);
            _socket.LineEnding = "\r\n";
            _socket.Connect();
            OnConnected();

            return ConnectResult.Connected;
        }

        protected virtual void OnConnected()
        {
        }

        public void CloseConnection()
        {
            CloseConnectionWorker(false);
        }

        protected void CloseConnectionWorker(bool raiseEvent)
        {
            if (IsConnected)
            {
                _socket.Close();
                if (raiseEvent && OnCloseConnection != null)
                {
                    OnCloseConnection();
                }
            }
        }

        protected void CloseIfNeeded(bool raiseEvent)
        {
            if (!KeepAlive)
            {
                CloseConnectionWorker(raiseEvent);
            }
        }

        public string Put(string token, params DataStreamValue[] values)
        {
            return Put(CurrentFeed, (string)token, values);
        }

        public string Put(params DataStreamValue[] values)
        {
            return Put(CurrentFeed, values);
        }

        public string Put(Feed feed, params DataStreamValue[] values)
        {
            return Put(feed, null, values);
        }

        public string Put(Feed feed, string token, params DataStreamValue[] values)
        {
            token = token ?? Guid.NewGuid().ToString();

            PutWorker(feed, values, token, GetHeaders());
            return token;
        }

        protected Hashtable GetHeaders()
        {
            var headers = new Hashtable();
            headers.Add("X-ApiKey", XivelyConfig.GetString(XivelyConfig.StringResources.ApiKey));
            return headers;
        }

        protected abstract void PutWorker(Feed feed, DataStreamValue[] values, string token, Hashtable headers);

        protected void SendData(Object dataObj, string asyncToken)
        {

            //string json = "{\"method\" : \"put\", \"resource\" : \"/feeds/1996686508\", \"params\" : {}, \"headers\" : {\"X-ApiKey\":\"zrT2OYBhFdHhi0MsFxjlcwXhuoMMmekkkgok3MHcFNGkktF3\"},  \"body\" : {\"version\" : \"1.0.0\", \"datastreams\" : [{ \"id\" : \"Szoba\",\"current_value\" : \"25\"} ] }, \"token\" : \"0x12345\"}";
            //dataObj.Token = asyncToken;
            string jsonData = serializer.Serialize(dataObj);

            if (pendingReqWait.WaitOne())
            {
                lock (socketHelper.SocketLock)
                {
                    if (pendingReqWait.WaitOne())
                    {
                        pendingReqWait.Reset();                                            
                        SendData(jsonData, asyncToken);
                    }
                }
            }
            else
            {
                Debug.Print("Could not wait for last request finish");
            }

        }

        protected abstract void SendData(string json, string waitForToken = null);

        protected void EnsureConnection()
        {
            Connect();
        }

        protected XivelyClient(Uri xivelyUrl, ISocketHelper helper)
        {
            socketHelper = helper;
            address = xivelyUrl;
        }

        public virtual bool KeepAlive { get { return false; } }
    }
}
