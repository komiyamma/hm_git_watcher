using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    private string ToHtmlStyleColor(Color color)
    {
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
    }
    public string ConvertSystemColorNameToRGB(string system_color_name)
    {
        if (system_color_name == "syswindow")
        {
            return ToHtmlStyleColor(SystemColors.Window);
        }
        else if (system_color_name == "syswindowtext")
        {
            return ToHtmlStyleColor(SystemColors.WindowText);
        }
        return system_color_name;
    }


}
