# HmGitWatcher

![HmGitWatcher v1.2.0.1](https://img.shields.io/badge/HmGitWatcher-v1.2.0-6479ff.svg)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)
![Hidemaru 9.35](https://img.shields.io/badge/Hidemaru-v9.35-6479ff.svg)
![.NET 4.8 or .NET 9.0](https://img.shields.io/badge/.NET-4.8_|_9.0-6479ff.svg)

「秀丸エディタ」で「git」「github」を扱うためのコンポーネント

https://秀丸マクロ.net/?page=nobu_tool_hm_git_watcher

---
## 概要

`HmGitWatcher` は、高機能テキストエディタ「秀丸エディタ」上でGit/GitHubを利用したバージョン管理をより直感的かつスムーズに行うためのコンポーネントおよびツール群です。

### 主な機能

-   **Gitステータスの常時表示**:
    -   秀丸エディタで開いているファイルが属するGitリポジトリの状態を自動で監視します。
    -   「リモートからプルすべき変更」「ローカルでコミットすべき変更」「リモートへプッシュすべきコミット」の有無を検知し、エディタ内の専用ペインにアイコンで分かりやすく表示します。

-   **クイックなGit操作**:
    -   表示されたアイコンボタンをクリックすることで、`git pull`, `git commit`, `git push` などの基本的なGitコマンドを直接実行できます。
    -   これにより、エディタから離れることなく、迅速なバージョン管理作業が可能になります。

-   **VSCodeとの連携**:
    -   補助的な[Visual Studio Code拡張機能](./vscode_extension/)が同梱されています。
    -   秀丸エディタ上の操作一つで、同じリポジトリをVSCodeの「ソース管理」ビューで開き、より高度なGit操作へシームレスに移行できます。

### 技術的な特徴

本ツールは、以下の要素で構成されています。

-   **コアコンポーネント (C#)**:
    -   Gitの状態監視やコマンド実行といった中核機能を担うCOMコンポーネントです。
    -   .NET Framework 4.8 と .NET 8.0 の両方のランタイムに対応しており、幅広い環境で動作します。

-   **秀丸マクロ (JavaScript)**:
    -   C#コンポーネントを呼び出し、秀丸エディタのUI（レンダリングペイン）との連携を実現します。
    -   ステータスの更新やユーザー操作の橋渡しを行います。
