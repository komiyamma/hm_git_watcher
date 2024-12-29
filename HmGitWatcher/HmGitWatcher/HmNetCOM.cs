/*
 * HmNetCOM ver 2.087
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/


#if NET
#nullable disable
#endif

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;


/*
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/


namespace HmNetCOM
{
    internal partial class Hm
    {
        public interface IComDetachMethod
        {
            void OnReleaseObject(int reason=0);
        }

        public interface IComSupportX64
        {
#if (NET || NETCOREAPP3_1)

            bool X64MACRO() { return true; }
#else
            bool X64MACRO();
#endif
        }

        static Hm()
        {
            SetVersion();
            BindHidemaruExternFunctions();
        }

        private static void SetVersion()
        {
            if (Version == 0)
            {
                string hidemaru_fullpath = GetHidemaruExeFullPath();
                System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(hidemaru_fullpath);
                Version = 100 * vi.FileMajorPart + 10 * vi.FileMinorPart + 1 * vi.FileBuildPart + 0.01 * vi.FilePrivatePart;
            }
        }
        /// <summary>
        /// 秀丸バージョンの取得
        /// </summary>
        /// <returns>秀丸バージョン</returns>
        public static double Version { get; private set; } = 0;

        private const int filePathMaxLength = 512;

        private static string GetHidemaruExeFullPath()
        {
            var sb = new StringBuilder(filePathMaxLength);
            GetModuleFileName(IntPtr.Zero, sb, filePathMaxLength);
            string hidemaru_fullpath = sb.ToString();
            return hidemaru_fullpath;
        }

        /// <summary>
        /// 呼ばれたプロセスの現在の秀丸エディタのウィンドウハンドルを返します。
        /// </summary>
        /// <returns>現在の秀丸エディタのウィンドウハンドル</returns>
        public static IntPtr WindowHandle
        {
            get
            {
                return pGetCurrentWindowHandle();
            }
        }

        private static T HmClamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private static bool LongToInt(long number, out int intvar)
        {
            int ret_number = 0;
            while (true)
            {
                if (number > Int32.MaxValue)
                {
                    number = number - 4294967296;
                    number = number - Int32.MinValue;
                    number = number % 4294967296;
                    number = number + Int32.MinValue;
                }
                else
                {
                    break;
                }
            }
            while (true)
            {
                if (number < Int32.MinValue)
                {
                    number = number + 4294967296;
                    number = number + Int32.MinValue;
                    number = number % 4294967296;
                    number = number - Int32.MinValue;
                }
                else
                {
                    break;
                }
            }

            bool success = false;
            if (Int32.MinValue <= number && number <= Int32.MaxValue)
            {
                ret_number = (int)number;
                success = true;
            }

            intvar = ret_number;
            return success;
        }

        private static bool IsDoubleNumeric(object value)
        {
            return value is double || value is float;
        }
    }
}

namespace HmNetCOM
{

    internal partial class Hm
    {
        // 秀丸本体から出ている関数群
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr TGetCurrentWindowHandle();

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr TGetTotalTextUnicode();

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr TGetLineTextUnicode(int nLineNo);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr TGetSelectedTextUnicode();

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TGetCursorPosUnicode(out int pnLineNo, out int pnColumn);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TGetCursorPosUnicodeFromMousePos(IntPtr lpPoint, out int pnLineNo, out int pnColumn);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TEvalMacro([MarshalAs(UnmanagedType.LPWStr)] String pwsz);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TDebugInfo([MarshalAs(UnmanagedType.LPWStr)] String pwsz);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TCheckQueueStatus();

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TAnalyzeEncoding([MarshalAs(UnmanagedType.LPWStr)] String pwszFileName, IntPtr lParam1, IntPtr lParam2);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr TLoadFileUnicode([MarshalAs(UnmanagedType.LPWStr)] String pwszFileName, int nEncode, ref int pcwchOut, IntPtr lParam1, IntPtr lParam2);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr TGetStaticVariable([MarshalAs(UnmanagedType.LPWStr)] String pwszSymbolName, int sharedMemoryFlag);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TSetStaticVariable([MarshalAs(UnmanagedType.LPWStr)] String pwszSymbolName, [MarshalAs(UnmanagedType.LPWStr)] String pwszValue, int sharedMemoryFlag);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int TGetInputStates();

        // 秀丸本体から出ている関数群
        private static TGetCurrentWindowHandle pGetCurrentWindowHandle;
        private static TGetTotalTextUnicode pGetTotalTextUnicode;
        private static TGetLineTextUnicode pGetLineTextUnicode;
        private static TGetSelectedTextUnicode pGetSelectedTextUnicode;
        private static TGetCursorPosUnicode pGetCursorPosUnicode;
        private static TGetCursorPosUnicodeFromMousePos pGetCursorPosUnicodeFromMousePos;
        private static TEvalMacro pEvalMacro;
        private static TDebugInfo pDebugInfo;
        private static TCheckQueueStatus pCheckQueueStatus;
        private static TAnalyzeEncoding pAnalyzeEncoding;
        private static TLoadFileUnicode pLoadFileUnicode;
        private static TGetStaticVariable pGetStaticVariable;
        private static TSetStaticVariable pSetStaticVariable;
        private static TGetInputStates pGetInputStates;

        // 秀丸本体のexeを指すモジュールハンドル
        private static UnManagedDll hmExeHandle;

        private static void BindHidemaruExternFunctions()
        {
            // 初めての代入のみ
            if (hmExeHandle == null)
            {
                try
                {
                    hmExeHandle = new UnManagedDll(GetHidemaruExeFullPath());

                    pGetTotalTextUnicode = hmExeHandle.GetProcDelegate<TGetTotalTextUnicode>("Hidemaru_GetTotalTextUnicode");
                    pGetLineTextUnicode = hmExeHandle.GetProcDelegate<TGetLineTextUnicode>("Hidemaru_GetLineTextUnicode");
                    pGetSelectedTextUnicode = hmExeHandle.GetProcDelegate<TGetSelectedTextUnicode>("Hidemaru_GetSelectedTextUnicode");
                    pGetCursorPosUnicode = hmExeHandle.GetProcDelegate<TGetCursorPosUnicode>("Hidemaru_GetCursorPosUnicode");
                    pEvalMacro = hmExeHandle.GetProcDelegate<TEvalMacro>("Hidemaru_EvalMacro");
                    pCheckQueueStatus = hmExeHandle.GetProcDelegate<TCheckQueueStatus>("Hidemaru_CheckQueueStatus");

                    pGetCursorPosUnicodeFromMousePos = hmExeHandle.GetProcDelegate<TGetCursorPosUnicodeFromMousePos>("Hidemaru_GetCursorPosUnicodeFromMousePos");
                    pGetCurrentWindowHandle = hmExeHandle.GetProcDelegate<TGetCurrentWindowHandle>("Hidemaru_GetCurrentWindowHandle");

                    if (Version >= 890)
                    {
                        pAnalyzeEncoding = hmExeHandle.GetProcDelegate<TAnalyzeEncoding>("Hidemaru_AnalyzeEncoding");
                        pLoadFileUnicode = hmExeHandle.GetProcDelegate<TLoadFileUnicode>("Hidemaru_LoadFileUnicode");
                    }
                    if (Version >= 898)
                    {
                        pDebugInfo = hmExeHandle.GetProcDelegate<TDebugInfo>("Hidemaru_DebugInfo");
                    }
                    if (Version >= 915)
                    {
                        pGetStaticVariable = hmExeHandle.GetProcDelegate<TGetStaticVariable>("Hidemaru_GetStaticVariable");
                        pSetStaticVariable = hmExeHandle.GetProcDelegate<TSetStaticVariable>("Hidemaru_SetStaticVariable");
                    }
                    if (Version >= 919)
                    {
                        pGetInputStates = hmExeHandle.GetProcDelegate<TGetInputStates>("Hidemaru_GetInputStates");
                    }

                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

            }
        }
    }
}

namespace HmNetCOM
{
    namespace HmNativeMethods {
        internal partial class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            protected extern static uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

            [DllImport("kernel32.dll", SetLastError = true)]
            protected extern static IntPtr GlobalLock(IntPtr hMem);

            [DllImport("kernel32.dll", SetLastError = true)]

            [return: MarshalAs(UnmanagedType.Bool)]
            protected extern static bool GlobalUnlock(IntPtr hMem);

            [DllImport("kernel32.dll", SetLastError = true)]
            protected extern static IntPtr GlobalFree(IntPtr hMem);

            [StructLayout(LayoutKind.Sequential)]
            protected struct POINT
            {
                public int X;
                public int Y;
            }
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            protected extern static bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
            protected static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Unicode)]
            protected static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

            [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Unicode)]
            protected static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, StringBuilder wParam, StringBuilder lParam);
        }
    }

    internal partial class Hm : HmNativeMethods.NativeMethods
    {
    }
}

namespace HmNetCOM
{
    namespace HmNativeMethods {
        internal partial class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            protected static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32", CharSet = CharSet.Ansi, BestFitMapping=false, ExactSpelling=true, SetLastError=true)]
            protected static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32", SetLastError = true)]
            protected static extern bool FreeLibrary(IntPtr hModule);
        }
    }

    internal partial class Hm
    {
        // アンマネージドライブラリの遅延での読み込み。C++のLoadLibraryと同じことをするため
        // これをする理由は、この実行dllとHideamru.exeが異なるディレクトリに存在する可能性があるため、
        // C#風のDllImportは成立しないからだ。
        internal sealed class UnManagedDll : HmNativeMethods.NativeMethods, IDisposable
        {

            IntPtr moduleHandle;
            public UnManagedDll(string lpFileName)
            {
                moduleHandle = LoadLibrary(lpFileName);
            }

            // コード分析などの際の警告抑制のためにデストラクタをつけておく
            ~UnManagedDll()
            {
                // C#はメインドメインのdllは(このコードが存在するdll)はプロセスが終わらない限り解放されないので、
                // ここではネイティブのdllも事前には解放しない方がよい。(プロセス終了による解放に委ねる）
                // デストラクタでは何もしない。
                // コード分析でも警告がでないように、コード分析では実行されないことがわからない形で
                // 決して実行されないコードにしておく
                if (moduleHandle == (IntPtr)(-1)) { this.Dispose(); };
            }

            public IntPtr ModuleHandle
            {
                get
                {
                    return moduleHandle;
                }
            }

            public T GetProcDelegate<T>(string method) where T : class
            {
                IntPtr methodHandle = GetProcAddress(moduleHandle, method);
                T r = Marshal.GetDelegateForFunctionPointer(methodHandle, typeof(T)) as T;
                return r;
            }

            public void Dispose()
            {
                FreeLibrary(moduleHandle);
            }
        }

    }
}




/*
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/


namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class Macro
        {
            /// <summary>
            /// マクロの debuginfo と同じ関数だが、マクロ実行中以外でも扱える。
            /// 必要かどうかは微妙なところだが、マクロの debuginfo(0)〜debuginfo(2) など、表示設定の反映の恩恵を受けられる点が異なる
            /// マクロの debuginfo とは異なり、マクロの実行が終えたからといって、自動的に debuginfo(0)にリセットされたりはしない。
            /// よって非表示にするためには、明示的に debuginfo(0)を、マクロ側もしくはプログラム側から明示的に実行する必要がある。
            /// </summary>
            /// <returns>成功したときは0以外、失敗したときは0を返す。</returns>
            public static int DebugInfo(params Object[] messages)
            {
                if (Version < 898)
                {
                    throw new MissingMethodException("Hidemaru_Macro_DebugInfo_Exception");
                }
                if (pDebugInfo == null)
                {
                    throw new MissingMethodException("Hidemaru_Macro_DebugInfo_Exception");
                }
                List<String> list = new List<String>();
                foreach (var exp in messages)
                {
                    var mixedString = exp.ToString();
                    string unifiedString = mixedString.Replace("\r\n", "\n").Replace("\n", "\r\n");
                    list.Add(unifiedString);
                }

                String joind = String.Join(" ", list);

                return pDebugInfo(joind);
            }

            /// <summary>
            /// マクロを実行中か否かを判定する
            /// </summary>
            /// <returns>実行中ならtrue, そうでなければfalse</returns>
            public static bool IsExecuting
            {
                get
                {
                    const int WM_USER = 0x400;
                    const int WM_ISMACROEXECUTING = WM_USER + 167;

                    IntPtr hWndHidemaru = WindowHandle;
                    if (hWndHidemaru != IntPtr.Zero)
                    {
                        IntPtr cwch = SendMessage(hWndHidemaru, WM_ISMACROEXECUTING, IntPtr.Zero, IntPtr.Zero);
                        return (cwch != IntPtr.Zero);
                    }

                    return false;
                }
            }

            /// <summary>
            /// マクロの静的な変数
            /// </summary>
            internal static TStaticVar StaticVar = new TStaticVar();

            /// <summary>
            /// マクロの静的な変数
            /// </summary>
            internal partial class TStaticVar { 

                /// <summary>
                /// マクロの静的な変数の値(文字列)の読み書き
                /// </summary>
                /// <param name = "name">変数名</param>
                /// <param name = "value">書き込みの場合、代入する値</param>
                /// <param name = "sharedflag">共有フラグ</param>
                /// <returns>対象の静的変数名(name)に格納されている文字列</returns>
                public string this[string name, int sharedflag] {
                    get { return GetStaticVariable(name, sharedflag); }
                    set { SetStaticVariable(name, value, sharedflag); }
                }

                /// <summary>
                /// マクロの静的な変数の値(文字列)を取得する
                /// </summary>
                /// <param name = "name">変数名</param>
                /// <param name = "sharedflag">共有フラグ</param>
                /// <returns>対象の静的変数名(name)に格納されている文字列</returns>
                public string Get(string name, int sharedflag)
                {
                    return GetStaticVariable(name, sharedflag);
                }

                /// <summary>
                /// マクロの静的な変数へと値(文字列)を設定する
                /// </summary>
                /// <param name = "name">変数名</param>
                /// <param name = "value">設定する値(文字列)</param>
                /// <param name = "sharedflag">共有フラグ</param>
                /// <returns>取得に成功すれば真、失敗すれば偽が返る</returns>
                public bool Set(string name, string value, int sharedflag)
                {
                    var ret = SetStaticVariable(name, value, sharedflag);
                    if (ret != 0)
                    {
                        return true;
                    }
                    return false;
                }

                private static int SetStaticVariable(String symbolname, String value, int sharedMemoryFlag)
                {
                    try
                    {
                        if (Version < 915)
                        {
                            throw new MissingMethodException("Hidemaru_Macro_SetGlobalVariable_Exception");
                        }
                        if (pSetStaticVariable == null)
                        {
                            throw new MissingMethodException("Hidemaru_Macro_SetGlobalVariable_Exception");
                        }

                        return pSetStaticVariable(symbolname, value, sharedMemoryFlag);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e.Message);
                        throw;
                    }
                }

                private static string GetStaticVariable(String symbolname, int sharedMemoryFlag)
                {
                    try
                    {
                        if (Version < 915)
                        {
                            throw new MissingMethodException("Hidemaru_Macro_GetStaticVariable_Exception");
                        }
                        if (pGetStaticVariable == null)
                        {
                            throw new MissingMethodException("Hidemaru_Macro_GetStaticVariable_Exception");
                        }

                        string staticText = "";

                        IntPtr hGlobal = pGetStaticVariable(symbolname, sharedMemoryFlag);
                        if (hGlobal == IntPtr.Zero)
                        {
                            new InvalidOperationException("Hidemaru_Macro_GetStaticVariable_Exception");
                        }

                        var pwsz = GlobalLock(hGlobal);
                        if (pwsz != IntPtr.Zero)
                        {
                            staticText = Marshal.PtrToStringUni(pwsz);
                            GlobalUnlock(hGlobal);
                        }
                        GlobalFree(hGlobal);

                        return staticText;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e.Message);
                        throw;
                    }
                }
            }

            /// <summary>
            /// マクロをプログラム内から実行した際の返り値のインターフェイス
            /// </summary>
            /// <returns>(Result, Message, Error)</returns>
            public interface IResult
            {
                int Result { get; }
                String Message { get; }
                Exception Error { get; }
            }

            private class TResult : IResult
            {
                public int Result { get; set; }
                public string Message { get; set; }
                public Exception Error { get; set; }

                public TResult(int Result, String Message, Exception Error)
                {
                    this.Result = Result;
                    this.Message = Message;
                    this.Error = Error;
                }
            }

            /// <summary>
            /// 現在のマクロ実行中に、プログラム中で、マクロを文字列で実行。
            /// マクロ実行中のみ実行可能なメソッド。
            /// </summary>
            /// <returns>(Result, Message, Error)</returns>

            public static IResult Eval(String expression)
            {
                TResult result;
                if (!IsExecuting)
                {
                    Exception e = new InvalidOperationException("Hidemaru_Macro_IsNotExecuting_Exception");
                    result = new TResult(-1, "", e);
                    return result;
                }
                int success = 0;
                try
                {
                    success = pEvalMacro(expression);
                }
                catch (Exception)
                {
                    throw;
                }
                if (success == 0)
                {
                    Exception e = new InvalidOperationException("Hidemaru_Macro_Eval_Exception");
                    result = new TResult(0, "", e);
                    return result;
                }
                else
                {
                    result = new TResult(success, "", null);
                    return result;
                }

            }

            public static partial class Exec
            {
                /// <summary>
                /// マクロを実行していない時に、プログラム中で、マクロファイルを与えて新たなマクロを実行。
                /// マクロを実行していない時のみ実行可能なメソッド。
                /// </summary>
                /// <returns>(Result, Message, Error)</returns>
                public static IResult File(string filepath)
                {
                    TResult result;
                    if (IsExecuting)
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_IsExecuting_Exception");
                        result = new TResult(-1, "", e);
                        return result;
                    }
                    if (!System.IO.File.Exists(filepath))
                    {
                        Exception e = new FileNotFoundException(filepath);
                        result = new TResult(-1, "", e);
                        return result;
                    }

                    const int WM_USER = 0x400;
                    const int WM_REMOTE_EXECMACRO_FILE = WM_USER + 271;
                    IntPtr hWndHidemaru = WindowHandle;

                    StringBuilder sbFileName = new StringBuilder(filepath);
                    StringBuilder sbRet = new StringBuilder("\x0f0f", 0x0f0f + 1); // 最初の値は帰り値のバッファー
                    IntPtr cwch = SendMessage(hWndHidemaru, WM_REMOTE_EXECMACRO_FILE, sbRet, sbFileName);
                    if (cwch != IntPtr.Zero)
                    {
                        result = new TResult(1, sbRet.ToString(), null);
                    }
                    else
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_Eval_Exception");
                        result = new TResult(0, sbRet.ToString(), e);
                    }
                    return result;
                }

                /// <summary>
                /// マクロを実行していない時に、プログラム中で、文字列で新たなマクロを実行。
                /// マクロを実行していない時のみ実行可能なメソッド。
                /// </summary>
                /// <returns>(Result, Message, Error)</returns>
                public static IResult Eval(string expression)
                {
                    TResult result;
                    if (IsExecuting)
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_IsExecuting_Exception");
                        result = new TResult(-1, "", e);
                        return result;
                    }

                    const int WM_USER = 0x400;
                    const int WM_REMOTE_EXECMACRO_MEMORY = WM_USER + 272;
                    IntPtr hWndHidemaru = WindowHandle;

                    StringBuilder sbExpression = new StringBuilder(expression);
                    StringBuilder sbRet = new StringBuilder("\x0f0f", 0x0f0f + 1); // 最初の値は帰り値のバッファー
                    IntPtr cwch = SendMessage(hWndHidemaru, WM_REMOTE_EXECMACRO_MEMORY, sbRet, sbExpression);
                    if (cwch != IntPtr.Zero)
                    {
                        result = new TResult(1, sbRet.ToString(), null);
                    }
                    else
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_Eval_Exception");
                        result = new TResult(0, sbRet.ToString(), e);
                    }
                    return result;
                }
            }
        }
    }
}

namespace HmNetCOM
{

    internal static class HmMacroExtentensions
    {
        public static void Deconstruct(this Hm.Macro.IResult result, out int Result, out Exception Error, out String Message)
        {
            Result = result.Result;
            Error = result.Error;
            Message = result.Message;
        }

        public static void Deconstruct(this Hm.Macro.IFunctionResult result, out object Result, out List<Object> Args, out Exception Error, out String Message)
        {
            Result = result.Result;
            Args = result.Args;
            Error = result.Error;
            Message = result.Message;
        }

        public static void Deconstruct(this Hm.Macro.IStatementResult result, out int Result, out List<Object> Args, out Exception Error, out String Message)
        {
            Result = result.Result;
            Args = result.Args;
            Error = result.Error;
            Message = result.Message;
        }
    }
}



/*
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/


namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class Edit
        {
            /// <summary>
            /// キー入力があるなどの理由で処理を中断するべきかを返す。
            /// </summary>
            /// <returns>中断するべきならtrue、そうでなければfalse</returns>
            public static bool QueueStatus
            {
                get { return pCheckQueueStatus() != 0; }
            }

            /// <summary>
            /// 現在アクティブな編集領域のテキスト全体を返す。
            /// </summary>
            /// <returns>編集領域のテキスト全体</returns>
            public static string TotalText
            {
                get
                {
                    string totalText = "";
                    try
                    {
                        IntPtr hGlobal = pGetTotalTextUnicode();
                        if (hGlobal == IntPtr.Zero)
                        {
                            new InvalidOperationException("Hidemaru_GetTotalTextUnicode_Exception");
                        }

                        var pwsz = GlobalLock(hGlobal);
                        if (pwsz != IntPtr.Zero)
                        {
                            totalText = Marshal.PtrToStringUni(pwsz);
                            GlobalUnlock(hGlobal);
                        }
                        GlobalFree(hGlobal);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    return totalText;
                }
                set
                {
                    // 935.β6以降は、settotaltext() が実装された。
                    if (Version >= 935.06)
                    {
                        SetTotalText2(value);
                    }
                    else
                    {
                        SetTotalText(value);
                    }
                }
            }
            static partial void SetTotalText(string text);
            static partial void SetTotalText2(string text);


            /// <summary>
            /// 現在、単純選択している場合、その選択中のテキスト内容を返す。
            /// </summary>
            /// <returns>選択中のテキスト内容</returns>
            public static string SelectedText
            {
                get
                {
                    string selectedText = "";
                    try
                    {
                        IntPtr hGlobal = pGetSelectedTextUnicode();
                        if (hGlobal == IntPtr.Zero)
                        {
                            new InvalidOperationException("Hidemaru_GetSelectedTextUnicode_Exception");
                        }

                        var pwsz = GlobalLock(hGlobal);
                        if (pwsz != IntPtr.Zero)
                        {
                            selectedText = Marshal.PtrToStringUni(pwsz);
                            GlobalUnlock(hGlobal);
                        }
                        GlobalFree(hGlobal);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    return selectedText;
                }
                set
                {
                    SetSelectedText(value);
                }
            }
            static partial void SetSelectedText(string text);

            /// <summary>
            /// 現在、カーソルがある行(エディタ的)のテキスト内容を返す。
            /// </summary>
            /// <returns>カーソルがある行のテキスト内容</returns>
            public static string LineText
            {
                get
                {
                    string lineText = "";

                    ICursorPos pos = CursorPos;
                    if (pos.LineNo < 0 || pos.Column < 0)
                    {
                        return lineText;
                    }

                    try
                    {
                        IntPtr hGlobal = pGetLineTextUnicode(pos.LineNo);
                        if (hGlobal == IntPtr.Zero)
                        {
                            new InvalidOperationException("Hidemaru_GetLineTextUnicode_Exception");
                        }

                        var pwsz = GlobalLock(hGlobal);
                        if (pwsz != IntPtr.Zero)
                        {
                            lineText = Marshal.PtrToStringUni(pwsz);
                            GlobalUnlock(hGlobal);
                        }
                        GlobalFree(hGlobal);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    return lineText;
                }
                set
                {
                    SetLineText(value);
                }
            }
            static partial void SetLineText(string text);

            /// <summary>
            /// CursorPos の返り値のインターフェイス
            /// </summary>
            /// <returns>(LineNo, Column)</returns>
            public interface ICursorPos
            {
                int LineNo { get; }
                int Column { get; }
            }

            private struct TCursurPos : ICursorPos
            {
                public int Column { get; set; }
                public int LineNo { get; set; }
            }

            /// <summary>
            /// MousePos の返り値のインターフェイス
            /// </summary>
            /// <returns>(LineNo, Column, X, Y)</returns>
            public interface IMousePos
            {
                int LineNo { get; }
                int Column { get; }
                int X { get; }
                int Y { get; }
            }

            private struct TMousePos : IMousePos
            {
                public int LineNo { get; set; }
                public int Column { get; set; }
                public int X { get; set; }
                public int Y { get; set; }
            }

            /// <summary>
            /// ユニコードのエディタ的な換算でのカーソルの位置を返す
            /// </summary>
            /// <returns>(LineNo, Column)</returns>
            public static ICursorPos CursorPos
            {
                get
                {
                    int lineno = -1;
                    int column = -1;
                    int success = pGetCursorPosUnicode(out lineno, out column);
                    if (success != 0)
                    {
                        TCursurPos pos = new TCursurPos();
                        pos.LineNo = lineno;
                        pos.Column = column;
                        return pos;
                    }
                    else
                    {
                        TCursurPos pos = new TCursurPos();
                        pos.LineNo = -1;
                        pos.Column = -1;
                        return pos;
                    }

                }
            }

            /// <summary>
            /// ユニコードのエディタ的な換算でのマウスの位置に対応するカーソルの位置を返す
            /// </summary>
            /// <returns>(LineNo, Column, X, Y)</returns>
            public static IMousePos MousePos
            {
                get
                {
                    POINT lpPoint;
                    bool success_1 = GetCursorPos(out lpPoint);

                    TMousePos pos = new TMousePos
                    {
                        LineNo = -1,
                        Column = -1,
                        X = -1,
                        Y = -1,
                    };

                    if (!success_1)
                    {
                        return pos;
                    }

                    int column = -1;
                    int lineno = -1;
                    int success_2 = pGetCursorPosUnicodeFromMousePos(IntPtr.Zero, out lineno, out column);
                    if (success_2 == 0)
                    {
                        return pos;
                    }

                    pos.LineNo = lineno;
                    pos.Column = column;
                    pos.X = lpPoint.X;
                    pos.Y = lpPoint.Y;
                    return pos;
                }
            }

            /// <summary>
            /// 現在開いているファイル名のフルパスを返す、無題テキストであれば、nullを返す。
            /// </summary>
            /// <returns>ファイル名のフルパス、もしくは null</returns>

            public static string FilePath
            {
                get
                {
                    IntPtr hWndHidemaru = WindowHandle;
                    if (hWndHidemaru != IntPtr.Zero)
                    {
                        const int WM_USER = 0x400;
                        const int WM_HIDEMARUINFO = WM_USER + 181;
                        const int HIDEMARUINFO_GETFILEFULLPATH = 4;

                        StringBuilder sb = new StringBuilder(filePathMaxLength); // まぁこんくらいでさすがに十分なんじゃないの...
                        IntPtr cwch = SendMessage(hWndHidemaru, WM_HIDEMARUINFO, new IntPtr(HIDEMARUINFO_GETFILEFULLPATH), sb);
                        String filename = sb.ToString();
                        if (String.IsNullOrEmpty(filename))
                        {
                            return null;
                        }
                        else
                        {
                            return filename;
                        }
                    }
                    return null;
                }
            }

            /// <summary>
            /// 現在開いている編集エリアで、文字列の編集や何らかの具体的操作を行ったかチェックする。マクロ変数のupdatecount相当
            /// </summary>
            /// <returns>一回の操作でも数カウント上がる。32bitの値を超えると一周する。初期値は1以上。</returns>

            public static int UpdateCount
            {
                get
                {
                    if (Version < 912.98)
                    {
                        throw new MissingMethodException("Hidemaru_Edit_UpdateCount_Exception");
                    }
                    IntPtr hWndHidemaru = WindowHandle;
                    if (hWndHidemaru != IntPtr.Zero)
                    {
                        const int WM_USER = 0x400;
                        const int WM_HIDEMARUINFO = WM_USER + 181;
                        const int HIDEMARUINFO_GETUPDATECOUNT = 7;

                        IntPtr updatecount = SendMessage(hWndHidemaru, WM_HIDEMARUINFO, (IntPtr)HIDEMARUINFO_GETUPDATECOUNT, IntPtr.Zero);
                        return (int)updatecount;
                    }
                    return -1;
                }
            }

            /// <summary>
            /// <para>各種の入力ができるかどうかを判断するための状態を表します。（V9.19以降）</para>
            /// <para>以下の値の論理和です。</para>
            /// <para>0x00000002 ウィンドウ移動/サイズ変更中</para>
            /// <para>0x00000004 メニュー操作中</para>
            /// <para>0x00000008 システムメニュー操作中</para>
            /// <para>0x00000010 ポップアップメニュー操作中</para>
            /// <para>0x00000100 IME入力中</para>
            /// <para>0x00000200 何らかのダイアログ表示中</para>
            /// <para>0x00000400 ウィンドウがDisable状態</para>
            /// <para>0x00000800 非アクティブなタブまたは非表示のウィンドウ</para>
            /// <para>0x00001000 検索ダイアログの疑似モードレス状態</para>
            /// <para>0x00002000 なめらかスクロール中</para>
            /// <para>0x00004000 中ボタンによるオートスクロール中</para>
            /// <para>0x00008000 キーやマウスの操作直後(100ms 以内)</para>
            /// <para>0x00010000 何かマウスのボタンを押している</para>
            /// <para>0x00020000 マウスキャプチャ状態(ドラッグ状態)</para>
            /// <para>0x00040000 Hidemaru_CheckQueueStatus相当</para>
            /// </summary>
            /// </summary>
            /// <returns>一回の操作でも数カウント上がる。32bitの値を超えると一周する。初期値は1以上。</returns>
            public static int InputStates
            {
                get
                {
                    if (Version < 919.11)
                    {
                        throw new MissingMethodException("Hidemaru_Edit_InputStates");
                    }
                    if (pGetInputStates == null)
                    {
                        throw new MissingMethodException("Hidemaru_Edit_InputStates");
                    }

                    return pGetInputStates();
                }
            }

        }
    }
}


namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class Edit
        {
            static partial void SetTotalText(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                string cmd = $@"
                begingroupundo;
                rangeeditout;
                selectall;
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                insert member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                endgroupundo;
                ";
                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                } else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }

            static partial void SetTotalText2(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                string cmd = $@"
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                settotaltext member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                ";
                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                } else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }


            static partial void SetSelectedText(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                string cmd = $@"
                if (selecting) {{
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                insert member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                }}
                ";

                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                }
                else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }

            static partial void SetLineText(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                var pos = Edit.CursorPos;
                string cmd = $@"
                begingroupundo;
                selectline;
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                insert member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                moveto2 {pos.Column}, {pos.LineNo};
                endgroupundo;
                ";

                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                }
                else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }

        }
    }
}


namespace HmNetCOM
{

    internal static class HmEditExtentensions
    {
        public static void Deconstruct(this Hm.Edit.ICursorPos pos, out int LineNo, out int Column)
        {
            LineNo = pos.LineNo;
            Column = pos.Column;
        }

        public static void Deconstruct(this Hm.Edit.IMousePos pos, out int LineNo, out int Column, out int X, out int Y)
        {
            LineNo = pos.LineNo;
            Column = pos.Column;
            X = pos.X;
            Y = pos.Y;
        }
    }
}



/*
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/



namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class File
        {
            public interface IHidemaruEncoding
            {
                int HmEncode { get; }
            }
            public interface IMicrosoftEncoding
            {
                int MsCodePage { get; }
            }

            public interface IEncoding : IHidemaruEncoding, IMicrosoftEncoding
            {
            }

            public interface IHidemaruStreamReader : IDisposable
            {
                IEncoding Encoding { get; }
                String Read();
                String FilePath { get; }
                void Close();
            }

            // 途中でエラーが出るかもしれないので、相応しいUnlockやFreeが出来るように内部管理用
            private enum HGlobalStatus { None, Lock, Unlock, Free };

            private static String ReadAllText(String filepath, int hm_encode = -1)
            {
                if (pLoadFileUnicode == null)
                {
                    throw new MissingMethodException("Hidemaru_LoadFileUnicode");
                }

                if (hm_encode == -1)
                {
                    hm_encode = GetHmEncode(filepath);
                }

                if (!System.IO.File.Exists(filepath))
                {
                    throw new System.IO.FileNotFoundException(filepath);
                }

                String curstr = "";
                int read_count = 0;
                IntPtr hGlobal = pLoadFileUnicode(filepath, hm_encode, ref read_count, IntPtr.Zero, IntPtr.Zero);
                HGlobalStatus hgs = HGlobalStatus.None;
                if (hGlobal == IntPtr.Zero)
                {
                    throw new System.IO.IOException(filepath);
                }
                if (hGlobal != IntPtr.Zero)
                {
                    try
                    {
                        IntPtr ret = GlobalLock(hGlobal);
                        hgs = HGlobalStatus.Lock;
                        curstr = Marshal.PtrToStringUni(ret);
                        GlobalUnlock(hGlobal);
                        hgs = HGlobalStatus.Unlock;
                        GlobalFree(hGlobal);
                        hgs = HGlobalStatus.Free;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e.Message);
                    }
                    finally
                    {
                        switch (hgs)
                        {
                            // ロックだけ成功した
                            case HGlobalStatus.Lock:
                                {
                                    GlobalUnlock(hGlobal);
                                    GlobalFree(hGlobal);
                                    break;
                                }
                            // アンロックまで成功した
                            case HGlobalStatus.Unlock:
                                {
                                    GlobalFree(hGlobal);
                                    break;
                                }
                            // フリーまで成功した
                            case HGlobalStatus.Free:
                                {
                                    break;
                                }
                        }
                    }
                }
                if (hgs == HGlobalStatus.Free)
                {
                    return curstr;
                }
                else
                {
                    throw new System.IO.IOException(filepath);
                }
            }

            private static int[] key_encode_value_codepage_array = {
                0,      // Unknown
                932,    // encode = 1 ANSI/OEM Japanese; Japanese (Shift-JIS)
                1200,   // encode = 2 Unicode UTF-16, little-endian
                51932,  // encode = 3 EUC
                50221,  // encode = 4 JIS
                65000,  // encode = 5 UTF-7
                65001,  // encode = 6 UTF-8
                1201,   // encode = 7 Unicode UTF-16, big-endian
                1252,   // encode = 8 欧文 ANSI Latin 1; Western European (Windows)
                936,    // encode = 9 簡体字中国語 ANSI/OEM Simplified Chinese (PRC, Singapore); Chinese Simplified (GB2312)
                950,    // encode =10 繁体字中国語 ANSI/OEM Traditional Chinese (Taiwan; Hong Kong SAR, PRC); Chinese Traditional (Big5)
                949,    // encode =11 韓国語 ANSI/OEM Korean (Unified Hangul Code)
                1361,   // encode =12 韓国語 Korean (Johab)
                1250,   // encode =13 中央ヨーロッパ言語 ANSI Central European; Central European (Windows)
                1257,   // encode =14 バルト語 ANSI Baltic; Baltic (Windows)
                1253,   // encode =15 ギリシャ語 ANSI Greek; Greek (Windows)
                1251,   // encode =16 キリル言語 ANSI Cyrillic; Cyrillic (Windows)
                42,     // encode =17 シンボル
                1254,   // encode =18 トルコ語 ANSI Turkish; Turkish (Windows)
                1255,   // encode =19 ヘブライ語 ANSI Hebrew; Hebrew (Windows)
                1256,   // encode =20 アラビア語 ANSI Arabic; Arabic (Windows)
                874,    // encode =21 タイ語 ANSI/OEM Thai (same as 28605, ISO 8859-15); Thai (Windows)
                1258,   // encode =22 ベトナム語 ANSI/OEM Vietnamese; Vietnamese (Windows)
                10001,  // encode =23 x-mac-japanese Japanese (Mac)
                850,    // encode =24 OEM/DOS
                0,      // encode =25 その他
                12000,  // encode =26 Unicode (UTF-32) little-endian
                12001,  // encode =27 Unicode (UTF-32) big-endian

            };

            /// <summary>
            /// 秀丸でファイルのエンコードを取得する
            /// （秀丸に設定されている内容に従う）
            /// </summary>
            /// <returns>IEncoding型のオブジェクト。MsCodePage と HmEncode のプロパティを得られる。</returns>
            public static IEncoding GetEncoding(string filepath)
            {
                int hm_encode = GetHmEncode(filepath);
                int ms_codepage = GetMsCodePage(hm_encode);
                IEncoding encoding = new Encoding(hm_encode, ms_codepage);
                return encoding;
            }

            private static int GetHmEncode(string filepath)
            {

                if (pAnalyzeEncoding == null)
                {
                    throw new MissingMethodException("Hidemaru_AnalyzeEncoding");
                }

                return pAnalyzeEncoding(filepath, IntPtr.Zero, IntPtr.Zero);
            }

            private static int GetMsCodePage(int hidemaru_encode)
            {
                int result_codepage = 0;

                if (pAnalyzeEncoding == null)
                {
                    throw new MissingMethodException("Hidemaru_AnalyzeEncoding");
                }

                /*
                 *
                    Shift-JIS encode=1 codepage=932
                    Unicode encode=2 codepage=1200
                    EUC encode=3 codepage=51932
                    JIS encode=4 codepage=50221
                    UTF-7 encode=5 codepage=65000
                    UTF-8 encode=6 codepage=65001
                    Unicode (Big-Endian) encode=7 codepage=1201
                    欧文 encode=8 codepage=1252
                    簡体字中国語 encode=9 codepage=936
                    繁体字中国語 encode=10 codepage=950
                    韓国語 encode=11 codepage=949
                    韓国語(Johab) encode=12 codepage=1361
                    中央ヨーロッパ言語 encode=13 codepage=1250
                    バルト語 encode=14 codepage=1257
                    ギリシャ語 encode=15 codepage=1253
                    キリル言語 encode=16 codepage=1251
                    シンボル encode=17 codepage=42
                    トルコ語 encode=18 codepage=1254
                    ヘブライ語 encode=19 codepage=1255
                    アラビア語 encode=20 codepage=1256
                    タイ語 encode=21 codepage=874
                    ベトナム語 encode=22 codepage=1258
                    Macintosh encode=23 codepage=0
                    OEM/DOS encode=24 codepage=0
                    その他 encode=25 codepage=0
                    UTF-32 encode=27 codepage=12000
                    UTF-32 (Big-Endian) encode=28 codepage=12001
                */
                if (hidemaru_encode <= 0)
                {
                    return result_codepage;
                }

                if (hidemaru_encode < key_encode_value_codepage_array.Length)
                {
                    // 把握しているコードページなので入れておく
                    result_codepage = key_encode_value_codepage_array[hidemaru_encode];
                    return result_codepage;
                }
                else // 長さ以上なら、予期せぬ未来のencode番号対応
                {
                    return result_codepage;
                }

            }

            // コードページを得る
            private static int GetMsCodePage(string filepath)
            {

                int result_codepage = 0;

                if (pAnalyzeEncoding == null)
                {
                    throw new MissingMethodException("Hidemaru_AnalyzeEncoding");
                }


                int hidemaru_encode = pAnalyzeEncoding(filepath, IntPtr.Zero, IntPtr.Zero);

                /*
                 *
                    Shift-JIS encode=1 codepage=932
                    Unicode encode=2 codepage=1200
                    EUC encode=3 codepage=51932
                    JIS encode=4 codepage=50221
                    UTF-7 encode=5 codepage=65000
                    UTF-8 encode=6 codepage=65001
                    Unicode (Big-Endian) encode=7 codepage=1201
                    欧文 encode=8 codepage=1252
                    簡体字中国語 encode=9 codepage=936
                    繁体字中国語 encode=10 codepage=950
                    韓国語 encode=11 codepage=949
                    韓国語(Johab) encode=12 codepage=1361
                    中央ヨーロッパ言語 encode=13 codepage=1250
                    バルト語 encode=14 codepage=1257
                    ギリシャ語 encode=15 codepage=1253
                    キリル言語 encode=16 codepage=1251
                    シンボル encode=17 codepage=42
                    トルコ語 encode=18 codepage=1254
                    ヘブライ語 encode=19 codepage=1255
                    アラビア語 encode=20 codepage=1256
                    タイ語 encode=21 codepage=874
                    ベトナム語 encode=22 codepage=1258
                    Macintosh encode=23 codepage=0
                    OEM/DOS encode=24 codepage=0
                    その他 encode=25 codepage=0
                    UTF-32 encode=27 codepage=12000
                    UTF-32 (Big-Endian) encode=28 codepage=12001
                */
                if (hidemaru_encode <= 0)
                {
                    return result_codepage;
                }

                if (hidemaru_encode < key_encode_value_codepage_array.Length)
                {
                    // 把握しているコードページなので入れておく
                    result_codepage = key_encode_value_codepage_array[hidemaru_encode];
                    return result_codepage;
                }
                else // 長さ以上なら、予期せぬ未来のencode番号対応
                {
                    return result_codepage;
                }
            }

            private class Encoding : IEncoding
            {
                private int m_hm_encode;
                private int m_ms_codepage;
                public Encoding(int hmencode, int mscodepage)
                {
                    this.m_hm_encode = hmencode;
                    this.m_ms_codepage = mscodepage;
                }
                public int HmEncode { get { return this.m_hm_encode; } }
                public int MsCodePage { get { return this.m_ms_codepage; } }
            }

            private class HidemaruStreamReader : IHidemaruStreamReader
            {
                String m_path;

                IEncoding m_encoding;

                Hm.File.IEncoding Hm.File.IHidemaruStreamReader.Encoding { get { return this.m_encoding; } }

                public string FilePath { get { return this.m_path; } }

                public HidemaruStreamReader(String path, int hm_encode = -1)
                {
                    this.m_path = path;
                    // 指定されていなければ、
                    if (hm_encode == -1)
                    {
                        hm_encode = GetHmEncode(path);
                    }
                    int ms_codepage = GetMsCodePage(hm_encode);
                    this.m_encoding = new Encoding(hm_encode, ms_codepage);
                }

                ~HidemaruStreamReader()
                {
                    Close();
                }

                String IHidemaruStreamReader.Read()
                {
                    if (System.IO.File.Exists(this.m_path) == false)
                    {
                        throw new System.IO.FileNotFoundException(this.m_path);
                    }

                    try
                    {
                        String text = ReadAllText(this.m_path, this.m_encoding.HmEncode);
                        return text;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                public void Close()
                {
                    if (this.m_path != null)
                    {
                        this.m_encoding = null;
                        this.m_path = null;
                    }
                }

                public void Dispose()
                {
                    this.Close();
                }
            }

            /// <summary>
            /// 秀丸でファイルのエンコードを判断し、その判断結果に基づいてファイルのテキスト内容を取得する。
            /// （秀丸に設定されている内容に従う）
            /// </summary>
            /// <param name = "filepath">読み込み対象のファイルのパス</param>
            /// <param name = "hm_encode">エンコード(秀丸マクロの「encode」の値)が分かっているならば指定する、指定しない場合秀丸APIの自動判定に任せる。</param>
            /// <returns>IHidemaruStreamReader型のオブジェクト。</returns>
            public static IHidemaruStreamReader Open(string filepath, int hm_encode = -1)
            {
                if (System.IO.File.Exists(filepath) == false)
                {
                    throw new System.IO.FileNotFoundException(filepath);
                }
                var sr = new HidemaruStreamReader(filepath, hm_encode);
                return sr;
            }

        }
    }
}

