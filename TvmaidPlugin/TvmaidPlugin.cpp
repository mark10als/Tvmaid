#include "stdafx.h"

#define TVTEST_PLUGIN_CLASS_IMPLEMENT	// プラグインをクラスとして実装
#include "TVTestPlugin.h"
using namespace TVTest;

typedef WCHAR wchar;
const int null = 0;

wchar* PluginVer = L"Tvmaid Plugin R1";

//例外
class Exception
{
public:
	enum ErrorCode
	{
		NoError = 0,
		CreateShared,
		CreateWindow,
		CreateMutex,
		StartRec,
		StopRec,
		SetService,
		GetEvents,
		GetState,
		GetEnv,
		GetEventTime,
		GetTsStatus
	};

	ErrorCode Code;
	wchar* Message;	//動的メモリを割り当てないこと。固定メッセージのみ

	Exception(ErrorCode code, wchar* msg)
	{
		this->Code = code;
		this->Message = msg;
	}
};

//共有メモリ
class Shared
{
	HANDLE map;			//メモリマップハンドル
	wchar* view;		//メモリマップのポインタ
	wchar* position;	//書き込み位置

	const int argSize = 1 * 1000;			//引数サイズ(文字数)
	const int retSize = 200 * 1000;			//戻り値サイズ(文字数)
	const int memSize = argSize + retSize;	//全体のサイズ(文字数)

public:
	Shared(wchar* name)
	{
		map = CreateFileMapping((HANDLE)INVALID_HANDLE_VALUE, null, PAGE_READWRITE, 0, memSize * sizeof(wchar), name);
		if (map == null)
		{
			throw Exception(Exception::CreateShared, L"共有メモリの作成に失敗しました。");
		}

		view = (wchar*)MapViewOfFile(map, FILE_MAP_ALL_ACCESS, 0, 0, 0);
		if (view == null)
		{
			throw Exception(Exception::CreateShared, L"共有メモリの作成に失敗しました。");
		}
		Reset();
	}

	~Shared()
	{
		UnmapViewOfFile(view);
		CloseHandle(map);
	}

	void Reset()
	{
		position = view + argSize;
	}

	wchar* Read()
	{
		return (wchar*)view;
	}

	void Write(wchar* format, ...)
	{
		if (view + memSize - position < 1000)
		{
			throw Exception(Exception::CreateShared, L"共有メモリが足りません。書き込めませんでした。");
		}

		va_list args;
		va_start(args, format);
		int len = _vsnwprintf_s(position, view + memSize - position, _TRUNCATE, format, args);
		va_end(args);

		position += len;
	}
};

//ミューテックス
class Mutex
{
	HANDLE mutex;

public:
	Mutex(wchar* name)
	{
		mutex = CreateMutex(null, FALSE, name);
		if (mutex == null)
		{
			throw Exception(Exception::CreateMutex, L"Mutexの作成に失敗しました。");
		}
	}
	
	bool GetOwner(int timeout)
	{
		DWORD err = WaitForSingleObject(mutex, timeout);
		return (err == WAIT_ABANDONED || err == WAIT_OBJECT_0);
	}

	~Mutex()
	{
		if (mutex != null)
		{
			ReleaseMutex(mutex);
			CloseHandle(mutex);
			mutex = null;
		}
	}
};

//通信用ウインドウ
class Window
{
	HWND window;

public:
	Window(WNDPROC proc, wchar* id, LPVOID data)
	{
		const wchar* wndClass = L"/tvmaid/win";

		WNDCLASS wc;
		wc.style = 0;
		wc.lpfnWndProc = proc;
		wc.cbClsExtra = 0;
		wc.cbWndExtra = 0;
		wc.hInstance = g_hinstDLL;
		wc.hIcon = null;
		wc.hCursor = null;
		wc.hbrBackground = null;
		wc.lpszMenuName = null;
		wc.lpszClassName = wndClass;

		if (GetClassInfo(g_hinstDLL, wndClass, &wc) == 0)	//クラスが登録済みか
		{
			if (RegisterClass(&wc) == 0)
			{
				throw Exception(Exception::CreateWindowW, L"ウインドウの作成に失敗しました。");
			}
		}

		window = CreateWindow(wndClass, id, 0, 0, 0, 0, 0, HWND_MESSAGE, null, g_hinstDLL, data);
		if (window == null)
		{
			throw Exception(Exception::CreateWindow, L"ウインドウの作成に失敗しました。");
		}
	}

	~Window()
	{
		if (window != null)
		{
			DestroyWindow(window);
			window = null;
		}
	}
};

