
//javascript汎用関数

//クエリ取得
function getQueryString(s, defaultVal)
{
	var qstring;
	var url = location.href;
	url = decodeURIComponent(url.replace(/\+/g, '%20'));
	var query = new Array();
	query = url.split("?");

	if(!!query[1])
	{
		var querys = new Array();
		querys = query[1].split("&");
		var qcount = querys.length;
		qstring = new Array();
		var temporary_text = new Array();
		var hash_name;
		for(count = 0; count < qcount; count++)
		{
			temporary_text = querys[count].split("=");
			hash_name = temporary_text[0];
			qstring[hash_name] = temporary_text[1];
		}
		
		if (qstring[s] != null)
		{
			return qstring[s];
		}
	}
	return defaultVal;
}
/*
// Query String から 配列を返す
function getParameter(str)
{
	var dec = decodeURIComponent;
	var par = new Array, itm;
	if(typeof(str) == 'undefined') return par;
	if(str.indexOf('?', 0) > -1) str = str.split('?')[1];
	str = str.split('&');
	for(var i = 0; str.length > i; i++)
	{
		itm = str[i].split("=");
		if(itm[0] != ''){
			par[itm[0]] = typeof(itm[1]) == 'undefined' ? true : dec(itm[1]);
		}
	}
	return par;
}
*/
// 配列 から Query Stringを返す
function setParameter(par)
{
	var enc = encodeURIComponent;
	var str = '', amp = '';
	if(!par) return '';
	for(var i in par)
	{
		str = str + amp + i + "=" + par[i];	//enc(par[i]);　//エンコードしない
		amp = '&'
	}
	return str;
}

//文字列フォーマット
//@author phi
//
//添字引数版
//var str = "{0} : {1} + {2} = {3}".format("足し算", 8, 0.5, 8+0.5);
//
//オブジェクト版
//str = "名前 : {name}, 年齢 : {age}".format( { "name":"山田", "age":128 } );
//
if (String.prototype.format == undefined)
{
    String.prototype.format = function(arg)
    {
        // 置換ファンク
        var rep_fn = undefined;
        
        // オブジェクトの場合
        if (typeof arg == "object")
        {
            rep_fn = function(m, k) { return arg[k]; }
        }
        // 複数引数だった場合
        else
        {
            var args = arguments;
            rep_fn = function(m, k) { return args[ parseInt(k) ]; }
        }
        
        return this.replace( /\{(\w+)\}/g, rep_fn );
    }
}

//htmlエスケープ
String.prototype.escapeHTML = function()
{
  return this.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
}

//htmlエスケープを戻す
String.prototype.unescapeHTML = function()
{
  var div = document.createElement("div");
  div.innerHTML = this.replace(/</g,"&lt;")
                     .replace(/>/g,"&gt;")
                     .replace(/ /g, "&nbsp;")
                     .replace(/\r/g, "&#13;")
                     .replace(/\n/g, "&#10;");
  return div.textContent || div.innerText;
}