/*
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/

namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class Macro
        {
            public static partial class Flags {

                public static partial class Encode {
                    //OPENFILE等のENCODE相当
                    public const int Sjis = 0x01;
                    public const int Utf16 = 0x02;
                    public const int Euc = 0x03;
                    public const int Jis = 0x04;
                    public const int Utf7 = 0x05;
                    public const int Utf8 = 0x06;
                    public const int Utf16_be = 0x07;
                    public const int Euro = 0x08;
                    public const int Gb2312 = 0x09;
                    public const int Big5 = 0x0a;
                    public const int Euckr = 0x0b;
                    public const int Johab = 0x0c;
                    public const int Easteuro = 0x0d;
                    public const int Baltic = 0x0e;
                    public const int Greek = 0x0f;
                    public const int Russian = 0x10;
                    public const int Symbol = 0x11;
                    public const int Turkish = 0x12;
                    public const int Hebrew = 0x13;
                    public const int Arabic = 0x14;
                    public const int Thai = 0x15;
                    public const int Vietnamese = 0x16;
                    public const int Mac = 0x17;
                    public const int Oem = 0x18;
                    public const int Default = 0x19;
                    public const int Utf32 = 0x1b;
                    public const int Utf32_be = 0x1c;
                    public const int Binary = 0x1a;
                    public const int LF = 0x40;
                    public const int CR = 0x80;

                    //SAVEASの他のオプションの数値指定
                    public const int Bom = 0x0600;
                    public const int NoBom = 0x0400;
                    public const int Selection = 0x2000;

                    //OPENFILEの他のオプションの数値指定
                    public const int NoAddHist = 0x0100;
                    public const int WS = 0x0800;
                    public const int WB = 0x1000;
                }

                public static partial class SearchOption {
                    //searchoption(検索関係)
                    public const int Word =                 0x00000001;
                    public const int Casesense =            0x00000002;
                    public const int NoCasesense =          0x00000000;
                    public const int Regular =              0x00000010;
                    public const int NoRegular =            0x00000000;
                    public const int Fuzzy =                0x00000020;
                    public const int Hilight =              0x00003800;
                    public const int NoHilight =            0x00002000;
                    public const int LinkNext =             0x00000080;
                    public const int Loop =                 0x01000000;

                    //searchoption(マスク関係)
                    public const int MaskComment =          0x00020000;
                    public const int MaskIfdef =            0x00040000;
                    public const int MaskNormal =           0x00010000;
                    public const int MaskScript =           0x00080000;
                    public const int MaskString =           0x00100000;
                    public const int MaskTag =              0x00200000;
                    public const int MaskOnly =             0x00400000;
                    public const int FEnableMaskFlags =     0x00800000;

                    //searchoption(置換関係)
                    public const int FEnableReplace =       0x00000004;
                    public const int Ask =                  0x00000008;
                    public const int NoClose =              0x02000000;

                    //searchoption(grep関係)
                    public const int SubDir =               0x00000100;
                    public const int Icon =                 0x00000200;
                    public const int Filelist =             0x00000040;
                    public const int FullPath =             0x00000400;
                    public const int OutputSingle =         0x10000000;
                    public const int OutputSameTab =        0x20000000;

                    //searchoption(grepして置換関係)
                    public const int BackUp =               0x04000000;
                    public const int Preview =              0x08000000;
                    
                    // searchoption2を使うよ、というフラグ。なんと、int32_maxを超えているので、特殊な処理が必要。
                    public static long FEnableSearchOption2
                    {
                        get
                        {
                            if (IntPtr.Size == 4) { return -0x80000000; } else { return 0x80000000; }
                        }
                    }
                }

                public static partial class SearchOption2 {
                    //searchoption2
                    public const int UnMatch =              0x00000001;
                    public const int InColorMarker =        0x00000002;
                    public const int FGrepFormColumn =      0x00000008;
                    public const int FGrepFormHitOnly =     0x00000010;
                    public const int FGrepFormSortDate =    0x00000020;
                }
            }
        }
    }
}

/*
 * Copyright (C) 2021-2024 Akitsugu Komiyama
 * under the MIT License
 **/


