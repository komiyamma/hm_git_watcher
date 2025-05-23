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

    var eventId = 0;

    // イベントIDが意図的に伝搬してきているならば...
    if (argcount > 0) {
        eventId = Number(getarg(0));
    } else {
        eventId = event();
    }

    // ファイル名が無題。何もしない。
    if (!filename2()) {
        return 0;
    }

    // 「手動実行」なら強制的に再実行。
    if (eventId == 0) {
        return 1;
    }

    // 「ファイルを開いた直後」もしくは「ファイルを保存時」の場合にのみ処理を継続。それ以外は何もしない。
    if (eventId != 1 && eventId != 3) {
        return 0;
    }

    // 保存直前からの呼び出しは対象外。保存直後｛ event==3 && geteventparam(0)==1 ｝からの呼び出しには対応。
    // 「ファイル名をつけて保存」だと、保存直前のタイミングで処理をしたとしても、直後にファイルに紐づいたjsmode(の実行空間)が解放されるので無意味なため。
    if (eventId == 3 && geteventparam(0) != 1) {
        return 0;
    }

    // コンポーネントは存在している。何もしない。
    if (typeof (gitWatcherComponent) != "undefined") {
        return 0;
    }

    // ユーザーが特別に用意した最低条件関数
    if (typeof(necessaryCustomCondition) == "function") {
        if (!necessaryCustomCondition()) {
            return 0;
        }
    }

    // 改めて実行する必要がある。
    return 1;
}

setVar("#ShouldMacroReExecute", shouldMacroReExecute());

