using UnityEngine;
using TMPro;

public class D : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifetime = 1f;
    private TextMeshPro textMesh;
    

    void Awake()
    {
        // Get the TextMeshPro component early in the lifecycle
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            // Configure default text properties
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableWordWrapping = false;
            textMesh.fontSize = 4f; // Adjust this value as needed
            textMesh.color = Color.red; // Set default color
        }
        else
        {
            Debug.LogError("TextMeshPro component not found on DamageText prefab!", gameObject);
        }
    }

    void Start()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Make text always face the camera
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                           Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void SetText(string text)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        if (textMesh != null)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "0";
            }
            textMesh.text = text;
            textMesh.ForceMeshUpdate();
            Debug.Log($"DamageText set successfully to: {text}", gameObject);
        }
        else
        {
            Debug.LogError("TextMeshPro component is still null in SetText!", gameObject);
        }
    }
}