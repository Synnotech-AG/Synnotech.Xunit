using System;
using Microsoft.Extensions.Configuration;

namespace Synnotech.Xunit
{
    /// <summary>
    /// Provides an ambient context for test settings. This class loads up to three
    /// optional configuration files ("testsettings.json", "testsettings.Development.json",
    /// and "testsettings.Build.json") and provides the <see cref="Configuration"/> property
    /// to access them. The files are searched for in the current working directory - thus you
    /// should copy these files to the output directory as part of your build.
    /// </summary>
    public static class TestSettings
    {
        private static readonly Lazy<IConfiguration> LazyConfiguration = new (LoadConfiguration);

        /// <summary>
        /// Gets the configuration that was loaded from the optional files "testsettings.json",
        /// "testsettings.Development.json", and "testsettings.Build.json".
        /// </summary>
        public static IConfiguration Configuration => LazyConfiguration.Value;

        private static IConfiguration LoadConfiguration() =>
            new ConfigurationBuilder().AddJsonFile("testsettings.json", true)
                                      .AddJsonFile("testsettings.Development.json", true)
                                      .AddJsonFile("testsettings.Build.json", true)
                                      .Build();
    }
}