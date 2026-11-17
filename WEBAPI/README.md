# RMS Web API

## Seeder Data Performa

Seeder performa membuat data bervolume besar untuk menguji endpoint berat
(dashboard batch, daftar aplikasi, tahapan tes, sesi wawancara, dan log email).
Data master yang sudah ada tidak diubah perilakunya dan data performa hanya
dibuat saat diminta.

### Menjalankan

Cara tercepat, lewat argumen CLI:

```powershell
dotnet run -- --seed-performance
```

Atau aktifkan permanen di `appsettings.Development.json`:

```json
"Seed": {
  "Performance": {
    "Enabled": true
  }
}
```

Konfigurasi lengkap tersedia di section `Seed:Performance`. Nilai default
(skala sedang):

| Opsi | Default | Keterangan |
| --- | --- | --- |
| `Enabled` | `false` | Aktifkan seeder performa saat startup |
| `ResetBeforeSeed` | `true` | Hapus data perf lama sebelum membuat data baru |
| `ClearOnCheckpointReset` | `true` | Ikut hapus sisa data perf saat reset checkpoint |
| `UseSqlBulkCopy` | `true` | Gunakan `SqlBulkCopy` untuk tabel volume besar |
| `BatchSize` | `1000` | Jumlah baris per batch insert |
| `Interviewers` | `30` | Akun/pegawai pewawancara tambahan |
| `Batches` | `14` | Batch rekrutmen (Open/Closed/Draft) |
| `Candidates` | `3000` | Kandidat unik |
| `ApplicationsPerCandidate` | `3` | Lamaran per kandidat |
| `EmailLogs` | `10000` | Log email |
| `RescheduleRequests` | `500` | Permintaan reschedule |
| `InterviewSessionRatio` | `0.35` | Proporsi tahap wawancara yang punya sesi |
| `CandidateAnswerRatio` | `1.0` | Proporsi tahap tes yang punya jawaban |
| `RandomSeed` | `20261117` | Seed RNG agar data deterministik |

Contoh mengecilkan volume untuk uji cepat:

```powershell
$env:Seed__Performance__Candidates = "200"
$env:Seed__Performance__Batches = "5"
$env:Seed__Performance__EmailLogs = "500"
dotnet run -- --seed-performance
```

Untuk skala besar, naikkan `Candidates`, `ApplicationsPerCandidate`,
`Batches`, dan `EmailLogs`. Perkiraan jumlah tahapan adalah
`lamaran x rata-rata 3 tahap`; sesi wawancara dan jawaban tes mengikuti rasio
di atas.

### Data yang dibuat

- Kandidat dengan email `perf.cand.*@example.test` dan nomor telepon unik.
- Batch rekrutmen dengan kode `PERF-*`, terdiri dari batch Open, Closed, dan
  Draft. Hanya batch Open/Closed yang memiliki lamaran.
- Tahapan batch lengkap dengan `NextStageId` yang tersambung, satu stage
  `Active` per batch Open, serta penugasan pewawancara.
- Lamaran (`BatchApplication`) beserta `ApplicationStage` untuk seluruh alur:
  tes, interview HR, interview user, sampai hasil akhir dan offering letter.
- Jawaban tes (snapshot `QuestionText`, `QuestionOptionText`, `DiscType`,
  `Score`) untuk stage Test yang sudah selesai, meliputi Psikotes, Logical Test,
  dan Technical Test. `TotalScore` dihitung dari jawaban agar stage tidak punya
  score tanpa jawaban. Pada stage non-DISC, `DiscType` menggunakan nilai default
  dan tidak memengaruhi ringkasan DISC.
- Sesi wawancara, panel `SessionInterview` (tepat satu lead per sesi), dan
  `InterviewAssessment` untuk sesi yang sudah selesai.
- `RescheduleRequest` untuk sebagian sesi, termasuk status Pending/Approved/
  Rejected.
- Log email dengan subjek berawalan `PERF`.

### Akun pewawancara

Akun dibuat otomatis dengan pola:

- Username: `perf.emp.001`, `perf.emp.002`, dst.
- Email: `perf.emp.001@rms.local`, dst.
- Password: `Password123!`

Akun dan pegawai `perf.emp.*` sengaja dipertahankan antar-reset agar bisa
dipakai ulang. Hapus manual jika sudah tidak diperlukan.

### Reset dan idempotensi

- Dengan `ResetBeforeSeed = true` (default), setiap kali seeder dijalankan data
  perf lama dihapus lebih dulu, sehingga aman dijalankan berulang tanpa
  duplikasi.
- Jika `ResetBeforeSeed = false` dan data perf sudah ada, seeder akan dilewati.
- `--seed-reset` / `--reset-checkpoint` juga membersihkan sisa kandidat dan log
  email perf. Data transaksional lain tetap dibersihkan seperti biasa.
- Marker yang digunakan: batch `PERF-*`, kandidat `perf.cand.*`, pegawai
  `perf.emp.*`, subjek email `PERF`, dan path file berawalan `perf/`.

> **Penting:** `Seed:ResetToCheckpointOnStartup` bernilai `true` di
> `appsettings.Development.json`. Saat menguji performa, set ke `false` agar
> setiap restart tidak menghapus data yang baru saja dibuat.

```json
"Seed": {
  "ResetToCheckpointOnStartup": false
}
```

### Catatan teknis

- Tabel volume besar (`Candidates`, `BatchApplications`, `ApplicationStages`,
  `CandidateAnswer`, `EmailLog`) diisi dengan `SqlBulkCopy`. Enum dikonversi ke
  `int`, `DateOnly` ke `DateTime`, dan `KeepNulls` dipakai agar nilai nullable
  tetap benar.
- Tabel kecil (batch, stage, penugasan, sesi, penilaian, reschedule) memakai
  EF Core dengan chunk `BatchSize`.
- Semua ID memakai `Guid.CreateVersion7()` dan RNG memakai seed tetap sehingga
  isi data konsisten antar-run.
