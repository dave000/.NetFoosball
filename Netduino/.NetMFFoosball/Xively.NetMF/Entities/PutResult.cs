using System;
using Microsoft.SPOT;

namespace Xively.NetMF.Entities
{
    public class PutResult
    {
        public static String JSON_Name_Status = "status";
        public long Status { get; set; }
        
        public static String JSON_Name_Token = "token";
        public String Token { get; set; }

        public static String JSON_Name_Resource = "resource";
        public String Resource { get; set; }
        
    }
}
