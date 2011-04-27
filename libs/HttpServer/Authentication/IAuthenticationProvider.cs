using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Authentication
{
    /// <summary>
    /// Used to authenticate users 
    /// </summary>
    /// <remarks>
    /// Authentication is requested by throwing 
    /// </remarks>
    public interface IAuthenticationProvider
    {
        void RequestAuthentication(IHttpContext context, string realm);
    }
}
