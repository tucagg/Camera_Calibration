using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class PhotoProjectionNVM : MonoBehaviour
{
    public string nvmFilePath = "Assets/Data/dense_recons.nvm"; // NVM dosyasının yolu
    public GameObject photoQuadPrefab; // Fotoğraf Quad prefab'ı
    public GameObject teapotPrefab; // Çaydanlık prefab'ı
    public Vector2 imageSize = new Vector2(1920, 1080); // Fotoğraf çözünürlüğü (örnek)

    private List<CameraData> cameras = new List<CameraData>(); // Kameraların bilgilerini saklar

    // Kamera bilgilerini temsil eden sınıf
    public class CameraData
    {
        public string name; // Fotoğraf adı
        public float focalLength; // Odak uzaklığı
        public Quaternion rotation; // Kamera dönüşü (quaternion)
        public Vector3 position; // Kamera pozisyonu
    }

    void Start()
    {
        // NVM dosyasını oku ve kamera bilgilerini al
        cameras = ParseNVM(nvmFilePath);
        Debug.Log($"Toplam Kamera Sayısı: {cameras.Count}");

        // Örnek bir dünya noktası (dünya koordinatları)
        Vector3 exampleWorldPoint = new Vector3(0.0f, 0.0f, 2.0f); // Düzenlenebilir
        ProjectPointToAllPhotos(exampleWorldPoint);
    }

    // NVM dosyasını parse eden fonksiyon
    List<CameraData> ParseNVM(string path)
    {
        var cameras = new List<CameraData>();

        // Dosyayı satır satır oku
        string[] lines = File.ReadAllLines(path);
        int numberOfCameras = int.Parse(lines[2].Trim()); // 3. satırdaki kamera sayısını oku

        // Kameraların bilgilerini al
        for (int i = 3; i < 3 + numberOfCameras; i++)
        {
            string[] parts = lines[i].Split('\t'); // Tab ile ayır

            if (parts.Length >= 9)
            {
                // Kamera adı
                string name = parts[0];

                // Odak uzaklığı
                float focalLength = float.Parse(parts[1], CultureInfo.InvariantCulture);

                // Quaternion dönüş bilgisi
                Quaternion rotation = new Quaternion(
                    float.Parse(parts[3], CultureInfo.InvariantCulture),
                    float.Parse(parts[4], CultureInfo.InvariantCulture),
                    float.Parse(parts[5], CultureInfo.InvariantCulture),
                    float.Parse(parts[2], CultureInfo.InvariantCulture)
                );

                // Pozisyon bilgileri
                Vector3 position = new Vector3(
                    float.Parse(parts[6], CultureInfo.InvariantCulture),
                    float.Parse(parts[7], CultureInfo.InvariantCulture),
                    float.Parse(parts[8], CultureInfo.InvariantCulture)
                );

                cameras.Add(new CameraData { name = name, focalLength = focalLength, rotation = rotation, position = position });
            }
            else
            {
                Debug.LogWarning($"Hatalı kamera formatı: {lines[i]}");
            }
        }

        return cameras;
    }

    // Dünya koordinatını tüm fotoğraflara projekte eden fonksiyon
    void ProjectPointToAllPhotos(Vector3 worldPoint)
    {
        foreach (CameraData camera in cameras)
        {
            // Dünya noktasını UV koordinatlarına projekte et
            Vector2 uv = ProjectWorldPointToUV(worldPoint, camera);

            // UV'nin geçerli olup olmadığını kontrol et
            if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            {
                Debug.LogWarning($"Fotoğraf {camera.name}: UV koordinatları geçerli değil ({uv})");
                continue;
            }

            // UV'yi Quad üzerindeki yerel pozisyona dönüştür
            Vector3 localPoint = new Vector3(uv.x - 0.5f, uv.y - 0.5f, 0); // Yerel pozisyon
            GameObject quad = Instantiate(photoQuadPrefab); // Fotoğraf Quad'ını oluştur
            quad.transform.position = camera.position; // Kameranın pozisyonuna yerleştir
            quad.transform.rotation = camera.rotation; // Kameranın dönüşüne göre döndür

            Vector3 worldPointOnQuad = quad.transform.TransformPoint(localPoint); // Dünya koordinatı
            Instantiate(teapotPrefab, worldPointOnQuad, Quaternion.identity); // Çaydanlığı yerleştir

            Debug.Log($"Fotoğraf {camera.name}: UV = {uv}, Dünya Pozisyonu = {worldPointOnQuad}");
        }
    }

    // Dünya koordinatını UV'ye projekte eden fonksiyon
    Vector2 ProjectWorldPointToUV(Vector3 worldPoint, CameraData camera)
    {
        // Kamera dönüşüm matrisini oluştur
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(camera.rotation);
        Vector3 relativePoint = rotationMatrix.MultiplyPoint(worldPoint - camera.position);

        // Perspektif projeksiyon
        float u = (camera.focalLength * relativePoint.x) / relativePoint.z;
        float v = (camera.focalLength * relativePoint.y) / relativePoint.z;

        // UV koordinatlarına çevir
        float uvX = (u / imageSize.x + 0.5f);
        float uvY = (0.5f - v / imageSize.y);
        return new Vector2(uvX, uvY);
    }
}