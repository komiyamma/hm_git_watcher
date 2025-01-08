var gRepoFullPathAtPushButton = ""; // ボタンを押した瞬間のリポジトリを控えておくため。

function onButtonPushed(command_label) {
    gRepoFullPathAtPushButton = gRepoFullPath; // 押した瞬間に
    try {
        if (gRepoFullPathAtPushButton) {
            if (command_label == "pull_all") {
                gitPullAll(gRepoFullPathAtPushButton);
            }
            else if (command_label == "push_all") {
                gitPushAll(gRepoFullPathAtPushButton);
            }
            else if (command_label == "commit_all") {
                if (gitWatcherComponent) {
                    // 先にコミット用コメントの画面。閉じたら何もしないキャンセル相当になる。
                    // 承認相当行為を押した時だけ「gitCommitAllCallBack」が実行される。
                    gitWatcherComponent.ShowGitCommitForm(gitCommitAllCallBack);
                }
            }
        }
        if (command_label == "open_vscode") {
            openVSCode();
        }

    } catch (e) {
        writeOutputPane(e);
    }
}

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
    } finally {
        process = null;
    }
}

// -------------------------- P U L L 用 -------------------------------------

var gitPullProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitPullAll(repoFullPath) {

    if (!repoFullPath) {
        return;
    }

    try {
        gitPullProcess = hidemaru.runProcess("git pull", repoFullPath, "stdio", "utf8");
        gitPullProcess.stdOut.onReadAll(onStdOutReadAllGitPull);
        gitPullProcess.stdErr.onReadAll(onStdErrReadAllGitPull);
        gitPullProcess.onClose(onCloseGitPull);
    } catch (e) {
        destroyProcess(gitPullProcess);
        writeOutputPane(e);
    }
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
}



// -------------------------- P U S H 用 -------------------------------------

var gitPushProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitPushAll(repoFullPath) {

    if (!repoFullPath) {
        return;
    }
    try {
        gitPushProcess = hidemaru.runProcess("git push", repoFullPath, "stdio", "utf8");
        gitPushProcess.stdOut.onReadAll(onStdOutReadAllGitPush);
        gitPushProcess.stdErr.onReadAll(onStdErrReadAllGitPush);
        gitPushProcess.onClose(onCloseGitPush);
    } catch (e) {
        destroyProcess(gitPushProcess);
        writeOutputPane(e);
    }
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
}



// -------------------------- C O M M I T 用 -------------------------------------

function gitCommitAllCallBack(comment) {
    // コミットコメントが何もないと「Aborting commit due to empty commit message.」となるので、「コミット」というコメントにする。
    if (comment == "") comment = "コミット";
    gitAdd(gRepoFullPathAtPushButton, comment);
}


var gitAddProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitAdd(repoFullPath, comment) {

    if (!repoFullPath) {
        return;
    }
    try {
        gitAddProcess = hidemaru.runProcess("git add .", repoFullPath, "stdio", "utf8");
        gitAddProcess.stdOut.onReadAll(onStdOutReadAllGitAdd);
        gitAddProcess.stdErr.onReadAll(onStdErrReadAllGitAdd);
        gitAddProcess.onClose = function () {
            destroyProcess(gitAddProcess);
            gitCommit(repoFullPath, comment);
        }
    } catch (e) {
        destroyProcess(gitAddProcess);
        writeOutputPane(e);
    }
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
    try {
        var jsonComment = JSON.stringify(comment);
        gitCommitProcess = hidemaru.runProcess("git commit -m " + jsonComment, repoFullPath, "stdio", "utf8");
        gitCommitProcess.stdOut.onReadAll(onStdOutReadAllGitCommit);
        gitCommitProcess.stdErr.onReadAll(onStdErrReadAllGitCommit);
        gitCommitProcess.onClose(onCloseGitCommit);
    } catch (e) {
        destroyProcess(gitCommitProcess);
        writeOutputPane(e);
    }
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
    }, 100);
}

// VSCodeを「ソースビューモード」でオープンする。リポイトリに帰属していない場合は、通常モードでオープンする。
// カーソルの位置（もしくは秀丸上で見えてるもの）なども大いに考慮され、可能な限り引き継がれる。
function openVSCode() {
    pushPostExecMacroFile('"' + currentMacroDirectory + '\\HmOpenVSCodeFromHidemaru.mac"', "scm");
}
