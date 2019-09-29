using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Concept.Authentication
{
    public sealed class HeaderAuthenticationHandler : AuthenticationHandler<HeaderOptions>
    {
        public HeaderAuthenticationHandler(IOptionsMonitor<HeaderOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authValue))
                return
                    Task.FromResult(
                        AuthenticateResult.Fail(
                            "Please make sure you are using Authorization header in your request."));

            if (!authValue.Equals(Options.Authorization))
                return
                    Task.FromResult(
                        AuthenticateResult.Fail("Unable to authenticate. Provided password was wrong."));

            var claims = new[] {new Claim(ClaimTypes.Name, "Authenticated")};
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}