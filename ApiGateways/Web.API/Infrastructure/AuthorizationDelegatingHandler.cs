using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EmpowerBlog.Web.API.Infrastructure
{
    public class AuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly HttpContext httpContext;

        public AuthorizationDelegatingHandler(IHttpContextAccessor contextAccessor/*, ITokenAcquisition tokenAcquisition*/)
        {
            this.httpContext = contextAccessor?.HttpContext ?? throw new ArgumentNullException();
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // TODO : Complete downstream token generation
            //var downstreamToken = await tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "access_as_user" }, tenantId: "95090b01-7b00-476f-8a70-c6820af4a2d1");
            var authorizationHeader = httpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }

            var token = await httpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
