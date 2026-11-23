using System;
using UnityEngine;

/// <summary>
/// Nơi một sản phẩm được phép trưng bày. Dùng [Flags] để một mặt hàng
/// có thể đặt được ở cả Kệ lẫn Tủ lạnh (ví dụ: Nước cam, Sữa).
/// </summary>
[Flags]
public enum ProductPlacement
{
    None   = 0,
    Shelf  = 1 << 0, // Kệ thường
    Fridge = 1 << 1, // Tủ lạnh
}

/// <summary>Phản ứng của khách với giá bán (dùng ở T38).</summary>
public enum PriceReaction
{
    Buy,      // <= buyThreshold × giá đề xuất
    Hesitate, // giữa buyThreshold và leaveThreshold
    Leave     // > leaveThreshold × giá đề xuất
}

/// <summary>
/// T08 – Dữ liệu tĩnh của một mặt hàng. Mỗi mặt hàng = 1 file .asset.
/// Chỉ chứa dữ liệu KHÔNG đổi khi chơi. Giá bán do người chơi đặt (T33)
/// và số lượng tồn trên kệ là dữ liệu runtime, lưu ở chỗ khác.
/// </summary>
[CreateAssetMenu(fileName = "NewProduct", menuName = "Shelf Stocking/Product Data", order = 0)]
public class ProductData : ScriptableObject
{
    [Header("Định danh")]
    [Tooltip("Mã cố định, viết thường, không dấu (vd: milk). KHÔNG đổi sau khi đã dùng — SaveSystem (T36) lưu theo mã này.")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [Tooltip("Ảnh nhỏ cho UI máy đặt hàng (T28). Có thể để trống lúc đầu.")]
    [SerializeField] private Sprite icon;

    [Header("Hình ảnh 3D")]
    [Tooltip("Model/prefab của 1 sản phẩm (file .glb đã import hoặc prefab bọc ngoài).")]
    [SerializeField] private GameObject modelPrefab;

    [Header("Kinh tế (VNĐ cho 1 sản phẩm)")]
    [Min(0)] [SerializeField] private int purchasePrice;
    [Min(0)] [SerializeField] private int suggestedPrice;

    [Header("Đóng gói & trưng bày")]
    [Min(1)] [SerializeField] private int unitsPerBox = 8;
    [SerializeField] private ProductPlacement placement = ProductPlacement.Shelf;

    [Header("Mở khóa (T37)")]
    [Min(1)] [SerializeField] private int unlockLevel = 1;

    [Header("Phản ứng giá của khách (T38, chỉnh ở T34)")]
    [Min(1f)] [SerializeField] private float buyThreshold = 1.2f;
    [Min(1f)] [SerializeField] private float leaveThreshold = 1.5f;

    // ---- Truy cập chỉ đọc cho các hệ thống khác ----
    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject ModelPrefab => modelPrefab;
    public int PurchasePrice => purchasePrice;
    public int SuggestedPrice => suggestedPrice;
    public int UnitsPerBox => unitsPerBox;
    public ProductPlacement Placement => placement;
    public int UnlockLevel => unlockLevel;
    public float BuyThreshold => buyThreshold;
    public float LeaveThreshold => leaveThreshold;

    /// <summary>Giá nhập cả thùng — dùng ở OrderSystem (T20).</summary>
    public int BoxCost => purchasePrice * unitsPerBox;

    /// <summary>Kệ/tủ có loại vị trí này có nhận mặt hàng không (T18).</summary>
    public bool CanPlaceOn(ProductPlacement where) => (placement & where) != 0;

    /// <summary>Khách phản ứng thế nào với giá bán người chơi đặt (T38).</summary>
    public PriceReaction GetReaction(int sellPrice)
    {
        if (suggestedPrice <= 0) return PriceReaction.Buy;
        float ratio = (float)sellPrice / suggestedPrice;
        if (ratio <= buyThreshold) return PriceReaction.Buy;
        if (ratio <= leaveThreshold) return PriceReaction.Hesitate;
        return PriceReaction.Leave;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id)) id = name.Trim().ToLowerInvariant().Replace(' ', '_');
        if (string.IsNullOrWhiteSpace(displayName)) displayName = name;
        if (leaveThreshold < buyThreshold) leaveThreshold = buyThreshold;
    }
#endif
}