namespace FileUploadLib.Utils
{
    /// <summary>
    /// ファイルサービス共通ユーティリティ
    /// </summary>
    public static class FileUploadUtils
    {
        /// <summary>
        /// ファイル名正規化
        /// 
        /// </summary>
        /// <remarks>
        /// * パス内の "//" を "/" に置換
        /// * "/" 一文字だけなら空白に変換
        /// した形式のパスに変換
        /// </remarks>
        /// <param name="path">パス</param>
        /// <returns>正規化後パス</returns>
        public static string NormalizePath(string path)
        {
            // 連続/除去
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }

            if (path == "/")
            {
                path = "";
            }
            return path;
        }

    }
}
