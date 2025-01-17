function isRenderPaneReadyStateComplete() {
    var readyState = renderpanecommand({ target: strRanderPaneName, get: "readyState" });
    return readyState == "complete";
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

function updateRenderPane(jsCommand) {
    renderpanecommand({
        target: strRanderPaneName,
        uri: jsCommand,
        show: 1,      // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 0  // 表示する
    });
}


function getBGColor() {
    // BGR順 → RGB順とする。
    function convertBGRtoRGB(bgrColor) {
        var red = (bgrColor & 0xFF);         // 下位8ビットが青
        var green = (bgrColor >> 8) & 0xFF;  // 中位8ビットが緑
        var blue = (bgrColor >> 16) & 0xFF;  // 上位8ビットが赤

        return (red << 16) | (green << 8) | blue; // RGBの順番で結合
    }

    // 背景色を文字列化する
    var numBGColor = getconfigcolor(0, 1);
    numBGColor = convertBGRtoRGB(numBGColor);

    return sprintf("%06x", numBGColor);
}

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
debuginfo(2);

function openRenderPane() {

    var bgColor = getBGColor();

    var htmlUrl = getHtmlUrl();

    // ボタンが押された時の関数
    var callFuncId = hidemaru.getFunctionId(onButtonPushed);

    // funcIDとbgcolorを伝えながら、URLを開く
    var targetUrl = htmlUrl + '?callFuncId=' + callFuncId + '&bgColor=' + bgColor;

    var defaultDpi = 96; // デフォルト
    var coef = 1;
    if (gitWatcherComponent) {
        var currentWindowDpi = gitWatcherComponent.GetDpiFromWindowHandle(hidemaru.getCurrentWindowHandle());
        if (currentWindowDpi > 0) {
            coef = currentWindowDpi/defaultDpi;
        }
    }

    var xDPI = Math.ceil(32 * coef);
    var yDPI = Math.ceil(26 * coef);
    var cxDPI = Math.ceil(36 * coef);
    var cyDPI = Math.ceil(140 * coef);
    
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
