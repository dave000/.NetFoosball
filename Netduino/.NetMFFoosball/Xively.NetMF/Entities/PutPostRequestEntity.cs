using System;
using Microsoft.SPOT;
using System.Collections;

namespace Xively.NetMF.Entities
{


    public class PutPostRequestEntity : TcpRequestEntity
    {
        public static String JSON_Name_Body = "body";
        public Hashtable Body { get; private set; }
        private ArrayList dataStreams = null;

        private PutPostRequestEntity(RequestMethod method, string resource, string token, Hashtable headers): base(method == RequestMethod.Post ? "post" : "put", resource, token, headers)
        {
            
        }

        public static PutPostRequestEntity CreatePostRequest(string resource, string token, Hashtable headers)
        {
            return new PutPostRequestEntity(RequestMethod.Post, resource, token, headers);
        }

        public static PutPostRequestEntity CreatePutRequest(string resource, string token, Hashtable headers)
        {
            return new PutPostRequestEntity(RequestMethod.Put, resource, token, headers);
        }

        public void AddDataStreamValues(DataStreamValue[] values)
        {
            if (dataStreams == null)
            {
                initBody();
            }

            foreach (var value in values)
            {
                dataStreams.Add(value);
            }
        }

        private void initBody()
        {
            Body = new Hashtable();
            Body.Add("version", "1.0.0");
            dataStreams = new ArrayList();
            Body.Add("datastreams", dataStreams);
        }
    }
}
