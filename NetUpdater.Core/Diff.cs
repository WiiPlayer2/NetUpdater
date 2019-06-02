using System;
using System.Collections.Generic;
using System.Linq;

namespace NetUpdater.Core
{
    public class Diff
    {
        #region Public Constructors

        public Diff(IEnumerable<Uri> updates, IEnumerable<Uri> deletions)
        {
            Updates = updates.ToList();
            Deletions = deletions.ToList();
        }

        #endregion Public Constructors

        #region Public Properties

        public IEnumerable<Uri> Deletions { get; }

        public IEnumerable<Uri> Updates { get; }

        #endregion Public Properties
    }
}