using System;
using Light.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace Synnotech.Xunit;

/// <summary>
/// <para>
/// Provides an ambient context for test settings. This class loads up to three
/// optional configuration files ("testsettings.json", "testsettings.Development.json",
/// and "testsettings.Build.json") as well as environment variables and
/// provides the <see cref="Configuration" /> property  to access them
/// (if you don't want to use the default settings, you can omit the
/// <see cref="Configuration" /> property and instead call <see cref="LoadConfiguration" />.
/// The files are searched for in the current working directory - thus you
/// should copy the JSON files to the output directory as part of your build.
/// </para>
/// <para>
/// All of the sources are optional and can be configured
/// by supplying a dedicated section in at least one
/// of the JSON files:
/// </para>
/// <code>
/// {
///     "testConfiguration": {
///         "isInBuildServerModeEnvironmentVariableName": "CI",
///         "loadEnvironmentVariables": true,
///         "environmentVariablesPrefix": "SynnotechTest_",
///         "loadDevelopmentSettingsFileInBuildServerMode": false
///     }
/// }
/// </code>
/// <para>
/// The three files are treated in the following way:
/// </para>
/// <list type="bullet">
/// <item>
/// <term>testsettings.json</term>
/// <description>
/// This is the standard file which contains all configuration sections and values. It allows developers
/// to easily discover the available settings. All integration tests (i.e. tests that require a third-party
/// system like a database, web service, etc.) should be deactivated by default to allow simply cloning
/// of the repo on a dev machine and successfully running all tests (skipping integration tests).
/// </description>
/// </item>
/// <item>
/// <term>testsettings.Development.json</term>
/// <description>
/// This file allows developers to set specific settings to different values on his or her dev machine. It is
/// not checked into the repository. By default, this file is not loaded when Build Server Mode is active,
/// unless you set "loadDevelopmentSettingsFileInBuildServerMode" to true.
/// </description>
/// </item>
/// <item>
/// <term>testsettings.Build.json</term>
/// <description>
/// This file should be used on build servers. It is only loaded when the
/// <see cref="LoadConfiguration" /> method determines that it runs in Build Server mode.
/// This file can be checked into the repository for non-sensitive settings.
/// </description>
/// </item>
/// </list>
/// </summary>
public static class TestSettings
{
    private static readonly Lazy<IConfiguration> LazyConfiguration = new (() => LoadConfiguration());

    /// <summary>
    /// <para>
    /// Gets the default configuration that was loaded from the optional files "testsettings.json",
    /// "testsettings.Development.json", and "testsettings.Build.json", as well as environment variables.
    /// Please see the XML comment on <see cref="TestSettings" /> on details about the default configuration.
    /// </para>
    /// <para>
    /// The field behind this property is lazily loaded using Lazy&lt;T&gt; with default settings.
    /// You can also load an <see cref="IConfiguration" /> instance by yourself if you call the
    /// <see cref="LoadConfiguration" /> method.
    /// </para>
    /// </summary>
    public static IConfiguration Configuration => LazyConfiguration.Value;

