
//検索用部品

//サービスリストを作成
function setService(selected)
{
	var sql = "select fsid, name from service group by fsid order by id";
	getTable(sql, function(ret)
	{
		if(ret.Code != 0)
		{
			alert("サービスが取得できませんでした。\n[詳細] " + ret.Message);
			return;
		}
		
		var services = ret.Data1;
		
		for(var i = 0; i < services.length; i++)
		{
			$('#service').append($('<option />').val(services[i][0]).html(services[i][1]));
		}
		
		if(selected != null)
		{
			$("#service").val(selected);
		}
	});
}

//サービス以外の部品を作成
function setOption(selected)
{
	var weekText = ["日", "月", "火", "水", "木", "金", "土"];
	for(var i = 0; i < weekText.length; i++)
	{
		$('#week').append($('<option />').val(i).html(weekText[i]));
	}

	for(var i = 0; i < 24; i++)
	{
		$('#hour').append($('<option />').val(i).html(i + " 時"));
	}
	
	for(var i = 0; i < 11; i++)
	{
		$('#genre').append($('<option />').val(i).html(getGenre(i)));
	}
	
	if(selected != null)
	{
		$("#week").val(selected.week);
		$("#hour").val(selected.hour);
		$("#genre").val(selected.genre);
	}
}

//リストの中に-1があれば空の配列を返す
function checkList(name)
{
	var list = $(name).val() || [];
	return list.indexOf("-1") == -1 ? list : [];
}

//入力データをデータオブジェクトで返す
function getInputObject()
{
	var obj = {
		keyword: $('#keyword').val(),
		service: checkList("#service"),
		genre: checkList('#genre'),
		week: checkList('#week'),
		hour: checkList('#hour')
	};
	return obj;
}

//検索実行
function search(thisWindow)
{
	if($('#keyword').val() == "")
	{
		alert("キーワードを入力してください。");
		return;
	}
	
	var obj = getInputObject();
	var json = JSON.stringify(obj);
	if(thisWindow)
	{
		location.href = "search.html?option=" + encodeURIComponent(json);
	}
	else
	{
		window.open().location.href = "search.html?option=" + encodeURIComponent(json);
	}
}
