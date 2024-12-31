using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmGitWatcher;


public partial class HmGitWatcher
{
    public string ConvertToUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        // パスがURL形式の場合はそのまま返す
        if (filePath.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
        {
            return filePath;
        }

        // パスが絶対パスでない場合は例外を投げる
        if (!System.IO.Path.IsPathRooted(filePath))
        {
            throw new ArgumentException("The provided path is not an absolute path.", nameof(filePath));
        }

        // Uri オブジェクトを生成
        Uri fileUri = new Uri(filePath);
        return fileUri.AbsoluteUri;
    }

}

