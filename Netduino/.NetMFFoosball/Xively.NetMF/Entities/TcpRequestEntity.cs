using System;
using Microsoft.SPOT;
using System.Collections;

namespace Xively.NetMF.Entities
{
    public class TcpRequestEntity
    {
        public static String JSON_Name_Method = "method";
        public string Method { get; private set; }
        
        public static String JSON_Name_Resource = "resource";
        public string Resource { get; private set; }

        public static String JSON_Name_Params = "params";
        public Hashtable Params { get; private set; }

        public static String JSON_Name_Headers = "headers";
        public Hashtable Headers { get; private set; }

        public static String JSON_Name_Token = "token";
        public string Token { get; private set; }

        protected TcpRequestEntity(string method, string resource, string token, Hashtable headers)
        {
            Method = method;
            Resource = resource;
            Token = token;
            Params = new Hashtable();
            Headers = headers ?? new Hashtable();
        }
    }
}
