using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elementleri")]
    public Transform menuButonPaneli; // Butonların içine doğacağı panel
    public GameObject butonPrefab;    // CustomPrefab klasöründeki butonun

    [Header("UI Pencereleri (Gelecek İçin)")]
    public GameObject creditsPenceresi; 

    // Menü butonlarının isimleri ve sıralaması
    private List<string> butonYazilari = new List<string> { "Tek Kişilik Oyun", "İki Kişilik Oyun", "Emeği Geçenler", "Oyundan Çık" };

    void Start()
    {
        MenuButonlariniOlustur();
    }

    void MenuButonlariniOlustur()
    {
        // Panel içini temizle
        foreach (Transform child in menuButonPaneli)
        {
            Destroy(child.gameObject);
        }

        // Listemizdeki her bir başlık için dinamik buton üretiyoruz
        for (int i = 0; i < butonYazilari.Count; i++)
        {
            GameObject yeniButon = Instantiate(butonPrefab, menuButonPaneli);
            yeniButon.name = "MenuButon_" + i;

            // Butonun üzerindeki yazıyı ayarla
            yeniButon.GetComponentInChildren<TextMeshProUGUI>().text = butonYazilari[i];

            // --- SİHİRLİ CİLA DOKUNUŞU ---
            // Tek oyunculudan gelen siluet resmini ana menü butonlarında gizliyoruz
            Transform siluetGorseli = yeniButon.transform.Find("SiluetGorseli");
            if (siluetGorseli != null)
            {
                siluetGorseli.gameObject.SetActive(false); 
            }
            // -----------------------------

            // Tıklama olaylarını endekslerine göre koda bağlıyoruz
            int butonIndex = i; 
            yeniButon.GetComponent<Button>().onClick.AddListener(() => MenuButonunaBasildi(butonIndex));
        }
    }

    void MenuButonunaBasildi(int index)
    {
        switch (index)
        {
            case 0: // Tek Kişilik Oyun
                TekKisilikOyna();
                break;
            case 1: // İki Kişilik Oyun
                IkiKisilikOyna();
                break;
            case 2: // Emeği Geçenler
                CreditsAc();
                break;
            case 3: // Oyundan Çık
                OyundanCik();
                break;
        }
    }

    public void TekKisilikOyna()
    {
        // İlk yaptığımız oyun sahnesinin adı neyse onu yazmalısın.
        // Eğer ismi değiştirmediyse varsayılan "SampleScene"dir.
        SceneManager.LoadScene("TekOyunculu"); 
    }

    public void IkiKisilikOyna()
    {
        SceneManager.LoadScene("IkiOyunculu");
    }

    public void CreditsAc()
    {
        Debug.Log("Emeği Geçenler ekranı tetiklendi.");
        if (creditsPenceresi != null) creditsPenceresi.SetActive(true);
    }

    public void CreditsKapat()
    {
        if (creditsPenceresi != null) creditsPenceresi.SetActive(false);
    }

    public void OyundanCik()
    {
        Debug.Log("Oyundan çıkılıyor...");
        Application.Quit();
    }
}