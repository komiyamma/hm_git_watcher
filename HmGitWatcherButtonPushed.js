var gRepoFullPathAtPushButton = ""; // ボタンを押した瞬間のリポジトリを控えておくため。

function onButtonPushed(command_label) {

    if (!isRenderPaneShowAndVisible()) {
        return;
    }

    gRepoFullPathAtPushButton = gRepoFullPath; // 押した瞬間に
    if (!gRepoFullPathAtPushButton) { return; }

    try {
        switch (command_label) {
            /*
            case "visible":
                // 表示ペインを表示
                showRenderPane();
                break;
            */
            case "pull_all":
                // 全ての変更をプル
                gitPullAll(gRepoFullPathAtPushButton);
                break;
            case "push_all":
                // 全ての変更をプッシュ
                gitPushAll(gRepoFullPathAtPushButton);
                break;
            case "commit_all":
                // コミットダイアログを表示
                gitCommentDialog(gRepoFullPathAtPushButton);
                break;
              case "open_vscode":
                // VSCodeを開く
                openVSCode(gRepoFullPathAtPushButton);
                break;
            default:
               // 対応するコマンドがない場合は何も処理しない
               break;
        }

    } catch (error) {
         // エラーが発生した場合は出力ペインにエラー内容を表示
        writeOutputPane(error);
    }}

// 変化が起きたということを意図的に伝搬することによって、次の状態検知チェックまでの間隔を通常より速くする。
function changeNotify() {
    try {
        if (gitWatcherComponent) {
            gitWatcherComponent.ChangeNotify();
        }
    } catch (e) { }
}

// プロセス明示的に閉じ。(どうも秀丸 hidemaru.runProcess は残るんじゃね？ 疑惑があるので意図して閉じる)
function destroyProcess(process) {
    try {
        if (process) {
            process.kill();
        }
    } catch (e) {
    }
}

// -------------------------- P U L L 用 -------------------------------------

var gitPullProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitPullAll(repoFullPath) {

    if (!repoFullPath) {
        return;
    }

    if (gitPullProcess) {
        return;
    }

    onStartGitPull();

    try {
        gitPullProcess = hidemaru.runProcess("git pull", repoFullPath, "stdio", "utf8");
        gitPullProcess.stdOut.onReadAll(onStdOutReadAllGitPull);
        gitPullProcess.stdErr.onReadAll(onStdErrReadAllGitPull);
        gitPullProcess.onClose(onCloseGitPull);
    } catch (e) {
        destroyProcess(gitPullProcess);
        gitPullProcess = null;
        writeOutputPane(e);
    }
}

function onStartGitPull() {
    writeOutputPane("------------ P U L L ------------");
}

function onStdOutReadAllGitPull(outputText) {
    writeOutputPane(outputText);
}

function onStdErrReadAllGitPull(outputText) {
    writeOutputPane(outputText);
}

function onCloseGitPull() {
    changeNotify();
    destroyProcess(gitPullProcess);
    gitPullProcess = null;
}



// -------------------------- P U S H 用 -------------------------------------

var gitPushProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitPushAll(repoFullPath) {

    if (!repoFullPath) {
        return;
    }

    if (gitPushProcess) {
        return;
    }

    onStartGitPush();

    try {
        gitPushProcess = hidemaru.runProcess("git push", repoFullPath, "stdio", "utf8");
        gitPushProcess.stdOut.onReadAll(onStdOutReadAllGitPush);
        gitPushProcess.stdErr.onReadAll(onStdErrReadAllGitPush);
        gitPushProcess.onClose(onCloseGitPush);
    } catch (e) {
        destroyProcess(gitPushProcess);
        gitPushProcess = null;
        writeOutputPane(e);
    }
}

function onStartGitPush() {
    writeOutputPane("------------ P U S H ------------");
}


function onStdOutReadAllGitPush(outputText) {
    writeOutputPane(outputText);
}

function onStdErrReadAllGitPush(outputText) {
    writeOutputPane(outputText);
}

function onCloseGitPush() {
    changeNotify();
    destroyProcess(gitPushProcess);
    gitPushProcess = null;
}



// -------------------------- C O M M I T 用 -------------------------------------

