using HnsExplorer.HostContainerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace HnsExplorer;

static class Program
{
    public static HnsAccess HnsAccess = new();
    public static HcsAccess HcsAccess = new();

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static readonly Color DARK_COLOR_FOREGROUND = Color.FromArgb(225, 225, 225);
    public static readonly Color DARK_COLOR_HIGHLIGHT_FOREGROUND = Color.White;
    public static readonly Color DARK_COLOR_HIGHLIGHT_BACKGROUND = SystemColors.Highlight;
    public static readonly Color DARK_COLOR_BACKGROUND_WINDOW = Color.FromArgb(61, 61, 61);
    public static readonly Color DARK_COLOR_BACKFGROUND_TEXTBOX = Color.FromArgb(31, 31, 31);

    public static readonly Color LIGHT_COLOR_FOREGROUND = Color.Black;
    public static readonly Color LIGHT_COLOR_HIGHLIGHT_FOREGROUND = Color.White;
    public static readonly Color LIGHT_COLOR_HIGHLIGHT_BACKGROUND = SystemColors.Highlight;
    public static readonly Color LIGHT_COLOR_BACKGROUND_WINDOW = Color.Gainsboro;
    public static readonly Color LIGHT_COLOR_BACKFGROUND_TEXTBOX = Color.WhiteSmoke;

    public static Color ACTIVE_COLOR_FOREGROUND;
    public static Color ACTIVE_COLOR_HIGHLIGHT_FOREGROUND;
    public static Color ACTIVE_COLOR_HIGHLIGHT_BACKGROUND;
    public static Color ACTIVE_COLOR_SEARCH_FOREGROUND;
    public static Color ACTIVE_COLOR_SEARCH_HIGHLIGHT;
    public static Color ACTIVE_COLOR_BUTTON_HOVER;
    public static Color ACTIVE_COLOR_BUTTON_DOWN;
    public static Color ACTIVE_COLOR_BACKGROUND_WINDOW;
    public static Color ACTIVE_COLOR_BACKGROUND_TEXTBOX;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        SetWindowTheme();
        var summaryForm = new SummaryForm();
        SetTitleBarTheme(summaryForm.Handle);
        Application.Run(summaryForm);
    }

    private static bool CheckSystemLightMode()
    {
        string RegistryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        var theme = Registry.GetValue(RegistryKey, "AppsUseLightTheme", string.Empty)?.ToString() ?? "1";
        if (theme.Equals("1"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private static void SetTitleBarTheme(IntPtr handle)
    {
        if (!CheckSystemLightMode() && IsWindows10OrGreater(17763))
        {
            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (IsWindows10OrGreater(18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            var useImmersiveDarkMode = 1;
            _ = DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }
    }

    private static void SetWindowTheme()
    {
        if (CheckSystemLightMode())
        {
            ACTIVE_COLOR_FOREGROUND = LIGHT_COLOR_FOREGROUND;
            ACTIVE_COLOR_HIGHLIGHT_FOREGROUND = LIGHT_COLOR_HIGHLIGHT_FOREGROUND;
            ACTIVE_COLOR_HIGHLIGHT_BACKGROUND = LIGHT_COLOR_HIGHLIGHT_BACKGROUND;
            ACTIVE_COLOR_SEARCH_FOREGROUND = Color.Black;
            ACTIVE_COLOR_SEARCH_HIGHLIGHT = ControlPaint.Light(LIGHT_COLOR_HIGHLIGHT_BACKGROUND, (float)1.6);
            ACTIVE_COLOR_BUTTON_HOVER = ControlPaint.Light(LIGHT_COLOR_BACKGROUND_WINDOW, (float)0.25);
            ACTIVE_COLOR_BUTTON_DOWN = ControlPaint.Light(ACTIVE_COLOR_BUTTON_HOVER, (float)0.25);
            ACTIVE_COLOR_BACKGROUND_WINDOW = LIGHT_COLOR_BACKGROUND_WINDOW;
            ACTIVE_COLOR_BACKGROUND_TEXTBOX = LIGHT_COLOR_BACKFGROUND_TEXTBOX;
        }
        else
        {
            ACTIVE_COLOR_FOREGROUND = DARK_COLOR_FOREGROUND;
            ACTIVE_COLOR_HIGHLIGHT_FOREGROUND = DARK_COLOR_HIGHLIGHT_FOREGROUND;
            ACTIVE_COLOR_HIGHLIGHT_BACKGROUND = DARK_COLOR_HIGHLIGHT_BACKGROUND;
            ACTIVE_COLOR_SEARCH_FOREGROUND = Color.Black;
            ACTIVE_COLOR_SEARCH_HIGHLIGHT = ControlPaint.Light(DARK_COLOR_HIGHLIGHT_BACKGROUND, (float)1.6);
            ACTIVE_COLOR_BUTTON_HOVER = ControlPaint.Light(DARK_COLOR_BACKGROUND_WINDOW, (float)0.25);
            ACTIVE_COLOR_BUTTON_DOWN = ControlPaint.Light(ACTIVE_COLOR_BUTTON_HOVER, (float)0.25);
            ACTIVE_COLOR_BACKGROUND_WINDOW = DARK_COLOR_BACKGROUND_WINDOW;
            ACTIVE_COLOR_BACKGROUND_TEXTBOX = DARK_COLOR_BACKFGROUND_TEXTBOX;
        }
    }

    private static bool IsWindows10OrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }
}