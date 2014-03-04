using System;
using Microsoft.SPOT;

namespace Xively.NetMF
{
    public class DataStreamValue
    {
        public static String JSON_Name_Id = "id";
        public String Id { get; private set; }
        public static String JSON_Name_Value = "current_value";
        public String Value { get; set; }

        public DataStreamValue(string id)
        {
            Id = id;
        }


        public DataStreamValue(string id, string value)
            : this(id)
        {
            Value = value;
        }
    }
}
