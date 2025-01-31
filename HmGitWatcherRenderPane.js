function isRenderPaneReadyStateComplete() {
    var readyState = renderpanecommand({ target: strRanderPaneName, get: "readyState" });
    return readyState == "complete";
}

function isRenderPaneShowAndVisible() {
    var is_show = Number(renderpanecommand({ target: strRanderPaneName, get: "show" }));
    var is_invisible = Number(renderpanecommand({ target: strRanderPaneName, get: "invisible" }));

    // レンダリングペインを配置していない、もしくは、見えてない。
    if (!is_show || is_invisible) {
        return false;
    }

    return true;
}


// レンダリングを閉じる
function closeRenderPane() {
    renderpanecommand({
        target: strRanderPaneName,
        show: 0,      // コンポーネント破棄
        invisible: 1  // 隠す
    });
}

function showRenderPane() {
    renderpanecommand({
        target: strRanderPaneName,
        show: 1,     // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 0 // 表示する
    });
}

// 背景が白なら、背景のミスマッチがないため、bgcolor伝達前に早めに表示をしてしまう。
function checkAndShowBrowserPaneEarly() {
    var bgcolor = getBGColor();
    if (bgcolor == 0xFFFFFF) {
        showRenderPane();
    }
}

function updateRenderPane(jsCommand) {
    renderpanecommand({
        target: strRanderPaneName,
        uri: jsCommand,
        show: 1,      // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 0  // 表示する
    });
}

// ----------- 編集ペインの背景をレンダリングペインへと伝える。nullを返す時は、改めて伝える必要がないということ ----------
var lastBGColor = "";
function getBGColor() {
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
        try {
            bgColor = gitWatcherComponent.ConvertSystemColorNameToRGB(curNormalColorJson.normal.back);
        } catch (e) { }
    }

    bgColor = bgColor.replace("#", "");

    return bgColor;
}

/*
// 背景色をチョクチョク変更することなどはないので、10秒に一度程度でよいだろう。
if (typeof (colorTickInterval) != "undefined") {
    hidemaru.clearInterval(colorTickInterval);
}
var colorTickInterval;
colorTickInterval = hidemaru.setInterval(checkBGColor, 10000);

function checkBGColor() {
    var curBGColor = getBGColor();
    if (!curBGColor) {
        return;
    }

    if (isRenderPaneShowAndVisible() && isRenderPaneReadyStateComplete()) {
        try {
            var jsCommand = "javascript:HmGitWatcher_UpdateBGColor('" + curBGColor + "');";
            updateRenderPane(jsCommand);
        } catch (e) {
            hidemaru.clearInterval(colorTickInterval);
        }
    }
}
*/

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

function getDpiScale() {
    var dpiScale = 1;
    if (gitWatcherComponent) {
        var currentWindowDpi = gitWatcherComponent.GetDpiFromWindowHandle(hidemaru.getCurrentWindowHandle());
        if (currentWindowDpi > 0) {
            dpiScale = currentWindowDpi / 96;
        }
    }
    return dpiScale;
}

function openRenderPane() {

    var bgColor = getBGColor();

    var htmlUrl = getHtmlUrl();

    // ボタンが押された時の関数
    var callFuncId = hidemaru.getFunctionId(onButtonPushed);

    // funcIDとbgcolorを伝えながら、URLを開く
    var targetUrl = htmlUrl + '?callFuncId=' + callFuncId + '&bgColor=' + bgColor;

    var dpiScale = getDpiScale();

    var xDPI = Math.ceil(32 * dpiScale);
    var yDPI = Math.ceil(26 * dpiScale);
    var cxDPI = Math.ceil(32 * dpiScale);     // 横には１つずつ並べる
    var cyDPI = Math.ceil(32 * 4 * dpiScale); // 縦に４つのボタン

    // invisibleな隠した状態で配置しておく
    renderpanecommand({
        target: strRanderPaneName,
        show: 1,      // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 1, // 隠した状態での配置
        uri: targetUrl,
        place: "overlay",
        align: "right",
        initialize: "async",
        x: xDPI,
        y: yDPI,
        cx: cxDPI,
        cy: cyDPI,
    });

}
