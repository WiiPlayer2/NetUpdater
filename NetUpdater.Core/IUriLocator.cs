using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    public interface IUriLocator
    {
        #region Public Methods

        Task<Stream> Locate(Uri uri);

        #endregion Public Methods
    }
}