using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class QuanLyNhiemVu : MonoBehaviour
{
    [Header("Nhiem Vu")]
    [SerializeField] private int soLuongCan = 3;

    [Header("Giao Dien")]
    [SerializeField] private TMP_Text chuNhiemVu;

    private int soLuongHienTai;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TangTienDo()
    {
        soLuongHienTai++;
        CapNhatChu();
    }

    public void GiamTienDo()
    {
        soLuongHienTai = Mathf.Max(0, soLuongHienTai - 1);
        CapNhatChu();
    }
    private void CapNhatChu()
    {
        if(soLuongHienTai >= soLuongCan)
        {
            chuNhiemVu.text = "Hoan thanh!";
        }
        else
        {
            chuNhiemVu.text = "Xep hang: " + soLuongHienTai + "/" + soLuongCan;
        }
    }
}