//プラグインクラスの基底クラス
//制御用ウインドウ、メモリマップ、Mutexの管理を行う
class TvProcessBase : public CTVTestPlugin
{
private:
	Window* window;					//制御用ウインドウ
	Mutex* mutex;					//チューナ排他制御Mutex。このMutexは、複数のTVTest間でチューナの排他制御をするためのものなので、他で所有権を取得しないこと
	wchar driverId[MAX_PATH];		//ドライバID
	bool userStart = true;			//起動したのがユーザかどうか
	const int timeout = 60 * 1000;	//ドライバ開放待ち時間(Mutex)

protected:
	Shared* shared;					//共有メモリ

private:
	//TVTestから直接呼ばれるメソッド群
	//例外を必ず処理する

	virtual bool GetPluginInfo(TVTest::PluginInfo *pInfo)
	{
		pInfo->Type = PLUGIN_TYPE_NORMAL;
		pInfo->Flags = 0;
		pInfo->pszPluginName = L"Tvmaid Plugin";
		pInfo->pszCopyright = L"Tvmaid";
		pInfo->pszDescription = L"Tvmaid Plugin";

		return true;
	}

	virtual bool Initialize()
	{
		/*
		::AllocConsole();
		_wfreopen(L"CON", L"r", stdin);
		_wfreopen(L"CON", L"w", stdout);
		_wsetlocale(LC_ALL, L"japanese");
		*/
		Log(PluginVer);

		window = null;
		mutex = null;
		shared = null;

		m_pApp->SetEventCallback(EventCallback, this);
		return true;
	}
	
	virtual bool Finalize()
	{
		Dispose();
		return true;
	}

	//TVTestイベントコールバック
	friend static LRESULT CALLBACK EventCallback(UINT Event, LPARAM lParam1, LPARAM lParam2, void *pClientData)
	{
		TvProcessBase* _this = static_cast<TvProcessBase*>(pClientData);

		switch (Event)
		{
		case EVENT_DRIVERCHANGE:
			//ユーザがドライバを変更した
			try
			{
				_this->userStart = true;	//ユーザが起動したことにする
				_this->Dispose();
				_this->Init(null);
			}
			catch (Exception& e)
			{
				_this->Log(e.Message);
				_this->Dispose();
			}
			break;
		case EVENT_STARTUPDONE:
			_this->Debug(L"Startup done");
			_this->m_pApp->EnablePlugin(true);
			break;
		case EVENT_PLUGINENABLE:
			try
			{
				_this->Debug(L"Enable Plugin");
				_this->EnablePlugin(lParam1 != 0);
			}
			catch (Exception& e)
			{
				_this->Log(e.Message);
				_this->Dispose();

				//Tvmaidからの起動なら終了する
				if (_this->userStart == false) { _this->m_pApp->Close(); }
			}
			break;
		}
		return 0;
	}
	
	//通信用ウインドウコールバック
	friend static LRESULT CALLBACK WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
	{
		TvProcessBase* _this = reinterpret_cast<TvProcessBase*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));

		//Tvmaidからの呼び出し
		//エラーコードを返す
		if (uMsg >= 0xb000)
		{
			try
			{
				_this->Call(uMsg, wParam, lParam);
				return 0;	//エラー無し
			}
			catch (Exception& e)
			{
				return e.Code;	//エラーコードを返す
			}
		}
		else
		{
			switch (uMsg)
			{
			case WM_CREATE:
			{
				LPCREATESTRUCT pcs = reinterpret_cast<LPCREATESTRUCT>(lParam);
				_this = reinterpret_cast<TvProcessBase*>(pcs->lpCreateParams);
				SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(_this));
				return 0;	//0を返すこと
			}
			}
			return ::DefWindowProc(hwnd, uMsg, wParam, lParam);
		}
	}
	//ここまでTVTestメソッド

protected:
	virtual void Call(UINT callNum, WPARAM arg1, LPARAM arg2) = 0;

	void Log(wchar* msg)
	{
		m_pApp->AddLog(msg);
	}

	void Debug(wchar* msg)
	{
#ifdef _DEBUG
		m_pApp->AddLog(msg);
#endif
	}

	//ドライバ名を取得
	void GetDriver(wchar* driver)
	{
		wchar path[MAX_PATH];
		wchar ext[MAX_PATH];

		m_pApp->GetDriverFullPathName(path, MAX_PATH);
		_wsplitpath_s(path, null, 0, null, 0, driver, MAX_PATH, ext, MAX_PATH);

		wcscat_s(driver, MAX_PATH, ext);
	}

