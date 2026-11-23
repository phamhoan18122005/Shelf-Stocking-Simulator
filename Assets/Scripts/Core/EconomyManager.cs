using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Loại giao dịch, dùng để tách doanh thu / chi phí khi tổng kết (T23).</summary>
public enum TransactionType
{
    Sale,          // Khách thanh toán ở quầy (T26)  → tiền vào
    StockPurchase, // Đặt hàng nhập kho (T20)         → tiền ra
    Upgrade,       // Mua thêm kệ (T35)                → tiền ra
    Other          // Khác (thưởng, phạt, test...)
}

/// <summary>Một dòng giao dịch. amount dương = tiền vào, âm = tiền ra.</summary>
[Serializable]
public struct Transaction
{
    public int day;
    public TransactionType type;
    public int amount;
    public string note;

    public Transaction(int day, TransactionType type, int amount, string note)
    {
        this.day = day; this.type = type; this.amount = amount; this.note = note;
    }
}

/// <summary>Số liệu một ngày cho bảng tổng kết (T23, T29).</summary>
public struct DayReport
{
    public int Day;
    public int Revenue;      // Tổng tiền bán hàng
    public int StockCost;    // Tổng tiền nhập hàng
    public int OtherCost;    // Mua kệ + chi khác
    public int SaleCount;    // Số lượt thanh toán = số khách đã phục vụ (nếu mỗi khách 1 lần thanh toán)
    public int Profit => Revenue - StockCost - OtherCost;
}

/// <summary>Dữ liệu cần lưu cho SaveSystem (T36).</summary>
[Serializable]
public class EconomySaveData
{
    public int balance;
    public int totalRevenue;
    public List<Transaction> transactions = new List<Transaction>();
}

/// <summary>
/// T09 – Quản lý tiền: số dư, cộng/trừ, ghi lại mọi giao dịch.
/// Đơn vị: VNĐ, kiểu int (không dùng float để tránh lệch tiền thừa ở quầy).
/// </summary>
[DefaultExecutionOrder(-90)]
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField, Min(0)] private int startingBalance = 1_000_000;

    private readonly List<Transaction> transactions = new List<Transaction>();

    public int Balance { get; private set; }
    /// <summary>Tổng doanh thu từ đầu game — T37 dùng để lên cấp cửa hàng.</summary>
    public int TotalRevenue { get; private set; }
    public IReadOnlyList<Transaction> Transactions => transactions;

    /// <summary>Số dư mới sau mỗi lần thay đổi — HUD (T14) nghe event này.</summary>
    public event Action<int> OnBalanceChanged;
    /// <summary>Mỗi giao dịch mới được ghi.</summary>
    public event Action<Transaction> OnTransactionAdded;
    /// <summary>Chi tiền thất bại vì không đủ (tham số = số tiền cần) — UI hiện thông báo.</summary>
    public event Action<int> OnSpendFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[EconomyManager] Có 2 EconomyManager trong scene, xóa bản thừa.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Balance = startingBalance;
    }

    private void Start() => OnBalanceChanged?.Invoke(Balance); // để HUD hiện số dư ban đầu

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------- API cho các hệ thống khác ----------------

    public bool CanAfford(int amount) => amount >= 0 && amount <= Balance;

    /// <summary>Cộng tiền. Ví dụ quầy thu ngân: AddMoney(tongTien, TransactionType.Sale, "Khách #12").</summary>
    public void AddMoney(int amount, TransactionType type = TransactionType.Sale, string note = "")
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[EconomyManager] AddMoney cần số dương, nhận {amount}.", this);
            return;
        }
        Balance += amount;
        if (type == TransactionType.Sale) TotalRevenue += amount;
        Record(type, amount, note);
    }

    /// <summary>
    /// Trừ tiền nếu đủ. Trả về false và KHÔNG trừ nếu thiếu tiền.
    /// Ví dụ đặt hàng: if (EconomyManager.Instance.TrySpend(gio.Total, TransactionType.StockPurchase, "Đơn hàng")) GiaoHang();
    /// </summary>
    public bool TrySpend(int amount, TransactionType type, string note = "")
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[EconomyManager] TrySpend cần số dương, nhận {amount}.", this);
            return false;
        }
        if (!CanAfford(amount))
        {
            OnSpendFailed?.Invoke(amount);
            return false;
        }
        Balance -= amount;
        Record(type, -amount, note);
        return true;
    }

    /// <summary>Tổng hợp số liệu của một ngày (mặc định: ngày hiện tại).</summary>
    public DayReport GetDayReport(int day = -1)
    {
        if (day < 0) day = CurrentDay();
        var report = new DayReport { Day = day };
        foreach (Transaction t in transactions)
        {
            if (t.day != day) continue;
            switch (t.type)
            {
                case TransactionType.Sale:          report.Revenue += t.amount; report.SaleCount++; break;
                case TransactionType.StockPurchase: report.StockCost += -t.amount; break;
                default:
                    if (t.amount < 0) report.OtherCost += -t.amount; else report.Revenue += t.amount;
                    break;
            }
        }
        return report;
    }

    // ---------------- Lưu / tải (T36) ----------------

    public EconomySaveData GetSaveData()
    {
        return new EconomySaveData
        {
            balance = Balance,
            totalRevenue = TotalRevenue,
            transactions = new List<Transaction>(transactions)
        };
    }

    public void LoadSaveData(EconomySaveData data)
    {
        if (data == null) return;
        Balance = data.balance;
        TotalRevenue = data.totalRevenue;
        transactions.Clear();
        if (data.transactions != null) transactions.AddRange(data.transactions);
        OnBalanceChanged?.Invoke(Balance);
    }

    /// <summary>Định dạng tiền kiểu Việt Nam: 1250000 → "1.250.000đ".</summary>
    public static string Format(int vnd)
    {
        string sign = vnd < 0 ? "-" : "";
        return sign + Math.Abs((long)vnd).ToString("#,0").Replace(',', '.') + "đ";
    }

    // ---------------- Nội bộ ----------------

    private void Record(TransactionType type, int signedAmount, string note)
    {
        var t = new Transaction(CurrentDay(), type, signedAmount, note);
        transactions.Add(t);
        OnTransactionAdded?.Invoke(t);
        OnBalanceChanged?.Invoke(Balance);
    }

    private static int CurrentDay() => GameManager.Instance != null ? GameManager.Instance.CurrentDay : 0;
}