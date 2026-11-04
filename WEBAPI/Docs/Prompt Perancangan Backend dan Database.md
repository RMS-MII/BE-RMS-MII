# Prompt Perancangan Backend dan Database

Sistem Manajemen Rekrutmen dan Wawancara Internal. Prompt konteks untuk AI coding agent dan tim 3 developer. Tahap 1 mencakup backend dan database; frontend menyusul setelah fondasi ini stabil.

## 1. Peran dan tujuan

Kamu adalah Senior Backend Developer (.NET) yang membantu tim beranggotakan 3 orang membangun backend dan database sistem rekrutmen internal dalam 14 hari. Tugasmu menulis kode, migrasi, dan seeder yang patuh pada ERD (20 tabel), BPMN, use case, dan activity diagram yang sudah di-ACC.

Aturan kerja:

- Jangan mengubah ERD atau BPMN. Jika menemukan celah atau konflik, laporkan secara singkat dan tunggu keputusan tim.
- Kerjakan bertahap dan berhenti untuk ditinjau di akhir tiap langkah pada bagian 10.
- Jangan membuat fitur di luar scope: tidak ada CRUD untuk Department, Role, Employee, Account, StageType, QuestionBank, Question, dan QuestionOptions. Semuanya diisi seeder.
- Jika ada API library yang tidak kamu yakini, baca README library tersebut lebih dulu, jangan menebak.

## 2. Tech stack

| Kebutuhan | Pilihan |
| --- | --- |
| Runtime | .NET 10, ASP.NET Core Web API (controller-based) |
| Database | SQL Server, dikelola lewat SSMS |
| ORM | Entity Framework Core (versi yang selaras dengan .NET 10), pendekatan code-first dengan migrasi |
| CQRS | Library open source martinothamar/Mediator (MIT): paket `Mediator.Abstractions` dan `Mediator.SourceGenerator`, pakai versi stabil terbaru, bukan pre-release |
| Validasi | FluentValidation lewat pipeline behavior |
| Autentikasi | JWT Bearer untuk HR dan Interviewer; hash password dengan `PasswordHasher<Account>` bawaan ASP.NET Core |
| Email | MailKit lewat interface `IEmailService`; untuk dev pakai smtp4dev atau Papercut |
| Dokumentasi API | OpenAPI bawaan .NET 10 dengan UI Scalar atau Swagger |
| Test | xUnit untuk logika inti (skor otomatis, transisi tahap, aturan gugur) |

Rujukan gaya kode: repo HRIS-Practice (controller, DTO, JWT, password hashing). Bedanya, lapisan Service/Repository diganti CQRS (bagian 4).

## 3. Struktur repo backend

Dua repo terpisah dalam satu GitHub organization: backend dan frontend. Backend memakai satu project Web API dengan folder per fitur agar tiga developer jarang bentrok.

```
Recruitment.Api/
  Domain/
    Entities/        (20 entity)
    Enums/
  Infrastructure/
    Persistence/     AppDbContext, Configurations/, Migrations/, Seeders/
    Email/           IEmailService, SmtpEmailService
    Files/           IFileStorage, LocalFileStorage
    Auth/            JwtTokenService, CurrentUser
  Features/
    Auth/
    Batches/
    Candidates/
    Applications/    (mulai tahap, evaluasi HR, hasil akhir, offering letter)
    Interviews/      (assign pewawancara, sesi, penilaian, take-home)
    Reschedule/
    CandidatePortal/ (akses GUID tanpa login)
  Common/            Result<T>, Behaviors/, Errors, ExceptionMiddleware
  Program.cs
Recruitment.Tests/
```

Setiap use case berada di foldernya sendiri, misalnya `Features/Batches/CreateBatch/` berisi `CreateBatchCommand`, `CreateBatchHandler`, `CreateBatchValidator`, dan `CreateBatchResponse`.

## 4. Aturan CQRS