private:
	void EnablePlugin(bool enable)
	{
		if (enable)
		{
			//環境変数で渡されている場合は、そのDriverIdを使う
			wchar env[MAX_PATH];
			size_t size;
			_wgetenv_s(&size, null, 0, L"DriverId");
			if (size == 0)
			{
				Debug(L"ユーザが起動しました。");
				userStart = true;
				Init(null);
			}
			else
			{
				size = MAX_PATH;
				if (_wgetenv_s(&size, env, size, L"DriverId") != 0)
				{
					throw Exception(Exception::GetEnv, L"環境変数の取得に失敗しました。");
				}
				else
				{
					Debug(L"Tvmaidから起動しました。");
					userStart = false;
					Init(env);
				}
			}
		}
		else
		{
			Dispose();
		}
	}
	
	void Init(wchar* id)
	{
		id == null ? InitMutex() : InitMutex(id);

		wchar name[MAX_PATH];
		swprintf_s(name, L"/tvmaid/map/%s", driverId);
		shared = new Shared(name);

		window = new Window(WndProc, driverId, this);	//ウインドウをメモリマップより後に作る。これで通信可能か判断しているため

		wchar msg[MAX_PATH];
		wcscpy_s(msg, L"Tvmaid DriverId: ");
		wcscat_s(msg, driverId);
		Log(msg);
	}

	void Dispose()
	{
		if (window != null)
		{
			delete window;
			window = null;
		}
		if (mutex != null)
		{
			delete mutex;
			mutex = null;
		}
		if (shared != null)
		{
			delete shared;
			shared = null;
		}
	}

	void InitMutex(wchar* id)
	{
		wchar name[MAX_PATH];
		swprintf_s(name, L"/tvmaid/mutex/%s", id);

		mutex = new Mutex(name);
		if (mutex->GetOwner(timeout))
		{ 
			wcscpy_s(driverId, id);
		}
		else
		{
			throw Exception(Exception::CreateMutex, L"ミューテックスの作成に失敗しました。");
		}
	}

	void InitMutex()
	{
		wchar driver[MAX_PATH];
		GetDriver(driver);

		int i = 0;
		while (1)
		{
			wchar name[MAX_PATH];

			swprintf_s(driverId, L"%s/%d", driver, i);
			swprintf_s(name, L"/tvmaid/mutex/%s", driverId);

			mutex = new Mutex(name);
			if(mutex->GetOwner(0))
			{
				break;
			}
			else
			{
				i++;
			}
		}
	}
};

//プラグインクラス
class TvProcess : public TvProcessBase
{
	virtual void Call(UINT callNum, WPARAM arg1, LPARAM arg2)
	{
		enum
		{
			_Close = 0xb000,
			_GetState,
			_GetServices,
			_GetEvents,
			_SetService,
			_StartRec,
			_StopRec,
			_GetEventTime,
			_GetTsStatus
		};

		void(TvProcess::*call[])() =
		{
			&TvProcess::Close,
			&TvProcess::GetState,
			&TvProcess::GetServices,
			&TvProcess::GetEvents,
			&TvProcess::SetService,
			&TvProcess::StartRec,
			&TvProcess::StopRec,
			&TvProcess::GetEventTime,
			&TvProcess::GetTsStatus
		};

		shared->Reset();
		(this->*call[callNum - _Close])();
	}
	
	void StartRec()
	{
		RecordInfo info;
		info.Mask = RECORD_MASK_FILENAME;
		info.Flags = 0;
		info.StartTimeSpec = RECORD_START_NOTSPECIFIED;
		info.StopTimeSpec = RECORD_STOP_NOTSPECIFIED;
		info.pszFileName = shared->Read();

		if (m_pApp->StartRecord(&info) == false)
		{
			throw Exception(Exception::StartRec, L"録画を開始できません。");
		}
	}

	void StopRec()
	{
		if (m_pApp->StopRecord() == false)
		{
			throw Exception(Exception::StopRec, L"録画を停止できません。");
		}
	}

	void Close()
	{
		if (m_pApp->Close(CLOSE_EXIT) == false)
		{
			throw Exception(Exception::StopRec, L"TVTestの終了に失敗しました。");
		}
	}

	//サービスのリストを取得する
	void GetServices()
	{
		int num = 0;
		m_pApp->GetTuningSpace(&num);

		// 全てのチューニング空間のサービスを取得する
		for (int space = 0; space < num; space++)
		{
			ChannelInfo info;
			info.Size = sizeof(info);

			for (int ch = 0; m_pApp->GetChannelInfo(space, ch, &info); ch++)
			{
				//有効なサービスだけ取得
				if(info.Flags != CHANNEL_FLAG_DISABLED)
				{
					shared->Write(L"%d\x1%d\x1%d\x1%s\x2",
									info.NetworkID,
									info.TransportStreamID,
									info.ServiceID,
									info.szChannelName);
				}
			}
		}
	}
	
