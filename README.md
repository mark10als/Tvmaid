Tvmaid ソースコードについて
=============

## 開発環境
* Microsoft Windows 8.1 Pro 64bit
* Microsoft Visual Studio Express 2013 for Windows Desktop

## ビルド
特に必要な物はありません。
Visual Studioで、ソリューションを開いてビルドするだけです。

## フォルダの説明
* bin
　実行ファイル出力フォルダ

* doc
　ドキュメントと、オリジナルの設定ファイル

* Maidmon
　メイドモニタのソースコード

* Setup
　セットアッププログラムのソースコード

* Tvmaid
　Tvmaid本体のソースコード

* TvmaidPlugin
　TVTest Pluginのソースコード

* wwwroot
　内蔵Webサーバルートフォルダ

## デバッグ方法
bin\debugフォルダに、Tvmaidの実行環境を作ります。
(bin\debugフォルダを、リリース版のTvmaidと同じファイル構成にする)

具体的には、
doc\originalフォルダと、wwwrootフォルダをbin\debugにコピーします。

デバッグで使うTVTestも、bin\debug\TVTestへコピーしてください。
その中のPluginsフォルダにデバッグ版TvmaidPlugin.dllが作成されます。

準備ができたら、Tvmaidのデバッグを開始します。

TvmaidPluginのデバッグは、TVTestを起動し、プロセスにアタッチします。


## ライセンス

このソースコードを改造、流用する場合は以下の条件があります。

* 改造、流用の結果に関して作者は責任を負いません。
* 改造版を公開するときは、改造元があることを記述してください。
　(Tvmaidが改造元です、くらいでいいです)
* 下に記述する他者の著作物のライセンスに留意してください。


# 他者の著作物が含まれています。
著作権は、それぞれの製作者にあります。
著作物ごとのライセンスに従って取り扱ってください。


* System.Data.SQLite
Public domain.


* DynamicJson
created and maintained by neuecc <ils@neue.cc>
licensed under Microsoft Public License(Ms-PL)


* jQuery
Copyright 2013 jQuery Foundation and other contributors
The MIT License
<http://opensource.org/licenses/mit-license.php>


* jQuery Cookie Plugin
Copyright 2006, 2014 Klaus Hartl
Released under the MIT license
<http://opensource.org/licenses/mit-license.php>

* XDate
Copyright 2011 Adam Shaw
Dual-licensed under MIT or GPL
<http://opensource.org/licenses/mit-license.php>

* アイコン
Icons made by Freepik from www.flaticon.com
