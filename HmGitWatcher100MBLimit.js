var create100MBLimitPreCommitFileDone = false;
function create100MBLimitPreCommitFile(repoPath) {

    if (create100MBLimitPreCommitFileDone) {
         return;
    }
    create100MBLimitPreCommitFileDone = true;

    if (!repoPath) { return; }

    try {
	    var fso = createobject("Scripting.FileSystemObject");

	    var configFilePath = repoPath + "\\.git\\config";
	    if (!fso.FileExists(configFilePath)) {
	        return false;
	    }
	    var postCheckoutFilePath = repoPath + "\\.git\\hooks\\post-checkout";
	    var postCommitFilePath = repoPath + "\\.git\\hooks\\post-commit";
	    var postMergeFilePath = repoPath + "\\.git\\hooks\\post-merge";
	    var prePushFilePath = repoPath + "\\.git\\hooks\\pre-push";

	    // これはLFSで初期化されている
	    if (fso.FileExists(postCheckoutFilePath) && fso.FileExists(postCommitFilePath) && fso.FileExists(postMergeFilePath) && fso.FileExists(prePushFilePath)) {
	        return false;
	    }

	    var preCommitFilePath = repoPath + "\\.git\\hooks\\pre-commit";
	    if (fso.FileExists(preCommitFilePath)) {
	        // console.log('pre-commitファイル発見!!: ' + preCommitFilePath);
	        return false;
	    }

	    var scriptContent = hidemaru.loadTextFile( currentMacroDirectory + "\\HmGitWatcher100MBLimit.v1.txt");
	    hidemaru.saveTextFile(preCommitFilePath, scriptContent, "utf8");

		writeOutputPane("このリポジトリに100MB以上のファイルをコミットしようとすると自動的にキャンセルする仕組みを導入しました。");
	} catch(e) {
    }
    
}
