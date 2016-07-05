using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiSampleLikeStripe.Helpers;

namespace WebApiSampleLikeStripe.Controllers
{
    public class SecretKeysController : ApiController
    {
        public string Get(string id)
        {
            return KeyGenerator.GenerateSecretKey(id, true);
        }
    }
}
