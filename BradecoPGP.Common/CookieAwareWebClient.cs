using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BradescoPGP.Common
{
    public class CookieAwareWebClient : WebClient
    {
        public const String DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";

        public CookieAwareWebClient()
        {
            CookieContainer = new CookieContainer();
        }

        public CookieAwareWebClient(String user, String password)
        {
            Credential = new NetworkCredential(user, password);

            CookieContainer = new CookieContainer();
        }

        public CookieContainer CookieContainer { get; private set; }

        public NetworkCredential Credential { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);

            request.CookieContainer = CookieContainer;

            if (Credential != null)
            {
                var credentialCache = new CredentialCache();
                credentialCache.Add(address, "Basic", Credential);
                request.Credentials = credentialCache;
                request.PreAuthenticate = true;
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            }

            return request;
        }

        public void AddBaseHeaders()
        {
            this.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            this.Headers[HttpRequestHeader.AcceptLanguage] = "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7";
            this.Headers[HttpRequestHeader.Upgrade] = "1";
            this.Headers[HttpRequestHeader.UserAgent] = DefaultUserAgent;
        }

        public static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode. // ... Then create a buffer and write into while reading from the GZIP stream.
            using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);

                        if (count > 0)
                            memory.Write(buffer, 0, count);

                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

    }
}