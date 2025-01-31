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

    const int CLR_INVALID = -1;

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetBkColor(IntPtr hdc);

    [DllImport("user32.dll")]
    private static extern void ReleaseDC(IntPtr hWnd, IntPtr hdc);

    public static String GetWindowBackgroundColor(IntPtr hWnd)
    {
        Color backgroundColor = Color.Empty;

        IntPtr hdc = GetDC(hWnd);
        if (hdc == IntPtr.Zero)
        {
            Console.WriteLine("Failed to get DC");
            return null;
        }

        int colorRef = GetBkColor(hdc);
        ReleaseDC(hWnd, hdc);

        String colorRGB = "";

        if (colorRef != CLR_INVALID)
        {
            // COLORREFはBGR形式で返されるため、変換する
            byte blue = (byte)(colorRef & 0xFF);
            byte green = (byte)((colorRef >> 8) & 0xFF);
            byte red = (byte)((colorRef >> 16) & 0xFF);
            //  backgroundColor = Color.FromArgb(red, green, blue);

            colorRGB = $"#{red:X2}{green:X2}{blue:X2}";

        }

        return colorRGB;
    }

}
