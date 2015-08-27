# Tvmaid R4 のmark10alsに依る改造版
Version は、 "R4 (´･ω･`) mod4 + mark10als" としています。

## https://github.com/mark10als/Tvmaid

これはTvmaid R4 に自分用に手を加えたものです。

## ■前回からの変更点
Tvmaid_nora_modの人のmod4での修正を取り込んだ
・チャンネル変更失敗時にTVTestを再起動させる
mark10alsの改造。
・録画ファイル名に対する変換文字列を追加
	TVTest0.9.0を参考にして追加しました。
	番組開始日時
	{start-date}			年月日
	{start-year}			年
	{start-year2}			年(下2桁)
	{start-month2}			月(2桁)
	{start-day2}			日(2桁)
	{start-time6}			時刻(時+分+秒)
	{start-hour2}			時(2桁)
	{start-minute2}			分(2桁)
	{start-second2}			秒(2桁)
	{start-day-of-week}		曜日(漢字)
	番組終了日時
	{end-date}				年月日
	{end-year}				年
	{end-year2}				年(下2桁)
	{end-month2}			月(2桁)
	{end-day2}				日(2桁)
	{end-time6}				時刻(時+分+秒)
	{end-hour2}				時(2桁)
	{end-minute2}			分(2桁)
	{end-second2}			秒(2桁)
	{end-day-of-week}		曜日(漢字)


## ■注意！
これは、C#をはじめて見た超初心者が、グーグル先生に聞いて
手探りで改造したものです。
どんなことが起こっても責任は取りません。絶対に！
自己責任で人柱になっても良いという方のみお使いください。

## ■謝辞

maidの中の人 ◆dGJLJP3RhQ 様に感謝です。
http://nana2.sarashi.com/

【開発】 TS関連ソフトウェア総合スレ Part15 [転載禁止]
で改造版を公開してくださった方にも感謝です。
>>323 ：名無しさん＠編集中：2015/08/12(水) 17:48:33.87 ID:UkSoE8bG
>>    作者様のご厚意に甘えて自分用にTvmaidをちょびっとだけ改造してみました。感謝感謝です
>>    http://www1.axfc.net/u/3516305?key=dtv
>>    素人の修正なので録画に失敗しても怒らない人用です

フォークをGithubに上げてくださった方にも感謝です。
>>324 ：EpgTimerWeb2 ◆0X7hT.k8kU 生きろ！(c)2ch.net：2015/08/12(水) 23:11:05.85 ID:4B9gji7a
>>    https://github.com/yukiboard/Tvmaid

## ■経緯
maidの中の人が302でプログラムソースを公開してくださった時から、
改造に挑戦していました。
私が苦労している間に、早々に改造版を公開された方が２人もおられました。
323と324の方々です。
そこで、324をベースにして、323のをマージした上で改造をやり直しました。

## ■追加・修正したとこ
機能は、オリジナルに先人の機能が追加されています。
「◆0X7hT.k8kU」の人の改造。
・Sleep待ちダイアログにProgressBarを追加 
Tvmaid_nora_modの人の改造。
mod3までの修正
・録画ファイル名に対する変換文字列を追加
・録画終了後コマンド実行
・ダミーTVTest起動
・ぴったり録画開始
・TVTestが異常終了した場合、プロセスが残ったままになる場合があることへの対処
mod4での修正
・チャンネル変更失敗時にTVTestを再起動させる

mark10alsの改造。
・user/Tvmaid.defのチェックの追加。
	"record.file"が未設定ならエラーにする。
・録画ファイル名に対する変換文字列を追加
	終了時間の変換を追加しました。「{end-time}で変換」
	TVTest0.9.0を参考にして追加しました。
	番組開始日時
	{start-date}			年月日
	{start-year}			年
	{start-year2}			年(下2桁)
	{start-month2}			月(2桁)
	{start-day2}			日(2桁)
	{start-time6}			時刻(時+分+秒)
	{start-hour2}			時(2桁)
	{start-minute2}			分(2桁)
	{start-second2}			秒(2桁)
	{start-day-of-week}		曜日(漢字)
	番組終了日時
	{end-date}				年月日
	{end-year}				年
	{end-year2}				年(下2桁)
	{end-month2}			月(2桁)
	{end-day2}				日(2桁)
	{end-time6}				時刻(時+分+秒)
	{end-hour2}				時(2桁)
	{end-minute2}			分(2桁)
	{end-second2}			秒(2桁)
	{end-day-of-week}		曜日(漢字)

・録画終了後コマンド実行
	リダイレクトを使用したバッチコマンドを指定したときに、
	コマンドが実行されなかった。（私だけかも？）
	そこで、外部プログラム実行の時に出力を読み取らないようにした。
	また、終了待ちの処理を変更した。

## ■修正したファイル
	doc\original\Tvmaid.def
	Tvmaid\Program.cs
	Tvmaid\RecTimer.cs
	Tvmaid\Tuner.cs
	このファイルをGithub( https://github.com/yukiboard/Tvmaid )の
	同じファイルと置き換えるだけでビルド出来ます。


## 開発環境
* Microsoft Windows 7 HP 64bit
* Microsoft Visual Studio Express 2013 for Windows Desktop

## ライセンス
オリジナルに準じます。

