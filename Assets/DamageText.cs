using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifetime = 1f;
    private TextMeshPro textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component is missing on DamageText prefab.");
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Ensure the text always faces the camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Flip to face the camera correctly
        }
    }

    public void SetText(string text)
    {
        if (textMesh)
        {
            textMesh.text = text;
        }
        else
        {
            Debug.LogWarning("TextMeshPro component is not assigned.");
        }
    }
}
