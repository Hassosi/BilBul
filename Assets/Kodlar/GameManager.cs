using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Elementleri")]
    public Transform butonPaneli; 
    public GameObject butonPrefab; 
    public TextMeshProUGUI soruMetni; 
    public Image haritaGostericiImage; 

    [Header("Metin Tabanlı Can Sistemi (Geri Getirildi)")]
    public TextMeshProUGUI canYazisi; // Editördeki eski can yazısı nesneni bağla

    [Header("Metin Tabanlı Skor Sistemi (Geri Getirildi)")]
    public TextMeshProUGUI skorYazisi; // Editördeki eski skor yazısı nesneni bağla

    [Header("Görsel Ayarlar (Renkler)")]
    public Color dogruRenk = Color.green;
    public Color yanlisRenk = Color.red;

    [Header("Şehir Listesi Yükleyici")]
    [TextArea(10, 15)] 
    public string sehirleriBurayaYapistir;

    [Header("Senin Hazırladığın Paneller ve Metinler")]
    public GameObject durdurmaPenceresi;     // Editörden DurdurmaPaneli'ni sürükle
    public GameObject oyunBittiPenceresi;    // Editörden OyunBittiPaneli'ni sürükle
    public TextMeshProUGUI oyunBittiDurumMetni; // Oyun bitti panelindeki yazı nesnesi

    private List<string> tumSehirIsimleri = new List<string>();
    private int toplamSoruSayisi = 12; 
    private int maksimumYanlisHakki = 3;
    private int mevcutSoruNumarasi = 0;
    private int yapilanYanlisSayisi = 0;
    private int dogruCevapSayisi = 0;
    private bool cevapVerilebilirMi = true; 

    private Vector2[] anchorMinler = { new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(0.5f, 0f) };
    private Vector2[] anchorMaxlar = { new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f) };
    private List<GameObject> olusturulanButonlar = new List<GameObject>();
    private string mevcutDogruSehirAdi;
    private int mevcutSoruTipi = 1; 

    void Awake()
    {
        Time.timeScale = 1f; // Zamanı sıfırla
        SehirListesiniYukle();
    }

    void Start()
    {
        if (tumSehirIsimleri.Count < 4) return;
        CanYazisiniGuncelle();
        SkorYazisiniGuncelle();
        SoruHazirla();
    }

    void SehirListesiniYukle()
    {
        if (string.IsNullOrEmpty(sehirleriBurayaYapistir)) return;
        string[] satirlar = sehirleriBurayaYapistir.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string satir in satirlar)
        {
            string temizSatir = satir.Trim();
            if (!string.IsNullOrEmpty(temizSatir)) tumSehirIsimleri.Add(temizSatir);
        }
    }

    public void SoruHazirla()
    {
        if (mevcutSoruNumarasi >= toplamSoruSayisi || yapilanYanlisSayisi >= maksimumYanlisHakki)
        {
            OyunBitti();
            return;
        }

        mevcutSoruNumarasi++;
        cevapVerilebilirMi = true; 

        int modHesabi = ((mevcutSoruNumarasi - 1) / 3) % 3;
        mevcutSoruTipi = modHesabi + 1;
        mevcutDogruSehirAdi = tumSehirIsimleri[Random.Range(0, tumSehirIsimleri.Count)];

        string ingilizceIsim = TurkceKarakterleriTemizle(mevcutDogruSehirAdi);
        Sprite genelHarita = Resources.Load<Sprite>("GenelHaritalar/" + ingilizceIsim);
        Sprite kirpilmisHarita = Resources.Load<Sprite>("KirpilmisHaritalar/" + ingilizceIsim + "Kirp");

        switch (mevcutSoruTipi)
        {
            case 1:
                soruMetni.text = $"Soru {mevcutSoruNumarasi}: Haritada kırmızıyla boyanmış ilimiz hangisidir?";
                haritaGostericiImage.sprite = genelHarita; haritaGostericiImage.color = Color.white;
                break;
            case 2:
                soruMetni.text = $"Soru {mevcutSoruNumarasi}: Aşağıdaki seçeneklerden hangisi \"{mevcutDogruSehirAdi}\" ilinin silüetidir?";
                haritaGostericiImage.color = Color.clear; 
                break;
            case 3:
                soruMetni.text = $"Soru {mevcutSoruNumarasi}: Yakınlaştırılmış silüeti verilen ilimiz hangisidir?";
                haritaGostericiImage.sprite = kirpilmisHarita; haritaGostericiImage.color = Color.white;
                break;
        }

        List<string> secenekler = new List<string> { mevcutDogruSehirAdi };
        while (secenekler.Count < 4)
        {
            string yanlisSehir = tumSehirIsimleri[Random.Range(0, tumSehirIsimleri.Count)];
            if (!secenekler.Contains(yanlisSehir)) secenekler.Add(yanlisSehir);
        }

        for (int i = 0; i < secenekler.Count; i++)
        {
            string temp = secenekler[i];
            int r = Random.Range(i, secenekler.Count);
            secenekler[i] = secenekler[r]; secenekler[r] = temp;
        }

        ButonlariDinamikOlustur(secenekler);
    }

    void ButonlariDinamikOlustur(List<string> gelenSecenekler)
    {
        foreach (GameObject btn in olusturulanButonlar) Destroy(btn);
        olusturulanButonlar.Clear();

        for (int i = 0; i < 4; i++)
        {
            GameObject yeniButon = Instantiate(butonPrefab, butonPaneli);
            olusturulanButonlar.Add(yeniButon);

            RectTransform rTransform = yeniButon.GetComponent<RectTransform>();
            rTransform.anchorMin = anchorMinler[i]; rTransform.anchorMax = anchorMaxlar[i];
            rTransform.offsetMin = new Vector2(15, 15); rTransform.offsetMax = new Vector2(-15, -15);

            TextMeshProUGUI btnYazi = yeniButon.GetComponentInChildren<TextMeshProUGUI>();
            Image btnGorsel = yeniButon.transform.Find("SiluetGorseli").GetComponent<Image>();
            string butonunSehri = gelenSecenekler[i];

            if (mevcutSoruTipi == 1 || mevcutSoruTipi == 3)
            {
                btnYazi.text = butonunSehri; btnGorsel.color = Color.clear;
            }
            else if (mevcutSoruTipi == 2)
            {
                btnYazi.text = ""; 
                string ingilizceButonSehri = TurkceKarakterleriTemizle(butonunSehri);
                btnGorsel.sprite = Resources.Load<Sprite>("KirpilmisHaritalar/" + ingilizceButonSehri + "Kirp");
                btnGorsel.color = Color.white;
            }

            yeniButon.GetComponent<Button>().onClick.AddListener(() => CevapVerildi(butonunSehri, yeniButon));
        }
    }

    void CevapVerildi(string secilenSehirAdi, GameObject basilanButon)
    {
        if (!cevapVerilebilirMi) return;
        cevapVerilebilirMi = false; 

        if (secilenSehirAdi == mevcutDogruSehirAdi)
        {
            dogruCevapSayisi++;
            SkorYazisiniGuncelle(); // Skoru metin olarak güncelle
            basilanButon.GetComponent<Image>().color = dogruRenk;
            Invoke("SoruHazirla", 1.2f);
        }
        else
        {
            yapilanYanlisSayisi++;
            CanYazisiniGuncelle(); 
            basilanButon.GetComponent<Image>().color = yanlisRenk;

            foreach (GameObject btn in olusturulanButonlar)
            {
                TextMeshProUGUI btnYazi = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (mevcutSoruTipi == 2)
                {
                    Image btnGorsel = btn.transform.Find("SiluetGorseli").GetComponent<Image>();
                    string ingilizceDogruSehir = TurkceKarakterleriTemizle(mevcutDogruSehirAdi);
                    if(btnGorsel.sprite == Resources.Load<Sprite>("KirpilmisHaritalar/" + ingilizceDogruSehir + "Kirp")) btn.GetComponent<Image>().color = dogruRenk;
                }
                else
                {
                    if (btnYazi.text == mevcutDogruSehirAdi) btn.GetComponent<Image>().color = dogruRenk;
                }
            }
            Invoke("SoruHazirla", 1.2f);
        }
    }

    void CanYazisiniGuncelle()
    {
        if (canYazisi == null) return;

        string canMetni = "";
        for (int i = 0; i < maksimumYanlisHakki; i++)
        {
            if (i < yapilanYanlisSayisi)
                canMetni += "<color=#FF5555>X</color> ";
            else
                canMetni += "<color=#55FF55>O</color> ";
        }
        canYazisi.text = "CAN: " + canMetni.Trim();
    }

    void SkorYazisiniGuncelle()
    {
        if (skorYazisi == null) return;
        skorYazisi.text = "SKOR: " + dogruCevapSayisi + " / " + toplamSoruSayisi;
    }

    // --- SENİN HAZIRLADIĞIN PANELDEN TETİKLENECEK FONKSİYONLAR ---
    public void OyunuDurdur()
    {
        Time.timeScale = 0f; 
        if (durdurmaPenceresi != null) durdurmaPenceresi.SetActive(true);
    }

    public void OyunuDevamEttir()
    {
        Time.timeScale = 1f; 
        if (durdurmaPenceresi != null) durdurmaPenceresi.SetActive(false);
    }

    public void AnaMenuyeDon()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("AnaMenu");
    }

    public void TekrarOyna()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OyunBitti()
    {
        cevapVerilebilirMi = false;
        if(oyunBittiPenceresi != null) oyunBittiPenceresi.SetActive(true);
        
        if (yapilanYanlisSayisi >= maksimumYanlisHakki)
            oyunBittiDurumMetni.text = "KAYBETTİN!\nSkorun: " + dogruCevapSayisi + " / " + toplamSoruSayisi;
        else
            oyunBittiDurumMetni.text = "KAZANDIN! TEBRİKLER!\nSkorun: " + dogruCevapSayisi + " / " + toplamSoruSayisi;
    }

    string TurkceKarakterleriTemizle(string text)
    {
        return text.Replace("İ", "I").Replace("ı", "i").Replace("Ğ", "G").Replace("ğ", "g").Replace("Ü", "U").Replace("ü", "u").Replace("Ş", "S").Replace("ş", "s").Replace("Ö", "O").Replace("ö", "o").Replace("Ç", "C").Replace("ç", "c").Replace("â", "a");
    }
}