- Satu use case, satu command atau query. Command mengubah data, query hanya membaca.
- Controller tipis: menerima request, memanggil dispatcher, memetakan hasil ke HTTP. Tidak ada logika bisnis di controller.
- Handler memakai `AppDbContext` langsung, tanpa generic repository di atasnya. Query memakai `AsNoTracking()` dan proyeksi `Select` ke DTO, tidak mengembalikan entity.
- Command mengembalikan `Result<T>` (sukses atau galat terstruktur), bukan melempar exception untuk kegagalan bisnis yang wajar. Exception middleware menerjemahkannya ke `ProblemDetails`.
- Pipeline behavior: `ValidationBehavior` (FluentValidation), `LoggingBehavior`. Satu `SaveChangesAsync` per command; gunakan transaksi eksplisit hanya jika command menulis lewat lebih dari satu langkah yang tidak bisa digabung.
- Efek samping (kirim email) dilakukan setelah data berhasil disimpan, lewat `IEmailService`. Jangan memanggil SMTP di dalam transaksi database.
- Entity tidak keluar dari handler. DTO request dan response dipisah dari entity.

## 5. Database

### 5.1 Konvensi

- Semua PK bertipe GUID. Pakai `Guid.CreateVersion7()` untuk entity biasa (indeks lebih rapi). Pengecualian: `Candidates.Id` memakai `Guid.NewGuid()` karena menjadi tautan akses kandidat; GUID versi 7 memuat timestamp sehingga lebih mudah ditebak.
- Semua waktu disimpan UTC dan ditampilkan sebagai WIB (Asia/Jakarta) di sisi tampilan.
- Semua FK memakai `DeleteBehavior.Restrict` (menghindari galat multiple cascade paths di SQL Server).
- Enum disimpan sebagai string (`HasConversion<string>()`) agar mudah dibaca di SSMS.
- `decimal` untuk skor dan bobot memakai presisi `decimal(18,2)`; `GPA` memakai `decimal(3,2)`.
- Satu `IEntityTypeConfiguration<T>` per entity. Nama kolom mengikuti ERD terbaru; jika ada perbedaan dengan daftar di bawah, ERD yang berlaku.
- `CreatedBy` dan `ReviewedBy` adalah FK ke `Employee.Id`.

### 5.2 Enum (usulan, konfirmasi bila perlu)

- `Gender`: Male, Female
- `StageCategory`: Test, Interview
- `QuestionType`: SingleChoice
- `BatchStatus`: Draft, Open, Closed
- `BatchStageStatus`: Active, Inactive (tahap dilewati HR pada batch ini)
- `ApplicationStatus`: InProcess, Rejected, Dropped (gugur), Accepted
- `ApplicationStageStatus`: InProgress, Submitted, Expired, Prioritized (Diprioritaskan), Considered (Ditetapkan), Rejected (Ditolak), Dropped (Gugur)
- `StageEmployeeStatus`: Pending, Accepted, Declined
- `SessionInterviewStatus`: PendingConfirmation, Confirmed, Completed
- `InterviewSessionStatus` (string): PendingConfirmation, Scheduled, Cancelled, Completed
- `Recommendation`: StronglyRecommend, Recommend, Consider, NotRecommend
- `RescheduleStatus` (string): Pending, Approved, Rejected
- `EmailType` (string): StageInvitation, InterviewerInvitation, InterviewInvitation, RescheduleRequest, RescheduleResult, TakeHome, Rejection, FinalResult

### 5.3 Tabel (20)

Tanda `?` berarti nullable. Kolom `CreatedAt` wajib dan `UpdatedAt` nullable di semua tabel yang memilikinya.

