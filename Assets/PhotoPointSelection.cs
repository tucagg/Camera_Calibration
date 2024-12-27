using UnityEngine;

public class PhotoPointSelection : MonoBehaviour
{
    public GameObject photoQuad; // Fotoğraf objes,

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol fare tıklandığında
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == photoQuad)
            {
                // Tıklanan noktanın dünya koordinatları
                Vector3 worldPoint = hit.point;

                Debug.Log($"Tıklanan nokta: {worldPoint}");
            }
        }
    }
}