function gitCommentDialog(repoFullPath) {
    if (gitWatcherComponent) {

        // 先にコミット用コメントの画面。閉じたら何もしないキャンセル相当になる。
        // 承認相当行為を押した時だけ「gitCommitAllCallBack」が実行される。
        gitWatcherComponent.ShowGitCommitForm(
            function (comment) {
                 if (comment == "") comment = "...";
                 // コミットコメントが何もないと「Aborting commit due to empty commit message.」となるので、「...」という「続く」ということを意味するコメントにする。

                 // git add . へと移行
                 gitAdd(repoFullPath, comment);
            }
        );
    }

}


var gitAddProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitAdd(repoFullPath, comment) {

    if (!repoFullPath) {
        return;
    }

    if (gitAddProcess || gitCommitProcess) {
        return;
    }

    onStartGitAdd();

    try {
        gitAddProcess = hidemaru.runProcess("git add .", repoFullPath, "stdio", "utf8");
        gitAddProcess.stdOut.onReadAll(onStdOutReadAllGitAdd);
        gitAddProcess.stdErr.onReadAll(onStdErrReadAllGitAdd);
        gitAddProcess.onClose = function () {
            destroyProcess(gitAddProcess);
            gitAddProcess = null;
            gitCommit(repoFullPath, comment);
        }
    } catch (e) {
        destroyProcess(gitAddProcess);
        gitAddProcess = null;
        writeOutputPane(e);
    }
}

function onStartGitAdd() {
    // writeOutputPane("------------ A D D ------------");
}

function onStdOutReadAllGitAdd(outputText) {
    writeOutputPane(outputText);
}

function onStdErrReadAllGitAdd(outputText) {
    writeOutputPane(outputText);
}



var gitCommitProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitCommit(repoFullPath, comment) {

    if (!repoFullPath) {
        return;
    }

    onStartGitCommit();

    try {
        // JSONエスケープでとりあえず安全にした後、
        var jsonComment = JSON.stringify(comment);
        // 改行は、改行のままにする。
        jsonComment = jsonComment.replace(/\\r\\n/g, "\n");
        jsonComment = jsonComment.replace(/\\n/g, "\n");

        gitCommitProcess = hidemaru.runProcess("git commit -m " + jsonComment, repoFullPath, "stdio", "utf8");
        gitCommitProcess.stdOut.onReadAll(onStdOutReadAllGitCommit);
        gitCommitProcess.stdErr.onReadAll(onStdErrReadAllGitCommit);
        gitCommitProcess.onClose(onCloseGitCommit);
    } catch (e) {
        destroyProcess(gitCommitProcess);
        gitCommitProcess = null;
        writeOutputPane(e);
    }
}

function onStartGitCommit() {
    writeOutputPane("------------ C O M M I T ------------");
}

function onStdOutReadAllGitCommit(outputText) {
    writeOutputPane(outputText);
}

function onStdErrReadAllGitCommit(outputText) {
    writeOutputPane(outputText);
}

function onCloseGitCommit() {
    changeNotify();
    destroyProcess(gitCommitProcess);
    gitCommitProcess = null;
}








// -------------------------- V S C O D E 用 -------------------------------------


// hidemaru.pushPostExecMacroFileの実行を確かなものとする関数
function pushPostExecMacroFile(command, arg) {
    var isScheduled = 0;
    // まずは0ディレイで実行を試みる。setTimeoutに乗せる。
    hidemaru.setTimeout(function () {
        if (!isScheduled) {
            isScheduled = hidemaru.postExecMacroFile(command, arg);
            if (isScheduled !== 0) { isScheduled = 1; }
        }
    }, 0);

    // この下の処理が必要な理由は秀丸エディタv9.22～v9.34のバグのため。setTimeoutが同じフレーム(1秒60フレーム)内に
    // ２回実行されると、一方が実行されないバグのため。このため、上の処理が本当に成功したのか？ の確認が必要になる。
    var peRetry = hidemaru.setInterval(function () {
        if (isScheduled === 0) {
            isScheduled = hidemaru.postExecMacroFile(command, arg);
            if (isScheduled !== 0) { isScheduled = 1; }
        }
        if (isScheduled) { hidemaru.clearInterval(peRetry); }
    }, 250);
}


// VSCodeを「ソースビューモード」でオープンする。リポイトリに帰属していない場合は、通常モードでオープンする。
// カーソルの位置（もしくは秀丸上で見えてるもの）なども大いに考慮され、可能な限り引き継がれる。
function openVSCode(repoFullPath) {
    pushPostExecMacroFile('"' + currentMacroDirectory + '\\HmGitWatcherVSCode.mac"', repoFullPath);
}