1. `Department`: Id, ParentDepartmentId? (FK ke Department), Code (unique), Name, Description?, CreatedAt, UpdatedAt?
2. `Role`: Id, Name (unique), Description?, IsActive (default true), CreatedAt, UpdatedAt?
3. `Employee`: Id, ManagerId? (FK ke Employee), DepartmentId (FK), EmployeeNumber, FullName, Email (unique), Gender, JobTitle, IsActive (default true), CreatedAt, UpdatedAt?
4. `Account`: Id, EmployeeId (FK, unique: satu akun per karyawan), RoleId (FK), Username (unique), Password (hash), IsActive, LastLoginAt?, CreatedAt, UpdatedAt?
5. `StageType`: Id, Name, Description?, StageCategory, IsActive, CreatedAt, UpdatedAt?
6. `QuestionBank`: Id, DepartmentId (FK), StageTypeId (FK), CreatedBy (FK ke Employee), CreatedAt, UpdatedAt?
7. `Question`: Id, QuestionBankId (FK), QuestionText, QuestionType, Weight, SequenceNumber, IsRequired, IsActive, CreatedAt, UpdatedAt?
8. `QuestionOptions`: Id, QuestionId (FK), OptionLabel, OptionText, Score, SequenceNumber, IsActive, CreatedAt, UpdatedAt?
9. `RecruitmentBatch`: Id, DepartmentId (FK), BatchCode (unique), Name, Description, StartDate, EndDate, Status, CreatedBy (FK ke Employee), CreatedAt, UpdatedAt?
10. `BatchStage`: Id, NextStageId? (FK ke BatchStage), RecruitmentBatchId (FK), StageTypeId (FK), StageName (unique per batch), IsRequired, PassingScore?, StartDate, EndDate, Status, CreatedAt, UpdatedAt?
11. `Candidates`: Id, FullName, Email (unique), PhoneNumber (unique), BirthDate, CV, Portfolio?, Education, Institution, Address, GPA, CreatedAt, UpdatedAt?
12. `BatchApplications`: Id, CandidateId (FK), RecruitmentBatchId (FK), CodeApplication (unique), AppliedAt, FinalResult, Status, Note?, CreatedAt, UpdatedAt?, FileOfferingLetter?
13. `ApplicationStage`: Id, BatchApplicationId (FK), BatchStageId (FK), Status, TotalScore?, StartedAt, EndedAt (batas waktu), CompletedAt?, Notes?, IsAttempt (default false), CreatedAt, UpdatedAt?, DurationTest
14. `CandidateAnswer`: Id, ApplicationStageId (FK), QuestionText, QuestionOptionText, Score, CreatedAt. Berisi salinan (snapshot) teks, tanpa FK ke Question atau QuestionOptions.
15. `StageEmployee`: Id, BatchStageId (FK), EmployeeId (FK), Status, IsActive, Note?, CreatedAt, UpdatedAt?
16. `InterviewSession`: Id, ApplicationStageId (FK), SessionName, ScheduledStart, ScheduledEnd, IsOnline, Location?, MeetingUrl?, Status, UpdatedAt?, CreatedBy (FK ke Employee), FileSubmission?, EndedSubmissionAt?
17. `SessionInterview`: Id, StageEmployeeId (FK), InterviewSessionId (FK), Status, CreatedAt, UpdatedAt?
18. `InterviewAssessment`: Id, SessionInterviewId (FK), Notes?, Recommendation, Score?, CreatedAt, UpdatedAt?
19. `RescheduleRequest`: Id, InterviewSessionId (FK), RequesterEmail, ProposedStart, ProposedEnd, Reason, Status, ReviewedBy? (FK ke Employee), ReviewedAt?, ReviewNotes?
20. `EmailLog`: Id, SenderEmail, RecipientEmail, Type, Subject, Body, SentAt

### 5.4 Constraint dan indeks tambahan

- Unique `(RecruitmentBatchId, StageName)` pada `BatchStage`.
- Unique `(CandidateId, RecruitmentBatchId)` pada `BatchApplications` (satu kandidat satu kali per batch).
- Unique `(BatchApplicationId, BatchStageId)` pada `ApplicationStage`.
- Unique `(BatchStageId, EmployeeId)` pada `StageEmployee`.
- Unique `(StageEmployeeId, InterviewSessionId)` pada `SessionInterview`.
- Unique `SessionInterviewId` pada `InterviewAssessment` (satu penilaian per pewawancara per sesi).
- `InterviewSession` boleh banyak per `ApplicationStage`, karena reschedule yang disetujui membatalkan sesi lama dan membuat sesi baru.
- Indeks pada FK yang sering difilter: `BatchApplications.RecruitmentBatchId`, `ApplicationStage.BatchApplicationId`, `InterviewSession.ScheduledStart`, `RescheduleRequest.Status`.

