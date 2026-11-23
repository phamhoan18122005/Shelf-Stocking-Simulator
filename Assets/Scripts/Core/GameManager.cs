using System;
using UnityEngine;

/// <summary>Các trạng thái lớn của game.</summary>
public enum GameState
{
    MainMenu,    // Đang ở menu chính (T41)
    Preparing,   // Đầu ngày, cửa hàng CHƯA mở: đặt hàng, xếp kệ, đặt giá
    Open,        // Đang mở cửa, khách vào mua
    Closing,     // Đã đóng cửa, không sinh khách mới; chờ khách cuối thanh toán (TC11)
    DaySummary,  // Hiện bảng tổng kết cuối ngày (T23, T29)
    Paused       // Tạm dừng
}

/// <summary>
/// T09 – Khung trạng thái game. Chỉ quản lý TRẠNG THÁI và SỐ NGÀY,
/// không chứa logic đồng hồ (để dành cho T22 – Chu kỳ ngày).
///
/// Vòng đời một ngày:
///   Preparing --OpenStore()--> Open --CloseStore()--> Closing --EndDay()--> DaySummary --StartNextDay()--> Preparing (ngày +1)
/// </summary>
[DefaultExecutionOrder(-100)] // Chạy Awake trước các script khác để Instance luôn sẵn sàng
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState initialState = GameState.Preparing;
    [SerializeField, Min(1)] private int startDay = 1;
    [SerializeField] private bool logStateChanges = true;

    public GameState State { get; private set; }
    public int CurrentDay { get; private set; }
    public bool IsPaused => State == GameState.Paused;
    public bool IsStoreOpen => State == GameState.Open;

    /// <summary>(trạng thái cũ, trạng thái mới)</summary>
    public event Action<GameState, GameState> OnStateChanged;
    /// <summary>Gọi khi bắt đầu một ngày mới (tham số = số ngày).</summary>
    public event Action<int> OnDayStarted;
    /// <summary>Gọi khi kết thúc ngày, ngay trước khi vào DaySummary (T23 nghe event này).</summary>
    public event Action<int> OnDayEnded;

    private GameState stateBeforePause;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Có 2 GameManager trong scene, xóa bản thừa.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentDay = startDay;
        State = initialState;
    }

    private void Start()
    {
        if (State == GameState.Preparing) OnDayStarted?.Invoke(CurrentDay);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    // ---------------- Các lệnh chuyển trạng thái ----------------

    /// <summary>Từ menu chính vào game (chơi mới).</summary>
    public void StartNewGame()
    {
        CurrentDay = startDay;
        SetState(GameState.Preparing);
        OnDayStarted?.Invoke(CurrentDay);
    }

    /// <summary>Dùng cho SaveSystem (T36) sau khi tải file lưu.</summary>
    public void LoadDay(int day)
    {
        CurrentDay = Mathf.Max(1, day);
        SetState(GameState.Preparing);
        OnDayStarted?.Invoke(CurrentDay);
    }

    public void OpenStore()
    {
        if (!Require(GameState.Preparing, nameof(OpenStore))) return;
        SetState(GameState.Open);
    }

    public void CloseStore()
    {
        if (!Require(GameState.Open, nameof(CloseStore))) return;
        SetState(GameState.Closing);
    }

    /// <summary>T22 gọi khi đã đóng cửa VÀ khách cuối đã rời đi.</summary>
    public void EndDay()
    {
        if (State != GameState.Closing && State != GameState.Open)
        {
            Debug.LogWarning($"[GameManager] EndDay() không hợp lệ khi đang {State}.", this);
            return;
        }
        OnDayEnded?.Invoke(CurrentDay);
        SetState(GameState.DaySummary);
    }

    public void StartNextDay()
    {
        if (!Require(GameState.DaySummary, nameof(StartNextDay))) return;
        CurrentDay++;
        SetState(GameState.Preparing);
        OnDayStarted?.Invoke(CurrentDay);
    }

    public void Pause()
    {
        if (State == GameState.Paused || State == GameState.MainMenu) return;
        stateBeforePause = State;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(stateBeforePause);
    }

    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    // ---------------- Nội bộ ----------------

    private bool Require(GameState expected, string action)
    {
        if (State == expected) return true;
        Debug.LogWarning($"[GameManager] {action}() chỉ dùng được khi đang {expected}, hiện đang {State}.", this);
        return false;
    }

    private void SetState(GameState next)
    {
        if (next == State) return;
        GameState old = State;
        State = next;
        if (logStateChanges) Debug.Log($"[GameManager] Ngày {CurrentDay}: {old} → {next}", this);
        OnStateChanged?.Invoke(old, next);
    }
}