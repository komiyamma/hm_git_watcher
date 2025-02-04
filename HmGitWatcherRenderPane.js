// レンダリングペインの表示状態は全て完了
function isRenderPaneReadyStateComplete() {
    var readyState = renderpanecommand({ target: strRanderPaneName, get: "readyState" });
    return readyState == "complete";
}

// レンダリングペインは「インスタンス」があり、かつ「表示」されている。
function isRenderPaneShowAndVisible() {
    var is_show = Number(renderpanecommand({ target: strRanderPaneName, get: "show" }));
    var is_invisible = Number(renderpanecommand({ target: strRanderPaneName, get: "invisible" }));

    // レンダリングペインを配置していない、もしくは、見えてない。
    if (!is_show || is_invisible) {
        return false;
    }

    return true;
}


// レンダリングを閉じる。インスタンスも破棄
function closeRenderPane() {
    renderpanecommand({
        target: strRanderPaneName,
        show: 0,      // コンポーネント破棄
        invisible: 1  // 隠す
    });

    stopBGColorInterval();
    stopDPIInterval();
    stopUpdatedRenderPaneStatusRetry();
}

// レンダリングペインの表示。インスタンス確立と実際の表示。
function showRenderPane() {
    renderpanecommand({
        target: strRanderPaneName,
        show: 1,     // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 0 // 表示する
    });

    startBGColorInterval();
    startDPIInterval();
}

// レンダリングペインにjsコマンドを送る
function updateRenderPane(jsCommand) {
    renderpanecommand({
        target: strRanderPaneName,
        uri: jsCommand,
        show: 1,      // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 0  // 表示する
    });
}

// ----------- 編集ペインの背景を取得する。nullを返す時は、取得しなおす必要がないということ ----------
var lastBGColor = "";
function getBGColor() {
    try {
        var curNormalColorJson = getconfigcolor({ "normal": "*" });

        if (!curNormalColorJson) {
            return null;
        }

        var bgColor = curNormalColorJson.normal.back;

        if (bgColor == lastBGColor) {
            return null;
        }

        lastBGColor = bgColor;

        if (gitWatcherComponent) {
            bgColor = gitWatcherComponent.ConvertSystemColorNameToRGB(curNormalColorJson.normal.back);
        }

        bgColor = bgColor.replace("#", "");

        return bgColor;

    } catch (e) {
    }

    return null;
}

// ---- 設定の変更などで、背景色を変更したら、それを反映される。同じファイルに対して背景を変更する結構かなりまれな行為なのでゆっくり反映で良いだろう。
var bgColorIntervalHandle; // 初期化してはならない
var bgColorIntervalTime = 5000; // チック間隔

function startBGColorInterval(intervalTime) {

    if (!intervalTime) {
        intervalTime = bgColorIntervalTime;
    }

    // 同一ファイルに対して背景色をチョクチョク変更することなどはないので、5秒に一度程度でよいだろう。
    if (typeof (bgColorIntervalHandle) != "undefined") {
        hidemaru.clearInterval(bgColorIntervalHandle);
    }
    bgColorIntervalHandle = hidemaru.setTimeout(tickBGColor, intervalTime);
}

function stopBGColorInterval() {
    if (typeof (bgColorIntervalHandle) != "undefined") {
        hidemaru.clearInterval(bgColorIntervalHandle);
    }
}

function tickBGColor() {
    var hasError = false;
    try {
        var curBGColor = getBGColor();
        if (!curBGColor) {
            return;
        }

        var jsCommand = "javascript:HmGitWatcher_UpdateBGColor('" + curBGColor + "');";
        renderpanecommand({
            target: strRanderPaneName,
            uri: jsCommand
        });
    } catch (e) {
        stopBGColorInterval();
        hasError = true;
    } finally {
        if (!hasError) {
            // 背景カラーを変更しているとすればほとんどダイアログだろう。この時Tick間隔を狭める
            if (isDialogOperation()) {
                var overrideIntervalTime = Math.floor(bgColorIntervalTime / 5);
                startBGColorInterval(overrideIntervalTime);
            } else {
                startBGColorInterval(bgColorIntervalTime);
            }
        }
    }
}

function isDialogOperation() {
    /*
    ◯ 0x00000200 何らかのダイアログ表示中
    */
    var s = hidemaru.getInputStates();
    if (s & 0x00000200) {
        return true;
    }

    return false;
}


