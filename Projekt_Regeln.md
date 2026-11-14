# PLC-OptiFlow – Proje Kuralları (AI için)

Dashboard’da olması gerekenler (öncelik sırasıyla)

Üstte durum şeridi: Bağlantı (Verbunden/Getrennt), makine durumu (Läuft / Gestoppt / Notaus) ve aktif alarm sayısı. Rozetler yeşil, gri, kırmızı. En önemli ekleme bu.
Büyük sıcaklık değeri + kadran: Sende var. Kadranda renk bölgeleri olsun: normal yeşil, 80 °C sarı, 90 °C kırmızı.
Trend grafiği + Sollwert çizgisi: Sende var. Hedef sıcaklığı yatay kesikli çizgiyle göstermek grafiği anlamlı yapar, “hedefe ne kadar yaklaştı” görülür.
Küçük durum kartları (3 adet): Heizung (Ein/Aus lambası), Pumpe (Ein/Aus lambası), Sollwert (60 °C). Tek satırlık, sade kartlar.
Start/Stop + Letzte Antwort: Sende var, kalsın.

┌──────────────────────────────────────────────────┐
│ ● Verbunden   ● Läuft   ⚠ 1 Alarm                │  ← durum şeridi
├───────────────────────┬──────────────────────────┤
│  PLC TEMPERATUR       │                          │
│  54.3 °C   [kadran]   │   Temperaturverlauf      │
│  [Start/Stop]         │   (grafik + Sollwert)    │
├───────────┬───────────┼──────────────────────────┤
│ Heizung ● │ Pumpe ●   │ Sollwert 60 °C           │  ← 3 küçük kart
└───────────┴───────────┴──────────────────────────┘

## Simüle edilen makine: tek bir Kessel (Kazan)
Sadece şu değerler olacak:
- **Temperatur** (°C, double)
- **Heizung** (Ein/Aus)
- **Pumpe** (Ein/Aus)
- **Sollwert** (hedef sıcaklık, varsayılan 60 °C)
- **Notaus** (acil durdurma)

Davranış (Backend `PlcService` içinde):
- Heizung Ein → sıcaklık yavaşça yükselir; Aus → yavaşça düşer.
- Küçük rastgele titreşim (±0.3 °C).
- Otomatik mod: sıcaklık Sollwert'in altına inince Heizung açılır, üstüne çıkınca kapanır.
- Notaus → Heizung ve Pumpe kapanır, sistem kilitlenir, Reset gerekir.

## Alarmlar (sıcaklıktan türetilir, sahte liste YOK)
- ≥ 80 °C → **Warnung**
- ≥ 90 °C → **Kritisch** (Heizung otomatik kapanır)
- Bağlantı kopması → **Warnung** ("Getrennt" göstergesi)

## Ekranlar (yeni ekran EKLEME)
1. **Übersicht**: sıcaklık, kadran, trend grafiği, Start/Stop, durum lambaları
2. **Handbetrieb**: Pumpe, Heizung, Sollwert, Notaus
3. **Alarm**: aktif alarmlar, Quittieren
4. **Einstellungen**: Backend adresi, aralık, tema

## Mimari kuralları
- MVVM: View'da iş mantığı yok. Code-behind sadece saf UI işi için.
- ViewModel yalnızca `IPlcService` bilir (`GrpcPlcService` veya `SimulierterPlcService`).
- Backend, WPF'e bağımlı olmaz. `plc.proto` iki tarafta aynı kalır.
- Ağ çağrıları `async`, UI asla donmaz.
- Veri eski veya kopuksa ekran bunu açıkça gösterir.
- Renk anlam taşır: yeşil normal, sarı uyarı, kırmızı alarm.
- Tüm renkler `Themes/Dunkel.xaml` ve `Hell.xaml` kaynaklarından gelir, sabit hex yok.
- Kod/isimler Almanca-İngilizce karışık mevcut stile uyar (`Temperatur`, `Quittieren`).

## YAPMA (kapsam dışı)
- Kullanıcı girişi/roller, veritabanı, birden fazla makine
- Gerçek protokoller (Modbus, S7, OPC-UA)
- Yeni NuGet paketi (zorunlu değilse sormadan ekleme)
- Büyük refactor veya mevcut klasör yapısını değiştirme

## Tamamlanma kriteri (Definition of Done)
- [ ] Proto: `LeseStatus`, `SchreibeSollwert`, `SchreibeHeizung`, `Notaus`
- [ ] Backend fiziği ve alarm eşikleri çalışıyor
- [ ] Handbetrieb ve Alarm ekranları gerçek veriye bağlı
- [ ] Testler: fizik (ısıtıcı → sıcaklık artar), alarm eşikleri, Notaus
- [ ] README: amaç, mimari şeması, ekran görüntüleri, çalıştırma adımları

## Çalışma şekli
- Her seferinde **tek küçük adım**, sonra dur ve özetle.
- Değiştirdiğin dosyaları listele.
- Emin değilsen tahmin etme, sor.
- Her adımdan sonra proje derlenmeli ve testler geçmeli.