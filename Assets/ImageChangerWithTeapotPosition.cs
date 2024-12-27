using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageChangerWithTeapotPosition : MonoBehaviour
{
    public Material quadMaterial; // Quad üzerindeki materyal
    public List<Texture2D> images; // Quad'a koymak için görüntüler
    public GameObject teapotPrefab; // Çaydanlık prefab'ı
    public Transform teapotParent; // Çaydanlıkların yerleştirileceği parent
    public string coordinatesFilePath = "/Users/tuce/Unity/benimki/Assets/Data/output_coordinates.txt"; // Koordinat dosyası yolu

    private int currentImageIndex = 0; // Şu anki görüntü indeksi
    private Dictionary<string, Vector3> coordinatesMap; // İsim-koordinat eşleşmeleri
    private GameObject currentTeapot; // Mevcut çaydanlık

    void Start()
    {
        // Koordinatları yükle
        LoadCoordinates();

        // İlk görüntü ve teapot konumunu ayarla
        UpdateImageAndTeapot();
    }

    // Koordinat dosyasını yükle
    void LoadCoordinates()
    {
        coordinatesMap = new Dictionary<string, Vector3>();

        if (!File.Exists(coordinatesFilePath))
        {
            Debug.LogError($"Koordinat dosyası bulunamadı: {coordinatesFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(coordinatesFilePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length == 4) // İsim ve X, Y, Z koordinatları
            {
                string imageName = parts[0]; // Görüntü adı
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);
                coordinatesMap[imageName] = new Vector3(x, y, z);
            }
        }
    }

    // Görüntüyü ve çaydanlık pozisyonunu güncelle
    void UpdateImageAndTeapot()
    {
        if (images.Count == 0)
        {
            Debug.LogWarning("Görüntü listesi boş!");
            return;
        }

        // Görüntüyü güncelle
        Texture2D currentTexture = images[currentImageIndex];
        quadMaterial.mainTexture = currentTexture;


        // Görüntü adına göre koordinatları al
        string imageName = Path.GetFileNameWithoutExtension(currentTexture.name) + ".JPG"; // Görüntü adını .JPG olarak kullan
        if (coordinatesMap.TryGetValue(imageName, out Vector3 position))
        {
            // Eski çaydanlığı temizle
            if (currentTeapot != null)
            {
                Destroy(currentTeapot);
            }
            
            Quaternion teapotRotation = Quaternion.Euler(180, 0, 0); // Çaydanlık döndürme

            // Yeni çaydanlığı oluştur
            currentTeapot = Instantiate(teapotPrefab, position, teapotRotation, teapotParent);
        }
        else
        {
            Debug.LogWarning($"Koordinatlar bulunamadı: {imageName}");
        }
    }

    // Bir sonraki görüntüye geçiş
    public void NextImage()
    {
        currentImageIndex = (currentImageIndex + 1) % images.Count;
        UpdateImageAndTeapot();
    }

    // Bir önceki görüntüye geçiş
    public void PreviousImage()
    {
        currentImageIndex = (currentImageIndex - 1 + images.Count) % images.Count;
        UpdateImageAndTeapot();
    }
}