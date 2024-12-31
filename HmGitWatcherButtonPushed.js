  function onButtonPushed(command_label){
    try {
	    //実行の順番(5) 手動操作時
	    console.log(command_label);
	    console.log(typeof(command_label));
        if (gRepoFullPath) {
		    if (command_label=="pull_all") {
				gitPullAll(gRepoFullPath);
		    }
		    else if (command_label=="push_all") {
				gitPushAll(gRepoFullPath);
		    }
		    else if (command_label=="commit_all") {
				gitCommitAll(gRepoFullPath);
		    }
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
console.log("プッシュ!!");
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
  
