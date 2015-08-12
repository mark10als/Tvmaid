
//ユーティリティ関数

function writeMenu()
{
	$("#menu").append(
		"<a href='epg.html'>番組表</a>" +
		"<a href='search.html'>検索</a>" +
		"<a href='record.html'>予約</a>" +
		"<a href='result.html'>結果</a>" +
		"<a href='auto-record-edit.html?id=-1'>自動予約</a>" +
		"<a href='record-edit.html?id=-1'>時間予約</a>"
	);
}

function getWeekText(num)
{
	var weekText = ["日", "月", "火", "水", "木", "金", "土"];
	return weekText[num];
}

