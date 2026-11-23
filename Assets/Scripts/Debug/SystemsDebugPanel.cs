using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// T09 – Bảng test nhanh GameManager + EconomyManager bằng nút bấm trên màn hình,
/// không cần UI thật. Chỉ dùng trong scene test; xóa hoặc tắt trước khi build.
/// Nếu scene có player khóa chuột, nhấn Esc để hiện chuột rồi mới bấm nút.
/// </summary>
public class SystemsDebugPanel : MonoBehaviour
{
    [SerializeField] private bool show = true;

    private string lastMessage = "";

    private void OnEnable()
    {
        // Đăng ký ở Start/OnEnable đều được vì 2 manager có DefaultExecutionOrder chạy trước
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnSpendFailed += HandleSpendFailed;
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnSpendFailed -= HandleSpendFailed;
    }

    private void HandleSpendFailed(int needed) =>
        lastMessage = $"Không đủ tiền! Cần {EconomyManager.Format(needed)}";

    private void Update()
    {
        // Hỗ trợ cả Input System mới (mặc định ở Unity 6) lẫn Input Manager cũ
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) show = !show;
#else
        if (Input.GetKeyDown(KeyCode.F1)) show = !show;
#endif
    }

    private void OnGUI()
    {
        if (!show) return;
        GameManager gm = GameManager.Instance;
        EconomyManager eco = EconomyManager.Instance;

        GUILayout.BeginArea(new Rect(10, 10, 300, 520), GUI.skin.box);
        GUILayout.Label("<b>T09 – Debug</b> (F1 ẩn/hiện)", new GUIStyle(GUI.skin.label) { richText = true });

        if (gm == null || eco == null)
        {
            GUILayout.Label("Thiếu GameManager hoặc EconomyManager trong scene!");
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label($"Ngày: {gm.CurrentDay}   Trạng thái: {gm.State}");
        GUILayout.Label($"Số dư: {EconomyManager.Format(eco.Balance)}");
        GUILayout.Label($"Tổng doanh thu: {EconomyManager.Format(eco.TotalRevenue)}");

        GUILayout.Space(6);
        GUILayout.Label("Trạng thái game");
        if (GUILayout.Button("Mở cửa (OpenStore)")) gm.OpenStore();
        if (GUILayout.Button("Đóng cửa (CloseStore)")) gm.CloseStore();
        if (GUILayout.Button("Kết thúc ngày (EndDay)")) gm.EndDay();
        if (GUILayout.Button("Sang ngày mới (StartNextDay)")) gm.StartNextDay();
        if (GUILayout.Button(gm.IsPaused ? "Tiếp tục (Resume)" : "Tạm dừng (Pause)")) gm.TogglePause();

        GUILayout.Space(6);
        GUILayout.Label("Tiền");
        if (GUILayout.Button("+ Bán hàng 50.000đ")) { eco.AddMoney(50_000, TransactionType.Sale, "Test bán"); lastMessage = "Đã cộng 50.000đ"; }
        if (GUILayout.Button("- Nhập hàng 200.000đ"))
        {
            if (eco.TrySpend(200_000, TransactionType.StockPurchase, "Test nhập")) lastMessage = "Đã trừ 200.000đ";
        }
        if (GUILayout.Button("- Nhập hàng 5.000.000đ (thử thiếu tiền)"))
        {
            if (eco.TrySpend(5_000_000, TransactionType.StockPurchase, "Test thiếu")) lastMessage = "Đã trừ 5.000.000đ";
        }

        GUILayout.Space(6);
        DayReport r = eco.GetDayReport();
        GUILayout.Label($"Hôm nay: thu {EconomyManager.Format(r.Revenue)} · nhập {EconomyManager.Format(r.StockCost)}");
        GUILayout.Label($"Lãi/lỗ: {EconomyManager.Format(r.Profit)} · {r.SaleCount} lượt bán");

        if (!string.IsNullOrEmpty(lastMessage))
        {
            GUILayout.Space(6);
            GUILayout.Label(lastMessage);
        }
        GUILayout.EndArea();
    }
}