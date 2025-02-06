

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

// 自動的に「現在のファイル」の「リポジトリの変化」や「リモートリポジトリとの変化」を監視し、変化があれば、JavaScriptの関数を非同期で呼び出す。
var gitWatcherComponent = createobject(currentMacroDirectory + "\\HmGitWatcher.comhost.dll", "{CD5AADB6-1A50-436F-85A1-84D72CFAECEB}");

function onGitReposFound(repoFullPath) {
    hidemaruversion("9.25.99"); // なぜか必要。別スレ経由なため、うまく伝搬しないことがある？

    repoFullPath = repoFullPath.replace(/\//g, '\\');
    gRepoFullPath = repoFullPath;

    if (!repoFullPath) {
        // レンダリングペインは消してしまう
        closeRenderPane();
        return;
    }

    showRenderPane();

    startDPIWatcher();

    if (use100MBLimitPreCommitFile) {
        create100MBLimitPreCommitFile(repoFullPath);
    }
}


var updatedRenderPaneStatusRetry; // 初期化しないこと。

function stopUpdatedRenderPaneStatusRetry() {
    if (typeof(updatedRenderPaneStatusRetry) != "undefined") {
        hidemaru.clearInterval(updatedRenderPaneStatusRetry);
    }
}

stopUpdatedRenderPaneStatusRetry();

// この関数は「C#のdllの中」から「非同期」で呼び出される。(JavaScriptとして非同期で呼ばれる)
// 「ローカルリポジトリ」「リモートリポジトリ」との変化を検知した際に呼び出される。
function onGitStatusChange(repoFullPath, gitStatus, gitStatusPorchain, gitCherry) {
    hidemaruversion("9.25.99"); // なぜか必要。別スレ経由なため、うまく伝搬しないことがある？

    repoFullPath = repoFullPath.replace(/\//g, '\\');
    gRepoFullPath = repoFullPath;

    // リポジトリに所属していないならば、
    if (!repoFullPath) {
        // レンダリングペインは消してしまう
        closeRenderPane();
        return;
    }

    // --------------------------------------------------------------------
    // プルする必要があるかどうかの判定
    // --------------------------------------------------------------------
    // derived(派生)している。(github上のコミット進行とローカルリポジトリのコミット進行が異なってしまっている)
    if (gitStatus.indexOf('use "git pull" to merge') !== -1) {
        gitStatus = 2;
    }
    // 普通にプル可能
    else if (gitStatus.indexOf('use "git pull" to update') !== -1) {
        gitStatus = 1;
    }
    // 未知だが、まぁ新たな同士
    else if (gitStatus.indexOf('use "git pull" to ') !== -1) {
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

    function updateRenderPaneButton() {
        if (isRenderPaneReadyStateComplete()) {
            var jsCommand = 'javascript:HmGitWatcher_Update(' + gitStatus + ',' + gitStatusPorchain + ',' + gitCherry + ');';
            updateRenderPane(jsCommand);
            return true;
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

    updatedRenderPaneStatusRetry = hidemaru.setInterval(
        function () {
            if (updatedRenderPaneStatus) {
                stopUpdatedRenderPaneStatusRetry();
                return;
            }

            if (isNotDetectedOperation()) {
                return;
            }

            if (retryCounter > 5) {
                writeOutputPane("HmGitWatcherアイコンの更新失敗");
                stopUpdatedRenderPaneStatusRetry();
                return;
            }

            updatedRenderPaneStatus = updateRenderPaneButton();
            retryCounter++;
        },
        1000
    );
}

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

// 監視コンポーネント・リスタート。
// リポジトリを発見したら「onGitReposFound」を呼び出す。  
// 変化を感じ取ったら「onGitStatusChange」関数を呼び出す。
gitWatcherComponent.ReStart(onGitReposFound, onGitStatusChange);



openRenderPane();



// エラーメッセージ用
function writeOutputPane(msg) {
    if (msg === null) { return; }
    var dll = loaddll("HmOutputPane.dll");
    msg = msg.toString().replace(/\r\n/g, "\n").replace(/\n/g, "\r\n");
    dll.dllFunc.Output(hidemaru.getCurrentWindowHandle(), msg + "\r\n");
}

