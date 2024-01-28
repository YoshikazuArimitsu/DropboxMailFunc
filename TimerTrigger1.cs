using System;
using DropboxMail;
using FileUploadLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DropboxMailFunc
{
    public class TimerTrigger1
    {
        private readonly ILogger _logger;
        private readonly IDropboxMailService _dropboxMail;

        public TimerTrigger1(ILoggerFactory loggerFactory, IDropboxMailService dropboxMail)
        {
            _logger = loggerFactory.CreateLogger<TimerTrigger1>();
            _dropboxMail = dropboxMail;
        }

        [Function("TimerTrigger1")]
        public void Run([TimerTrigger("0 */3 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            _dropboxMail.RunAsync().Wait();
        }

    }
}
