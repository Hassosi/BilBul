using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Oyuncu 1 UI (Sol)")]
    public Transform p1ButonPaneli;
    public Image p1HaritaGosterici;
    public TextMeshProUGUI p1SkorYazisi; // Sol oyuncunun eski TMP skor yazısını bağla

    [Header("Oyuncu 2 UI (Sağ)")]
    public Transform p2ButonPaneli;
    public Image p2HaritaGosterici;
    public TextMeshProUGUI p2SkorYazisi; // Sağ oyuncunun eski TMP skor yazısını bağla

    [Header("Ortak UI")]
    public TextMeshProUGUI merkezBilgiMetni; // "Soru 1 / 12" veya "Sıra Sağda" yazan yer
    public GameObject butonPrefab;

    [Header("Görsel Ayarlar")]
    public Color dogruRenk = Color.green;
    public Color yanlisRenk = Color.red;

    [Header("Şehir Listesi")]
    [TextArea(10, 15)] 
    public string sehirleriBurayaYapistir;

    [Header("Senin Hazırladığın Paneller ve Metinler")]
    public GameObject durdurmaPenceresi;     // Editörden DurdurmaPaneli'ni sürükle
    public GameObject oyunBittiPenceresi;    // Editörden OyunBittiPaneli'ni sürükle
    public TextMeshProUGUI oyunBittiDurumMetni; // Oyun bitti panelindeki yazı nesnesi

    private List<string> tumSehirIsimleri = new List<string>();
    private int p1Skor = 0; 
    private int p2Skor = 0;
    private int mevcutSoruNumarasi = 0; 
    private int toplamSoruSayisi = 12;
    private bool p1CevapVerebilirMi = true; 
    private bool p2CevapVerebilirMi = true;

    private Vector2[] anchorMinler = { new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(0.5f, 0f) };
    private Vector2[] anchorMaxlar = { new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f) };
    private List<GameObject> p1OlusturulanButonlar = new List<GameObject>();
    private List<GameObject> p2OlusturulanButonlar = new List<GameObject>();
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
        SkorYazilariniGuncelle();
        SoruHazirla(); 
    }

    void SehirListesiniYukle()
    {
        if (string.IsNullOrEmpty(sehirleriBurayaYapistir)) return;
        string[] satirlar = sehirleriBurayaYapistir.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string satir in satirlar) { string t = satir.Trim(); if (!string.IsNullOrEmpty(t)) tumSehirIsimleri.Add(t); }
    }

    void SoruHazirla()
{
    if (mevcutSoruNumarasi >= toplamSoruSayisi) { OyunBitti(); return; }

    mevcutSoruNumarasi++;
    p1CevapVerebilirMi = true; p2CevapVerebilirMi = true;

    int modHesabi = ((mevcutSoruNumarasi - 1) / 3) % 3;
    mevcutSoruTipi = modHesabi + 1;
    mevcutDogruSehirAdi = tumSehirIsimleri[Random.Range(0, tumSehirIsimleri.Count)];

    string ingilizceIsim = TurkceKarakterleriTemizle(mevcutDogruSehirAdi);
    Sprite genelHarita = Resources.Load<Sprite>("GenelHaritalar/" + ingilizceIsim);
    Sprite kirpilmisHarita = Resources.Load<Sprite>("KirpilmisHaritalar/" + ingilizceIsim + "Kirp");

    // --- SORU METİNLERİ ---
    switch (mevcutSoruTipi)
    {
        case 1:
            merkezBilgiMetni.text = $"Soru {mevcutSoruNumarasi}: Haritadaki il hangisidir?";
            p1HaritaGosterici.sprite = genelHarita; p1HaritaGosterici.color = Color.white;
            p2HaritaGosterici.sprite = genelHarita; p2HaritaGosterici.color = Color.white;
            break;
        case 2:
            merkezBilgiMetni.text = $"Soru {mevcutSoruNumarasi}: Hangisi \"{mevcutDogruSehirAdi}\" ilinin silüetidir?";
            p1HaritaGosterici.color = Color.clear; p2HaritaGosterici.color = Color.clear; 
            break;
        case 3:
            merkezBilgiMetni.text = $"Soru {mevcutSoruNumarasi}: Yakınlaştırılmış silüet hangi ile aittir?";
            p1HaritaGosterici.sprite = kirpilmisHarita; p1HaritaGosterici.color = Color.white;
            p2HaritaGosterici.sprite = kirpilmisHarita; p2HaritaGosterici.color = Color.white;
            break;
    }

    List<string> secenekler = new List<string> { mevcutDogruSehirAdi };
    while (secenekler.Count < 4) { string y = tumSehirIsimleri[Random.Range(0, tumSehirIsimleri.Count)]; if (!secenekler.Contains(y)) secenekler.Add(y); }

    for (int i = 0; i < secenekler.Count; i++) { string temp = secenekler[i]; int r = Random.Range(i, secenekler.Count); secenekler[i] = secenekler[r]; secenekler[r] = temp; }
    ButonlariDinamikOlustur(secenekler);
}

    void ButonlariDinamikOlustur(List<string> gelenSecenekler)
    {
        CleanButtons(p1OlusturulanButonlar); CleanButtons(p2OlusturulanButonlar);
        for (int i = 0; i < 4; i++)
        {
            string butonunSehri = gelenSecenekler[i];
            GameObject btn1 = Instantiate(butonPrefab, p1ButonPaneli); p1OlusturulanButonlar.Add(btn1);
            SetupButtonRect(btn1, i); ConfigureButtonContent(btn1, butonunSehri);
            btn1.GetComponent<Button>().onClick.AddListener(() => CevapVerildi(1, butonunSehri, btn1));

            GameObject btn2 = Instantiate(butonPrefab, p2ButonPaneli); p2OlusturulanButonlar.Add(btn2);
            SetupButtonRect(btn2, i); ConfigureButtonContent(btn2, butonunSehri);
            btn2.GetComponent<Button>().onClick.AddListener(() => CevapVerildi(2, butonunSehri, btn2));
        }
    }

    void ConfigureButtonContent(GameObject btn, string sehirAdi)
    {
        TextMeshProUGUI btnYazi = btn.GetComponentInChildren<TextMeshProUGUI>();
        Image btnGorsel = btn.transform.Find("SiluetGorseli").GetComponent<Image>();
        btnGorsel.gameObject.SetActive(true); 

        if (mevcutSoruTipi == 1 || mevcutSoruTipi == 3) { btnYazi.text = sehirAdi; btnGorsel.color = Color.clear; }
        else if (mevcutSoruTipi == 2)
        {
            btnYazi.text = ""; 
            string ing = TurkceKarakterleriTemizle(sehirAdi);
            btnGorsel.sprite = Resources.Load<Sprite>("KirpilmisHaritalar/" + ing + "Kirp"); btnGorsel.color = Color.white;
        }
    }

    void SetupButtonRect(GameObject btn, int index)
    {
        RectTransform rTransform = btn.GetComponent<RectTransform>();
        rTransform.anchorMin = anchorMinler[index]; rTransform.anchorMax = anchorMaxlar[index];
        rTransform.offsetMin = new Vector2(10, 10); rTransform.offsetMax = new Vector2(-10, -10);
    }

    void CleanButtons(List<GameObject> list) { foreach (GameObject btn in list) Destroy(btn); list.Clear(); }

    void CevapVerildi(int oyuncuNo, string secilenSehir, GameObject basilanButon)
    {
        if (oyuncuNo == 1 && !p1CevapVerebilirMi) return;
        if (oyuncuNo == 2 && !p2CevapVerebilirMi) return;

        if (secilenSehir == mevcutDogruSehirAdi)
        {
            p1CevapVerebilirMi = false; p2CevapVerebilirMi = false;
            basilanButon.GetComponent<Image>().color = dogruRenk;
            RevealCorrectAnswer();

            if (oyuncuNo == 1) { p1Skor++; merkezBilgiMetni.text = "SOL OYUNCU BİLDİ!"; }
            else { p2Skor++; merkezBilgiMetni.text = "SAĞ OYUNCU BİLDİ!"; }

            SkorYazilariniGuncelle();
            Invoke("SoruHazirla", 1.5f);
        }
        else
        {
            basilanButon.GetComponent<Image>().color = yanlisRenk;
            if (oyuncuNo == 1) { p1CevapVerebilirMi = false; merkezBilgiMetni.text = "Sol Yanlış Bildi! Sıra Sağda..."; }
            else { p2CevapVerebilirMi = false; merkezBilgiMetni.text = "Sağ Yanlış Bildi! Sıra Solda..."; }

            if (!p1CevapVerebilirMi && !p2CevapVerebilirMi) { merkezBilgiMetni.text = "İki Oyuncu da Bilemedi!"; RevealCorrectAnswer(); Invoke("SoruHazirla", 1.5f); }
        }
    }

    void RevealCorrectAnswer() { HighlightCorrectInList(p1OlusturulanButonlar); HighlightCorrectInList(p2OlusturulanButonlar); }
    void HighlightCorrectInList(List<GameObject> list)
    {
        foreach (GameObject btn in list)
        {
            TextMeshProUGUI btnYazi = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (mevcutSoruTipi == 2)
            {
                Image btnGorsel = btn.transform.Find("SiluetGorseli").GetComponent<Image>();
                string ing = TurkceKarakterleriTemizle(mevcutDogruSehirAdi);
                if(btnGorsel.sprite == Resources.Load<Sprite>("KirpilmisHaritalar/" + ing + "Kirp")) btn.GetComponent<Image>().color = dogruRenk;
            }
            else { if (btnYazi.text == mevcutDogruSehirAdi) btn.GetComponent<Image>().color = dogruRenk; }
        }
    }

    void SkorYazilariniGuncelle()
    {
        if (p1SkorYazisi != null) p1SkorYazisi.text = "SKOR: " + p1Skor;
        if (p2SkorYazisi != null) p2SkorYazisi.text = "SKOR: " + p2Skor;
    }

    // --- FONKSİYONLAR ---
    public void OyunuDurdur() { Time.timeScale = 0f; if (durdurmaPenceresi != null) durdurmaPenceresi.SetActive(true); }
    public void OyunuDevamEttir() { Time.timeScale = 1f; if (durdurmaPenceresi != null) durdurmaPenceresi.SetActive(false); }
    public void AnaMenuyeDon() { Time.timeScale = 1f; SceneManager.LoadScene("AnaMenu"); }
    public void TekrarOyna() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

    void OyunBitti()
    {
        p1CevapVerebilirMi = false; p2CevapVerebilirMi = false;
        if(oyunBittiPenceresi != null) oyunBittiPenceresi.SetActive(true);

        if (p1Skor > p2Skor) oyunBittiDurumMetni.text = "OYUN BİTTİ\nSOL OYUNCU KAZANDI!\nSağ Oyuncu Kaybetti.";
        else if (p2Skor > p1Skor) oyunBittiDurumMetni.text = "OYUN BİTTİ\nSAĞ OYUNCU KAZANDI!\nSol Oyuncu Kaybetti.";
        else oyunBittiDurumMetni.text = "OYUN BİTTİ\nBERABERE!";
    }

    string TurkceKarakterleriTemizle(string text)
    {
        return text.Replace("İ", "I").Replace("ı", "i").Replace("Ğ", "G").Replace("ğ", "g").Replace("Ü", "U").Replace("ü", "u").Replace("Ş", "S").Replace("ş", "s").Replace("Ö", "O").Replace("ö", "o").Replace("Ç", "C").Replace("ç", "c").Replace("â", "a");
    }
}