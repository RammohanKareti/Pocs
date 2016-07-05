using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;

namespace WebApiSampleLikeStripe.Filters
{
    public class AuthorizationHandler : DelegatingHandler
    {
        private const string AuthorizationKey = "Authorization";

        private const int Key_Size = 32;

        private const string SecretKey_Appender = "sk_";

        private const string Publishable_Appender = "pk_";

        private const string TestKeyType = "test_";

        private const string LiveKeyType = "live_";

        private const string BearerAuthScheme = "Bearer";


        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var route = request.GetRouteData();

            var controller = Convert.ToString(route.Values["controller"]);

            if (controller == "SecretKeys")
            {
                return base.SendAsync(request, cancellationToken);
            }
            if (!IsValidToken(request))
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                var taskCompletion = new TaskCompletionSource<HttpResponseMessage>();
                taskCompletion.SetResult(response);
                return taskCompletion.Task;
            }
            return base.SendAsync(request, cancellationToken);
        }

        private bool IsValidToken(HttpRequestMessage request)
        {
            var authrorizationHeader = request.Headers.Authorization;
            if (authrorizationHeader == null || authrorizationHeader.Scheme != BearerAuthScheme || string.IsNullOrEmpty(authrorizationHeader.Parameter))
            {
                return false;
            }
            if (authrorizationHeader.Parameter.Length != Key_Size)
            {
                return false;
            }
            string userId;
            var isValid = ValidateToken(authrorizationHeader.Parameter, out userId);
            return isValid;

        }

        private bool ValidateToken(string key, out string userId)
        {
            try
            {
                bool isTest;
                bool isSecret;
                var token = ParseToken(key, out isTest, out isSecret);
                using (var db = new DBEntities())
                {
                    if (isSecret)
                    {
                        userId = db.SecretKeys.Where(k => k.Value == token && k.IsTest == isTest && !k.IsRevoked).Select(k => k.UserId).First();
                    }
                    else
                    {
                        userId = db.PublishableKeys.Where(pk => pk.Value == token && pk.IsTest == isTest && !pk.IsRevoked).Select(pk => pk.UserId).First();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                userId = string.Empty;
                return false;
            }
        }

        private string ParseToken(string key, out bool isTest, out bool isSecret)
        {
            isSecret = key.StartsWith(SecretKey_Appender);
            var keyWithMode = key.Substring((isSecret ? SecretKey_Appender : Publishable_Appender).Length);
            isTest = keyWithMode.StartsWith(TestKeyType);
            return keyWithMode.Substring((isTest ? TestKeyType : LiveKeyType).Length);
        }
    }
}