using System;
using System.Configuration;

namespace SolutionFactory
{
    /// <summary>
    /// This class was generated by the AppSettings T4 template
    /// </summary>
    public static class AppSettings
    {
        public static string SolutionFactoryKey { get { return GetConfigSettingItem("SolutionFactoryKey"); } }
        public static string SolutionFactoryFriendlyKey { get { return GetConfigSettingItem("SolutionFactoryFriendlyKey"); } }
        public static string FileExtensionsToEdit { get { return GetConfigSettingItem("FileExtensionsToEdit"); } }

        private const string MISSING_CONFIG = "Invalid configuration. Required AppSettings section is missing";
        private const string INVALID_CONFIG_SETTING = "Invalid configuration setting name: {0}";

        private static string GetConfigSettingItem(string name)
        {
            if (ConfigurationManager.AppSettings == null)
                throw new ConfigurationErrorsException(MISSING_CONFIG);

            string value = null;
            if (ConfigurationManager.AppSettings.Count != 0)
            {
                try
                {
                    value = ConfigurationManager.AppSettings.Get(name);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(SettingItemErrorMessage(name, exception));
                }
            }
            return value;
        }

        private static string SettingItemErrorMessage(string name)
        {
            return string.Format(INVALID_CONFIG_SETTING, name);
        }

        private static string SettingItemErrorMessage(string name, Exception exception)
        {
            return string.Format(INVALID_CONFIG_SETTING, name) + exception.Message;
        }
    }
}