using MicroElements.Functional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    internal static class Helper
    {
        #region Public Methods

        public static bool Matches(this string s, string pattern) => Regex.IsMatch(s, pattern);

        public static Result<T, Exception> Try<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                return Result.Fail(e);
            }
        }

        public static async Task<Result<T, Exception>> Try<T>(Func<Task<T>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                return Result.Fail(e);
            }
        }

        #endregion Public Methods
    }
}