using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadLib
{
    /// <summary>
    /// ファイルアップロードサービス
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// ファイルアップロード
        /// </summary>
        /// <param name="filename">アップロードパス</param>
        /// <param name="s">ファイル内容</param>
        /// <returns></returns>
        Task UploadAsync(string filename, Stream s);

        /// <summary>
        /// ファイル削除
        /// </summary>
        /// <param name="filename">パス</param>
        /// <returns></returns>
        Task DeleteAsync(string filename);

        /// <summary>
        /// 指定パス下のフォルダ一覧取得
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="limit">取得最大数</param>
        /// <returns>フォルダ名のリスト</returns>
        Task<IEnumerable<string>> ListFoldersAsync(string path, int limit = -1);

        /// <summary>
        /// 指定パス下のファイル一覧取得
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>アイテム名のリスト</returns>
        Task<IEnumerable<string>> ListFilesAsync(string path, int limit = -1);
    }
}
