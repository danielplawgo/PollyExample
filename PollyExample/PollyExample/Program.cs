using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyExample
{
    class Program
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            _logger.Info("Start");
            try
            {
                var content = DownloadWithRetryForever("http://plawgo.pl");

                _logger.Info("Success");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Error");
            }
            _logger.Info("End");
        }
        static string Download(string url)
        {
            var client = new WebClient();

            return client.DownloadString(url);
        }

        static string DownloadWithRetry(string url)
        {
            var client = new WebClient();

            return Policy
                .Handle<WebException>()
                .Retry(3, (ex, retryCount) =>
                {
                    _logger.Error(ex, $"Error - try retry (count: {retryCount})");
                })
                .Execute(() => client.DownloadString(url));
        }

        static string DownloadWithRetryAndDelay(string url)
        {
            var client = new WebClient();

            return Policy
                .Handle<WebException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(50)
                }, (ex, timeSpan, retryCount, context) =>
                {
                    _logger.Error(ex, $"Error - try retry (count: {retryCount}, timeSpan: {timeSpan})");
                })
                .Execute(() => client.DownloadString(url));
        }

        static string DownloadWithRetryForever(string url)
        {
            var client = new WebClient();

            return Policy
                .Handle<WebException>()
                .RetryForever(ex =>
                {
                    _logger.Error(ex, $"Error - try retry forever");
                })
                .Execute(() => client.DownloadString(url));
        }
    }
}
