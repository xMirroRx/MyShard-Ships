# MyShard Ships

Ultima Online gemi sistemi, MyShard'ın sunucusundan (Sphere / Source-X fork'u, C++) ve istemcisinden (ClassicUO fork'u, C#) A'dan Z'ye çıkarılmış hali.

*The complete ship system of MyShard, extracted from its server (a Sphere / Source-X fork, C++) and its client (a ClassicUO fork, C#): moving multis, piloting, boat movement on the client, and MyShard's own additions (ship dye, ship glow, hull-shaped ship regions).*

Kaynak sürümler: MyShard-Server `56237551`, MyShard-Client `fbdba5303`.

## İçerik

```
server/                      Sunucu (C++)
  src/game/items/CItemShip.*            Gemi eşyası: dümen, iskele, ambar, komutlar, yön
  src/game/components/CCMultiMovable.*  Hareket eden multi: gemi hareketi, dönüş, üstündekilerin taşınması,
                                        MyShard ship dye / ship glow durumu
  integration/                          Gemiye dokunan diğer dosyalardan alıntılar (satır numaralı)

client/                      İstemci (C#)
  src/.../Managers/BoatMovingManager.cs Geminin ve üstündekilerin akıcı hareketi, adımların sıraya alınması
  src/.../Network/PacketHandlers/BoatMoving.cs   0xF6 gemi hareket paketi
  src/.../UI/Gumps/BoatControl.cs       Gemi kontrol penceresi
  src/.../Managers/ShipDyeManager.cs    Store'dan alınan gemi boyası (MyShard)
  src/.../Managers/ShipGlowManager.cs   Store'dan alınan gemi parlaması (MyShard)
  src/ClassicUO.Assets/gumpartassets/*-ship.gif  Store'daki gemi önizlemeleri
  integration/                          Gemiye dokunan diğer dosyalardan alıntılar (satır numaralı)

patches/                     MyShard'ın gemi commitleri, git am ile uygulanabilir
  server/7d95d91c.patch      Gemi bölgesi gövdeyi izler, kapsayan kutuyu değil
  server/8ec1a04a.patch      Ship Dye (pilotken gemi boyama)
  server/375a7185.patch      Store gemi glow: tüm parçalar + görüş senkronu
  client/39b6455ff-boat-walking.patch  Gemi üstünde yürüme düzeltmesi
  client/7546fff6c-ship-dye.patch      Store Ship Dye (Mount Dye ile aynı commit)
  client/92c5c366d-ship-glow.patch     Store gemi glow: Medium/Large Dragon Ship
```

`integration/` altındaki her `.txt` dosyası, kaynak dosyada gemiyle ilgili satırları ve çevresindeki 10 satırı, orijinal satır numaralarıyla verir. Bu kısımlar sunucunun ve istemcinin geri kalanına bağlı olduğu için tek başına derlenmez. Bir projeye taşırken hangi noktalara dokunmak gerektiğini gösterirler.

## Ağ paketleri

| Paket | Yön | Ne yapar |
|---|---|---|
| `0xF6` (`PacketMoveShip`) | sunucu → istemci | Geminin ve üstündekilerin tek pakette hareketi (High Seas) |
| `0xBF.0x33` (`PacketWheelBoatMove`) | istemci → sunucu | Pilotun fareyle (High Seas) gemiyi yönlendirmesi: yön ve hız |
| konuşma | istemci → sunucu | Dümene söylenen klasik komutlar (forward, turn right, stop…) |
| `0xCE` `ShipDye` | istemci → sunucu | Oyuncunun uyguladığı Store gemi boyası (0 = yok) |
| `0xCE` `ShipDyeState` | sunucu → herkes | Bir gemi parçasının boya rengi; pilotken tüm gemi boyanır |
| `0xCE` `ShipGlow` | istemci → sunucu | Oyuncunun uyguladığı Store gemi parlaması türleri (bit maskesi) |
| `0xCE` `ShipGlowState` | sunucu → herkes | Bir geminin parlaması açık/kapalı; pilotken tüm gemi parlar |

`0xCE`, MyShard'ın kendi genişletilmiş paket kanalıdır (`EnhancedPackets`). Gemi parlaması türleri, gemi multi id'sine göre belirlenir: Medium Dragon = `0x0C`–`0x0F`, Large Dragon = `0x14`–`0x17`.

## Taşırken

1. Sunucu: `CItemShip` ve `CCMultiMovable` dosyalarını ekle, sonra `server/integration/` altındaki noktaları (eşya türleri `IT_SHIP*`, `CItemMulti`, sektör uyandırma, istemci kullanımı, paketler) kendi kodunda karşılığıyla birleştir.
2. İstemci: `BoatMovingManager`, `BoatMoving` paket işleyicisi ve `BoatControl` gump'ını ekle, `World` içinde yöneticiyi oluştur, paket kayıtlarını (`HandlerRegistry`) bağla.
3. MyShard eklentileri (dye / glow) isteğe bağlıdır; `patches/` altındaki yamalar bunların tamamını gösterir.

## Lisans

Bu repo iki açık kaynak projeden kod içerir; her parça kendi lisansıyla gelir:

- `server/` ve `patches/server/`: Sphere / Source-X, **Apache License 2.0** — `LICENSE-SphereServer-Apache2.txt`
- `client/` ve `patches/client/`: ClassicUO, **BSD 2-Clause** — `LICENSE-ClassicUO-BSD2.md`

MyShard'ın eklediği kısımlar da aynı lisanslar altındadır.
