using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public class WebLocator : IUriLocator
    {
        #region Public Fields

        public static readonly WebLocator Instance = new WebLocator();

        #endregion Public Fields

        #region Public Methods

        public async Task<Stream> Locate(Uri uri)
        {
            var request = WebRequest.CreateHttp(uri);
            var response = await request.GetResponseAsync();
            return response.GetResponseStream();
        }

        #endregion Public Methods
    }
}