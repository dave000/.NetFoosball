using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XivelyClient
{
    public class Listener
    {
        public delegate void XivelyTrigger(dynamic result);

        public static bool ConnectedToXively = false;

        private static object consoleLock = new object();
        private const int sendChunkSize = 256;
        private const int receiveChunkSize = 64;
        private const bool verbose = true;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(1000);

        public static async Task Connect(string uri, string feed, XivelyTrigger callback)
        {
            ClientWebSocket webSocket = null;

            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

                Subscribe(webSocket, feed, ConfigurationManager.AppSettings["XivelyAPIKey"], callback);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
                             
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        class ApiKeyHeader
        {
            [JsonProperty(PropertyName = "X-ApiKey")]
            public String XApiKey { get; set; }
            public ApiKeyHeader(string apiKey)
            {
                XApiKey = apiKey;
            }
        }

        private static void Subscribe(ClientWebSocket webSocket, string feed, string apiKey, XivelyTrigger callback)
        {
            object subscribeObject = new { method = "subscribe", resource = String.Format("/feeds/{0}", feed), headers = new ApiKeyHeader(apiKey), token = "subscribeFeed" };

            var subscribeMessage = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeObject);


            Send(webSocket, subscribeMessage).Wait();
            Receive(webSocket, callback);
        }

        private static async Task Send(ClientWebSocket webSocket, string message)
        {
            var sendData = Encoding.UTF8.GetBytes(message);
            var dataLength = sendData.Length;
            var currentChunk = Math.Min(dataLength, sendChunkSize);
            var offset = 0;
            var isEndOfMessage = false;
            while (!isEndOfMessage)
            {
                isEndOfMessage = (offset + currentChunk) >= dataLength;
                await webSocket.SendAsync(new ArraySegment<byte>(sendData, offset, currentChunk), WebSocketMessageType.Text, isEndOfMessage, CancellationToken.None);
                offset += sendChunkSize;
            }
        }

        private static void Receive(ClientWebSocket webSocket, XivelyTrigger callback)
        {
            
            List<byte> loadedData = new List<byte>();
            while (webSocket.State == WebSocketState.Open)
            {
                byte[] buffer = new byte[receiveChunkSize]; 
                var result = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                result.Wait();
                loadedData.AddRange(buffer);
                    
                if (result.Result.EndOfMessage) {
                    var message = Encoding.UTF8.GetString(loadedData.ToArray());
                    Trace.WriteLine(String.Format("R: {0}, {1}, {2}, {3}, {4}", result.Result.MessageType, result.Result.CloseStatus, result.Result.CloseStatusDescription, result.Result.EndOfMessage, message));
                
                    loadedData.Clear();
                    object response = JsonConvert.DeserializeObject(message);
                    dynamic dResponse = (dynamic)response;
                    if (dResponse.token == "subscribeFeed" && dResponse.body == null)
                    {
                        ConnectedToXively = dResponse.status == 200;
                    }

                    if (dResponse.body != null)
                    {
                        callback(dResponse.body);
                    }
                }
                
            }
        }

       
    }


}

