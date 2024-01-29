using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace SendMailLib
{
    /// <summary>
    /// SMTP設定
    /// </summary>
    public class SmtpConfig
    {
        /// <summary>
        /// SMTPサーバ
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// SMTPポート
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// SSL使用接続(一般的にはポート465使用時のみtrue)
        /// </summary>
        public bool UseSSL { get; set; } = false;

        /// <summary>
        /// ユーザ名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// リトライ回数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// リトライ間隔(秒)
        /// </summary>
        public int RetryInterval { get; set; }

        /// <summary>
        /// 送信者名
        /// </summary>
        public string SenderName { get; set; } = "ScanPod（郵便室）";

    }

    public class SendMailSmtpService : ISendMailService
    {
        #region properties
        private readonly ILogger<SendMailSmtpService> _Logger;
        private readonly SmtpConfig _SmtpConfig;

        private SmtpClient _SmtpClient = new SmtpClient();

        private SmtpClient SmtpClient
        {
            get
            {
                if (_SmtpClient == null)
                {
                    _SmtpClient = new SmtpClient();
                }
                return _SmtpClient;
            }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="config">SendGrid設定</param>
        // public SendMailSmtpService(ILogger<SendMailSmtpService> logger, SmtpConfig config)
        // {
        //     _Logger = logger;
        //     _SmtpConfig = config;
        // }
        public SendMailSmtpService(ILogger<SendMailSmtpService> logger, IConfiguration config)
        {
            _Logger = logger;
            _SmtpConfig = config.GetSection("Smtp").Get<SmtpConfig>() ??
                new SmtpConfig();
        }
        #endregion

        /// <summary>
        /// テストメール送信
        /// </summary>
        /// <param name="to">送信先メールアドレス</param>
        /// <returns></returns>
        public async Task SendTestAsync(string to)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("From", "yarimit.duon@gmail.com"));
            message.To.Add(new MailboxAddress("To", to));
            message.Subject = "Test mail";
            message.Body = new TextPart("plain")
            {
                Text = @"Hello, This is the test"
            };
            await SendMessageAsync(message);
        }

        /// <summary>
        /// メッセージ送信
        /// </summary>
        /// <param name="message">送信メール</param>
        /// <returns></returns>
        private async Task sendMailAsync(MimeMessage message)
        {
            await LoginAsync();

            try
            {
                /* From書き換え */
                message.From.Clear();
                message.From.Add(new MailboxAddress(_SmtpConfig.SenderName, _SmtpConfig.Username));

                _Logger.LogInformation($"メールを送信開始します。 (To:{message.To}, Subject:{message.Subject})");
                await SmtpClient.SendAsync(message);
            }
            finally
            {
                await SmtpClient.DisconnectAsync(true);
            }

        }

        /// <summary>
        /// リトライ付きメッセージ送信
        /// </summary>
        /// <param name="message">送信メール</param>
        /// <returns></returns>
        public async Task SendMessageAsync(MimeMessage message)
        {
            if (message == null)
            {
                // 送信メッセージが無い場合
                _Logger.LogDebug($"message is null, skip.");
                return;
            }

            Exception lastException = new InvalidProgramException();
            for (int retry = _SmtpConfig.RetryCount; retry >= 0; retry--)
            {
                try
                {
                    await sendMailAsync(message);
                    return;
                }
                catch (AuthenticationException)
                {
                    // 認証エラーは上位に流す
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                // リトライ待機処理
                if (retry > 0)
                {
                    _Logger.LogWarning($"メール送信失敗・リトライ待機開始。({lastException.Message})");
                    await Task.Delay(_SmtpConfig.RetryInterval * 1000);
                }
            }

            _Logger.LogError($"メール送信に失敗しました。({lastException.Message})");
            throw lastException;
        }


        public async Task LoginAsync()
        {
            if (SmtpClient.IsConnected == false)
            {
                await SmtpClient.ConnectAsync(_SmtpConfig.Host, _SmtpConfig.Port, _SmtpConfig.UseSSL);

                try
                {
                    await SmtpClient.AuthenticateAsync(_SmtpConfig.Username, _SmtpConfig.Password);
                }
                catch (AuthenticationException ex)
                {
                    _Logger.LogCritical($"SMTPサーバの認証エラーが発生しました。({ex.Message})");
                    throw;
                }
            }
        }
    }
}
