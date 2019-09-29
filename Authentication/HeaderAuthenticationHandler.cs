using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Concept.Authentication
{
    public class HeaderAuthenticationHandler : AuthenticationHandler<HeaderOptions>
    {
        public const string AuthorizationHeaderName = "Authorization";

        public HeaderAuthenticationHandler(IOptionsMonitor<HeaderOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(AuthorizationHeaderName))
                return NoResult();

            if (!Request.Headers.TryGetValue(AuthorizationHeaderName, out var authValue))
                return NoResult();

            if (!authValue.Equals(Options.Authorization))
                return Task.FromResult(AuthenticateResult.Fail("Invalid password"));

            var claims = new[] { new Claim(ClaimTypes.Name, "Authenticated") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private Task<AuthenticateResult> NoResult()
            => Task.FromResult(AuthenticateResult.NoResult());
    }
}