### 5.5 Seeder

Seeder dijalankan saat startup di environment Development dan bersifat idempoten (aman dijalankan berulang). Gunakan GUID tetap agar data konsisten di mesin ketiga developer.

- Department: minimal 3, termasuk satu relasi induk dan anak (contoh: Technology dengan anak Engineering; Finance; Human Resources).
- Role: HR, Interviewer.
- Employee: 1 sampai 2 HR, minimal 4 pewawancara dari departemen berbeda, dengan relasi `ManagerId` bertingkat.
- Account: satu untuk tiap HR dan pewawancara. Kata sandi demo diambil dari konfigurasi Development, di-hash saat seeding, dan tidak ditulis polos di kode.
- StageType: Psikotes (Test), Technical & Logical Test (Test), Interview HR (Interview), Interview User (Interview).
- QuestionBank dan Question: satu bank per kombinasi departemen dan tipe tes, masing-masing sekitar 10 soal pilihan ganda dengan skor tiap opsi. Soal psikotes boleh dummy.

## 6. Aturan bisnis yang harus dijaga handler

1. Pendaftaran kandidat: cari `Candidates` berdasarkan email atau nomor telepon; jika ada, pakai data lama, jika tidak buat baru. Tolak jika kandidat sudah terdaftar di batch yang sama. Buat `BatchApplications` dengan `CodeApplication` unik dan status InProcess.
2. Susunan tahap: `BatchStage` dihubungkan berurutan lewat `NextStageId`. Tahap berstatus Inactive dilewati saat menelusuri rantai. Tahap terakhir memiliki `NextStageId` kosong.
3. `ApplicationStage` dibuat saat HR memulai tahap seorang kandidat, bukan sekaligus saat kandidat didaftarkan.
4. Tahap bertipe Test: hanya diizinkan sekali (`IsAttempt`); kirim email berisi tautan GUID kandidat. Submit sebelum `EndedAt`: salin teks soal, teks opsi, dan skor ke `CandidateAnswer`, lalu jumlahkan ke `TotalScore`. Lewat `EndedAt`: tandai Expired tanpa jawaban dan skor, lewat pekerjaan latar (BackgroundService) dan juga saat tautan dibuka.
5. Evaluasi HR: HR menetapkan Prioritized, Considered, atau Rejected beserta `Notes`. `PassingScore` hanya pembanding, keputusan tetap di HR. Rejected: set status aplikasi Rejected dan kirim email penolakan. Lolos: lanjut ke `NextStageId` aktif berikutnya, atau ke hasil akhir jika tidak ada.
6. Penugasan pewawancara per tahap (`StageEmployee`) dimulai berstatus Pending dan email undangan dicatat di `EmailLog`. Jika Declined (dengan `Note`), HR memilih pengganti sampai ada yang Accepted.
7. Penjadwalan: HR membuat `InterviewSession` dan memilih pewawancara yang sudah Accepted lewat `SessionInterview`. Email penugasan ke pewawancara dikirim lebih dulu; email undangan ke kandidat baru dikirim setelah pewawancara mengonfirmasi.
8. Reschedule: permintaan dari pewawancara atau kandidat dicatat di `RescheduleRequest` (status Pending) untuk ditinjau HR. Disetujui: sesi lama menjadi Cancelled dan HR menjadwalkan ulang atau mengganti pewawancara. Ditolak: jadwal tetap; jika pemohon kandidat dan tidak hadir pada jadwal tetap, status tahap dan aplikasi menjadi Dropped (gugur).
9. Take-home test: pewawancara menetapkan `EndedSubmissionAt`; instruksi tugas dikirim lewat email (tercatat di `EmailLog`). Kandidat mengunggah file ke `FileSubmission` sebelum tenggat; setelah tenggat, unggahan ditolak dan take-home ditandai terlambat.
10. Penilaian: tiap pewawancara pada sesi wajib mengisi `InterviewAssessment` (Notes, Recommendation, Score). HR hanya bisa memutuskan kelulusan tahap wawancara setelah semua pewawancara pada sesi itu selesai menilai.
11. Hasil akhir: setelah semua tahap aktif selesai, HR melihat rekap lalu menetapkan `FinalResult`, `Status`, dan `Note`. Diterima: HR mengunggah offering letter (path disimpan di `FileOfferingLetter`) dan sistem mengirim email beserta lampiran. Ditolak: kirim email penolakan akhir.
12. Setiap email yang terkirim dicatat di `EmailLog`.

