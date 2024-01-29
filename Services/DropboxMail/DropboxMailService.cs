using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Files.Routes;
using FileUploadLib;
using FileUploadLib.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SendMailLib;

namespace DropboxMail
{
    public class DropboxMailConfig
    {
        public string? To { get; set; }

    }


    public class DropboxMailService : IDropboxMailService
    {
        #region properties
        private readonly ILogger<DropboxMailService> _Logger;
        private readonly DropboxMailConfig _Config;
        private readonly IFileUploadService _FileUpload;
        private readonly ISendMailService _SendMail;


        #endregion

        #region .ctor
        public DropboxMailService(ILogger<DropboxMailService> logger,
        IConfiguration config,
        IFileUploadService fileUpload,
        ISendMailService sendMail)
        {
            _Logger = logger;
            _Config = config.GetSection("DropboxMail").Get<DropboxMailConfig>() ??
                new DropboxMailConfig();

            _FileUpload = fileUpload;
            _SendMail = sendMail;
        }
        #endregion

        public async Task RunAsync() {
            var files = await _FileUpload.ListFilesAsync("/");
            foreach (var file in files) {
                var ms = await _FileUpload.DownloadAsync(file);

                // メール送信
                var message = new MimeMessage();
                message.To.Add(InternetAddress.Parse(_Config.To));
                var builder = new BodyBuilder();
                builder.TextBody = "";
                builder.Attachments.Add(file, ms, ContentType.Parse("image/png"));
                message.Body = builder.ToMessageBody();

                await _SendMail.SendMessageAsync(message);

                // 削除
                await _FileUpload.DeleteAsync(file);
            }
        }

    }
}
