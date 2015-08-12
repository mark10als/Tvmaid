
//Web Api

//呼び出し
function webapi(arg, callback)
{
	$.getJSON("/webapi", arg, callback)
		.fail(
			function()
			{
				alert("通信に失敗しました。");
			}
		);
}

//SQL文を指定してテーブルを取得
function getTable(sql, callback)
{
	webapi({ api: "GetTable", sql: sql }, callback);
}

//SQL文で、文字列を指定する場合のエスケープ
function sqlEncode(text)
{
	return text.split("'").join("''");
}

//SQLのlike文のエスケープ
//like文を使う場合は「escape '^'」を追加してください
function sqlLikeEncode(text)
{
    text = sqlEncode(text);

	text = text.split("^").join("^^");
	text = text.split("_").join("^_");
	text = text.split("%").join("^%");

    return text;
}

//.netのDateTimeをXDateに変換
function convertXDate(time)
{
	time = time / 10000 - 62135596800000;	//ナノ秒をミリ秒にして、1970年分引く
	var timezone = (new Date()).getTimezoneOffset() * 60 * 1000;	//タイムゾーン
	return new XDate(time + timezone);
}

//XDateを.netのDateTimeに変換
function convertDateTime(xdate)
{
	var timezone = (new Date()).getTimezoneOffset() * 60 * 1000;
	return (xdate.getTime() - timezone + 62135596800000) * 10000;
}
