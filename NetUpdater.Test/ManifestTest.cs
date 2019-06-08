using FluentAssertions;
using NetUpdater.Core;
using NUnit.Framework;
using System;

namespace NetUpdater.Test
{
    public class ManifestTest
    {
        #region Private Fields

        private Manifest anyManifest;
        private Manifest newerManifest;
        private Manifest olderManifest;

        #endregion Private Fields

        #region Public Methods

        [Test]
        public void CreateDiff_WithNewerManifest_ReturnsDiffWithUpdates()
        {
            var anyManifest = olderManifest;

            var result = anyManifest.CreateDiff(newerManifest);

            result.Updates.Should().Contain(new Uri("qwer", UriKind.Relative));
            result.Deletions.Should().BeEmpty();
        }

        [Test]
        public void CreateDiff_WithOlderManifest_ReturnsDiffWithDeletions()
        {
            var anyManifest = newerManifest;

            var result = anyManifest.CreateDiff(olderManifest);

            result.Deletions.Should().Contain(new Uri("qwer", UriKind.Relative));
            result.Updates.Should().BeEmpty();
        }

        [Test]
        public void CreateDiff_WithSameManifest_ReturnsEmptyDiff()
        {
            var sameManifest = anyManifest;

            var result = anyManifest.CreateDiff(sameManifest);

            result.Updates.Should().BeEmpty();
            result.Deletions.Should().BeEmpty();
        }

        [SetUp]
        public void Setup()
        {
            olderManifest = new Manifest("-", new Version(1, 0), new[]
            {
                ("asdf", new Uri("asdf", UriKind.Relative)),
            });
            newerManifest = new Manifest("-", new Version(2, 0), new[]
            {
                ("asdf", new Uri("asdf", UriKind.Relative)),
                ("qwer", new Uri("qwer", UriKind.Relative)),
            });
            anyManifest = olderManifest;
        }

        #endregion Public Methods
    }
}