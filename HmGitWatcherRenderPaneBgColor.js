// ----------- 編集ペインの背景を取得する。nullを返す時は、取得しなおす必要がないということ ----------
var lastBGR = -1;
function getBGColor() {
    try {
        var backBGR = getconfigcolor(0, 1);

        if (backBGR == lastBGR) {
            return null;
        }

        lastBGR = backBGR;

        // BGRの順をRGBの順へ
        var R = backBGR & 0xFF;
        var G = (backBGR >> 8) & 0xFF;
        var B = (backBGR >> 16) & 0xFF;
        var backRGB = (R << 16) | (G << 8) | B;

        return sprintf("%06X", backRGB);

    } catch (e) {
    }

    return null;
}

// ---- 設定の変更などで、背景色を変更したら、それを反映される。同じファイルに対して「背景色の設定を変更する」などは、レアな行為なのでゆっくり反映で良いだろう。
var bgColorIntervalHandle; // 初期化してはならない
var bgColorIntervalTime = 5000; // チック間隔

function startBGColorInterval(intervalTime) {

    if (!intervalTime) {
        intervalTime = bgColorIntervalTime;
    }

    // 同一ファイルに対して背景色をチョクチョク変更することなどはないので、5秒に一度程度でよいだろう。
    if (typeof (bgColorIntervalHandle) != "undefined") {
        hidemaru.clearTimeout(bgColorIntervalHandle);
    }
    bgColorIntervalHandle = hidemaru.setTimeout(tickBGColor, intervalTime);
}

function stopBGColorInterval() {
    if (typeof (bgColorIntervalHandle) != "undefined") {
        hidemaru.clearTimeout(bgColorIntervalHandle);
    }
}

// ファイル編集の途中で背景色を反映するとすれば、ほとんどの場合は、ダイアログだろう。
// なので、秀丸が何らかのダイアログを出している、チェック間隔を短くする。
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
        hasError = true;
    } finally {
        // エラーがあったらストップ
        if (hasError) {
            stopBGColorInterval();
            return;
        }

        // ダイアログの時は間隔を短くする。
        if (isDialogOperation()) {
            startBGColorInterval(Math.floor(bgColorIntervalTime / 5));
        } else {
            startBGColorInterval(bgColorIntervalTime);
        }
    }
}

function isDialogOperation() {
    /*
    ◯ 0x00000200 何らかのダイアログ表示中
    */
    var s = hidemaru.getInputStates();
    return s & 0x00000200;
}



