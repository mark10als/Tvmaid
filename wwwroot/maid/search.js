
//検索用関数

function getSearchSql(option)
{
	var sql = getKeywordSql(option.keyword);
	sql += getInSql(option.service, "event.fsid");
	sql += getInSql(option.genre, "event.genre");
	sql += getInSql(option.week, "event.week");
	
	//開始時刻を取得 start / (10 * 1000 * 1000 * 60 * 60) % 24
	sql += getInSql(option.hour, "(event.start / 36000000000 % 24)");
	
	return sql;
}

//ジャンル、曜日等のSQLを作成
function getInSql(list, field)
{
	var sql = "";
	if(list != null && list.length > 0)
	{
		var str = list.join(",");
		sql = " and {0} in ({1})".format(field, str);
	}
	return sql;
}

//キーワードのSQLを作成
function getKeywordSql(keyword)
{
	keyword = keyword.split("|").join(" | ");	//"|"を" | "に置換
	keyword = keyword.split("｜").join(" | ");	//全角"｜"も可
	var words = keyword.split(/ |　/g);	//スペースで区切る
	var sql = "";
	var or = false;
	
	for(var i = 0; i < words.length; i++)
	{
		var word = words[i];
		
		if(word == "" || word == "-") { continue; }
		
		if(word == "|")
		{
			or = true;	//orフラグをセット
			continue;
		}
		
		if(sql != "")
		{
			if(or)
			{
				 sql += " or "; 
				 or = false;
			}
			else
			{
				sql += " and "; 
			}
		}
		
		if(word.charAt(0) == "-")
		{
			word = word.substr(1, word.length - 1);
			sql += "not ((event.title||desc||longdesc||genre_text) like '%{0}%' escape '^')".format(sqlLikeEncode(word));
		}
		else
		{
			sql += "(event.title||desc||longdesc||genre_text) like '%{0}%' escape '^'".format(sqlLikeEncode(word));
		}
	}
	
	if(sql == "") { sql = "1"; }	//キーワードがなければ、"1"を入れておく
	return "(" + sql + ")";
}
