using UnityEngine;

public class HieuUngChon : MonoBehaviour
{
    [SerializeField] private Color mauSang = Color.green;
    [SerializeField] private float doSang = 0.6f;

    private Renderer[] cacsRenderer;

    private void Awake()
    {
        cacsRenderer = GetComponentsInChildren<Renderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Bat(bool bat)
    {
        foreach(Renderer r in cacsRenderer)
        {
            foreach (Material m in r.materials)
            {
                if (bat)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", mauSang * doSang);
                }
                else
                {
                    m.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}