namespace HmNetCOM
{
    // このインターフェイスは秀丸マクロのjsmode(WebView2)でCOMを呼び出す際に必要
    interface IHmMacroCOMVar
    {
        object DllToMacro();
        int MacroToDll(object variable);
        int MethodToDll(String dllfullpath, String typefullname, String methodname, String message_param);
    }

    public partial class HmMacroCOMVar {
        private const string HmMacroCOMVarInterface = "afcc1751-bbca-4d55-98e5-59221e98694f";
    }
}




namespace HmNetCOM
{
    // 秀丸のCOMから呼び出して、マクロ⇔COMといったように、マクロとプログラムで変数値を互いに伝搬する
    [ComVisible(true)]
#if (NET || NETCOREAPP3_1)
#else
    [ClassInterface(ClassInterfaceType.None)]
#endif
    [Guid(HmMacroCOMVarInterface)]
    public partial class HmMacroCOMVar : IHmMacroCOMVar, Hm.IComSupportX64
    {
        private static object marcroVar = null;
        public object DllToMacro()
        {
            return marcroVar;
        }
        public int MacroToDll(object variable)
        {
            marcroVar = variable;
            return 1;
        }
        public int MethodToDll(String dllfullpath, String typefullname, String methodname, String message_param)
        {
            marcroVar = message_param;

            try
            {
                MethodToDllHelper(dllfullpath, typefullname, methodname, message_param);
                return 1;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e);
            }
            return 0;
        }