	//サービス変更
	void SetService()
	{
		int nid, tsid, sid;
		swscanf_s(shared->Read(), L"%d\x1%d\x1%d", &nid, &tsid, &sid);

		int num = 0;
		m_pApp->GetTuningSpace(&num);

		for (int space = 0; space < num; space++)
		{
			ChannelInfo info;
			info.Size = sizeof(info);

			for (int ch = 0; m_pApp->GetChannelInfo(space, ch, &info); ch++)
			{
				if (info.Flags != CHANNEL_FLAG_DISABLED && info.NetworkID == nid && info.TransportStreamID == tsid && info.ServiceID == sid)
				{
					if(m_pApp->SetChannel(space, ch) == false)
					{
						throw Exception(Exception::SetService, L"サービスの変更に失敗しました。");
					}
					return;
				}
			}
		}
		throw Exception(Exception::SetService, L"サービスの変更に失敗しました。");
	}

	void GetTimeStr(SYSTEMTIME* time, wchar* buf, size_t size)
	{
		swprintf_s(buf, size, L"%d/%d/%d %02d:%02d:00",
			time->wYear,
			time->wMonth,
			time->wDay,
			time->wHour,
			time->wMinute);
	}

	//指定番組の時間を取得(録画用時、時間チェック用)
	void GetEventTime()
	{
		int nid, tsid, sid, eid;
		swscanf_s(shared->Read(), L"%d\x1%d\x1%d\x1%d\x1%d\x1%d\x1%d", &nid, &tsid, &sid, &eid);

		EpgEventQueryInfo info;
		info.NetworkID = nid;
		info.TransportStreamID = tsid;
		info.ServiceID = sid;
		info.EventID = eid;
		info.Type = EPG_EVENT_QUERY_EVENTID;
		info.Flags = 0;
		EpgEventInfo* event = m_pApp->GetEpgEventInfo(&info);

		if (event != null)
		{
			wchar time[30];
			GetTimeStr(&event->StartTime, time, sizeof(time) / sizeof(wchar_t));
			shared->Write(L"%s\x1%d\x2", time, event->Duration);
			m_pApp->FreeEpgEventInfo(event);
		}
		else
		{
			throw Exception(Exception::GetEventTime, L"番組時間の取得に失敗しました。");
		}
	}

	void GetEvents()
	{
		int nid, tsid, sid;
		swscanf_s(shared->Read(), L"%d\x1%d\x1%d", &nid, &tsid, &sid);

		EpgEventList list;
		list.NetworkID = nid;
		list.TransportStreamID = tsid;
		list.ServiceID = sid;

		if (m_pApp->GetEpgEventList(&list))
		{
			for (int i = 0; i < list.NumEvents; i++)
			{
				EpgEventInfo* event = list.EventList[i];

				wchar time[30];
				GetTimeStr(&event->StartTime, time, sizeof(time) / sizeof(wchar_t));
				
				//ジャンルを取得
				int genre = 0x10;
				int subGenre = 0x10;

				if (event->ContentListLength > 0 && event->ContentList != null)
				{
					genre = event->ContentList->ContentNibbleLevel1;
					if (event->ContentListLength > 1)
					{
						subGenre = event->ContentList->ContentNibbleLevel2;
					}
				}

				wchar* nullStr = L"";
				shared->Write(L"%d\x1%s\x1%d\x1%s\x1%s\x1%s\x1%d\x1%d\x2",
					event->EventID,
					time,
					event->Duration,
					event->pszEventName == null ? nullStr : event->pszEventName,
					event->pszEventText == null ? nullStr : event->pszEventText,
					event->pszEventExtendedText == null ? nullStr : event->pszEventExtendedText,
					genre,
					subGenre
					);
			}
			m_pApp->FreeEpgEventList(&list);
		}
		else
		{
			throw Exception(Exception::GetEvents, L"番組表の取得に失敗しました。");
		}
	}

	void GetState()
	{
		RecordStatusInfo info;
		info.Size = sizeof(RecordStatusInfo);

		if (m_pApp->GetRecordStatus(&info))
		{
			shared->Write(L"%d\x2", info.Status);
		}
		else
		{
			throw Exception(Exception::GetState, L"状態の取得に失敗しました。");
		}
	}

	void GetTsStatus()
	{
		StatusInfo info;
		info.Size = sizeof(StatusInfo);

		if (m_pApp->GetStatus(&info))
		{
			shared->Write(L"%d\x1%d\x1%d\x2", info.ErrorPacketCount, info.DropPacketCount, info.ScramblePacketCount);
		}
		else
		{
			throw Exception(Exception::GetTsStatus, L"TS状態の取得に失敗しました。");
		}
	}
};

CTVTestPlugin* CreatePluginClass()
{
	return new TvProcess;
}
