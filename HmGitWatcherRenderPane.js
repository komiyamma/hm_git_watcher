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
console.log("◯closeRenderPane");
    renderpanecommand({
        target: strRanderPaneName,
        show: 0,      // コンポーネント破棄
        invisible: 1  // 隠す
    });

    stopBGColorInterval();
    stopUpdatedRenderPaneStatusRetry();
}

// レンダリングペインの表示。インスタンス確立と実際の表示。
function showRenderPane() {
console.log("◯showRenderPane");
    renderpanecommand({
        target: strRanderPaneName,
        show: 1,     // 見えるではなく、コンポーネント配置の意味なので注意
        invisible: 0 // 表示する
    });

    startBGColorInterval();
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

// DPIに変化がある時だけ、onDPIChange が呼ばれるようにする。
// 事実上のシングルスレッドになってるJavaScriptのTickによるリソースは使わず、別のCore/Threadへと分散する。
function startDPIWatcher() {
    if (gitWatcherComponent) {
        gitWatcherComponent.ReCreateDPIWatcher(onDPIChange);
    }
}

// ---- ディスプレイ間の移動・もしくはディスプレイの設定などでDPIが変わったら、この関数が非同期でdllから呼ばれる
//      それに応じて、レンダリングペインの位置やスケールを変更する。
function onDPIChange(currentWindowDPI) {
    hidemaruversion(targetHidemaruversion); // なぜか必要。別スレ経由なため、うまく伝搬しないことがある？

    var dpiScale = 1;
    try {
        if (currentWindowDPI > 0) {
            dpiScale = currentWindowDPI / 96;
        }

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
    }
}

function getStartDPIScale() {
    var dpiScale = 1;
    if (gitWatcherComponent) {
        try {
            var currentWindowDPI = gitWatcherComponent.GetStartDPI();
            if (currentWindowDPI > 0) {
                dpiScale = currentWindowDPI / 96;
            }
        } catch (e) { }
    }

    return dpiScale;
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
console.log("◯openRenderPane");

    var bgColor = getBGColor();

    var htmlUrl = getHtmlUrl();

    // ボタンが押された時の関数
    var callFuncId = hidemaru.getFunctionId(onButtonPushed);

    // funcIDとbgcolorを伝える、URLを確立
    var targetUrl = htmlUrl + '?callFuncId=' + callFuncId + '&bgColor=' + bgColor;

    // overlayは、DPIの影響をもろうけするため、DPIを考慮する必要がある。
    var dpiScale = getStartDPIScale();
    var windowRect = getWindowRect(dpiScale);

    renderpanecommand({
        target: strRanderPaneName,
        show: 0,      // インスタンスを破棄してからやらないと、以前のゴミが残っていたらヤバい
    });

console.log("◯instanceRenderPane");
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

    if (gitWatcherComponent) {
        gitWatcherComponent.sendCommandRenderPaneCommand();
    }

    // レインダリングペインがReadyStateになるまで待つ。
    // これをしないと、ReadyStateになる前に他のウィンドウがアクティブになるとヤバい。
    // 以下の２行を足せば、バグは置きないようだが、待機が目立つのでやりたくはないところ。
    // hidemaru.clearTimeout(handleOpenRanderPaneComplete);
    // hidemaru.setTimeout(waitOpenRanderPaneComplete, 150, 0);
}

/*
var handleOpenRanderPaneComplete; // 初期化しないこと

// レインダリングペインがReadyStateになるまで待つ。これをしないと、やばい。
function waitOpenRanderPaneComplete(tryCount) {
    tryCount++;
    if (isRenderPaneReadyStateComplete()) {
        hidemaru.notifyAwait("HmGitWatcherRnderPaneComplete");//awaitjsの待機を抜ける
        return;
    }
    if (tryCount > 10) {
        hidemaru.notifyAwait("HmGitWatcherRnderPaneComplete");//awaitjsの待機を抜ける
        return;
    }
    hidemaru.setTimeout(waitOpenRanderPaneComplete, 150, tryCount);
}
*/