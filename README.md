# 🏥 Klinik 

REST API manajemen klinik sederhana yang mencakup data **Pasien**, **Dokter**, dan **Jadwal Konsultasi**.  
Dibuat sebagai pemenuhan tugas LKM 1 – Penerapan API (Pemrograman Antarmuka Aplikasi).

---

## 🛠 Teknologi yang Digunakan

| Komponen   | Detail                          |
|------------|---------------------------------|
| Bahasa     | C# (.NET 8)                     |
| Framework  | ASP.NET Core Web API            |
| Database   | PostgreSQL                      |
| ORM/Driver | Npgsql (raw SQL + prepared stmt)|
| Auth       | JWT Bearer Token                |
| Docs       | Swagger (Swashbuckle)           |
| Tools      | Visual Studio 2022, pgAdmin 4   |

---

## 📁 Struktur Folder

```
KlinikApi/
├── Controllers/
│   ├── AuthController.cs       ← Register & Login (JWT)
│   ├── DokterController.cs     ← CRUD Dokter
│   ├── PasienController.cs     ← CRUD Pasien
│   └── JadwalController.cs     ← CRUD Jadwal + PATCH status
├── Context/
│   └── DbHelper.cs             ← Koneksi PostgreSQL (Npgsql)
├── Models/
│   └── Models.cs               ← Semua model & DTO
├── Program.cs                  ← Entry point, DI, Swagger, JWT
├── appsettings.json            ← Konfigurasi koneksi & JWT
├── KlinikApi.csproj            ← Dependency NuGet
└── database.sql                ← DDL + sample data
```

---

## ⚙️ Langkah Instalasi & Menjalankan Project

### 1. Clone / buka project di Visual Studio

```bash
# Clone (jika dari GitHub)
git clone (https://github.com/Dinazakiyah/2097_DinaZakiyahW_Klinik.git)
cd KlinikApi
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

Atau klik kanan pada project di Visual Studio → **Manage NuGet Packages** → **Restore**.

### 3. Konfigurasi Database

Edit file **`appsettings.json`**, sesuaikan:

```json
"ConnectionStrings": {
  "koneksi": "Host=localhost;Port=5432;Database=klinik_db;Username=postgres;Password=YOUR_PASSWORD"
}
```

Ganti `YOUR_PASSWORD` dengan password PostgreSQL kamu.

### 4. Jalankan Project

```bash
dotnet run
```

Atau tekan **F5** di Visual Studio.

Aplikasi akan berjalan di:
- `https://localhost:7xxx` atau `http://localhost:5xxx`
- Swagger UI otomatis terbuka di root URL `/`

---

## 🗄️ Cara Import Database

### Menggunakan pgAdmin 4

1. Buka **pgAdmin 4**
2. Klik kanan **Databases** → **Create** → **Database** → beri nama `klinik_db`
3. Klik kanan database `klinik_db` → **Query Tool**
4. Buka file `database.sql` → jalankan semua (**F5**)

### Menggunakan psql (terminal)

```bash
psql -U postgres -c "CREATE DATABASE klinik_db;"
psql -U postgres -d klinik_db -f database.sql
```

---

## 🔐 Cara Menggunakan JWT di Swagger

1. Panggil endpoint `POST /api/auth/register` → buat user baru
2. Panggil endpoint `POST /api/auth/login` → salin token dari response
3. Klik tombol **Authorize 🔒** di Swagger UI
4. Masukkan: `Bearer <token_kamu>`
5. Semua endpoint lain sudah bisa diakses

---

## 📋 Daftar Endpoint

### 🔑 Auth

| Method | URL                   | Keterangan                        | Auth |
|--------|-----------------------|-----------------------------------|------|
| POST   | `/api/auth/register`  | Daftar user baru                  | ❌   |
| POST   | `/api/auth/login`     | Login, mendapatkan JWT token      | ❌   |

### 🩺 Dokter

| Method | URL                   | Keterangan                        | Auth |
|--------|-----------------------|-----------------------------------|------|
| GET    | `/api/dokter`         | Ambil semua data dokter           | ✅   |
| GET    | `/api/dokter/{id}`    | Ambil detail dokter by ID         | ✅   |
| POST   | `/api/dokter`         | Tambah dokter baru                | ✅   |
| PUT    | `/api/dokter/{id}`    | Update seluruh data dokter        | ✅   |
| DELETE | `/api/dokter/{id}`    | Hapus dokter (soft delete)        | ✅   |

### 🧑‍⚕️ Pasien

| Method | URL                   | Keterangan                        | Auth |
|--------|-----------------------|-----------------------------------|------|
| GET    | `/api/pasien`         | Ambil semua data pasien           | ✅   |
| GET    | `/api/pasien/{id}`    | Ambil detail pasien by ID         | ✅   |
| POST   | `/api/pasien`         | Tambah pasien baru                | ✅   |
| PUT    | `/api/pasien/{id}`    | Update seluruh data pasien        | ✅   |
| DELETE | `/api/pasien/{id}`    | Hapus pasien (soft delete)        | ✅   |

### 📅 Jadwal

| Method | URL                         | Keterangan                              | Auth |
|--------|-----------------------------|-----------------------------------------|------|
| GET    | `/api/jadwal`               | Ambil semua jadwal (JOIN pasien+dokter) | ✅   |
| GET    | `/api/jadwal/{id}`          | Ambil detail jadwal by ID               | ✅   |
| POST   | `/api/jadwal`               | Buat jadwal baru                        | ✅   |
| PUT    | `/api/jadwal/{id}`          | Update seluruh data jadwal              | ✅   |
| DELETE | `/api/jadwal/{id}`          | Hapus jadwal                            | ✅   |
| PATCH  | `/api/jadwal/{id}/status`   | Update status jadwal saja               | ✅   |

---

## 📦 Format Request & Response

### Contoh POST `/api/auth/login`
**Request:**
```json
{ "username": "admin", "password": "password123" }
```
**Response sukses (200):**
```json
{
  "status": true,
  "message": "Login berhasil.",
  "data": { "id": 1, "username": "admin", "role": "admin", "token": "eyJ..." }
}
```
**Response error (401):**
```json
{ "status": false, "message": "Username atau password salah." }
```

### Contoh POST `/api/jadwal`
**Request:**
```json
{
  "pasienId": 1,
  "dokterId": 2,
  "tanggal": "2025-07-10",
  "jam": "09:00:00",
  "keluhan": "Batuk berkepanjangan",
  "status": "menunggu"
}
```

---

## 🎥 Video Presentasi

> 📺 Link Video: (https://youtu.be/luZmhGQtB5I)

---

## 👤 Identitas

| Field | Isi                       |
|-------|---------------------------|
| Nama  | Dina Zakiyah Wiliansyah   |
| NIM   | 242410102097              |
| Kelas | PAA B                     |
