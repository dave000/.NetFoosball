//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xively.NetMF
{
    
    internal partial class XivelyConfig
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((XivelyConfig.manager == null))
                {
                    XivelyConfig.manager = new System.Resources.ResourceManager("Xively.NetMF.XivelyConfig", typeof(XivelyConfig).Assembly);
                }
                return XivelyConfig.manager;
            }
        }
        internal static string GetString(XivelyConfig.StringResources id)
        {
            return ((string)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            ApiKey = -13171,
        }
    }
}