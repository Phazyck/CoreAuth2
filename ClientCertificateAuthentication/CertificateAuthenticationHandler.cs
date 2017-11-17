using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ClientCertificateAuthentication
{
    internal class CertificateAuthenticationHandler : AuthenticationHandler<CertficateAuthenticationOptions>
    {
        public CertificateAuthenticationHandler(IOptionsMonitor<CertficateAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, IDataProtectionProvider dataProtection, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            X509Certificate2 certificate = Context.Connection.ClientCertificate;
            
            if(certificate != null)
            {
                Context.Response.Headers["CLIENT-CERT-SOURCE"] = "Context.Connection.ClientCertificate";
            } 
            /*
            else if(certificate == null && Context.Request.Headers.TryGetValue("X-ARR-ClientCert", out StringValues certHeaders))
            {
                string certHeader = certHeaders.FirstOrDefault();
                byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                certificate = new X509Certificate2(clientCertBytes);
                Context.Response.Headers["CLIENT-CERT-SOURCE"] = "Header: X-ARR-ClientCert";
            }
            */
            else
            {
                Context.Response.Headers["CLIENT-CERT-SOURCE"] = "None";
            }
            
            if(certificate == null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            Context.Response.Headers["CLIENT-CERT-CONNECTION"] = Context.Request.IsHttps ? "HTTPS" : "HTTP";
            Context.Response.Headers["CLIENT-CERT-VERIFIED"] = certificate.Verify().ToString();

            /*
            if(!certificate.Verify())
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
            */

            string[] roles = GetRolesFromFirstMatchingCertificate(certificate);

            if (roles == null || roles.Length <= 0)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var userIdentity = new ClaimsIdentity(claims, Options.Challenge);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            var ticket = new AuthenticationTicket(userPrincipal, new AuthenticationProperties(), Options.Challenge);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private string[] GetRolesFromFirstMatchingCertificate(X509Certificate2 certificate)
        {
            var roles = (Options.CertificatesAndRoles
                .Where(r => r.Issuer == certificate.Issuer && r.Subject == certificate.Subject)
                .Select(r => r.Roles)).FirstOrDefault();

            return roles;
        }
    }
}
