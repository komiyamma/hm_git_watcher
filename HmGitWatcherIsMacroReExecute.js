function checkIfMacroReExecute() {
    // 「ファイルを保存時」の自動起動マクロから呼ばれている際は、
    // 前回チェック時と同じファイル名なら何もしない。
    if (event() == 3) {
        // 保存直前からの呼び出しは対象外。保存直後｛ event==3 && geteventparam(0)==1 ｝から呼び出すこと
        if (geteventparam(0) == 0) {
            return 0;
        }
        if (getstaticvariable("HmGitWatcherLastFile", 2) == filename2()) {
            // 前回HmGitWatcher開始時と同じファイルなのでパス
            return 0;
        }
    }

    setstaticvariable("HmGitWatcherLastFile", filename2(), 2); // 最後の解析ファイルフルパスを保持(不要なHmGitWatcher駆動を避ける)

    // 無題スタートは対象外
    if (!filename2()) {
        return 0;
    }

    return 1;
}

setVar("#IsMacroReExecute", checkIfMacroReExecute());

