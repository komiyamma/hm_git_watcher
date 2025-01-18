function shouldMacroReExecute() {

    var currentFileFullPath = filename2() || "";

    // 「ファイルを保存時」の自動起動マクロから呼ばれている際は、
    // 前回チェック時と同じファイル名なら何もしない。
    if (event() == 3) {

        // 保存直前からの呼び出しは対象外。保存直後｛ event==3 && geteventparam(0)==1 ｝から呼び出すこと
        // このタイミングで以後の処理をしたとしても、直後にファイルに紐づいたjsmode(の実行空間)が解放されるので無意味なため。
        if (geteventparam(0) == 0) {
            return 0;
        }

        // コンポーネントが解放されてしまっているなら、jsmodeの実行空間がクリアされてしまっている。改めて実行する必要がある。
	    if (typeof(gitWatcherComponent) == "undefined") {
		    setstaticvariable("HmGitWatcherLastFile", currentFileFullPath, 2); // 最後の解析ファイルフルパスを保持(不要なHmGitWatcher駆動を避ける)

		    return currentFileFullPath ? 1 : 0;
	    }

        // 保存直後、かつ、コンポーネントの残っている状態で、かつ、ファイル名が前回と同じであるならば、改めてマクロの続きを実行する必要はない。切り上げる。
        if (getstaticvariable("HmGitWatcherLastFile", 2) == currentFileFullPath) {
            // 前回HmGitWatcher開始時と同じファイルなのでパス
            return 0;
        }
    }

    setstaticvariable("HmGitWatcherLastFile", currentFileFullPath, 2); // 最後の解析ファイルフルパスを保持(不要なHmGitWatcher駆動を避ける)

    // 無題スタートは対象外
    return currentFileFullPath ? 1 : 0;
}

setVar("#ShouldMacroReExecute", shouldMacroReExecute());

