using System;

namespace HmGitWatcher;


public partial class HmGitWatcher
{
    public string ConvertToUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        // Uri オブジェクトを生成
        Uri fileUri = new Uri(filePath);
        return fileUri.AbsoluteUri;
    }

}

