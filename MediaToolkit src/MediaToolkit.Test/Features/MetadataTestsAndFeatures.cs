using System;
using System.Linq;
using MediaToolkit.Features;
using MediaToolkit.Util;
using NUnit.Framework;

namespace MediaToolkit.Test.Features
{
    [TestFixture]
    public class MetadataTestsAndFeatures
    {
        [Test]
        public void Init()
        {
            MetadataProvider provider = new MetadataProvider();
            var meta = provider.GetMetadata(
                @"../../TestVideo/BigBunny.m4v");

            meta.MetadataIndex.Select(kvp => "{0}:{1}".FormatInvariant(kvp.Key, kvp.Value) )
                              .ToList()
                              .ForEach(Console.WriteLine);
        }


    }
}