using System.Threading.Tasks;
using MimeKit;

namespace SendMailLib
{
    /// <summary>
    /// メール送信インタフェース
    /// </summary>
    public interface ISendMailService
    {
        /// <summary>
        /// ログインテスト
        /// </summary>
        /// <returns></returns>
        Task LoginAsync();

        /// <summary>
        /// テストメール送信
        /// </summary>
        /// <param name="to">送信先メールアドレス</param>
        /// <returns></returns>
        Task SendTestAsync(string to);

        /// <summary>
        /// メッセージ送信
        /// </summary>
        /// <param name="message">送信メール</param>
        /// <returns></returns>
        Task SendMessageAsync(MimeMessage message);
    }
}
