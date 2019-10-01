using System;
using System.Configuration;
using System.Globalization;

namespace PowerTradingOverview.Host
{
    public class ReportOptions
    {
        public ReportOptions(string timeZone, string directory, string filePattern, TimeSpan interval)
        {
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            FilePattern = filePattern ?? throw new ArgumentNullException(nameof(filePattern));
            Interval = interval;
        }

        public static ReportOptions ReadFromConfig()
        {
            return new ReportOptions(
                ConfigurationManager.AppSettings["ReportTimeZone"],
                ConfigurationManager.AppSettings["ReportDirectory"],
                ConfigurationManager.AppSettings["ReportFilePattern"],
                TimeSpan.Parse(ConfigurationManager.AppSettings["ReportInterval"], CultureInfo.InvariantCulture));            
        }

        public static void WriteToConfig(ReportOptions options, string configFileName)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(configFileName);
            var settings = configFile.AppSettings.Settings;

            settings["ReportTimeZone"].Value = options.TimeZone;
            settings["ReportDirectory"].Value = options.Directory;
            settings["ReportFilePattern"].Value = options.FilePattern;
            settings["ReportInterval"].Value = options.Interval.ToString();

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public string TimeZone { get; private set; }
        public string Directory { get; private set; }
        public string FilePattern { get; private set; }
        public TimeSpan Interval { get; private set; }
    }
}
