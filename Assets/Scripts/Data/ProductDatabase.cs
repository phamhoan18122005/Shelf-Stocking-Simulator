using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// T08 – Danh sách toàn bộ mặt hàng trong game (1 file ProductDatabase.asset).
/// Các hệ thống khác (đặt hàng, lưu game...) tra cứu sản phẩm qua đây.
/// </summary>
[CreateAssetMenu(fileName = "ProductDatabase", menuName = "Shelf Stocking/Product Database", order = 1)]
public class ProductDatabase : ScriptableObject
{
    [SerializeField] private List<ProductData> products = new List<ProductData>();

    public IReadOnlyList<ProductData> Products => products;

    /// <summary>Tìm mặt hàng theo mã id (dùng cho SaveSystem T36). Trả về null nếu không có.</summary>
    public ProductData GetById(string id)
    {
        foreach (ProductData p in products)
            if (p != null && p.Id == id) return p;
        return null;
    }

    /// <summary>Các mặt hàng đã mở khóa ở cấp hiện tại (T37).</summary>
    public List<ProductData> GetUnlocked(int level)
    {
        var result = new List<ProductData>();
        foreach (ProductData p in products)
            if (p != null && p.UnlockLevel <= level) result.Add(p);
        return result;
    }
}
