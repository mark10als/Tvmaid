このフォークについて
===============
このフォークは「maidの中の人**◆dGJLJP3RhQ**」氏の制作したTvmaidをmark10alsが改造したものです。

これには、以下の改造も含んでいます。
ベースにした、「**◆0X7hT.k8kU**」さんの改造。
・Iconをリソースへ
・Sleep待ちダイアログにProgressBarを追加
・Update README.md & Add .gitignore
・Add README.md
・first commit
・Status API Training Shop Blog About Pricing 


【開発】 TS関連ソフトウェア総合スレ の住人で改造版を公開してくださった方の改造。
添付のreadme.txtでは、「Tvmaid R4 (´･ω･`) mod4」の表記がありました。
・チャンネル変更失敗時にTVTestを再起動させる
	録画開始時にTVTestがチャンネル変更に失敗した場合、計3回まで再起動させてチャンネル変更を試みることにした

・録画ファイル名に対する変換文字列を追加
	標準の番組名と開始時間に加え、放送局名とサービスID(16進数)の変換を追加しました

・録画終了後コマンド実行
	録画終了後にuser/Tvmaid.def内に記述されたコマンドを実行します
	録画エラーで終了した場合は実行されません

・ダミーTVTest起動
	実際に録画する前にTVTestのチャンネルを合わせておく機能（連続録画時、ダミーは起動されません）
	録画2分前にチャンネルを設定したダミーのTVTestを10秒程度起動します
	チャンネル変更に失敗した場合、計2回までダミーTVtestの起動を試みます
	チャンネル変更に不安の無い場合はuser/Tvmaid.defにtvtest.no.dummy=1を追記してください

・ぴったり録画開始
	実際に録画する約10秒前にTVTestを起動し録画開始時間が来るまで待機させます

・TVTestが異常終了した場合、プロセスが残ったままになる場合があることへの対処
	TVTestという名前のプロセスを全て終了させます
	全てのTVTestが終了してしまいますので、複数チューナーで運用する場合等は
	user/Tvmaid.defにtvtest.no.kill=1を追記したほうがよいでしょう
	うちは1チューナーかつ他の目的でTVTestを使用しない運用なので・・

===============
# Tvmaid ソースコードについて(オリジナル)

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


### 他者の著作物が含まれています。
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