        private void TraceMethodInfo(String assm_path, String class_name, String method_name)
        {
            System.Diagnostics.Trace.WriteLine("アセンブリパス   :" + assm_path);
            System.Diagnostics.Trace.WriteLine("名前空間.クラス名:" + class_name);
            System.Diagnostics.Trace.WriteLine("メソッド名       :" + method_name);
        }
        private static void TraceExceptionInfo(Exception e)
        {
            System.Diagnostics.Trace.WriteLine(e.GetType());
            System.Diagnostics.Trace.WriteLine(e.Message);
            System.Diagnostics.Trace.WriteLine(e.StackTrace);
        }
        private Object MethodToDllHelper(String assm_path, String class_name, String method_name, String message_param)
        {
            Exception method_ex = null;
            try
            {
                Assembly assm = null;
                Type t = null;

                if (assm_path.Length > 0)
                {
                    assm = Assembly.LoadFile(assm_path);
                    if (assm == null)
                    {
                        System.Diagnostics.Trace.WriteLine("ロード出来ない");
                    }
                    else
                    {
                        // System::Diagnostics::Trace::WriteLine(assm->FullName);
                    }

                    foreach (Type t2 in assm.GetExportedTypes())
                    {
                        if (t2.ToString() == class_name)
                        {
                            t = assm.GetType(class_name);
                        }
                    }
                }
                else
                {
                    t = Type.GetType(class_name);
                }
                if (t == null)
                {
                    System.Diagnostics.Trace.WriteLine("MissingMethodException(クラスもしくはメソッドを見つけることが出来ない):");
                    TraceMethodInfo(assm_path, class_name, method_name);
                    return null;
                }

                // メソッドの定義タイプを探る。
                MethodInfo m;
                try
                {
                    m = t.GetMethod(method_name);
                }
                catch (Exception ex)
                {
                    // 基本コースだと一致してない系の可能性やオーバーロードなど未解決エラーを控えておく
                    // t->GetMethod(...)は論理的には不要だが、「エラー情報のときにわかりやすい情報を.NETに自動で出力してもらう」ためにダミーで呼び出しておく
                    method_ex = ex;

                    // オーバーロードなら1つに解決できるように型情報も含めてmは上書き
                    List<Type> args_types = new List<Type>();
                    args_types.Add(Type.GetType(message_param));
                    m = t.GetMethod(method_name, args_types.ToArray());
                }

                Object o = null;
                try
                {
                    // オーバーロードなら1つに解決できるように型情報も含めてmは上書き
                    List<Object> args_values = new List<Object>();
                    args_values.Add(message_param);
                    o = m.Invoke(null, args_values.ToArray());
                }
                catch (Exception)
                {
                    System.Diagnostics.Trace.WriteLine("指定のメソッドの実行時、例外が発生しました。");
                    throw;
                }
                return o;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("指定のアセンブリやメソッドを特定する前に、例外が発生しました。");
                TraceMethodInfo(assm_path, class_name, method_name);
                if (method_ex != null)
                {
                    TraceExceptionInfo(method_ex);
                }
                TraceExceptionInfo(e);
            }

            return null;

        }
        public bool X64MACRO() {
            return true;
        }
    }

    public partial class HmMacroCOMVar
    {
        static HmMacroCOMVar()
        {
            var h = new HmMacroCOMVar();
            myGuidLabel = h.GetType().GUID.ToString();
            myClassFullName = h.GetType().FullName;
        }

        internal static void SetMacroVar(object obj)
        {
            marcroVar = obj;
        }
        internal static object GetMacroVar()
        {
            return marcroVar;
        }
        private static string myGuidLabel = "";
        private static string myClassFullName = "";

        internal static string GetMyTargetDllFullPath(string thisDllFullPath)
        {
            string myTargetClass = myClassFullName;
            string thisComHostFullPath = System.IO.Path.ChangeExtension(thisDllFullPath, "comhost.dll");
            if (System.IO.File.Exists(thisComHostFullPath))
            {
                return thisComHostFullPath;
            }

            return thisDllFullPath;
        }

        internal static string GetMyTargetClass(string thisDllFullPath)
        {
            string myTargetClass = myClassFullName;
            string thisComHostFullPath = System.IO.Path.ChangeExtension(thisDllFullPath, "comhost.dll");
            if (System.IO.File.Exists(thisComHostFullPath))
            {
                myTargetClass = "{" + myGuidLabel + "}";
            }

            return myTargetClass;
        }

        internal static object GetVar(string var_name)
        {
            string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myTargetDllFullPath = GetMyTargetDllFullPath(myDllFullPath);
            string myTargetClass = GetMyTargetClass(myDllFullPath);
            ClearVar();
            var result = Hm.Macro.Eval($@"
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                #_COM_NET_PINVOKE_MACRO_VAR_RESULT = member(#_COM_NET_PINVOKE_MACRO_VAR, ""MacroToDll"", {var_name});
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                #_COM_NET_PINVOKE_MACRO_VAR_RESULT = 0;
            ");
            if (result.Error != null)
            {
                throw result.Error;
            }
            return HmMacroCOMVar.marcroVar;
        }

        internal static int SetVar(string var_name, object obj)
        {
            string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myTargetDllFullPath = GetMyTargetDllFullPath(myDllFullPath);
            string myTargetClass = GetMyTargetClass(myDllFullPath);
            ClearVar();
            HmMacroCOMVar.marcroVar = obj;
            var result = Hm.Macro.Eval($@"
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                {var_name} = member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
            ");
            if (result.Error != null)
            {
                throw result.Error;
            }
            return 1;
        }

        internal static void ClearVar()
        {
            HmMacroCOMVar.marcroVar = null;
        }

        internal static Hm.Macro.IResult BornMacroScopeMethod(String scopename, String dllfullpath, String typefullname, String methodname)
        {

            string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myTargetDllFullPath = GetMyTargetDllFullPath(myDllFullPath);
            string myTargetClass = GetMyTargetClass(myDllFullPath);
            ClearVar();
            var result = Hm.Macro.Exec.Eval($@"
                #_COM_NET_PINVOKE_METHOD_CALL = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                #_COM_NET_PINVOKE_METHOD_CALL_RESULT = member(#_COM_NET_PINVOKE_METHOD_CALL, ""MethodToDll"", @""{dllfullpath}"", @""{typefullname}"", @""{methodname}"",  R""MACRO_OF_SCOPENAME({scopename})MACRO_OF_SCOPENAME"");
                releaseobject(#_COM_NET_PINVOKE_METHOD_CALL);
                #_COM_NET_PINVOKE_METHOD_CALL_RESULT = 0;
            ");
            return result;
        }
    }


    internal partial class Hm
    {
        public static partial class Macro
        {

            public static partial class Exec
            {
                /// <summary>
                /// 指定のC#のstaticメソッドを「新たなマクロ実行空間」として呼び出す
                /// </summary>
                /// <param name = "message_parameter">文字列パラメータ</param>
                /// <param name = "delegate_method">呼び出したいC#メソッド「public methodname(string message_parameter)の型」に従うメソッドであること</param>
                /// <returns>(Result, Message, Error)</returns>
                public static IResult Method(string message_parameter, Delegate delegate_method)
                {
                    string parameter = message_parameter;
                    // 渡されたメソッドが自分自身のdllと異なるのはダメ
                    if (delegate_method.Method.DeclaringType.Assembly.Location != System.Reflection.Assembly.GetExecutingAssembly().Location)
                    {
                        string message_no_dll_myself = "The Delegate method must in " + System.Reflection.Assembly.GetExecutingAssembly().Location;
                        var result_no_dll_myself = new TResult(0, "", new MissingMethodException(message_no_dll_myself));
                        System.Diagnostics.Trace.WriteLine(result_no_dll_myself);
                        return result_no_dll_myself;
                    }
                    else if (delegate_method.Method.IsStatic && delegate_method.Method.IsPublic)
                    {
                        var ret = HmMacroCOMVar.BornMacroScopeMethod(parameter, delegate_method.Method.DeclaringType.Assembly.Location, delegate_method.Method.DeclaringType.FullName, delegate_method.Method.Name);
                        if (ret.Result > 0) {
                            var result = new TResult(ret.Result, message_parameter, ret.Error);
                            return result;
                        } else {
                            var result = new TResult(ret.Result, ret.Message, ret.Error);
                            return result;
                        }
                    }
                    else if (!delegate_method.Method.IsStatic)
                    {

                        string message_no_static = delegate_method.Method.DeclaringType.FullName + "." + delegate_method.Method.Name + " is not 'STATIC' in " + delegate_method.Method.DeclaringType.Assembly.Location;
                        var result_no_static = new TResult(0, "", new MissingMethodException(message_no_static));
                        System.Diagnostics.Trace.WriteLine(message_no_static);
                        return result_no_static;
                    }
                    else if (!delegate_method.Method.IsPublic)
                    {
                        string message_no_public = delegate_method.Method.DeclaringType.FullName + "." + delegate_method.Method.Name + " is not 'PUBLIC' in " + delegate_method.Method.DeclaringType.Assembly.Location;
                        var result_no_public = new TResult(0, "", new MissingMethodException(message_no_public));
                        System.Diagnostics.Trace.WriteLine(message_no_public);
                        return result_no_public;
                    }
                    string message_missing = delegate_method.Method.DeclaringType.FullName + "." + delegate_method.Method.Name + "is 'MISSING' access in " + delegate_method.Method.DeclaringType.Assembly.Location;
                    var result_missing = new TResult(0, "", new MissingMethodException(delegate_method.Method.Name + " is missing access"));
                    System.Diagnostics.Trace.WriteLine(result_missing);
                    return result_missing;
                }
            }
        }


        public static partial class Macro
        {
            // マクロでの問い合わせ結果系
            public interface IStatementResult
            {
                int Result { get; }
                String Message { get; }
                Exception Error { get; }
                List<Object> Args { get; }
            }


            private class TStatementResult : IStatementResult
            {
                public int Result { get; set; }
                public string Message { get; set; }
                public Exception Error { get; set; }
                public List<Object> Args { get; set; }

                public TStatementResult(int Result, String Message, Exception Error, List<Object> Args)
                {
                    this.Result = Result;
                    this.Message = Message;
                    this.Error = Error;
                    this.Args = new List<object>(Args); // コピー渡し
                }
            }

            private static int statement_base_random = 0;
            /// <summary>
            /// 秀丸マクロの関数のような「命令文」を実行
            /// </summary>
            /// <param name = "statement_name">（関数のような）命令文名</param>
            /// <param name = "args">命令文の引数</param>
            /// <returns>(Result, Args, Message, Error)</returns>
            internal static IStatementResult Statement(string statement_name, params object[] args)
            {
                string funcname = statement_name;
                if (statement_base_random == 0)
                {
                    statement_base_random = new System.Random().Next(Int16.MaxValue) + 1;

                }

                List<KeyValuePair<string, object>> arg_list = SetMacroVarAndMakeMacroKeyArray(args, statement_base_random);

                // keyをリスト化
                var arg_keys = new List<String>();
                foreach (var l in arg_list)
                {
                    arg_keys.Add(l.Key);
                }

                // それを「,」で繋げる
                string args_string = String.Join(", ", arg_keys);
                // それを指定の「文」で実行する形
                string expression = $"{funcname} {args_string};\n";

                // 実行する
                IResult ret = Macro.Eval(expression);
 
                int macro_result = ret.Result;
                if (ret.Error == null)
                {
                    try
                    {
                        Object tmp_var = Macro.Var["result"]; // この中のGetMethodで例外が発生する可能性あり

                        if (IntPtr.Size == 4)
                        {
                            macro_result = (Int32)tmp_var + 0; // 確実に複製を
                        }
                        else
                        {
                            Int64 macro_result64 = (Int64)tmp_var + 0; // 確実に複製を
                            Int32 macro_result32 = (Int32)HmClamp<Int64>(macro_result64, Int32.MinValue, Int32.MaxValue);
                            macro_result = (Int32)macro_result32;
                        }
                    } catch(Exception) {
                    }
                }

                // 成否も含めて結果を入れる。
                IStatementResult result = new TStatementResult(macro_result, ret.Message, ret.Error, new List<Object>());

                // 使ったので削除
                for (int ix = 0; ix < arg_list.Count; ix++)
                {
                    var l = arg_list[ix];
                    if (l.Value is Int32 || l.Value is Int64)
                    {
                        result.Args.Add(Macro.Var[l.Key]);
                        Macro.Var[l.Key] = 0;
                    }
                    else if (l.Value is string)
                    {
                        result.Args.Add(Macro.Var[l.Key]);
                        Macro.Var[l.Key] = "";
                    }

                    else if (l.Value.GetType() == new List<int>().GetType() || l.Value.GetType() == new List<long>().GetType() || l.Value.GetType() == new List<IntPtr>().GetType())
                    {
                        result.Args.Add(l.Value);
                        if (l.Value.GetType() == new List<int>().GetType())
                        {
                            List<int> int_list = (List<int>)l.Value;
                            for (int iix = 0; iix < int_list.Count; iix++)
                            {
                                Macro.Var[l.Key + "[" + iix + "]"] = 0;
                            }
                        }
                        else if (l.Value.GetType() == new List<long>().GetType())
                        {
                            List<long> long_list = (List<long>)l.Value;
                            for (int iix = 0; iix < long_list.Count; iix++)
                            {
                                Macro.Var[l.Key + "[" + iix + "]"] = 0;
                            }
                        }
                        else if (l.Value.GetType() == new List<IntPtr>().GetType())
                        {
                            List<IntPtr> ptr_list = (List<IntPtr>)l.Value;
                            for (int iix = 0; iix < ptr_list.Count; iix++)
                            {
                                Macro.Var[l.Key + "[" + iix + "]"] = 0;
                            }
                        }
                    }
                    else if (l.Value.GetType() == new List<String>().GetType())
                    {
                        result.Args.Add(l.Value);
                        List<String> ptr_list = (List<String>)l.Value;
                        for (int iix = 0; iix < ptr_list.Count; iix++)
                        {
                            Macro.Var[l.Key + "[" + iix + "]"] = "";
                        }
                    }
                    else
                    {
                        result.Args.Add(l.Value);
                    }
                }

                return result;
            }

            // マクロでの問い合わせ結果系
            public interface IFunctionResult
            {
                object Result { get; }
                String Message { get; }
                Exception Error { get; }
                List<Object> Args { get; }
            }

            private class TFunctionResult : IFunctionResult
            {
                public object Result { get; set; }
                public string Message { get; set; }
                public Exception Error { get; set; }
                public List<Object> Args { get; set; }

                public TFunctionResult(object Result, String Message, Exception Error, List<Object> Args)
                {
                    this.Result = Result;
                    this.Message = Message;
                    this.Error = Error;
                    this.Args = new List<object>(Args); // コピー渡し
                }
            }

            private static int funciton_base_random = 0;
            /// <summary>
            /// 秀丸マクロの「関数」を実行
            /// </summary>
            /// <param name = "func_name">関数名</param>
            /// <param name = "args">関数の引数</param>
            /// <returns>(Result, Args, Message, Error)</returns>
            public static IFunctionResult Function(string func_name, params object[] args)
            {
                return _AsFunction<Object>(func_name, args);
            }

            /// <summary>
            /// 秀丸マクロの「関数」を実行。関数だけだと返り値が不明な場合にこの<T>付きを使用する。
            /// </summary>
            /// <param name = "func_name">関数名</param>
            /// <param name = "args">関数の引数</param>
            /// <typeparam name="T">String | int | long | IntPtr | double。関数単体だけ確定されない返り値の型を「文字列タイプ」か「整数タイプ」かに振り分け直す。</typeparam>
            /// <returns>(Result, Args, Message, Error)</returns>
            public static IFunctionResult Function<T>(string func_name, params object[] args)
            {
                return _AsFunction<T>(func_name, args);
            }

            public static IFunctionResult _AsFunction<T>(string func_name, params object[] args)
            {
                string funcname = func_name;
                if (funciton_base_random == 0)
                {
                    funciton_base_random = new System.Random().Next(Int16.MaxValue) + 1;

                }

                List<KeyValuePair<string, object>> arg_list = SetMacroVarAndMakeMacroKeyArray(args, funciton_base_random);

                // keyをリスト化
                var arg_keys = new List<String>();
                foreach (var l in arg_list)
                {
                    arg_keys.Add(l.Key);
                }

                // それを「,」で繋げる
                string args_string = String.Join(", ", arg_keys);
                // それを指定の「関数」で実行する形
                string expression = "";

                string result_temp = "";
                Macro.IResult eval_result = new TResult(-1, "", null);
                if (typeof(T)==typeof(int) || typeof(T)==typeof(long) || typeof(T)==typeof(IntPtr) || typeof(T) == typeof(double))
                {
                    expression = $"{funcname}({args_string})";
                    result_temp = "##_tmp_dll_expression_ret";
                    string eval_expresson = result_temp + " = " + expression + ";\n";
                    eval_result = Eval(eval_expresson);
                    expression = result_temp;
                } else if (typeof(T)==typeof(String)) {
                    expression = $"{funcname}({args_string})";
                    result_temp = "$$_tmp_dll_expression_ret";
                    string eval_expresson = result_temp + " = " + expression + ";\n";
                    eval_result = Eval(eval_expresson);
                    expression = result_temp;
                } else {
                    expression = $"{funcname}({args_string})";
                }
                //----------------------------------------------------------------
                TFunctionResult result = new TFunctionResult(null, "", null, new List<Object>());
                result.Args = new List<object>();

                Object ret = null;
                try
                {
                    ret = Macro.Var[expression]; // この中のGetMethodで例外が発生する可能性あり

                    if (ret.GetType().Name != "String")
                    {
                        if (IntPtr.Size == 4)
                        {
                            result.Result = (Int32)ret + 0; // 確実に複製を
                            result.Message = "";
                            result.Error = null;
                        }
                        else
                        {
                            result.Result = (Int64)ret + 0; // 確実に複製を
                            result.Message = "";
                            result.Error = null;
                        }
                    }
                    else
                    {
                        result.Result = (String)ret + ""; // 確実に複製を
                        result.Message = "";
                        result.Error = null;
                    }

                }
                catch (Exception e)
                {
                    result.Result = null;
                    result.Message = "";
                    result.Error = e;
                }

                if (result_temp.StartsWith("#")) {
                    Macro.Var[result_temp] = 0;
                    if (eval_result?.Error != null) {
                        result.Result = null;
                        result.Message = "";
                        result.Error = eval_result.Error;
                    }
                } else if (result_temp.StartsWith("$")) {
                    Macro.Var[result_temp] = "";
                    if (eval_result?.Error != null) {
                        result.Result = null;
                        result.Message = "";
                        result.Error = eval_result.Error;
                    }
                }

                // 使ったので削除
                for (int ix = 0; ix < arg_list.Count; ix++)
                {
                    var l = arg_list[ix];
                    if (l.Value is Int32 || l.Value is Int64)
                    {
                        result.Args.Add(Macro.Var[l.Key]);
                        Macro.Var[l.Key] = 0;
                    }
                    else if (l.Value is string)
                    {
                        result.Args.Add(Macro.Var[l.Key]);
                        Macro.Var[l.Key] = "";
                    }

                    else if (l.Value.GetType() == new List<int>().GetType() || l.Value.GetType() == new List<long>().GetType() || l.Value.GetType() == new List<IntPtr>().GetType())
                    {
                        result.Args.Add(l.Value);
                        if (l.Value.GetType() == new List<int>().GetType())
                        {
                            List<int> int_list = (List<int>)l.Value;
                            for (int iix = 0; iix < int_list.Count; iix++)
                            {
                                Macro.Var[l.Key + "[" + iix + "]"] = 0;
                            }
                        }
                        else if (l.Value.GetType() == new List<long>().GetType())
                        {
                            List<long> long_list = (List<long>)l.Value;
                            for (int iix = 0; iix < long_list.Count; iix++)
                            {
                                Macro.Var[l.Key + "[" + iix + "]"] = 0;
                            }
                        }
                        else if (l.Value.GetType() == new List<IntPtr>().GetType())
                        {
                            List<IntPtr> ptr_list = (List<IntPtr>)l.Value;
                            for (int iix = 0; iix < ptr_list.Count; iix++)
                            {
                                Macro.Var[l.Key + "[" + iix + "]"] = 0;
                            }
                        }
                    }
                    else if (l.Value.GetType() == new List<String>().GetType())
                    {
                        result.Args.Add(l.Value);
                        List<String> ptr_list = (List<String>)l.Value;
                        for (int iix = 0; iix < ptr_list.Count; iix++)
                        {
                            Macro.Var[l.Key + "[" + iix + "]"] = "";
                        }
                    }
                    else
                    {
                        result.Args.Add(l.Value);
                    }
                }

                return result;
            }

            private static List<KeyValuePair<string, object>> SetMacroVarAndMakeMacroKeyArray(object[] args, int base_random)
            {
                var arg_list = new List<KeyValuePair<String, Object>>();
                int cur_random = new Random().Next(Int16.MaxValue) + 1;
                foreach (var value in args)
                {
                    bool success = false;
                    cur_random++;
                    object normalized_arg = null;
                    // Boolean型であれば、True:1 Flase:0にマッピングする
                    if (value is bool)
                    {
                        success = true;
                        if ((bool)value == true)
                        {
                            normalized_arg = 1;
                        }
                        else
                        {
                            normalized_arg = 0;
                        }
                    }

                    if (value is string || value is StringBuilder)
                    {
                        success = true;
                        normalized_arg = value.ToString();
                    }

                    // 配列の場合を追加
                    if (!success)
                    {
                        if (value.GetType() == new List<int>().GetType())
                        {
                            success = true;
                            normalized_arg = new List<int>((List<int>)value);
                        }
                        if (value.GetType() == new List<long>().GetType())
                        {
                            success = true;
                            normalized_arg = new List<long>((List<long>)value);
                        }
                        if (value.GetType() == new List<IntPtr>().GetType())
                        {
                            success = true;
                            normalized_arg = new List<IntPtr>((List<IntPtr>)value);
                        }
                    }

                    if (!success)
                    {
                        if (value.GetType() == new List<string>().GetType())
                        {
                            success = true;
                            normalized_arg = new List<String>((List<String>)value);
                        }
                    }
                    // 以上配列の場合を追加

                    if (!success)
                    {
                        // 32bit
                        if (IntPtr.Size == 4)
                        {
                            // まずは整数でトライ
                            Int32 itmp = 0;
                            try
                            {
                                // intでもIntPtrでもないならば...
                                if (value.GetType() != typeof(int).GetType() && value.GetType() != typeof(IntPtr).GetType())
                                {
                                    int itmp_cycle_bit = 0;
                                    long ltmp = 0;
                                    bool suc = Int64.TryParse(value.ToString(), out ltmp);
                                    if (suc)
                                    {
                                        success = LongToInt((long)ltmp, out itmp_cycle_bit);
                                        itmp = itmp_cycle_bit;
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                            if (!success)
                            {
                                success = Int32.TryParse(value.ToString(), out itmp);
                            }
                            if (success == true)
                            {
                                itmp = HmClamp<Int32>(itmp, Int32.MinValue, Int32.MaxValue);
                                normalized_arg = itmp;
                            }

                            else
                            {
                                // 次に少数でトライ
                                Double dtmp = 0;
                                if (IsDoubleNumeric(value))
                                {
                                    dtmp = (double)value;
                                    success = true;
                                }
                                else
                                {
                                    success = double.TryParse(value.ToString(), out dtmp);
                                }
                                if (success)
                                {
                                    dtmp = HmClamp<double>(dtmp, Int32.MinValue, Int32.MaxValue);
                                    normalized_arg = (Int32)(dtmp);
                                }

                                else
                                {
                                    normalized_arg = 0;
                                }
                            }
                        }

                        // 64bit
                        else
                        {
                            // まずは整数でトライ
                            Int64 itmp = 0;
                            success = Int64.TryParse(value.ToString(), out itmp);

                            if (success == true)
                            {
                                itmp = HmClamp<Int64>(itmp, Int64.MinValue, Int64.MaxValue);
                                normalized_arg = itmp;
                            }

                            else
                            {
                                // 次に少数でトライ
                                Double dtmp = 0;
                                if (IsDoubleNumeric(value))
                                {
                                    dtmp = (double)value;
                                    success = true;
                                }
                                else
                                {
                                    success = double.TryParse(value.ToString(), out dtmp);
                                }
                                if (success)
                                {
                                    dtmp = HmClamp<double>(dtmp, Int64.MinValue, Int64.MaxValue);
                                    normalized_arg = (Int64)(dtmp);
                                }
                                else
                                {
                                    normalized_arg = 0;
                                }
                            }
                        }
                    }


                    // 成功しなかった
                    if (!success)
                    {
                        normalized_arg = value.ToString();
                    }

                    if (normalized_arg is Int32 || normalized_arg is Int64)
                    {
                        string key = "#AsMacroArs_" + base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        Macro.Var[key] = normalized_arg;
                    }
                    else if (normalized_arg is string)
                    {
                        string key = "$AsMacroArs_" + base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        Macro.Var[key] = normalized_arg;
                    }
                    else if (value.GetType() == new List<int>().GetType() || value.GetType() == new List<long>().GetType() || value.GetType() == new List<IntPtr>().GetType())
                    {
                        string key = "$AsIntArrayOfMacroArs_" + base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        if (value.GetType() == new List<int>().GetType())
                        {
                            List<int> int_list = (List<int>)value;
                            for (int iix = 0; iix < int_list.Count; iix++)
                            {
                                Macro.Var[key + "[" + iix + "]"] = int_list[iix];
                            }
                        }
                        else if (value.GetType() == new List<long>().GetType())
                        {
                            List<long> long_list = (List<long>)value;
                            for (int iix = 0; iix < long_list.Count; iix++)
                            {
                                Macro.Var[key + "[" + iix + "]"] = long_list[iix];
                            }
                        }
                        else if (value.GetType() == new List<IntPtr>().GetType())
                        {
                            List<IntPtr> ptr_list = (List<IntPtr>)value;
                            for (int iix = 0; iix < ptr_list.Count; iix++)
                            {
                                Macro.Var[key + "[" + iix + "]"] = ptr_list[iix];
                            }
                        }
                    }
                    else if (value.GetType() == new List<string>().GetType())
                    {
                        string key = "$AsStrArrayOfMacroArs_" + base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        List<String> str_list = (List<String>)value;
                        for (int iix = 0; iix < str_list.Count; iix++)
                        {
                            Macro.Var[key + "[" + iix + "]"] = str_list[iix];
                        }
                    }
                }
                return arg_list;
            }


            internal static TMacroVar Var = new TMacroVar();
            internal sealed class TMacroVar
            {
                /// <summary>
                /// 対象の「秀丸マクロ変数名」への読み書き
                /// </summary>
                /// <param name = "var_name">変数のシンボル名</param>
                /// <param name = "value">書き込みの場合、代入する値</param>
                /// <returns>読み取りの場合は、対象の変数の値</returns>
                public Object this[String var_name]
                {
                    get
                    {
                        return GetMethod(var_name);
                    }
                    set
                    {
                        value = SetMethod(var_name, value);
                    }
                }

                private static object SetMethod(string var_name, object value)
                {
                    if (var_name.StartsWith("#"))
                    {
                        Object result = new Object();

                        // Boolean型であれば、True:1 Flase:0にマッピングする
                        if (value is bool)
                        {
                            if ((Boolean)value == true)
                            {
                                value = 1;
                            }
                            else
                            {
                                value = 0;
                            }
                        }

                        // 32bit
                        if (IntPtr.Size == 4)
                        {

                            // まずは整数でトライ
                            Int32 itmp = 0;
                            bool success = false;
                            try
                            {
                                // intでもIntPtrでもないならば...
                                if (value.GetType() != typeof(int).GetType() && value.GetType() != typeof(IntPtr).GetType())
                                {
                                    int itmp_cycle_bit = 0;
                                    long ltmp = 0;
                                    bool suc = Int64.TryParse(value.ToString(), out ltmp);
                                    if (suc)
                                    {
                                        success = LongToInt((long)ltmp, out itmp_cycle_bit);
                                        itmp = itmp_cycle_bit;
                                    }
                                }
                            }
                            catch(Exception)
                            {

                            }
                            if (!success)
                            {
                                success = Int32.TryParse(value.ToString(), out itmp);
                            }

                            if (success == true)
                            {
                                itmp = HmClamp<Int32>(itmp, Int32.MinValue, Int32.MaxValue);
                                result = itmp;
                            }

                            else
                            {
                                // 次に少数でトライ
                                Double dtmp = 0;
                                if (IsDoubleNumeric(value))
                                {
                                    dtmp = (double)value;
                                    success = true;
                                }
                                else
                                {
                                    success = double.TryParse(value.ToString(), out dtmp);
                                }
                                if (success)
                                {
                                    dtmp = HmClamp<double>(dtmp, Int32.MinValue, Int32.MaxValue);
                                    result = (Int32)(dtmp);
                                }

                                else
                                {
                                    result = 0;
                                }
                            }
                        }

                        // 64bit
                        else
                        {
                            // まずは整数でトライ
                            Int64 itmp = 0;
                            bool success = Int64.TryParse(value.ToString(), out itmp);

                            if (success == true)
                            {
                                itmp = HmClamp<Int64>(itmp, Int64.MinValue, Int64.MaxValue);
                                result = itmp;
                            }

                            else
                            {
                                // 次に少数でトライ
                                Double dtmp = 0;
                                if (IsDoubleNumeric(value))
                                {
                                    dtmp = (double)value;
                                    success = true;
                                }
                                else
                                {
                                    success = double.TryParse(value.ToString(), out dtmp);
                                }
                                if (success)
                                {
                                    dtmp = HmClamp<double>(dtmp, Int64.MinValue, Int64.MaxValue);
                                    result = (Int64)(dtmp);
                                }
                                else
                                {
                                    result = 0;
                                }
                            }
                        }
                        HmMacroCOMVar.SetVar(var_name, value);
                        HmMacroCOMVar.ClearVar();
                    }

                    else // if (var_name.StartsWith("$")
                    {

                        String result = value.ToString();
                        HmMacroCOMVar.SetVar(var_name, value);
                        HmMacroCOMVar.ClearVar();
                    }

                    return value;
                }

                private static object GetMethod(string var_name)
                {
                    HmMacroCOMVar.ClearVar();
                    Object ret = HmMacroCOMVar.GetVar(var_name);
                    if (ret.GetType().Name != "String")
                    {
                        if (IntPtr.Size == 4)
                        {
                            try {
                                return (Int32)ret + 0;
                            } catch(Exception) {
                            }
                            return (Int32)(dynamic)ret + 0; // 確実に複製を
                        }
                        else
                        {
                            try {
                                return (Int64)ret + 0;
                            } catch(Exception) {
                            }
                            return (Int64)(dynamic)ret + 0; // 確実に複製を
                        }
                    }
                    else
                    {
                        return (String)ret + ""; // 確実に複製を
                    }
                }
            }
        }
    }
}


namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class OutputPane
        {
            private static UnManagedDll hmOutputPaneHandle = null;

            // OutputPaneから出ている関数群
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TOutputPane_Output(IntPtr hHidemaruWindow, byte[] encode_data);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TOutputPane_OutputW(IntPtr hHidemaruWindow, [MarshalAs(UnmanagedType.LPWStr)] String pwszmsg);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TOutputPane_Push(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TOutputPane_Pop(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr TOutputPane_GetWindowHandle(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TOutputPane_SetBaseDir(IntPtr hHidemaruWindow, byte[] encode_data);

            private static TOutputPane_Output pOutputPane_Output;
            private static TOutputPane_OutputW pOutputPane_OutputW;
            private static TOutputPane_Push pOutputPane_Push;
            private static TOutputPane_Pop pOutputPane_Pop;
            private static TOutputPane_GetWindowHandle pOutputPane_GetWindowHandle;
            private static TOutputPane_SetBaseDir pOutputPane_SetBaseDir;

            static OutputPane()
            {
                try
                {
                    string exedir = System.IO.Path.GetDirectoryName(GetHidemaruExeFullPath());
                    hmOutputPaneHandle = new UnManagedDll(Path.Combine(exedir, "HmOutputPane.dll"));
                    pOutputPane_Output = hmOutputPaneHandle.GetProcDelegate<TOutputPane_Output>("Output");
                    pOutputPane_Push = hmOutputPaneHandle.GetProcDelegate<TOutputPane_Push>("Push");
                    pOutputPane_Pop = hmOutputPaneHandle.GetProcDelegate<TOutputPane_Pop>("Pop");
                    pOutputPane_GetWindowHandle = hmOutputPaneHandle.GetProcDelegate<TOutputPane_GetWindowHandle>("GetWindowHandle");

                    if (Version >= 877)
                    {
                        pOutputPane_SetBaseDir = hmOutputPaneHandle.GetProcDelegate<TOutputPane_SetBaseDir>("SetBaseDir");
                    }
                    if (Version >= 898)
                    {
                        pOutputPane_OutputW = hmOutputPaneHandle.GetProcDelegate<TOutputPane_OutputW>("OutputW");
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }
            }

            /// <summary>
            /// アウトプット枠への文字列の出力。
            /// 改行するには「\r\n」といったように「\r」も必要。
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int Output(string message)
            {
                try
                {
    			    if (pOutputPane_OutputW != null) {
                        int result = pOutputPane_OutputW(Hm.WindowHandle, message);
                        return result;
	    		    } else {
                        byte[] encode_data = HmOriginalEncodeFunc.EncodeWStringToOriginalEncodeVector(message);
                        int result = pOutputPane_Output(Hm.WindowHandle, encode_data);
                        return result;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }

            /// <summary>
            /// アウトプット枠にある文字列の一時退避
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int Push()
            {
                return pOutputPane_Push(Hm.WindowHandle); ;
            }

            /// <summary>
            /// Pushによって一時退避した文字列の復元
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int Pop()
            {
                return pOutputPane_Pop(Hm.WindowHandle); ;
            }

            /// <summary>
            /// アウトプット枠にある文字列のクリア
            /// </summary>
            /// <returns>現在のところ、成否を指し示す値は返ってこない</returns>
            public static int Clear()
            {
                //1009=クリア
                IntPtr r = OutputPane.SendMessage(1009);
                int ret = (int)HmClamp<long>((long)r, Int32.MinValue, Int32.MaxValue);
                return ret;
            }

            /// <summary>
            /// アウトプット枠のWindowHandle
            /// </summary>
            /// <returns>アウトプット枠のWindowHandle</returns>
            public static IntPtr WindowHandle
            {
                get
                {
                    return pOutputPane_GetWindowHandle(Hm.WindowHandle);
                }
            }

            /// <summary>
            /// アウトプット枠へのSendMessage
            /// </summary>
            /// <returns>SendMessageの返り値そのまま</returns>
            public static IntPtr SendMessage(int commandID)
            {
                IntPtr result = Hm.SendMessage(OutputPane.WindowHandle, 0x111, (IntPtr)commandID, IntPtr.Zero);
                return result;
            }

            /// <summary>
            /// アウトプット枠のベースとなるディレクトリの設定
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int SetBaseDir(string dirpath)
            {
                if (Version < 877)
                {
                    throw new MissingMethodException("HmOutputPane_SetBaseDir_Exception");
                }

                try
                {
                    if (pOutputPane_SetBaseDir == null)
                    {
                        throw new MissingMethodException("HmOutputPane_SetBaseDir_Exception");
                    }

                    byte[] encode_data = HmOriginalEncodeFunc.EncodeWStringToOriginalEncodeVector(dirpath);
                    int result = pOutputPane_SetBaseDir(Hm.WindowHandle, encode_data);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }
        }
    }
}

namespace HmNetCOM
{
    internal partial class Hm
    {
        public static partial class ExplorerPane
        {
            private static UnManagedDll hmExplorerPaneHandle = null;

            // ExplorerPaneから出ている関数群
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TExplorerPane_SetMode(IntPtr hHidemaruWindow, IntPtr mode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TExplorerPane_GetMode(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TExplorerPane_LoadProject(IntPtr hHidemaruWindow, byte[] encode_project_file_path);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TExplorerPane_SaveProject(IntPtr hHidemaruWindow, byte[] encode_project_file_path);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr TExplorerPane_GetProject(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr TExplorerPane_GetWindowHandle(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int TExplorerPane_GetUpdated(IntPtr hHidemaruWindow);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr TExplorerPane_GetCurrentDir(IntPtr hHidemaruWindow);

            private static TExplorerPane_SetMode pExplorerPane_SetMode;
            private static TExplorerPane_GetMode pExplorerPane_GetMode;
            private static TExplorerPane_LoadProject pExplorerPane_LoadProject;
            private static TExplorerPane_SaveProject pExplorerPane_SaveProject;
            private static TExplorerPane_GetProject pExplorerPane_GetProject;
            private static TExplorerPane_GetWindowHandle pExplorerPane_GetWindowHandle;
            private static TExplorerPane_GetUpdated pExplorerPane_GetUpdated;
            private static TExplorerPane_GetCurrentDir pExplorerPane_GetCurrentDir;

            static ExplorerPane()
            {
                try
                {
                    string exedir = System.IO.Path.GetDirectoryName(GetHidemaruExeFullPath());
                    hmExplorerPaneHandle = new UnManagedDll(exedir + @"\HmExplorerPane.dll");
                    pExplorerPane_SetMode = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_SetMode>("SetMode");
                    pExplorerPane_GetMode = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_GetMode>("GetMode");
                    pExplorerPane_LoadProject = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_LoadProject>("LoadProject");
                    pExplorerPane_SaveProject = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_SaveProject>("SaveProject");
                    pExplorerPane_GetProject = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_GetProject>("GetProject");
                    pExplorerPane_GetUpdated = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_GetUpdated>("GetUpdated");
                    pExplorerPane_GetWindowHandle = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_GetWindowHandle>("GetWindowHandle");

                    if (Version >= 885)
                    {
                        pExplorerPane_GetCurrentDir = hmExplorerPaneHandle.GetProcDelegate<TExplorerPane_GetCurrentDir>("GetCurrentDir");
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }
            }

            /// <summary>
            /// ファイルマネージャ枠のモードの設定
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int SetMode(int mode)
            {
                try
                {
                    int result = pExplorerPane_SetMode(Hm.WindowHandle, (IntPtr)mode);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }

            /// <summary>
            /// ファイルマネージャ枠のモードの取得
            /// </summary>
            /// <returns>モードの値</returns>
            public static int GetMode()
            {
                try
                {
                    int result = pExplorerPane_GetMode(Hm.WindowHandle);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }

            /// <summary>
            /// ファイルマネージャ枠に指定のファイルのプロジェクトを読み込む
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int LoadProject(string filepath)
            {
                try
                {
                    byte[] encode_data = HmOriginalEncodeFunc.EncodeWStringToOriginalEncodeVector(filepath);
                    int result = pExplorerPane_LoadProject(Hm.WindowHandle, encode_data);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }

            /// <summary>
            /// ファイルマネージャ枠のプロジェクトを指定ファイルに保存
            /// </summary>
            /// <returns>失敗なら0、成功なら0以外</returns>
            public static int SaveProject(string filepath)
            {
                try
                {
                    byte[] encode_data = HmOriginalEncodeFunc.EncodeWStringToOriginalEncodeVector(filepath);
                    int result = pExplorerPane_SaveProject(Hm.WindowHandle, encode_data);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }

            /// <summary>
            /// ファイルマネージャ枠にプロジェクトを読み込んでいるならば、そのファイルパスを取得する
            /// </summary>
            /// <returns>ファイルのフルパス。読み込んでいなければnull</returns>
            public static string GetProject()
            {
                try
                {
                    IntPtr startpointer = pExplorerPane_GetProject(Hm.WindowHandle);
                    List<byte> blist = GetPointerToByteArray(startpointer);

                    string project_name = HmOriginalDecodeFunc.DecodeOriginalEncodeVector(blist);

                    if (String.IsNullOrEmpty(project_name))
                    {
                        return null;
                    }
                    return project_name;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return null;
            }

            private static List<byte> GetPointerToByteArray(IntPtr startpointer)
            {
                List<byte> blist = new List<byte>();

                int index = 0;
                while (true)
                {
                    var b = Marshal.ReadByte(startpointer, index);

                    blist.Add(b);

                    // 文字列の終端はやはり0
                    if (b == 0)
                    {
                        break;
                    }

                    index++;
                }

                return blist;
            }

            /// <summary>
            /// ファイルマネージャ枠のカレントディレクトリを返す
            /// </summary>
            /// <returns>カレントディレクトリのフルパス。読み損ねた場合はnull</returns>
            public static string GetCurrentDir()
            {
                if (Version < 885)
                {
                    throw new MissingMethodException("HmOutputPane_GetCurrentDir_Exception");
                }
                try
                {
                    if (pExplorerPane_GetCurrentDir != null) {
                        IntPtr startpointer = pExplorerPane_GetCurrentDir(Hm.WindowHandle);
                        List<byte> blist = GetPointerToByteArray(startpointer);

                        string currentdir_name = HmOriginalDecodeFunc.DecodeOriginalEncodeVector(blist);

                        if (String.IsNullOrEmpty(currentdir_name))
                        {
                            return null;
                        }
                        return currentdir_name;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return null;
            }

            /// <summary>
            /// ファイルマネージャ枠が「プロジェクト」表示のとき、更新された状態であるかどうかを返します
            /// </summary>
            /// <returns>更新状態なら1、それ以外は0</returns>
            public static int GetUpdated()
            {
                try
                {
                    int result = pExplorerPane_GetUpdated(Hm.WindowHandle);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

                return 0;
            }

            /// <summary>
            /// ファイルマネージャ枠のWindowHandle
            /// </summary>
            /// <returns>ファイルマネージャ枠のWindowHandle</returns>
            public static IntPtr WindowHandle
            {
                get
                {
                    return pExplorerPane_GetWindowHandle(Hm.WindowHandle);
                }
            }

            /// <summary>
            /// ファイルマネージャ枠へのSendMessage
            /// </summary>
            /// <returns>SendMessageの返り値そのまま</returns>
            public static IntPtr SendMessage(int commandID)
            {
                //
                // loaddll "HmExplorerPane.dll";
                // #h=dllfunc("GetWindowHandle",hidemaruhandle(0));
                // #ret=sendmessage(#h,0x111/*WM_COMMAND*/,251,0); //251=１つ上のフォルダ
                //
                return Hm.SendMessage(ExplorerPane.WindowHandle, 0x111, (IntPtr)commandID, IntPtr.Zero);
            }

        }
    }
}


namespace HmNetCOM
{
    internal partial class Hm
    {
        internal static partial class HmOriginalDecodeFunc
        {
            static bool IsSTARTUNI_inline(uint byte4)
            {
                return (byte4 & 0xF4808000) == 0x04808000;
            }

            static long MakeWord(long low, long high)
            {
                return ((long)high << 8) | low;
            }

            static char GetUnicodeInText(byte[] pchSrc)
            {
                long value = MakeWord(
                    (pchSrc[1] & 0x7F | ((pchSrc[3] & 0x01) << 7)),
                    (pchSrc[2] & 0x7F | ((pchSrc[3] & 0x02) << 6))
                );

                byte[] byteArray = BitConverter.GetBytes(value);

                byte[] charByte = { byteArray[0], byteArray[1] };

                char wch = BitConverter.ToChar(charByte, 0);

                return wch;
            }

            public static string DecodeOriginalEncodeVector(List<byte> OriginalEncodeData)
            {
                try
                {
                    string result = "";

                    byte[] byteArray = OriginalEncodeData.ToArray();

                    // 一時バッファー用
                    List<byte> tmp_buffer = new List<byte>();
                    int len = OriginalEncodeData.Count;

                    int lastcheckindex = len - 4; // IsSTARTUNI_inline には 4バイト必要
                    if (lastcheckindex < 0)
                    {
                        lastcheckindex = 0;
                    }
                    for (int i = 0; i < len; i++)
                    {
                        // 一般の文字としてはほぼ利用されないスターマーク。
                        if (i <= lastcheckindex && byteArray[i] == '\x1A')
                        {
                            uint StarUni = BitConverter.ToUInt32(byteArray, i);

                            if (IsSTARTUNI_inline(StarUni))
                            {
                                // 今までの分はスターユニコードではないので、通常のSJISとみなし、utf16に変換して足し込み
                                if (tmp_buffer.Count > 0)
                                {
                                    result += System.Text.Encoding.GetEncoding(932).GetString(tmp_buffer.ToArray());
                                    tmp_buffer.Clear();
                                }

                                byte[] starByteArray = BitConverter.GetBytes(StarUni);
                                char wch = GetUnicodeInText(starByteArray);
                                i = i + 3; // 1バイトではなく4バイト消化したので、計算する
                                result += wch;
                                continue;
                            }
                        }
                        tmp_buffer.Add(byteArray[i]);
                    }

                    if (tmp_buffer.Count > 0)
                    {
                        result += System.Text.Encoding.GetEncoding(932).GetString(tmp_buffer.ToArray());
                        tmp_buffer.Clear();
                    }

                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e);
                }

                return "";
            }
        }

        internal static partial class HmOriginalEncodeFunc
        {
            public static readonly ulong[] encode_zen_han_map_compress = {
0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xbc9ef3f247fe9e7f,0xf040000545d580ff,0x5ff5f5fd3dd57b0f,0xf3dfffffffffffff,0xffffffffffbfffff,0xffffffffffffffff,0xffffffffffffffff,0xfffffffffffffd3f,0xfebffffdfffffff,0xffffe97f4577f7c8,0xfffbffffffffffff,0xffffffffffffffff,0xffcfffffffffffff,0xfb99fffffff7fff7,0xc305d01301450008,0x8400007f2dbbdfde,0xec617fffffffffff,0xff0076798681f800,0x241dfffb7d4fe060,0x859ffffffffcff00,0x7fffffc080000001,0xfff10ff9ffbfffce,0xffed46e48069ffff,0xffe401f7061b78c8,0x83ee65e0fc004000,0xffe95f43bbe00ef5,0,0,0x213e7040,0xfffffffffff,0xe18780010000c42c,0x7f847f003be3c004,0x410000000100000c,0x7804000031003140,0x6000000000000008,0x619c40000000c400,0x600004014210800c,0x7d840000338b8000,0x400810000000000d,0x7804020071837400,0x2000000000000000,0x8004000002000000,0x8000040000000003,0x83bc0600304400e0,0x400000000000000d,0x204000000004000,0x4000000000000000,0x6000032000000,0x10000,0x203a0002100000,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xceffcf66fffbfdcc,0xff7ffffffff87fff,0xffffff7ffffffffb,0x4ef9780000000000,0x80028000107bf66,0x603800e384007800,0x2604010608806408,0x4ec5b45549dff,0,0,0,0,0x1090000000840000,0,0x8400fc0096000000,0x869200c00000,0xab10aa00009292,0xbe0000077d7f6230,0xfa4026dacc89,0x1213b5a6230208ac,0xfffe00000,0x700000021ff000,0x187fc0007ff0001f,0xe0000037fc0a,0x7f0000007fe0003,0xc0fcc00400200014,0x3c005e103ffffff,0xf0000000000279e7,0xf03c0e0c0f00000f,0x300002000020003,0xf39ce7b83bffffcf,0xfffffcfdfbdb8000,0,0,0x2800000dfc,0x27fee5c0000ffc0,0xafbeffeef8ffffff,0xffffffffbfffff00,0xfaffffffffe000a3,0x1c000fff605b5c00,0,0xc0849ff87800,0x10a118224900fde0,0x52c07ae0eec0eec0,0x8400080300000000,0,0,0,0,0,0,0,0,0x7f0081c200ff,0x8418,0xffffff8548c0,0x9ffb87fdbfffffff,0xffffffffffeeffdf,0xfdfffbffffffffff,0xffe4000000000003,0x7f555fff555fffff,0x5555fffffffd555,0xfc3fff7d555555ff,0xfff5555555557fdf,0xfcc0ff00ffff,0xfc00ff000000fff0,0xff0000000007,0xfbc7f3f7ffc70006,0xaffffdffffffffff,0xffff0267fbffcfff,0xfffefff8ffffffe5,0xffffffff8000,0xffffffffffffff86,0x39ca1fffffc0ffc0,0x830ffffffffffff,0xffffffffffe00000,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffc90a,0x27ffe00ff0600101,0x8000009018221981,0x2021ffffec0fc,0xf00fc0800000,0x22000000,0xffe00000fffff000,0,0,0xffffffffffffffff,0xffffffffffffffff,0xfffffc00fffffdff,0x7fffffffffff0006,0x600000000000001,0xe900000000670000,0xa000883d80,0,0,0x50180fffc00,0,0x628000183ff0000,0,0,0,0,0xc03000000000c0,0x4d4cccc0000000,0xffffe1e000000000,0xf0001007fc,0x140000002,0,0,0x22003ea0b,0x4000600700000,0x760005c3c5014,0xc0000000,0x50000,0x404508000088404,0xf2ccd21afea84e7f,0x7f7d7f1d77c73dd4,0x775ff775803d037,0xfac867e779048012,0x2065710828018000,0,0xaaffffffff,0xfffc23acfcc3ffce,0xe800000000000000,0,0,0,0,0,0,0xf7fffc0500000000,0x7fffffffffffffff,0xfffff01e7fffffff,0xfffffffffffffe1e,0,0,0,0,0x6040,0,0xf800000,0,0x1004088033100210,0x6441000000001e,0x3000e40000000,0x804000000000000,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0xd1f6cfc242a462d9,0x341707c720005002,0xa6fa6cdbee9e9220,0xeb370fc75c06a318,0xd067c02c00018aec,0x1307d77e00419619,0x12b56ab4c0170601,0x3866c9b55c0316a3,0x6556a637efc0240,0xb3c186280408ae84,0x840448e0000c3d02,0x246546a215060c50,0xe8402ea140202139,0xdffea8282cfe4788,0xa66df744aef8ff06,0xe5c021cc36cc6e6,0x9bb24d040d6293f0,0x193d0a121c61fc05,0x11c46819927d4c4a,0xd54425d7bb0034e3,0xe7858764d82150f1,0xd7e479aa8243fd91,0x280012a186c6b210,0x30fe0b557cfff3a0,0x58ffc015825e02de,0xa2e2400100a0d71c,0x8a3ba0082cd9308e,0xe9c0008066c62025,0xb018a00000350c2,0x8e09130c1000001a,0x9b7000ef01fa8200,0xd00082918000146,0x2400b9000416a80,0x200a801008b808a0,0x83210800a4020a08,0xf08251b29002b065,0x98d5328903848190,0xa123c0404e600001,0x20d01000b8289000,0x93923a0990000968,0xa63004444821cc24,0xc03069e22050a442,0x1400110b009230f2,0x452855cb0c1be17c,0x207186f8257e6182,0x9c3ddb4b5ea1884,0x782412541c882072,0x250d86806b00211,0x504040a984410608,0xc240002022280001,0x20300000000e,0x6058830052400030,0x5802002028a202,0x9400cfbd1e50d4a0,0x95dd05fd3e029ec1,0xbf48b8976fda96a3,0x6ff75222a88440ff,0xc6b7d402cc0b4240,0x8c80042d853ba,0x5000040c0e58634,0x1124ff121400000,0x201ab008004d1200,0x29188004200a01ec,0x4cb21657927d16,0x3218427404159384,0x9d0a0d0178003eff,0x5921072883140780,0x70e19235f0c8dbbb,0x587ddf9147454d8a,0x44aa534c637cd149,0xfdb8f1c2c0e6194c,0x140c03ce8810c495,0x3a65cc7f14020,0x7234a465bcfd8500,0x5c7c2b3193603e8c,0x70054abd180fb70,0xd2c74431c188890f,0x29be14e53593df02,0x2122cae0095a2222,0x13fc800012002baf,0x80eefa374223b891,0xc1d614929082424b,0xb8607f9a0218445c,0x27edf9dcc00f5703,0x61c8404101910027,0x4d850801d22330,0x90cb2291f5f82c12,0x9a41a23050420e20,0x2051308028c0a22,0x20020080814102a0,0x108034260c56a058,0x650628a0f3096a26,0x8400008c18390fd3,0x48747c63208ad80,0x31cac1594d1d814c,0x5e192150e7cc4030,0xb36b1c0b8d874e19,0x785121472fc39a20,0x59c243c4a21228cb,0x4940026c8240bdff,0x90d70f1703be4ba5,0x4243d24bfe25f50b,0x558852ce2411bd0,0xca23d8a6294b5c13,0x78001302607028cc,0xfa86f84d11000b03,0x54051915a385d668,0xaf52da1d1032060,0xcc9f207876a08a45,0x201c6ee0b71dce,0x60224c9881026813,0x513e90c621942055,0x24383953707286e4,0xb9000c162103083,0x88068108b02a1090,0x7000503328a00030,0xc202205d10021e6,0x114a98517a6118d1,0x9a74068208317d39,0x86163a9100191000,0x81591018000983,0xe002852fa0a1b026,0x728cb270601250ff,0x4c68157474a1d000,0x818215304703696,0x4c0000681a6b09c0,0x58a165800092488,0x871964c24aba75f8,0x5f01475df5f00ea,0x1026029901132203,0xa009828d24212325,0x2c585c0600092983,0x62415c00b0186083,0x440008990a0788a8,0xea82410058205440,0xf2000c91c81b5622,0x455144b70388a002,0x840801011808c,0x4622040001f10f0,0x91004500004044,0x841086842000208,0x34a06482f4a0004,0x8a10c4100884042,0x50c2ad01870e045b,0x204801000aca63f,0x839820d561402846,0xe1002200a1096242,0x3a02a150e04c01cc,0x20031d0c730b03,0xc03010010a0a5b0,0x950020000403014,0xa0e6220294264482,0x1740000026623e21,0x4000047bb8619202,0x1050000321008004,0x210808b342e380,0x5c0e54ab1699f1b8,0xa7d487caf759b81,0x23734824141074ce,0x35242cd23040b82b,0x40540001e9008810,0x2388228861e8a2be,0xbb92927a020289e1,0x32a4231b5d222892,0x49d800138e4001a8,0x8300003056900043,0x124002840c925d,0x104013835471008,0x20082c00c7002821,0x402000408192828,0x430e55201161042a,0x4630c82001890804,0x410260400238802a,0xc1c09727a4840121,0xcc27081403229067,0xc05064815b488010,0x809609e98002611f,0xb247810070508a65,0x5880378100638221,0x1db5746df73582e1,0x2ab140d286881640,0x6ddea04050acc20,0x3152def40244880,0xcb5900048e441300,0x8104790151300187,0x900d8a818c081402,0x7054a5916d967046,0xa422228ba1012ab2,0xae348df8e01bb461,0x3e9b821a72827644,0x411079b01fedb7,0x13166aec8c92810,0x4c70201372126576,0x52342e660364805d,0x30ebba1800000000,0,0x2a0,0x50ecc8181f2a5c0,0x37aa082658c322c4,0x46282c00c2509058,0xde18a5c840801215,0x22022a36081bb47,0xf5812b4646d6820,0x1a0a02764c01488c,0x27e0003010415042,0x212dc010612c8e1,0xb0a1142c98c094a7,0xa2c450e195a4183a,0x65eea39b007a17c0,0x810000e52ab36382,0x142045061d50d4,0x400795b57105870,0x7e42038810916ec0,0x8461a08020001519,0x5621223a0b04404,0x452a12898051eb14,0x191e1000a068448c,0x2c20110725f4560,0x28108849400428d9,0x4a74c268000a0809,0x82005da1420c0404,0xd0f215e010f40102,0x89a0c9580afb8060,0x4045840c0c600172,0x2330132020058001,0x68c2b01104050,0x38140018718200,0xb560853084f00d2,0xb2e460a804400911,0x5a154192a20a81,0x2004000120111034,0x8b10a00080012352,0x507460071004250,0xaa0c31567090a507,0x609423422812cd01,0x7c010ccd40803cce,0x2928b00e04300290,0x580c02038a252903,0x53b113a043693025,0x8000202c13000880,0xb0aab39514245b38,0x4280ec12b25ef048,0x2d4c54a2df8ca04b,0x291d223beb1653a2,0xe90a8b74c2981042,0x404b12b90219e905,0x902ab26000000000,0,0x121,0x42aa8420603800e8,0x246e0886e1ffbb9d,0xf9a6503aba24883c,0xdb5ba0000000000,0xb14db00,0x4420004010801022,0x2019023550b11409,0xe1800700208c00,0x29e8844198002a08,0x4d3458404039c002,0x6bd20113010009e,0x14683c5d026110d3,0x2e4e010978000000,0x1b1187e139,0x2c024820267589e4,0xd617df67f10266ca,0x6577fecad5c727ad,0xf961400012a14480,0x4022001268840504,0x200024400104a000,0x7e2a8034683580,0x2154a10828310ca0,0xc3dfc3bf5f060609,0x226cc0200480969,0x8cd2c1722b004182,0x948056b801040140,0x1b540896430f9,0x220180e6f69b2430,0x8940b18800a840a,0x80b20090e8040,0x1280c3864c800080,0x110098e0401046a0,0x4c807032a020850f,0x4000000000000000,0,0,0x100,0xb1717ac0bd84205a,0x112c0e8864000000,0x840a32011,0xa81d801f3e28b7a4,0x6b70ddc91a1ebd8,0xcf5e465830b0a350,0x8ba7524a0920b0d6,0x3ac5664aead44868,0x4e15808892941800,0x1290100800063611,0x7689f1a0480c099c,0x21f0d920511d800,0xa14200,0x1a0000192057280e,0x1468b886c98a0006,0x2458e50000000000,0x3c2048e1808,0x4e80e49520066091,0x4403304000908102,0xb40605312c700000,0x848088,0x880158cb32374910,0x20000a135e36018,0x7c0748a000012c00,0x1003730180b64850,0x82616a107880e400,0x8aecbc104c07a072,0x380212081000280,0,0x5589a00,0x13c2241112b80013,0x4a80c04ec819a020,0x1410020085128b0,0x6a011040020520e4,0x7200000000,0x4c5,0x40987505066400fb,0x80fe8de84810b5c,0x1821400c0080012,0x332801f3282300,0x4040c80000000,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0xedce785ce2fd,0x7c000001210180,0x140018000210328,0x5a080008000700,0x75812000800000,0x6100000000000,0xff53005000511fff,0x705dc41f00000000,0,0xfffffffffd3fc001,0xffd0c13d1fffa6bf,0xddfb1cd577d6fc00,0xda,0xc3c0,0x20008040907300,0,0xffffc030,0,0,0,0x3bd880206000000,0,0x8008,0,0,0x1200009237df,0x7d71fff0800f7fb,0xce9ffa1bb7ef3316,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffffffffffffff,0xffffff0000000000,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0xf000,0x400000,0,0,0x800000000,0x3fffffffc0000,0,0,0,0x600000072d407678,0x829cccccccccffee,0xfffccccc3fc57fff,0xc00007fff3f00003,0,0x3f0000000,0,0x3800,0xf,0,0,0,0xfe000000,0xfbff,0xff9e799fffff8000,0x7f9999f9ee1ff9,0xffffffffffffffff,0xfffffffe7fffffff,0xffffffffffffffff,0xfffffffffdffffff
            };

            private static bool GetZenHanMap(int i)
            {
                if (0 <= i && i <= 0xFFFF)
                {
                    int shift = 64 - i % 64 - 1;
                    int ix = i / 64;

                    ulong temp = encode_zen_han_map_compress[ix];
                    temp = temp >> shift;
                    temp = temp & 0x1;
                    return temp == 1 ? true : false;
                }

                return false;
            }

            private static byte nZenkaku(char wch)
            {
                int ix = wch;
                if (GetZenHanMap(ix))
                {
                    return 0;
                }
                return 8;
            }


            private static List<byte> ToOriginalHmStarUnicode(char wch)
            {
                List<byte> ret = new List<byte>();
                ret.Add(0x1A);
                byte byte2ix = (byte)((byte)0x80 | (byte)(0xFF & wch));
                ret.Add(byte2ix);
                byte byte3ix = (byte)((byte)0x80 | (wch >> 8) & 0xFF);
                ret.Add(byte3ix);
                byte byte4ix = (byte)((byte)((byte)(wch & 0x80) >> 7) + (byte)((wch & 0x8000) >> 14) + (byte)4 + (byte)nZenkaku(wch));
                ret.Add(byte4ix);
                return ret;
            }

            // wchar_tに直接対応していないような古い秀丸では、この特殊な変換マップによる変換をしてバイトコードとして渡す必要がある。
            public static byte[] EncodeWStringToOriginalEncodeVector(string original_string) {
                List<byte> r = new List<byte>();
                foreach (char ch in original_string)
                {
                    List<byte> byte4 = ToOriginalHmStarUnicode(ch);
                    foreach (byte b in byte4)
                    {
                        r.Add(b);
                    }
                }

                r.Add(0);
                return r.ToArray();
            }
        }
    }
}

#if NET
#nullable enable
#endif
