/*
 * 一見何の処理をやってるのかと思うかもしれないが、
 * HmGitWatcher.mac が呼ばれた時に、本当に続き(=本処理)を
 * 「(再)実行するべきかどうか」のかの判定を行っている。
 * 
 * 原則、HmGitWatcher.mac は手で実行することは想定しておらず、
 * １：自動実行　ファイルを開いた時
 * ３：自動実行　ファイルを保存した(直後)の
 * ０：手動実行　一応は手動でも実行可能だが、このような使い方はあまり想定していない。
 *
 * Ａ：「ファイルの上書き保存」時にはJavaScriptは解放されないが、
 * Ｂ：「名前を付けて保存」でそのまま上書きすると，ファイル名は変化していないのに、JavaScriptが解放されてしまうため。
 * このＡとＢは通常のシステムでは同じ行為であるが、秀丸的には異なる（ＡはJSが解放されないが、ＢはJSが解放されてしまう）ため、
 * Ｂが行われた場合に、再びJSを再開する必要がある。しかし、そのアルゴリズムにおいて、Ａの場合は、再実行されないようにしておく必要がある。
 */



function shouldMacroReExecute() {

    var eventId = event();

    // 「手動実行」か「ファイルを開いた直後」もしくは「ファイルを保存時」の場合に処理を継続
    if (eventId != 0 && eventId != 1 && eventId != 3) {
        return 0;
    }

    var currentFileFullPath = filename2() || "";

    var lastFileStaticVariableLabel = "LastFile:" + hidemaru.getJsMode(); // まぁ適当にそれぞれの

    // 「ファイルを保存時」の自動起動マクロから呼ばれている際は、
    // 前回チェック時と同じファイル名なら何もしない。
    if (eventId == 3) {

        // 保存直前からの呼び出しは対象外。保存直後｛ event==3 && geteventparam(0)==1 ｝から呼び出すこと
        // このタイミングで以後の処理をしたとしても、直後にファイルに紐づいたjsmode(の実行空間)が解放されるので無意味なため。
        if (geteventparam(0) == 0) {
            return 0;
        }

        // コンポーネントが解放されてしまっているなら、jsmodeの実行空間がクリアされてしまっている。改めて実行する必要がある。
        if (typeof (gitWatcherComponent) == "undefined") {
            setstaticvariable(lastFileStaticVariableLabel, currentFileFullPath, 2); // 最後の解析ファイルフルパスを保持(不要なHmGitWatcher駆動を避ける)

            return currentFileFullPath ? 1 : 0;
        }

        // 保存直後、かつ、コンポーネントの残っている状態で、かつ、ファイル名が前回と同じであるならば、改めてマクロの続きを実行する必要はない。切り上げる。
        if (getstaticvariable(lastFileStaticVariableLabel, 2) == currentFileFullPath) {
            // 前回HmGitWatcher開始時と同じファイルなのでパス
            return 0;
        }
    }

    setstaticvariable(lastFileStaticVariableLabel, currentFileFullPath, 2); // 最後の解析ファイルフルパスを保持(不要なHmGitWatcher駆動を避ける)

    // 無題スタートは対象外
    return currentFileFullPath ? 1 : 0;
}

setVar("#ShouldMacroReExecute", shouldMacroReExecute());

