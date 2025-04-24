/*
 * 「gitのpre-commitファイル」の機能を使って、100MB以上のファイルのコミットを防止する
 * 「Python由来のpre-commit」を使っている場合は、同様の機能があるので、不要だが
 * いちいち、個々のリポジトリにコマンドで導入する必要すらないので、手軽。
 *  すでに pre-commit ファイル が存在する場合は何もしないので、
 * Pythonのpre-commit導入済みの場合は何もしない。
 * 状況証拠的に、Git LFS になっているリポジトリの場合は、100MB以上であってもキャンセルされない。
 */

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

        var scriptContent = hidemaru.loadTextFile(currentMacroDirectory + "\\HmGitWatcher100MBLimit.v1.pre-commit");
        hidemaru.saveTextFile(preCommitFilePath, scriptContent, "utf8");

        writeOutputPane("このリポジトリに、「ファイルサイズが100MB以上」のファイルをコミットしようとすると、自動的にキャンセルする仕組みを導入しました。");
    } catch (e) {
    }

}