## 7. API per developer

Prioritas: alur wawancara, lalu rekrutmen dan batch, lalu tes online dan evaluasi, lalu reschedule. Semua endpoint di bawah `/api`.

**Developer 1: alur wawancara dan penilaian**

| Endpoint | Use case |
| --- | --- |
| POST `/batches/{batchId}/stages/{stageId}/interviewers` | AssignInterviewersCommand |
| GET `/interviewer/invitations` dan POST `.../{id}/respond` | GetInvitationsQuery, RespondInvitationCommand |
| POST `/application-stages/{id}/interview-sessions` | ScheduleInterviewSessionCommand |
| POST `/session-interviews/{id}/confirm` | ConfirmSessionCommand (memicu email ke kandidat) |
| GET `/interviewer/dashboard` | GetInterviewerDashboardQuery |
| POST `/interview-sessions/{id}/take-home` | AssignTakeHomeCommand |
| GET `/interview-sessions/{id}/submission` | GetSubmissionQuery |
| POST `/session-interviews/{id}/assessment` | SubmitAssessmentCommand |

**Developer 2: login, batch, kandidat, evaluasi, hasil akhir**

| Endpoint | Use case |
| --- | --- |
| POST `/auth/login`, GET `/auth/me` | LoginCommand, GetCurrentUserQuery |
| POST `/batches`, GET `/batches`, GET `/batches/{id}` | CreateBatchCommand, GetBatchesQuery, GetBatchDetailQuery |
| PUT `/batches/{id}/stages` | ConfigureBatchStagesCommand |
| POST `/batches/{id}/applications`, GET `/batches/{id}/applications` | RegisterCandidatesCommand, GetApplicationsQuery |
| GET `/applications/{id}` | GetApplicationRecapQuery |
| POST `/applications/{id}/stages/start` | StartApplicationStageCommand |
| POST `/application-stages/{id}/evaluate` | EvaluateStageCommand (juga untuk tahap wawancara) |
| PUT `/applications/{id}/final-result` | SetFinalResultCommand |
| POST `/applications/{id}/offering-letter` | UploadOfferingLetterCommand |

**Developer 3: portal kandidat, email, tes online, reschedule**

| Endpoint | Use case |
| --- | --- |
| GET `/portal/{candidateId}/tasks` | GetCandidateTasksQuery |
| GET `/portal/{candidateId}/stages/{id}/test`, POST `.../submit` | GetTestQuery, SubmitTestCommand |
| POST `/portal/{candidateId}/sessions/{id}/confirm` | ConfirmScheduleCommand |
| POST `/portal/{candidateId}/sessions/{id}/submission` | UploadSubmissionCommand |
| POST `/portal/{candidateId}/sessions/{id}/reschedule` dan POST `/reschedule-requests` | RequestRescheduleCommand |
| GET `/reschedule-requests`, POST `.../{id}/review` | GetRescheduleRequestsQuery, ReviewRescheduleCommand |
| (internal) | `IEmailService` + pencatatan `EmailLog`, `ExpireTestsJob` |

Developer 3 menyediakan `IEmailService` lebih dulu (hari 1 sampai 2) agar developer lain bisa memakainya lewat interface tanpa menunggu.

## 8. Keamanan dan otorisasi

