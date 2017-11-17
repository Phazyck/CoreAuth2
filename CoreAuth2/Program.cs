using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace CoreAuth2
{
    public class Program
    {
        public static void Main(string[] args)
        {

            IWebHostBuilder builder =
                WebHost
                    .CreateDefaultBuilder(args)
                    .UseStartup<Startup>();

            string portString = Environment.GetEnvironmentVariable("LOCALHOST_PORT");
            string subject = Environment.GetEnvironmentVariable("LOCALHOST_SUBJECT");
            if (int.TryParse(portString, out int port) && !string.IsNullOrEmpty(subject))
            {
                X509Certificate2 certificate = GetServiceCertificate(subject);

                builder = builder.UseKestrel(options =>
                 {
                     options.Listen(new IPEndPoint(IPAddress.Loopback, port), listenOptions =>
                     {
                         var httpsConnectionAdapterOptions = new HttpsConnectionAdapterOptions()
                         {
                             ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                             SslProtocols = System.Security.Authentication.SslProtocols.Tls,
                             ServerCertificate = certificate
                         };
                         listenOptions.UseHttps(httpsConnectionAdapterOptions);
                     });
                 });
            }

            IWebHost host = builder.Build();
            host.Run();
        }

        private static X509Certificate2 GetServiceCertificate(string subjectName)
        {
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            var certCollection = certStore.Certificates.Find(
                                       X509FindType.FindBySubjectDistinguishedName, subjectName, false);
            // Get the first certificate
            X509Certificate2 certificate = null;
            if (certCollection.Count > 0)
            {
                certificate = certCollection[0];
            }
            certStore.Close();
            return certificate;
        }
    }
}
