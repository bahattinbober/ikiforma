# ikiforma

## Proje amacı

İki ayrı ürün aynı veri tabanı üzerinde çalışır:

1. **Kesişim sitesi**: Kullanıcı iki futbol takımı seçer, site her iki takımda da forma giymiş oyuncuları listeler.
2. **Hakem modu**: Canlı yayıncılar için maç sayacı + skor paneli. Durum SignalR ile gerçek zamanlı yayınlanır, OBS'ye "browser source" olarak eklenebilecek bir overlay sayfası bu veriyi gösterir.

## Mimari

| Bileşen | Teknoloji | Rol |
|---|---|---|
| Frontend | Next.js (TR/EN, i18n) | Kesişim sitesi + hakem paneli arayüzü + OBS overlay sayfası |
| API | ASP.NET Core Web API | REST uçları: oyuncu/takım sorguları, maç/skor CRUD |
| Gerçek zamanlı | SignalR hub | Hakem panelinden overlay'e skor/sayaç güncellemelerini canlı iter |
| Veritabanı | PostgreSQL (EF Core) | Kalıcı veri — host portu **5433** (5432 değil, muhtemelen yerel makinede başka bir Postgres instance'ı ile çakışmasın diye) |
| Veri toplama | .NET Worker Service | Wikidata SPARQL endpoint'inden oyuncu/takım/transfer verisini periyodik çeker, PostgreSQL'e yazar |

Akış: Worker → PostgreSQL ⇄ API → Next.js (kesişim sitesi + hakem paneli). Hakem panelindeki değişiklikler API üzerinden SignalR hub'a, oradan overlay sayfasına gider.

## Veritabanı şeması — spora bağımsız tasarım

Şema bilinçli olarak futbola özel alan içermez; yarın başka bir spora (basketbol, voleybol...) genişleme ihtimaline karşı çekirdek varlıklar spor-agnostik kurulur.

Çekirdek tablolar:
- **sport** — spor dalı (ör. "football")
- **league** — bir `sport`'a bağlı lig
- **team** — bir `sport`'a bağlı takım
- **player** — spordan bağımsız kişi (isim, doğum tarihi, Wikidata QID vb.)
- **stint** — bir `player`'ın bir `team`'de oynadığı dönem (başlangıç/bitiş tarihi). **Kesişim sorgusunun temel tablosu**: "iki takımda da oynamış oyuncu" = `stint` tablosunda `team_id = A` ve `team_id = B` olan satırların `player_id` kesişimi.

`league` ve sezon bazlı ilişkiler (hangi takım hangi ligde hangi sezon oynadı) ayrı bir ilişki tablosuyla modellenmeli, `team` tablosuna gömülmemeli — bir takım zamanla lig/sezon değiştirir, bu spordan bağımsız bir gerçek.

## Sürüm ve ortam

- **.NET 10**, `global.json` ile pin'li (`sdk.version: 10.0.401`, `rollForward: latestFeature`). Yeni bir .NET projesi oluştururken bu global.json'ın kapsadığı dizin altında kalınmalı.
- **PostgreSQL host portu: 5433**. Docker Compose / bağlantı string'lerinde bu port kullanılmalı, varsayılan 5432 değil.

## Çalışma tarzı — kullanıcı .NET öğreniyor

Kod **Claude tarafından yazılır**, ama:
- Önemli mimari kararlar ve "neden" bu şekilde yapıldığı kısaca açıklanır (özellikle EF Core migration'ları, DI, minimal API vs. controller, SignalR hub yaşam döngüsü gibi .NET'e özgü kavramlarda).
- Açıklamalar kod içine değil, konuşma içine yazılır — kodda sadece WHY gerektiren yerlerde tek satırlık yorum olur, öğretici/anlatan yorum bloğu yazılmaz.
- Yeni bir .NET kavramı ilk kez kullanıldığında (ör. ilk migration, ilk hub, ilk worker) neden o yaklaşımın seçildiği kısaca belirtilir.

## Dil

Frontend TR/EN i18n destekler. Kod, değişken adları, commit mesajları İngilizce; kullanıcıyla iletişim Türkçe.

## Git kuralları

**Commit, push veya branch işlemi yapma.** Commit'leri kullanıcı manuel atar. Bir adım/görev bitince kod içinde commit atmak yerine, kullanıcıya uygun bir commit mesajı öner — bu kadar.
