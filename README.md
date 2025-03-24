# satu: Tugas Besar I IF2211 Strategi Algoritma

## Daftar Isi
1. [Tentang Proyek](#tentang-proyek)
2. [Cara Menjalankan](#cara-menjalankan)
3. [Tentang Bot](#tentang-bot)
4. [Kelompok](#kelompok)

## Tentang Proyek
Tugas Besar 1 merupakan tugas besar yang bertujuan untuk menguji pemahaman mengenai algoritma Greedy. Pemahaman akan algoritma Greedy diuji dengan membuat bot pada Robocode Tank Royale. Terdapat 4 strategi Greedy yang diimplementasikan pada tugas ini, strategi tersebut berdasarkan pada serangan, deteksi, dan pergerakan acak. 

## Cara Menjalankan
1. Melakukan unduhan pada .NET jika belum ada.
2. Pastikan _game engine_ Robocode Tank Royale ada.
3. Clone repository ini dengan menjalankan perintah pada terminal dengan directory yang diinginkan
   ```sh
   git clone https://github.com/BerthaSoliany/Tubes1_satu.git
4. Melakukan pengecekan pada file .csproj setiap bot. Pastikan versi .NET sesuai dengan yang dimiliki. Jika tidak, lakukan perubahan versi .NET menyesuaikan versi yang dimiliki.
5. Jalankan Robocode Tank Royale dengan perintah berikut
   ```sh
   java -jar robocode-tankroyale-gui-0.30.0.jar
6. Atur konfigurasi booter dengan menekan tombol 'config' dilanjutkan 'Bot Root Directories' dan memasukkan directory folder dari semua bot yang nantinya ingin dimainkan.
7. Menekan 'Battle' lalu 'Start Battle' dilanjutkan memilih bot yang akan di-booting kemudian memilih bot yang sudah di-booting untuk dimainkan pada pertandingan.
8. Menekan tombol 'Start Battle' ketika semua bot yang ingin dimainkan berada pada kotak kanan-bawah.

## Tentang Bot
| Bot | Deskripsi |
|-----|------|
| Random Strafe Bot | Bot yang memiliki gerakan acak |
| Border Bot | Bot yang bergerak pada dinding |
| Johnny | Bot yang melakukan gerakan berputar dan mengunci target |
| Nearby Ram | Bot yang fokus melakukan _ramming_ |


## Kelompok
| NIM | Nama |
|-----|------|
| Nicholas Andhika Lucas | 13523014 |
| Bertha Soliany Frandi | 13523026 |
| Michael Alexander Angkawijaya | 13523102 |
