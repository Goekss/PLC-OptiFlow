# PLC-OptiFlow – Mimari Vizyon (Multi-Screen)

## Hedef
Tek bir makine verisi veya coklu makine verileri, birden fazla ekranda görülecek:
- **Makine başı (WPF):** Operatör izler ve kontrol eder.
- **Büro PC (WPF):** Operatör/ofis izler.
- **Tablet, telefon, 3. PC (Browser):** Mühendis veya şef sadece izler.

## Genel Yapı

```text
XYZ Makinesi (PLC, ileride TwinCAT 3)
        │  ADS (ileride) / Simülasyon (şimdi)
        ▼
     Backend  ← PLC ile sadece o konuşur
        │
   ┌────┼───────────────┐
   ▼    ▼               ▼
 WPF   WPF           Web (React)
 makine büro PC      tablet / telefon / 3. PC
 (kontrol) (izleme)  (sadece izleme)
```

## Temel Kurallar
1. **Sadece Backend PLC'ye bağlanır.** İstemciler PLC ile doğrudan konuşmaz.
2. **Backend canlı veriyi tüm istemcilere iter.** Web için SignalR, WPF için şimdilik gRPC.
3. **Yetki ayrımı:**
   - Makine başındaki WPF: Start/Stop, Sollwert, Notaus yapabilir.
   - Büro PC, tablet, telefon, tarayıcı: **sadece izler**.
4. **Güvenlik:** Makine ağı yerel kalır. Dışarıdan erişim yalnızca VPN veya HTTPS ile.
5. **Çoklu makine:** Her çağrı ve mesajda `MaschinenId` olur. Backend her makine için ayrı bağlantı tutar.

## Yol Haritası

| Faz | İçerik | Durum |
|-----|--------|-------|
| 1   | Tek makine simülasyonu (Kessel), WPF, alarmlar, testler | Şimdi |
| 2   | Backend'e SignalR, salt okunur React izleme sayfası | Sonra |
| 3   | Rol/yetki ayrımı (kontrol / izleme) | Sonra |
| 4   | TwinCAT 3 ADS bağlantısı (`Beckhoff.TwinCAT.Ads`) | Roadmap |
| 5   | Çoklu makine (`MaschinenId`, makine listesi) | Roadmap |

## Şimdi Dikkat Edilecekler (Faz 1)
- `IPlcService` soyutlamasını koru, ADS'ye geçiş sadece Backend'de olacak.
- Backend'i WPF'e bağımlı yapma.
- Proto ve servis tasarımında `MaschinenId` eklemeye açık kal (şimdi tek makine olsa bile).
- Faz 2 ve sonrası için kod yazma, sadece README'ye Roadmap olarak yaz.

## Kapsam Notu
`PROJEKT_REGELN.md` içindeki "kapsam dışı" maddeleri **Faz 1 için** geçerlidir. Faz 2 başladığında SignalR ve React o dosyada "kapsam içi" olarak güncellenmelidir.