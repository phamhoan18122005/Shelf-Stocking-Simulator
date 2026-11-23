
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : MonoBehaviour
{
    [Header("Tham Chieu")]
    [SerializeField] private Transform camNhin;
    [SerializeField] private Transform viTriCam;
    [SerializeField] private QuanLyNhiemVu quanLyNhiemVu;

    [Header("Cai Dat")]
    [SerializeField] private float khoangCachNhat = 3f;
    [SerializeField] private LayerMask lopTuongTac; // chon layer tuong tac
    [SerializeField] private LayerMask lopODat;
    [SerializeField] private LayerMask lopHang;

    private GameObject vatDangCam;
    private Collider ColliderVat;
    private Rigidbody rigidbodyVat;
    private HieuUngChon vienDangChon;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if(vatDangCam == null)
            {
                NhatVat();
            }
            else
            {
                ThaVat();
            }
        }
        //chuot trai tha vat len ke
        if(Mouse.current.leftButton.wasPressedThisFrame && vatDangCam != null)
        {
            DatLenKe();
        }

        if(Keyboard.current.fKey.wasPressedThisFrame && vatDangCam != null)
        {
            LayHangDatLenKe();
        }
    }

    private void NhatVat()
    {
        Ray tia = new Ray(camNhin.position, camNhin.forward);


        if (!Physics.Raycast(tia, out RaycastHit diemTrung, khoangCachNhat, lopTuongTac))
        {
            return;
        }
        GameObject vat = diemTrung.collider.attachedArticulationBody != null ? diemTrung.collider.attachedArticulationBody.gameObject : diemTrung.collider.gameObject;

        vatDangCam = vat;
        rigidbodyVat = vat.GetComponent<Rigidbody>();

        if (rigidbodyVat != null) rigidbodyVat.isKinematic = true;
        BatTatCollider(vat, false);

        // gan vat vao tay
        vatDangCam.transform.SetParent(viTriCam);
        vatDangCam.transform.localPosition = Vector3.zero;
        vatDangCam.transform.localRotation = Quaternion.identity;


            
    }

    private void ThaVat()
    {
        vatDangCam.transform.SetParent(null);

        Vector3 goc = vatDangCam.transform.eulerAngles;
        vatDangCam.transform.rotation = Quaternion.Euler(0f, goc.y, 0f);

        BatTatCollider(vatDangCam, true);
        if(rigidbodyVat != null)
        {
            rigidbodyVat.isKinematic = false;
        }
        QuenVat();
    }
    private void DatLenKe()
    {
        Ray tia = new Ray(camNhin.position, camNhin.forward);

        if(Physics.Raycast(tia, out RaycastHit diemTrung, khoangCachNhat, lopODat, QueryTriggerInteraction.Collide))
        {
            ODatHang oDat = diemTrung.collider.GetComponent<ODatHang>();
            if(oDat != null && oDat.DatVat(vatDangCam))
            {
                ColliderVat.enabled = true;
                if (quanLyNhiemVu != null) quanLyNhiemVu.TangTienDo();
                QuenVat();
            }
        }
    }

    private void QuenVat()
    {
        vatDangCam = null;
        ColliderVat = null;
        rigidbodyVat = null;
    }

    private void LayHangDatLenKe()
    {
        if (vatDangCam == null) { Debug.Log("F: chua cam thung"); return; }

        ODatHang oDat = TimOTrongGanNhat();
        if (oDat == null) { Debug.Log("F: khong co Slot trong nao gan huong nhin"); return; }

        Transform hang = TimHangTrongHop();
        if (hang == null) { Debug.Log("F: thung KHONG con hang layer HangHoa"); return; }

        if (oDat.DatVat(hang.gameObject))
        {
            BatTatCollider(hang.gameObject, true);
            if (quanLyNhiemVu != null) quanLyNhiemVu.TangTienDo();
            Debug.Log("F: dat thanh cong " + hang.name + " vao " + oDat.name);
        }
    }

    private ODatHang TimOTrongGanNhat()
    {
        Collider[] cac0 = Physics.OverlapSphere(camNhin.position, khoangCachNhat + 1f, lopODat, QueryTriggerInteraction.Collide);

        ODatHang tot = null;
        float gocTotNhat = 35f;

        foreach (Collider c in cac0)
        {
            ODatHang o = c.GetComponent<ODatHang>();
            if (o == null || o.DaDay) continue;

            float goc = Vector3.Angle(camNhin.forward, c.bounds.center - camNhin.position);
            if (goc < gocTotNhat)
            {
                gocTotNhat = goc;
                tot = o;
            }
        }
        return tot;
    }

    // tim mon hang dau tien con nam trong thung ( dua theo layer hanghoa)
    private Transform TimHangTrongHop()
    {
        foreach (Transform con in vatDangCam.GetComponentsInChildren<Transform>())
        {
            if (con.gameObject == vatDangCam) continue;
            if ((lopHang.value & (1 << con.gameObject.layer)) != 0)
                return con;
        }
        return null;
    }

    private void BatTatCollider(GameObject vat, bool bat)
    {
        if (vat == null) return;
        foreach (Collider c in vat.GetComponentsInChildren<Collider>(true))
            c.enabled = bat;
    }

    
}
