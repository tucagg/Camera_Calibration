using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class NVMParser : MonoBehaviour
{
    public string nvmFilePath = "Assets/Data/dense_recon.nvm";

    public class CameraData
    {
        public string name;
        public float focalLength;
        public Quaternion rotation;
        public Vector3 position;
    }

    public List<CameraData> ParseNVM()
    {
        var cameras = new List<CameraData>();

        // Dosyayı satır satır oku
        string[] lines = File.ReadAllLines(nvmFilePath);

        if (lines.Length < 4)
        {
            Debug.LogError("NVM dosyası beklenenden az satır içeriyor!");
            return cameras;
        }

        // Üçüncü satırdaki kamera sayısını al
        if (!int.TryParse(lines[2].Trim(), out int cameraCount))
        {
            Debug.LogError("Kamera sayısı okunamadı!");
            return cameras;
        }

        Debug.Log($"NVM dosyasındaki kamera sayısı: {cameraCount}");

        // Kamera bilgisi satırlarını işleme
        for (int i = 3; i < 3 + cameraCount; i++) // Kamera bilgilerini işle
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line)) continue; // Boş satırları atla

            string[] parts = line.Split(new[] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 9) // En az 9 parça olmalı
            {
                try
                {
                    // Kamera adı
                    string name = parts[0];

                    // Odak uzaklığı
                    float focalLength = float.Parse(parts[1], CultureInfo.InvariantCulture);

                    // Quaternion (dönüş bilgisi)
                    Quaternion rotation = new Quaternion(
                        float.Parse(parts[3], CultureInfo.InvariantCulture), // x
                        float.Parse(parts[4], CultureInfo.InvariantCulture), // y
                        float.Parse(parts[5], CultureInfo.InvariantCulture), // z
                        float.Parse(parts[2], CultureInfo.InvariantCulture)  // w
                    );

                    // Pozisyon bilgisi (x, y, z)
                    Vector3 position = new Vector3(
                        float.Parse(parts[6], CultureInfo.InvariantCulture),
                        float.Parse(parts[7], CultureInfo.InvariantCulture),
                        float.Parse(parts[8], CultureInfo.InvariantCulture)
                    );

                    // Kamera bilgilerini listeye ekle
                    cameras.Add(new CameraData { name = name, focalLength = focalLength, rotation = rotation, position = position });
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Hatalı kamera bilgisi: {line} - Hata: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Hatalı format: {line} - Parça sayısı: {parts.Length}");
            }
        }

        return cameras;
    }

    void Start()
    {
        var cameras = ParseNVM();

        if (cameras.Count == 0)
        {
            Debug.LogError("Hiç kamera bilgisi yüklenemedi!");
            return;
        }

        Debug.Log($"Toplam Kamera Sayısı: {cameras.Count}");
        foreach (var camera in cameras)
        {
            Debug.Log($"Kamera: {camera.name}, Pozisyon: {camera.position}, Dönüş: {camera.rotation}");
        }
    }
}