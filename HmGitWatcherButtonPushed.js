var onPushButtonRepoFullPath = ""; // ボタンを押した瞬間のリポジトリを控えておくため。

function onButtonPushed(command_label){
    onPushButtonRepoFullPath = gRepoFullPath; // 押した瞬間に
    try {
        //実行の順番(5) 手動操作時
        console.log(command_label);
        console.log(typeof(command_label));
        if (onPushButtonRepoFullPath) {
            if (command_label=="pull_all") {
                gitPullAll(onPushButtonRepoFullPath);
            }
            else if (command_label=="push_all") {
                gitPushAll(onPushButtonRepoFullPath);
            }
            else if (command_label=="commit_all") {
                gitCommitAll(onPushButtonRepoFullPath);
            }
        }
        if (command_label=="open_vscode") {
            openVSCode();
        }

    } catch(e) {
        console.log(e);
    }
  }

function changeNotify() {
    try {
    if (gitWatcherComponent) {
        gitWatcherComponent.ChangeNotify();
    }
    } catch(e) {}
}

function destroyProcess(process) {
    try {
    if (process) {
        process.kill();
        process = null;
    }
    } catch(e) {}
}



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


function gitCommitAll(repoFullPath) {
    gitAdd(repoFullPath);
}


var gitAddProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitAdd(repoFullPath) {

    if (!repoFullPath) {
        return;
    }
    try {
        gitAddProcess = hidemaru.runProcess("git add .", repoFullPath, "stdio", "utf8");
        gitAddProcess.stdOut.onReadAll(onStdOutReadAllGitAdd);
        gitAddProcess.stdErr.onReadAll(onStdErrReadAllGitAdd);
        gitAddProcess.onClose(onCloseGitAdd);
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

function onCloseGitAdd() {
    destroyProcess(gitAddProcess);
    gitCommit(onPushButtonRepoFullPath);
}





var gitCommitProcess;  // 初期化しないこと。再実行の際に、非同期でプロセスが動作していると初期化してはまずい。
function gitCommit(repoFullPath) {

    if (!repoFullPath) {
        return;
    }
    try {
        var comment = "コミット・コメント";
        var jsonComment = JSON.stringify(comment);
        gitCommitProcess = hidemaru.runProcess("git commit -m " + jsonComment, repoFullPath, "stdio", "utf8");
        gitCommitProcess.stdOut.onReadAll(onStdOutReadAllGitPush);
        gitCommitProcess.stdErr.onReadAll(onStdErrReadAllGitPush);
        gitCommitProcess.onClose(onCloseGitPush);
    } catch (e) {
        destroyProcess(gitCommitProcess);
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
    destroyProcess(gitCommitProcess);
}









  
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
    pushPostExecMacroFile('"' + currentMacroDirectory  + '\\HmOpenVSCodeFromHidemaru.mac"', "scm" );
}