// ---- ディスプレイ間の移動・もしくはディスプレイの設定などでDPIが変わったら、それに応じて、レンダリングペインの位置やサイズを変更する。
var dpiIntervalHandle; // 初期化してはならない
var dpiIntervalTime = 2000; // チック間隔

function startDPIInterval(intervalTime) {

    if (!intervalTime) {
        intervalTime = dpiIntervalTime;
    }

    if (typeof (dpiIntervalHandle) != "undefined") {
        hidemaru.clearInterval(dpiIntervalHandle);
    }
    dpiIntervalHandle = hidemaru.setTimeout(tickDPI, intervalTime);
}

function stopDPIInterval() {
    if (typeof (dpiIntervalHandle) != "undefined") {
        hidemaru.clearInterval(dpiIntervalHandle);
    }
}

function tickDPI() {
    var hasError = false;
    try {
        var dpiObj = getDpiScale();
        if (!dpiObj.update) {
            return;
        }

        var dpiScale = dpiObj.dpi;

        var windowRect = getWindowRect(dpiScale);

        renderpanecommand({
            target: strRanderPaneName,
            place: "overlay",
            x: windowRect.x,
            y: windowRect.y,
            cx: windowRect.cx,
            cy: windowRect.cy
        });
    } catch (e) {
        stopDPIInterval();
        hasError = true;
    } finally {
        startDPIInterval();
    }
}

var lastDPIScale = 1;
function getDpiScale() {
    var dpiScale = 1;
    if (gitWatcherComponent) {
        try {
            var currentWindowDpi = gitWatcherComponent.GetDpiFromWindowHandle(hidemaru.getCurrentWindowHandle());
            if (currentWindowDpi > 0) {
                dpiScale = currentWindowDpi / 96;
            }
        } catch (e) { }
    }

    if (lastDPIScale != dpiScale) {
        lastDPIScale = dpiScale;
        return { dpi: dpiScale, update: true };
    }
    return { dpi: dpiScale, update: false };
}

function getWindowRect(dpiScale) {
    var xDPI = Math.ceil(32 * dpiScale);
    var yDPI = Math.ceil(26 * dpiScale);
    var cxDPI = Math.ceil(32 * dpiScale);     // 横には１つずつ並べる
    var cyDPI = Math.ceil(32 * 4 * dpiScale); // 縦に４つのボタン

    return { "x": xDPI, "y": yDPI, "cx": cxDPI, "cy": cyDPI };

}

// ---- レンダリングペインのURL
function getHtmlUrl() {

    // HmGitWatcher.htmlを使ってボタンをレンダリング
    var urlFullPath = currentMacroDirectory + "\\HmGitWatcher.html";
    if (!existfile(urlFullPath)) {
        message(urlFullPath + "が存在しません")
    }
    // Windowsタイプのファイルの絶対パスを、URLタイプに。(WebView2の方ならあるが)JScriptだとこれがないため、.NETから関数を借りてくる
    if (gitWatcherComponent) {
        urlFullPath = gitWatcherComponent.ConvertToUrl(urlFullPath);
    }

    return urlFullPath;
}

// ---- レンダリングペインを開く（この段階ではまだ目に見えない。インスタンス確立だけ。実際に目に見えるのは、リポジトリに帰属することが決定してから）
function openRenderPane() {

    var bgColor = getBGColor();

    var htmlUrl = getHtmlUrl();

    // ボタンが押された時の関数
    var callFuncId = hidemaru.getFunctionId(onButtonPushed);

    // funcIDとbgcolorを伝える、URLを確立
    var targetUrl = htmlUrl + '?callFuncId=' + callFuncId + '&bgColor=' + bgColor;

    // overlayは、DPIの影響をもろうけするため、DPIを考慮する必要がある。
    var dpiObj = getDpiScale();
    var dpiScale = dpiObj.dpi;

    var windowRect = getWindowRect(dpiScale);

    // invisibleな隠した状態で配置しておく
    renderpanecommand({
        target: strRanderPaneName,
        show: 1,      // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 1, // 隠した状態での配置
        uri: targetUrl,
        place: "overlay",
        align: "right",
        initialize: "async",
        x: windowRect.x,
        y: windowRect.y,
        cx: windowRect.cx,
        cy: windowRect.cy
    });

}