- Role HR dan Interviewer lewat JWT. Claim minimal: `sub` (account id), `employeeId`, `role`. Gunakan authorization policy `HrOnly` dan `InterviewerOnly`.
- Pewawancara hanya boleh melihat dan mengubah data pada sesi tempat ia ditugaskan (cek lewat `StageEmployee.EmployeeId` sama dengan `employeeId` pada token), bukan hanya cek role.
- HR adalah approver tertinggi: keputusan kelulusan tahap dan hasil akhir hanya bisa oleh HR.
- Portal kandidat tidak memakai login. Setiap endpoint portal wajib memeriksa bahwa `ApplicationStage` atau sesi yang diminta memang milik `candidateId` pada URL. Pasang rate limiting bawaan (`AddRateLimiter`) pada grup `/portal`.
- DTO soal untuk kandidat tidak boleh memuat `Score` opsi atau petunjuk jawaban yang benar.
- Validasi unggahan file: batasi ekstensi dan ukuran, simpan lewat `IFileStorage` dengan nama acak; file tidak disajikan langsung dari folder publik tanpa pemeriksaan.
- Kata sandi tidak pernah dikembalikan di respons atau ditulis ke log. Rahasia (JWT key, kredensial SMTP) lewat user-secrets atau environment variable, bukan di repo.

## 9. Konfigurasi dan cara kerja tim

- Connection string SQL Server lokal, misalnya `Server=localhost;Database=RecruitmentDb;Trusted_Connection=True;TrustServerCertificate=True`, ditaruh di `appsettings.Development.json` dan dikecualikan dari secret.
- CORS mengizinkan origin frontend (Vite dev server) lewat konfigurasi.
- Cabang: `main` selalu dapat dijalankan; kerja di `feature/<nama>-<modul>`; merge lewat pull request yang ditinjau minimal satu anggota lain.
- Migrasi EF Core adalah titik rawan konflik. Aturan: buat migrasi hanya setelah `git pull` dari `main`; satu perubahan skema per pull request; jika `ModelSnapshot` konflik, hapus migrasi sendiri, ambil snapshot dari `main`, lalu buat ulang. Perubahan skema setelah hari 2 diumumkan ke seluruh tim sebelum di-merge.
- Definisi selesai satu use case: handler, validator, endpoint dengan otorisasi, tampil benar di OpenAPI, dan unit test untuk aturan bisnis kritis (skor otomatis, transisi `NextStageId`, aturan gugur, urutan email wawancara).

## 10. Urutan eksekusi (fase bersama, hari 1 sampai 2)

Berhenti dan minta tinjauan di akhir tiap langkah.

1. Buat solusi, struktur folder bagian 3, paket NuGet, `Program.cs` dasar, koneksi SQL Server, dan konfigurasi Mediator.
2. Tulis 20 entity, enum, dan konfigurasi EF Core sesuai bagian 5, lalu migrasi awal. Tunjukkan skrip SQL hasil migrasi untuk diperiksa di SSMS.
3. Tulis seeder sesuai bagian 5.5 dan verifikasi bahwa data muncul di SSMS.
4. Tulis kerangka CQRS: `Result<T>`, behaviors, exception middleware, JWT, policy, `CurrentUser`. Sertakan satu use case contoh lengkap (disarankan `CreateBatch`) sebagai pola yang ditiru ketiga developer.
5. Tiap developer mulai modulnya sendiri pada cabang terpisah sesuai bagian 7.

## 11. Hal yang perlu dikonfirmasi tim

- Representasi tahap yang dilewati HR: usulan memakai `BatchStage.Status` (Active atau Inactive), selain `IsRequired`. Konfirmasi agar sesuai maksud ERD.
- Handler memakai `AppDbContext` langsung tanpa repository. Konfirmasi dengan mentor bahwa CQRS tanpa lapisan repository dapat diterima.
- Nama kolom setelah perbaikan typo: dianggap `ManagerId`, `Portfolio`, `IsAttempt`, `UpdatedAt`, dan `RecipientEmail`. Sesuaikan jika ERD memakai nama lain.
- Penyimpanan file (CV, FileSubmission, FileOfferingLetter): usulan awal disk lokal lewat `IFileStorage`, sehingga mudah diganti ke penyimpanan cloud nanti.
- Kandidat tidak boleh mengulang tes (mengikuti `IsAttempt` dan unique `(BatchApplicationId, BatchStageId)`). Ubah jika HR perlu membuka tes ulang.
- Kompatibilitas paket `Mediator` dengan .NET 10: periksa README dan catatan rilis versi stabil yang dipilih sebelum langkah 1.
