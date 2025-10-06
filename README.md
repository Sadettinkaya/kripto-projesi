# Kripto Projesi 🚀

## 📖 Proje Açıklaması  
Bu proje, **bir kripto para borsası web uygulaması**dır.  

- **Giriş yapmamış kullanıcılar**, sadece ürünlere ait **anlık fiyat bilgilerini** görebilir.  
- Detay görmek isteyen kullanıcılar, **giriş ekranına yönlendirilir**.  
- **Kayıtlı kullanıcılar giriş yaptıktan sonra**, kripto paraların detay bilgilerini (anlık fiyat, 24 saatlik en yüksek değer, 24 saatlik hacim) görüntüleyebilir.  

Uygulama **Docker üzerinde container’lar ile ayağa kaldırılmaktadır**.  

---

## 🖼️ Ekran Görüntüleri  

### 🔹 Docker üzerinde uygulamanın ayağa kaldırılması
![Docker](images/docker.jpg)

### 🔹 Giriş yapmamış kullanıcının coin detaylarını görememesi
![Guest](images/guest.jpg)

### 🔹 Kayıt Ol ekranı
![Register](images/register.jpg)

### 🔹 Kayıt olup giriş yapan kullanıcının coin detaylarını görüntüleyebilmesi
![Details](images/details.jpg)

---

## 🛠️ Kullanılan Teknolojiler  
- **.NET Core 6+** (Backend API)  
- **React 18 (Vite.js)** (Frontend)  
- **Docker** (Containerization)  
- **PostgreSQL** (Veritabanı)  
- **Git & GitHub** (Versiyon kontrolü)  
- **JWT Token** (Authentication)  
- **WebSocket** (Gerçek zamanlı fiyat güncellemeleri)  
- **OKX Kripto Borsası Public API** (Veri kaynağı)  

---

## ⚙️ Özellikler  
- Docker Compose ile **backend, frontend ve veritabanı container’ları** kolayca ayağa kaldırma  
- **JWT tabanlı kimlik doğrulama**  
- **WebSocket** ile canlı fiyat akışı  
- OKX API’den anlık kripto fiyatları ve detayları çekme  
- **Kullanıcı girişi olmadan fiyat bilgisi**, giriş sonrası detay bilgisi görüntüleme  

---

## 🚀 Çalıştırma  
Proje dizininde:  
```bash
docker-compose up -d --build
