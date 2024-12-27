import numpy as np
from scipy.spatial.transform import Rotation as R

def parse_cameras(file_path):
    """
    Parse the camera information from the .nvm file.
    """
    cameras = []
    with open(file_path, 'r') as f:
        lines = f.readlines()
        for line in lines:
            parts = line.strip().split()
            if len(parts) < 10:
                continue
            camera = {
                "name": parts[0],
                "focal_length": float(parts[1]),
                "rotation": R.from_quat([float(parts[2]), float(parts[3]), float(parts[4]), float(parts[5])]),
                "position": np.array([float(parts[6]), float(parts[7]), float(parts[8])])
            }
            cameras.append(camera)
    return cameras

def transform_point(reference_point, reference_camera, target_camera):
    """
    Transform a world point from the reference camera's coordinate system to the target camera's system.
    """
    # Referans kameradan dünya koordinatlarına dönüştür
    world_point = reference_camera["rotation"].apply(reference_point) + reference_camera["position"]
    # Dünya koordinatlarından hedef kameraya dönüştür
    relative_point = target_camera["rotation"].inv().apply(world_point - target_camera["position"])

    #z değeri 0.0 olmalı
    relative_point[2] = 0.0
    return relative_point

def main():
    file_path = "/Users/tuce/Unity/benimki/Assets/Data/dense_recon.nvm"  # NVM dosyasının yolu
    output_file = "output_coordinates.txt"
    reference_camera_name = "IMG_5316.JPG"
    reference_point = np.array([0.87, -2.07, 0.00])  # Örnek dünya noktası referans kamera için

    # Kameraları ayrıştır
    cameras = parse_cameras(file_path)
    reference_camera = next((camera for camera in cameras if camera["name"] == reference_camera_name), None)
    if not reference_camera:
        print(f"{reference_camera_name} not found in {file_path}")
        return

    # Tüm kameralar için projeksiyon hesapla
    results = []
    for camera in cameras:
        if camera["name"] == reference_camera_name:
            # Referans kamera için dünya noktası doğrudan kullanılır
            results.append(f"{camera['name']} {reference_point[0]} {reference_point[1]} {reference_point[2]}")
        else:
            # Diğer kameralar için koordinatları hesapla
            transformed_point = transform_point(reference_point, reference_camera, camera)
            results.append(f"{camera['name']} {transformed_point[0]:.3f} {transformed_point[1]:.3f} {transformed_point[2]:.3f}")

    # Sonuçları kaydet
    with open(output_file, "w") as f:
        f.write("\n".join(results))

    print(f"Transformed points saved to {output_file}")

if __name__ == "__main__":
    main()
