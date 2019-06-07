using MicroElements.Functional;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetUpdater.Core
{
    internal static class Helper
    {
        #region Private Fields

        private static readonly HashAlgorithm hashAlgorithm = SHA512.Create();

        #endregion Private Fields

        #region Public Methods

        public static string ComputeHash(this FileInfo fileInfo)
        {
            using (var fStream = fileInfo.OpenRead())
                return hashAlgorithm.ComputeHash(fStream).ToHex();
        }

        public static Result<A, Error> Flatten<A, Error>(this Result<Option<A>, Error> result, Func<Error> onNone)
        {
            return result.Bind(
                option => option.Match(
                    value => Result.Success(value),
                    () => (Result<A, Error>)Result.Fail(onNone())));
        }

        public static Task<Result<B, Error>> MapAsync<A, Error, B>(this Result<A, Error> result, Func<A, Task<B>> map)
                    => result.BindAsync(async value => Result.Success<B, Error>(await map(value)));

        public static bool Matches(this string s, string pattern) => Regex.IsMatch(s, pattern);

        public static string ToHex(this byte[] buffer) => string.Join(string.Empty, buffer.Select(o => o.ToString("X2", CultureInfo.InvariantCulture)));

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