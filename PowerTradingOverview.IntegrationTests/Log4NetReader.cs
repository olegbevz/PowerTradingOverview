using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PowerTradingOverview.IntegrationTests
{
    public class Log4NetReader : IDisposable
    {
        private static readonly Regex _errorRegex = new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3} (\[\d+\])? ERROR (.+)$");

        private readonly StreamReader _streamReader;
        private bool _dispose;

        public Log4NetReader(string fileName)
        {
            _streamReader = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            _dispose = true;
        }

        public Log4NetReader(StreamReader streamReader)
        {
            _streamReader = streamReader;
        }

        public void Dispose()
        {
            if (_dispose)
                _streamReader.Dispose();
        }

        public IEnumerable<string> GetErrors()
        {
            while (!_streamReader.EndOfStream)
            {
                var line = _streamReader.ReadLine();
                var match = _errorRegex.Match(line);
                if (match.Success)
                {
                    if (match.Groups.Count > 0)
                        yield return match.Groups[match.Groups.Count - 1].ToString();
                    else
                        yield return match.ToString();
                }                    
            }
        }
    }
}