    /// <summary>
    /// <para>
    /// Loads an <see cref="IConfiguration" /> object from three files testsettings.json, testsettings.Build.json,
    /// and testsettings.Development.json, and from environment variables. All of the sources are optional and
    /// can be configured either via the parameters of this method, or by supplying a dedicated section in at least one
    /// of the JSON files:
    /// </para>
    /// <code>
    /// {
    ///     "testConfiguration": {
    ///         "isInBuildServerModeEnvironmentVariableName": "CI",
    ///         "loadEnvironmentVariables": true,
    ///         "environmentVariablesPrefix": "SynnotechTest_",
    ///         "loadDevelopmentSettingsFileInBuildServerMode": false
    ///     }
    /// }
    /// </code>
    /// <para>
    /// Please refer to the corresponding parameters with the same name to check their meaning.
    /// </para>
    /// <para>
    /// The three files are treated in the following way:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>testsettings.json</term>
    /// <description>
    /// This is the standard file which contains all configuration sections and values. It allows developers
    /// to easily discover the available settings. All integration tests (i.e. tests that require a third-party
    /// system like a database, web service, etc.) should be deactivated by default to allow simply cloning
    /// of the repo on a dev machine and successfully running all tests (skipping integration tests).
    /// </description>
    /// </item>
    /// <item>
    /// <term>testsettings.Development.json</term>
    /// <description>
    /// This file allows developers to set specific settings to different values on his or her dev machine. It is
    /// not checked into the repository. By default, this file is not loaded when Build Server Mode is active,
    /// unless you set <paramref name="loadDevelopmentSettingsFileInBuildServerMode" /> to true.
    /// </description>
    /// </item>
    /// <item>
    /// <term>testsettings.Build.json</term>
    /// <description>
    /// This file should be used on build servers. It is only loaded when the
    /// <see cref="LoadConfiguration" /> method determines that it runs in Build Server mode.
    /// This file can be checked into the repository for non-sensitive settings.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="testsettingsFileName">
    /// The name of the default test settings file ("testsettings.json").
    /// </param>
    /// <param name="developmentFileName">The name of the local development file (testsettings.Development.json").</param>
    /// <param name="buildServerFileName">The name of the build server file (testsettings.Build.json).</param>
    /// <param name="isInBuildServerMode">
    /// The value indicating whether Build Server mode is turned on. If this value is set to null,
    /// the value will be determined via <paramref name="isInBuildServerModeEnvironmentVariableName" />.
    /// </param>
    /// <param name="isInBuildServerModeEnvironmentVariableName">
    /// The name of the environment variable that indicates whether Build Server mode is active.
    /// The value of this environment variable must be either "True" or "1" for Build Server mode to
    /// be activated.
    /// </param>
    /// <param name="loadEnvironmentVariables">
    /// The value indicating whether the returned <see cref="IConfiguration" /> object will also contain
    /// values from environment variables. If this value is set null, this method will try to obtain the
    /// value from one of the JSON files.
    /// </param>
    /// <param name="environmentVariablesPrefix">
    /// The prefix that environment variable names must start with. The prefix will be removed from the environment variable is
    /// added to the <see cref="IConfiguration" /> instance.
    /// </param>
    /// <param name="loadDevelopmentSettingsFileInBuildServerMode">
    /// The value indicating whether the development settings file will also be loaded in Build Server mode. By default,
    /// only the testsettings.json and testsettings.Build.json files are loaded in this mode. If this value is set
    /// to null, this method will try to retrieve it from one of the JSON files.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either <paramref name="testsettingsFileName" />, or <paramref name="developmentFileName" />,
    /// or <paramref name="buildServerFileName" /> are null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when either <paramref name="testsettingsFileName" />, or <paramref name="developmentFileName" />,
    /// or <paramref name="buildServerFileName" /> are empty or contain only white space.
    /// </exception>
    public static IConfigurationRoot LoadConfiguration(string testsettingsFileName = "testsettings.json",
                                                       string developmentFileName = "testsettings.Development.json",
                                                       string buildServerFileName = "testsettings.Build.json",
                                                       bool? isInBuildServerMode = null,
                                                       string isInBuildServerModeEnvironmentVariableName = "",
                                                       bool? loadEnvironmentVariables = null,
                                                       string environmentVariablesPrefix = "",
                                                       bool? loadDevelopmentSettingsFileInBuildServerMode = null)
    {
        testsettingsFileName.MustNotBeNullOrWhiteSpace();
        developmentFileName.MustNotBeNullOrWhiteSpace();
        buildServerFileName.MustNotBeNullOrWhiteSpace();

        var firstConfiguration = new ConfigurationBuilder().AddJsonFile(testsettingsFileName, true)
                                                           .AddJsonFile(developmentFileName, true)
                                                           .AddJsonFile(buildServerFileName, true)
                                                           .Build();


        var testConfiguration = new TestConfiguration();
        firstConfiguration.GetSection("testConfiguration")
                          .Bind(testConfiguration);

        if (loadEnvironmentVariables == true)
            testConfiguration.LoadEnvironmentVariables = true;
        if (!environmentVariablesPrefix.IsNullOrWhiteSpace())
            testConfiguration.EnvironmentVariablesPrefix = environmentVariablesPrefix;
        if (!isInBuildServerModeEnvironmentVariableName.IsNullOrWhiteSpace())
            testConfiguration.IsInBuildServerModeEnvironmentVariableName = isInBuildServerModeEnvironmentVariableName;

        return new ConfigurationBuilder().AddJsonFile(testsettingsFileName, true)
                                         .AddFurtherConfigurationFiles(testConfiguration,
                                                                       isInBuildServerMode,
                                                                       loadDevelopmentSettingsFileInBuildServerMode,
                                                                       developmentFileName,
                                                                       buildServerFileName)
                                         .AddEnvironmentVariablesIfNecessary(testConfiguration)
                                         .Build();
    }

    private static IConfigurationBuilder AddFurtherConfigurationFiles(this IConfigurationBuilder configurationBuilder,
                                                                      TestConfiguration testConfiguration,
                                                                      bool? isBuildServerMode,
                                                                      bool? loadDevelopmentSettingsFileInBuildServerMode,
                                                                      string developmentFileName,
                                                                      string buildServerFileName)
    {
        if (isBuildServerMode == true || testConfiguration.IsInBuildServerMode)
        {
            configurationBuilder.AddJsonFile(buildServerFileName, true);
            if (loadDevelopmentSettingsFileInBuildServerMode == true || testConfiguration.LoadDevelopmentSettingsFileInBuildServerMode)
                configurationBuilder.AddJsonFile(developmentFileName, true);

            return configurationBuilder;
        }

        configurationBuilder.AddJsonFile(developmentFileName, true);

        return configurationBuilder;
    }

    private static IConfigurationBuilder AddEnvironmentVariablesIfNecessary(this IConfigurationBuilder configurationBuilder,
                                                                            TestConfiguration testConfiguration)
    {
        if (!testConfiguration.LoadEnvironmentVariables)
            return configurationBuilder;

        return testConfiguration.EnvironmentVariablesPrefix.IsNullOrWhiteSpace() ?
            configurationBuilder.AddEnvironmentVariables() :
            configurationBuilder.AddEnvironmentVariables(testConfiguration.EnvironmentVariablesPrefix);
    }
}