

// カレントマクロディレクトリはpostExecMacroMemoryなどしてしまうと拾うことが出来なくなるので、控えておく。
var currentMacroDirectory = currentmacrodirectory();
// レンダリングペイン名
var strRanderPaneName = "HmGitWatcher";
// 本当はグローバルにはしたくないが、グローバルにせざるを得ない。
var gRepoFullPath; // 初期化しないこと。非同期で使っているかもしれない。

// 前回マクロ実行した残骸が残ってるなら、ストップ
if (typeof gitWatcherComponent !== 'undefined') {
    gitWatcherComponent.Stop();
}


function registGitWatcherCommonObjectModel() {

    var net4runtimeFullPath = currentMacroDirectory + "\\HmGitWatcher.fw.dll";
    var net8runtimeFullPath = currentMacroDirectory + "\\HmGitWatcher.comhost.dll";

    var com = null;

    // 自動的に「現在のファイル」の「リポジトリの変化」や「リモートリポジトリとの変化」を監視し、変化があれば、JavaScriptの関数を非同期で呼び出す。
    if (existfile(net4runtimeFullPath)) {
        com = createobject(net4runtimeFullPath, "HmGitWatcher.HmGitWatcher");
        // ウィンドウズ10以降なら原則入っている 「.NET4.8 Framework」による 自動レジストリ登録が失敗している。
        if (!com) {
            writeOutputPane("「.NET 4.8」経由の「自動COM登録」に失敗しました。「.NET 8.0」等、別のものを試してください。");
        }
    }

    if (!com) {

        if (existfile(net8runtimeFullPath)) {
            // .NET8.0ランタイムインストールが必要とはなるが、COM登録インターフェイスの部分だけネイティブdll相当の.NET8.0経由で試みる
            com = createobject(net8runtimeFullPath, "{CD5AADB6-1A50-436F-85A1-84D72CFAECEB}");
            if (!com) {
                writeOutputPane("「.NET 8.0」経由の「自動COM登録」に失敗しました。");
            }
        }
    }

    if (!com) {
         writeOutputPane("コンポーネントを初期化できませんでした。");
    }

    return com;
}

