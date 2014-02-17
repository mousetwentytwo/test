using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Neurotoxin.Godspeed.Shell.Interfaces;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public static class HttpForm
    {
        private const string Boundary = "----GODSpeedFormBoundary";

        public static void Post(string url, IEnumerable<IFormData> formData)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "GODspeed";
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + Boundary;

            try
            {
                var requestStream = request.GetRequestStream();
                var sw = new StreamWriter(requestStream);
                foreach (var data in formData)
                {
                    sw.WriteLine("--" + Boundary);
                    data.Write(sw);
                }
                sw.WriteLine("--" + Boundary + "--");
                sw.Flush();
                request.GetResponse();
            }
            catch
            {

            }
        }

        public static void Post(string url, IDictionary<string, string> formData)
        {
            Post(url, formData.Select(kvp => new RawPostData(kvp.Key, kvp.Value)));
        }
    }
}