#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// T08 – Tạo một lần toàn bộ ProductData.asset từ bảng "Danh sách mặt hàng" (T03).
/// Menu: Tools > Shelf Stocking > Tạo dữ liệu sản phẩm
/// - Asset đã có thì KHÔNG ghi đè giá/thông số (để không mất số liệu cân bằng ở T34),
///   chỉ gán thêm model nếu đang trống và thêm vào database nếu chưa có.
/// </summary>
public static class ProductAssetGenerator
{
    private const string ProductFolder = "Assets/Prefabs/Products";
    private const string DatabasePath  = "Assets/Prefabs/ProductDatabase.asset";

    private const ProductPlacement Shelf  = ProductPlacement.Shelf;
    private const ProductPlacement Fridge = ProductPlacement.Fridge;
    private const ProductPlacement Both   = ProductPlacement.Shelf | ProductPlacement.Fridge;

    private struct Row
    {
        public string Id, Name, File;
        public int Buy, Sell, PerBox, Level;
        public ProductPlacement Place;

        public Row(string id, string name, string file, int buy, int sell, int perBox, ProductPlacement place, int level)
        {
            Id = id; Name = name; File = file; Buy = buy; Sell = sell; PerBox = perBox; Place = place; Level = level;
        }
    }

    // Giá (VNĐ/1 sản phẩm) là số ĐỀ XUẤT ban đầu — nhóm chỉnh lại trong Inspector hoặc ở T34.
    // Tên file giữ đúng như trong sheet (kể cả "Strawberrt.glb").
    private static readonly Row[] Rows =
    {
        //       id                 Tên               File                     Nhập    Đề xuất  /thùng  Vị trí  Cấp
        new Row("noodle_spicy",    "Mỳ cay",          "Noodle1.glb",            8000,   12000,  8, Shelf,  1),
        new Row("noodle_blackbean","Mỳ tương đen",    "Noodle2.glb",            9000,   14000,  8, Shelf,  1),
        new Row("snack_shrimp",    "Snack tôm",       "Snack1.glb",             5000,    8000,  8, Shelf,  1),
        new Row("snack_potato",    "Snack khoai tây", "Snack2.glb",            10000,   15000,  8, Shelf,  1),
        new Row("orange_juice",    "Nước cam",        "OrangeJuice.glb",       15000,   22000,  8, Both,   1),
        new Row("milk",            "Sữa",             "Milk.glb",              25000,   35000,  8, Both,   1),
        new Row("vegetable",       "Rau",             "Vegetable.glb",         10000,   15000,  3, Fridge, 1),
        new Row("laundry_liquid",  "Nước giặt",       "LaundryWater.glb",      90000,  125000,  6, Shelf,  2),
        new Row("laundry_powder",  "Bột giặt",        "LaundryDetergent.glb", 120000,  165000,  4, Shelf,  2),
        new Row("mouthwash",       "Nước súc miệng",  "MouthWash.glb",         60000,   85000,  8, Shelf,  2),
        new Row("strawberry",      "Dâu tây",         "Strawberry.glb",        50000,   75000,  6, Fridge, 2),
        new Row("mango",           "Xoài",            "Mango.glb",             30000,   45000,  6, Fridge, 2),
        new Row("kimbap",          "Kimpab",          "Kimpab.glb",            25000,   38000,  3, Fridge, 3),
        new Row("sushi",           "Sushi",           "Sushi.glb",             60000,   90000,  3, Fridge, 3),
        new Row("pork",            "Thịt lợn",        "Pork.glb",              45000,   65000,  3, Fridge, 3),
        new Row("beef",            "Thịt bò",         "Beef.glb",              90000,  130000,  3, Fridge, 3),
    };

    [MenuItem("Tools/Shelf Stocking/Tạo dữ liệu sản phẩm")]
    public static void Generate()
    {
        EnsureFolder(ProductFolder);

        var db = AssetDatabase.LoadAssetAtPath<ProductDatabase>(DatabasePath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ProductDatabase>();
            AssetDatabase.CreateAsset(db, DatabasePath);
        }

        var dbSo = new SerializedObject(db);
        SerializedProperty list = dbSo.FindProperty("products");
        int created = 0, modelsLinked = 0, missingModels = 0;

        foreach (Row r in Rows)
        {
            string path = $"{ProductFolder}/{r.Id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ProductData>(path);
            bool isNew = data == null;
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<ProductData>();
                AssetDatabase.CreateAsset(data, path);
                created++;
            }

            var so = new SerializedObject(data);
            if (isNew)
            {
                so.FindProperty("id").stringValue = r.Id;
                so.FindProperty("displayName").stringValue = r.Name;
                so.FindProperty("purchasePrice").intValue = r.Buy;
                so.FindProperty("suggestedPrice").intValue = r.Sell;
                so.FindProperty("unitsPerBox").intValue = r.PerBox;
                so.FindProperty("placement").intValue = (int)r.Place;
                so.FindProperty("unlockLevel").intValue = r.Level;
            }

            SerializedProperty model = so.FindProperty("modelPrefab");
            if (model.objectReferenceValue == null)
            {
                GameObject found = FindModel(r.File);
                if (found != null) { model.objectReferenceValue = found; modelsLinked++; }
                else { missingModels++; Debug.LogWarning($"[T08] Không tìm thấy model '{r.File}' cho {r.Name}. Kiểm tra đã import .glb (cần glTFast).", data); }
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!ListContains(list, data))
            {
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = data;
            }
        }

        dbSo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Selection.activeObject = db;
        EditorGUIUtility.PingObject(db);

        Debug.Log($"[T08] Xong: tạo mới {created} mặt hàng, gán {modelsLinked} model, thiếu {missingModels} model. Database: {DatabasePath}", db);
    }

    private static GameObject FindModel(string fileName)
    {
        string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
        foreach (string guid in AssetDatabase.FindAssets(nameNoExt))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(p).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return AssetDatabase.LoadAssetAtPath<GameObject>(p);
        }
        return null;
    }

    private static bool ListContains(SerializedProperty list, UnityEngine.Object obj)
    {
        for (int i = 0; i < list.arraySize; i++)
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == obj) return true;
        return false;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif