using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        // Uri オブジェクトを生成
        Uri fileUri = new Uri(filePath);
        return fileUri.AbsoluteUri;
    }

}

