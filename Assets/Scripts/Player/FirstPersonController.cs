using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Di Chuyển")]
    [SerializeField] private float tocDoDi = 4f;
    [SerializeField] private float tocDoChay = 7f;
    [SerializeField] private float trongLuc = -20f;


    [Header("Camera")]
    [SerializeField] private Transform cameraNhin;
    [SerializeField] private float doNhayChuot = 0.1f;
    [SerializeField] private float gocNhinToiDa = 85f;

    private CharacterController dieuKhien;
    private float vanTocDoi;
    private float gocCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dieuKhien = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        NhinQuanh();
        DiChuyen();
    }
    private void NhinQuanh()
    {
        Vector2 dichChuot = Mouse.current.delta.ReadValue() * doNhayChuot;
        transform.Rotate(Vector3.up * dichChuot.x);

        gocCamera = Mathf.Clamp(gocCamera - dichChuot.y, -gocNhinToiDa, gocNhinToiDa);
        cameraNhin.localRotation = Quaternion.Euler(gocCamera, 0f, 0f);
    }

    private void DiChuyen()
    {
        var banPhim = Keyboard.current;
        float ngang = (banPhim.dKey.isPressed ? 1 : 0) - (banPhim.aKey.isPressed ? 1 : 0);
        float doc = (banPhim.wKey.isPressed ? 1 : 0) - (banPhim.sKey.isPressed ? 1 : 0);

        float tocDo = banPhim.leftShiftKey.isPressed ? tocDoChay : tocDoDi;
        Vector3 huongDi = (transform.right * ngang + transform.forward * doc).normalized * tocDo;

        if(dieuKhien.isGrounded && vanTocDoi < 0f)
        {
            vanTocDoi = -2f;
        }
        vanTocDoi += trongLuc * Time.deltaTime;
        huongDi.y = vanTocDoi;
        dieuKhien.Move(huongDi * Time.deltaTime);

        if (banPhim.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
