using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Net.Http;

namespace SOMIOD.Utils
{
    public class RequestData
    {
        public static byte[] getRequestContenta(HttpRequestMessage request) {
            using (Stream stream = request.Content.ReadAsStreamAsync().Result)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}