function onGitReposFound(repoFullPath) {

    hidemaruversion(targetHidemaruversion); // なぜか必要。別スレ経由なため、うまく伝搬しないことがある？

    try {
        if (!repoFullPath) {
            // レンダリングペインは消してしまう
            closeRenderPane();
            return;
        }

        repoFullPath = repoFullPath.replace(/\//g, '\\');
        gRepoFullPath = repoFullPath;

        showRenderPane();

        startDPIWatcher();

        if (use100MBLimitPreCommitFile) {
            hidemaru.setTimeout( function() {
                create100MBLimitPreCommitFile(repoFullPath);
            }, 100);
        }
    } catch(e) {
    }
}

var updatedRenderPaneStatusRetry; // 初期化しないこと。

function stopUpdatedRenderPaneStatusRetry() {
    if (typeof (updatedRenderPaneStatusRetry) != "undefined") {
        hidemaru.clearTimeout(updatedRenderPaneStatusRetry);
    }
}

stopUpdatedRenderPaneStatusRetry();


// この関数は「C#のdllの中」から「非同期」で呼び出される。(JavaScriptとして非同期で呼ばれる)
// 「ローカルリポジトリ」「リモートリポジトリ」との変化を検知した際に呼び出される。
function onGitStatusChange(repoFullPath, gitStatus, gitStatusPorchain, gitCherry) {

    hidemaruversion(targetHidemaruversion); // なぜか必要。別スレ経由なため、うまく伝搬しないことがある？

    try {
        // リポジトリに所属していないならば、
        if (!repoFullPath) {
            // レンダリングペインは消してしまう
            closeRenderPane();
            return;
        }

        repoFullPath = repoFullPath.replace(/\//g, '\\');
        gRepoFullPath = repoFullPath;

        // --------------------------------------------------------------------
        // プルする必要があるかどうかの判定
        // --------------------------------------------------------------------
        // プル可能
        if (gitStatus.indexOf('use "git pull"') !== -1) {
            gitStatus = 1;
        }
        // プルする必要はない
        else {
            gitStatus = 0;
        }

        // --------------------------------------------------------------------
        // コミット可能かどうかの判定
        // --------------------------------------------------------------------
        if (gitStatusPorchain) {
            gitStatusPorchain = 1;
        } else {
            gitStatusPorchain = 0;
        }

        // --------------------------------------------------------------------
        // プッシュ可能かどうかの判定
        // --------------------------------------------------------------------
        if (gitCherry) {
            gitCherry = 1;
        } else {
            gitCherry = 0;
        }
    } catch(e) {
    }

    function updateRenderPaneButton() {
        if (isRenderPaneReadyStateComplete()) {
            try {
                var jsCommand = 'javascript:HmGitWatcher_Update(' + gitStatus + ',' + gitStatusPorchain + ',' + gitCherry + ');';
                updateRenderPane(jsCommand);
                return true;
            } catch(e) {}
        }

        return false;
    }

    var updatedRenderPaneStatus = updateRenderPaneButton();
    if (updatedRenderPaneStatus) {
        return;
    }

    // アップデートに失敗している。レンダリングペインが何かの事情でComplete出来ていないのかもしれない。


    var retryCounter = 0; // リトライ回数。何かの事情でずっと更新できない場合に備えて、１秒おきで５回やってもダメだったら諦める。

    stopUpdatedRenderPaneStatusRetry();

    function attemptRenderPaneStatusRetry() {
        if (updatedRenderPaneStatus) {
            stopUpdatedRenderPaneStatusRetry();
            return;
        }

        if (retryCounter > 5) {
            writeOutputPane("HmGitWatcherアイコンの更新に失敗しました。");
            stopUpdatedRenderPaneStatusRetry();
            return;
        }

        if (isNotDetectedOperation()) {
            updatedRenderPaneStatusRetry = hidemaru.setTimeout(attemptRenderPaneStatusRetry, 1000);
            return;
        }

        updatedRenderPaneStatus = updateRenderPaneButton();
        retryCounter++;

        if (updatedRenderPaneStatus) {
            stopUpdatedRenderPaneStatusRetry();
            return;
        }

        if (!updatedRenderPaneStatus) {
            updatedRenderPaneStatusRetry = hidemaru.setTimeout(attemptRenderPaneStatusRetry, 1000);
        }
    }

    updatedRenderPaneStatusRetry = hidemaru.setTimeout(attemptRenderPaneStatusRetry, 0);
}

// 非同期マクロから何か操作すべきではない状態
function isNotDetectedOperation() {
    /*
    ○ 0x00000002 ウィンドウ移動/サイズ変更中
    × 0x00000004 メニュー操作中
    × 0x00000008 システムメニュー操作中
    × 0x00000010 ポップアップメニュー操作中
    ○ 0x00000100 IME入力中
    × 0x00000200 何らかのダイアログ表示中
    × 0x00000400 ウィンドウがDisable状態
    × 0x00000800 非アクティブなタブまたは非表示のウィンドウ
    × 0x00001000 検索ダイアログの疑似モードレス状態
    ○ 0x00002000 なめらかスクロール中
    ○ 0x00004000 中ボタンによるオートスクロール中
    ○ 0x00008000 キーやマウスの操作直後
    ○ 0x00010000 何かマウスのボタンを押している
    × 0x00020000 マウスキャプチャ状態(ドラッグ状態)
    ○ 0x00040000 Hidemaru_CheckQueueStatus相当
    */
    var s = hidemaru.getInputStates();
    var notAllowedMask =
        0x00000004 | 0x00000008 | 0x00000010 |
        0x00000200 | 0x00000400 | 0x00000800 |
        0x00001000 | 0x00020000;
    return (s & notAllowedMask) != 0;
}

var gitWatcherComponent; // 宣言のみ

hidemaru.setTimeout( function() {

    gitWatcherComponent = registGitWatcherCommonObjectModel();
    if (gitWatcherComponent) {
        // 監視コンポーネント・リスタート。
        // リポジトリを発見したら「onGitReposFound」を呼び出す。  
        // 変化を感じ取ったら「onGitStatusChange」関数を呼び出す。
        gitWatcherComponent.ReStart(onGitReposFound, onGitStatusChange);

        openRenderPane();
    }

}, 0);



// エラーメッセージ用
function writeOutputPane(msg) {
    if (msg == null) { return; }
    if (msg == "") { return; }
    var dll = loaddll("HmOutputPane.dll");
    msg = msg.toString().replace(/\r\n/g, "\n").replace(/\n/g, "\r\n");
    dll.dllFunc.Output(hidemaru.getCurrentWindowHandle(), msg + "\r\n");
}

