using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class LocalDataManager : MonoBehaviour
{
    private string localFilePath;

    private void Awake()
    {
        // Kaydettiğiniz dosyanın ismi ve konumu
        localFilePath = Path.Combine(Application.persistentDataPath, "levels.json");
    }

    // JSON dosyasını okuyup LevelData listesi olarak döndüren fonksiyon
    public List<LevelData> LoadLocalLevelData()
    {
        // Dosya var mı kontrol edelim
        if (!File.Exists(localFilePath))
        {
            Debug.LogWarning("Local JSON dosyası bulunamadı: " + localFilePath);
            // Dosya yoksa boş bir liste döndürelim
            return new List<LevelData>();
        }

        // Dosyadan tüm içeriği metin olarak oku
        string jsonData = File.ReadAllText(localFilePath);

        // JSON'u, daha önce tanımladığımız LevelDataListWrapper formatında deserialize (parse) et
        LevelDataListWrapper wrapper = JsonUtility.FromJson<LevelDataListWrapper>(jsonData);

        // Eğer wrapper veya items boş ise, yine boş bir liste döndürelim
        if (wrapper == null || wrapper.items == null)
        {
            Debug.LogWarning("JSON verisi parse edilemedi veya items boş geldi.");
            return new List<LevelData>();
        }

        // Artık elimizde LevelData tipinde bir liste var.
        Debug.Log("Local JSON verisi başarıyla okundu. Toplam seviye sayısı: " + wrapper.items.Count);
        return wrapper.items;
    }
}
