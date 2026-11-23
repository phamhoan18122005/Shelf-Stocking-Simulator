using UnityEngine;

public class ODatHang : MonoBehaviour
{
    [Header("Trạng Thái")]
    [SerializeField] private bool daDay;
    public bool DaDay => daDay;
    public bool DatVat(GameObject vat)
    {
        if (daDay) return false;

        vat.transform.SetParent(transform);
        vat.transform.localPosition = Vector3.zero;
        vat.transform.localRotation = Quaternion.identity;
        daDay = true;
        return true;
    }

    public void LamTrong()
    {
        daDay = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null) return;
        Gizmos.color = daDay ? Color.red : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(bc.center, bc.size);
    }
}
