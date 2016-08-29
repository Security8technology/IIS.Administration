// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Tests
{
    using Microsoft.IIS.Administration.Core.Http;
    using System.Net.Http;

    public class ApiHttpClient : HttpClient
    {
        private string _keyId;
        private string _serverUri;

        public static HttpClient Create(string serverUri)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => {
                return true;
            };

            return new ApiHttpClient(serverUri, handler, true);
        }

        private ApiHttpClient(string serverUri, HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
            Init(serverUri);
        }

        private void Init(string serverUri)
        {

            var key = Utils.GetApiKey(serverUri, this);

            _keyId = key.Value<string>("id");
            _serverUri = serverUri;

            this.DefaultRequestHeaders.Add(HeaderNames.Access_Token, "Bearer " + key.Value<string>("access_token"));
            this.DefaultRequestHeaders.Add("Accept", "application/hal+json");
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.DefaultRequestHeaders.Clear();
                Utils.DeleteApiKey(_serverUri, _keyId, this);
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
