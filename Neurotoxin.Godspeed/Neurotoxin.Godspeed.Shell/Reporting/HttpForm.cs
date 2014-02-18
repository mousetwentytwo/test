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
            try
            {
                using (var client = new WebClient())
                {
                    byte[] body;
                    using (var ms = new MemoryStream())
                    {
                        var sw = new StreamWriter(ms);
                        foreach (var data in formData)
                        {
                            sw.WriteLine("--" + Boundary);
                            data.Write(sw);
                        }
                        sw.WriteLine("--" + Boundary + "--");
                        sw.Flush();
                        body = ms.ToArray();
                    }

                    client.Headers[HttpRequestHeader.UserAgent] = "GODspeed";
                    client.Headers[HttpRequestHeader.ContentType] = "multipart/form-data; boundary=" + Boundary;
                    client.UploadData(new Uri(url), body);
                }
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