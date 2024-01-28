using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using FileUploadLib;
using FileUploadLib.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendMailLib;

namespace DropboxMail
{

    /// <summary>
    /// ファイルアップロードサービス(Dropbox実装)
    /// </summary>
    public class DropboxMailService : IDropboxMailService
    {
        #region properties
        private readonly ILogger<DropboxMailService> _Logger;
        private readonly IFileUploadService _FileUpload;
        private readonly ISendMailService _SendMail;

        #endregion

        #region .ctor
        public DropboxMailService(ILogger<DropboxMailService> logger,
        IFileUploadService fileUpload,
        ISendMailService sendMail)
        {
            _Logger = logger;
            _FileUpload = fileUpload;
            _SendMail = sendMail;
        }
        #endregion

        public Task RunAsync() {
            return Task.CompletedTask;
        }

    }
}
