using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using FileUploadLib.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileUploadLib
{
    /// <summary>
    /// DropboxConfig設定
    /// </summary>
    public class DropboxConfig
    {
        /// <summary>
        /// アクセストークン
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// アップロード先パス('/{フォルダ名}')
        /// </summary>
        public string? Path { get; set; }

    }

    /// <summary>
    /// ファイルアップロードサービス(Dropbox実装)
    /// </summary>
    public class UploadDropboxService : IFileUploadService
    {
        #region properties
        private readonly ILogger<UploadDropboxService> _Logger;
        private readonly DropboxConfig _DropboxConfig;

        private DropboxClient? _Client;
        private DropboxClient Client
        {
            get
            {
                if (_Client == null)
                {
                    _Client = new DropboxClient(_DropboxConfig.AccessToken);
                }
                return _Client;
            }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logger">ロガー</param>
        // public UploadDropboxService(ILogger<UploadDropboxService> logger, DropboxConfig config)
        // {
        //     _Logger = logger;
        //     _DropboxConfig = config;
        // }
        public UploadDropboxService(ILogger<UploadDropboxService> logger, IConfiguration config)
        {
            _Logger = logger;
            _DropboxConfig = config.GetSection("Dropbox").Get<DropboxConfig>() ??
                new DropboxConfig();
        }
        #endregion

        /// <summary>
        /// ファイルアップロード
        /// </summary>
        /// <param name="filename">アップロードパス</param>
        /// <param name="s">ファイル内容</param>
        /// <returns></returns>
        public async Task UploadAsync(string filename, Stream s)
        {
            var path = $"{_DropboxConfig.Path}/{filename}";
            path = FileUploadUtils.NormalizePath(path);
            _Logger.LogInformation($"ファイルアップロードを実行します。 (path={path}, size={s.Length})");

            try
            {
                await Client.Files.UploadAsync(path, body: s);
            }
            catch (AuthException ex)
            {
                _Logger.LogCritical($"DropBox認証エラーによりファイルアップロードに失敗しました。 ({ex.Message})");
                throw;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"ファイルアップロードに失敗しました。 ({ex.Message})");
                throw;
            }
        }

        public async Task<Stream> DownloadAsync(string filename) {
            var path = $"{_DropboxConfig.Path}/{filename}";
            path = FileUploadUtils.NormalizePath(path);
            _Logger.LogInformation($"ファイルダウンロードを実行します。 (path={path})");

            try
            {
                using (var response = await Client.Files.DownloadAsync(path))
                {
                    var ms = new MemoryStream();
                    (await response.GetContentAsStreamAsync()).CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    return ms;
                }
            }
            catch (AuthException ex)
            {
                _Logger.LogCritical($"DropBox認証エラーによりファイルアップロードに失敗しました。 ({ex.Message})");
                throw;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"ファイルアップロードに失敗しました。 ({ex.Message})");
                throw;
            }

        }

        public async Task DeleteAsync(string filename)
        {
            var path = $"{_DropboxConfig.Path}/{filename}";
            path = FileUploadUtils.NormalizePath(path);
            _Logger.LogInformation($"ファイル削除を実行します。(path={path})");

            try
            {
                await Client.Files.DeleteV2Async(new DeleteArg(path));
            }
            catch (Exception ex)
            {
                _Logger.LogError($"ファイル削除に失敗しました。 ({ex.Message})");
                throw;
            }
        }


        private async Task<IEnumerable<string>> listItemsAsync(string path, Func<Metadata, bool> func, int limit)
        {
            var searchPath = $"{_DropboxConfig.Path}/{path}";
            searchPath = FileUploadUtils.NormalizePath(searchPath);

            var resultFolders = new List<string>();

            try
            {
                var results = await Client.Files.ListFolderAsync(
                    new Dropbox.Api.Files.ListFolderArg(searchPath));
                do
                {
                    // funcで与えられた条件にマッチするものの名前部分を抽出してリストに追加
                    var folderNames = results.Entries.Where(e => func(e)).Select(e => e.Name);
                    resultFolders.AddRange(folderNames);

                    if (limit > -1 && resultFolders.Count > limit)
                    {
                        break;
                    }

                    // 次ページが無ければ終了・あるなら次を取得して繰り返し。
                    if (results.HasMore == false)
                    {
                        break;
                    }

                    results = await Client.Files.ListFolderContinueAsync(new Dropbox.Api.Files.ListFolderContinueArg(results.Cursor));
                } while (true);

                string items = $"Items:{string.Join(",", resultFolders.Take(3))} ... ({resultFolders.Count} items)";
                _Logger.LogDebug($"ListItems searchPath={searchPath} {items}");
            }
            catch (AuthException ex)
            {
                _Logger.LogCritical($"DropBox認証エラーによりファイル/フォルダ一覧の取得に失敗しました。 ({ex.Message})");
                throw;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"ファイル/フォルダ一覧の取得に失敗しました。 (Path={searchPath}, {ex.Message})");
            }

            return resultFolders;
        }


        public async Task<IEnumerable<string>> ListFilesAsync(string path, int limit = -1)
        {
            Func<Metadata, bool> cond = (f) => f.IsFile;
            return await listItemsAsync(path, cond, limit);
        }


        public async Task<IEnumerable<string>> ListFoldersAsync(string path, int limit = -1)
        {
            Func<Metadata, bool> cond = (f) => f.IsFolder;
            return await listItemsAsync(path, cond, limit);
        }
    }
}
