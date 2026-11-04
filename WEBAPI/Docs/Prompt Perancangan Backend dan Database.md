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
| CQRS | MediatR (atau Mediator source generator) dengan pola Command & Query |
| Validasi | FluentValidation lewat pipeline behavior |
| Autentikasi | JWT Bearer untuk HR dan Interviewer; hash password dengan `PasswordHasher<Account>` bawaan ASP.NET Core |
| Email | MailKit lewat interface `IEmailService`; untuk dev pakai smtp4dev atau Papercut |
| Dokumentasi API | OpenAPI bawaan .NET 10 dengan UI Scalar atau Swagger |
| Test | xUnit untuk logika inti (skor otomatis, transisi tahap, aturan gugur) |

Rujukan gaya kode: repo HRIS-Practice (controller, DTO, JWT, password hashing, Repository pattern).

## 3. Struktur repo backend

Backend menggunakan satu project Web API dengan struktur folder yang saat ini ada di codebase:

```
WEBAPI/
  Controllers/
  Data/
    Configurations/   (IEntityTypeConfiguration<T> per entitas)
    Repositories/     (IRepository<T>, Repository<T>, repository khusus)
    RmsDbContext.cs
  Entities/
    Enums/           (Definisi enum & helper parsing/metode dalam kelas terpadu)
    BaseEntity.cs
    (20 class entitas)
  Features/          (Use cases CQRS: Commands, Queries, Handlers, Validators, DTOs)
  Common/            (Result<T>, Behaviors, Exceptions, ExceptionHandlers)
  Services/
  Extensions/
  Program.cs
```

## 4. Aturan CQRS & Repository

- Satu use case, satu command atau query. Command mengubah data, query hanya membaca.
- Controller tipis: menerima request, memanggil dispatcher (MediatR), memetakan hasil ke HTTP. Tidak ada logika bisnis di controller.
- Handler menggunakan **Repository Pattern** (`IRepository<T>` dan repositori khusus jika diperlukan) serta `RmsDbContext` sesuai kebutuhan data layer.
- Query memakai `AsNoTracking()` dan proyeksi `Select` ke DTO, tidak mengembalikan entity langsung ke client.
- Command mengembalikan `Result<T>` (sukses atau galat terstruktur), bukan melempar exception untuk kegagalan bisnis yang wajar. Exception handler menerjemahkannya ke `ProblemDetails`.
- Pipeline behavior: `ValidationBehavior` (FluentValidation), `LoggingBehavior`. Satu `SaveChangesAsync` per command; gunakan transaksi eksplisit hanya jika command menulis lewat lebih dari satu langkah yang tidak bisa digabung.
- Efek samping (kirim email) dilakukan setelah data berhasil disimpan, lewat `IEmailService`. Jangan memanggil SMTP di dalam transaksi database.
- Entity tidak keluar dari handler. DTO request dan response dipisah dari entity.

## 5. Database

### 5.1 Konvensi

- Semua PK bertipe GUID menggunakan `Guid.CreateVersion7()` (termasuk `Candidates.Id` disamakan, tidak dibedakan).
- Semua waktu disimpan UTC dan ditampilkan sebagai WIB (Asia/Jakarta) di sisi tampilan.
- Semua FK memakai `DeleteBehavior.Restrict` (menghindari galat multiple cascade paths di SQL Server).
- Kolom audit `CreatedBy` dan `ReviewedBy` disimpan sebagai `Guid` / `Guid?` biasa tanpa Foreign Key ke tabel Employee, karena akan diisi langsung melalui business logic di backend.
- Enum disimpan dalam bentuk **`int`** di database, dan dikelola/diparsing di backend melalui kelas terpadu dengan berbagai method pembantu (display name, validasi transisi, parsing, list opsi).
- `decimal` untuk skor dan bobot memakai presisi `decimal(18,2)`; `GPA` memakai `decimal(3,2)`; `ExpectedSalary` memakai `decimal(18,2)`.
- Satu `IEntityTypeConfiguration<T>` per entity. Nama kolom dan tipe data mengikuti ERD drawio terbaru (`Diagram Interviewer.drawio.xml`).

### 5.2 Enum & Metode Pembantu (Disimpan sebagai INT di Database)

Enum disimpan sebagai integer di database dan disediakan method ekstensi/helper dalam satu kelas (misal: validasi, transisi status, display name ke string/Bahasa Indonesia):

1. **Gender**: `Male = 1`, `Female = 2`
2. **StageCategory**: `Test = 1`, `Interview = 2`
3. **QuestionType**: `SingleChoice = 1`, `MultipleChoice = 2`
4. **BatchStatus**: `Draft = 1`, `Open = 2`, `Closed = 3`
5. **OfferingLetterStatus**: `Pending = 1`, `Approved = 2`, `Rejected = 3`, `Negotiate = 4`
6. **ApplicationStatus**: `InProcess = 1`, `Rejected = 2`, `Dropped = 3` (gugur), `Accepted = 4`
7. **FinalResultStatus**: `Accepted = 1`, `Rejected = 2` (status kelulusan akhir kandidat di batch application)
8. **ApplicationStageStatus**: `InProgress = 1`, `Submitted = 2`, `Expired = 3`, `Prioritized = 4`, `Considered = 5`, `Rejected = 6`, `Dropped = 7`
9. **StageEmployeeStatus**: `Pending = 1`, `Accepted = 2`, `Declined = 3`
10. **SessionInterviewStatus**: `InProgress = 1`, `Completed = 2`, `Cancelled = 3`
11. **InterviewSessionStatus**: `Scheduled = 1`, `PendingConfirmation = 2`, `Rescheduled = 3`, `AcceptedByInterviewer = 4`, `AcceptedByCandidate = 5`
12. **Recommendation**: `StronglyRecommend = 1`, `Recommend = 2`, `Consider = 3`, `NotRecommend = 4`
13. **RescheduleStatus**: `Pending = 1`, `Approved = 2`, `Rejected = 3`
14. **EmailType**: `StageInvitation = 1`, `InterviewerInvitation = 2`, `InterviewInvitation = 3`, `RescheduleRequest = 4`, `RescheduleResult = 5`, `TakeHome = 6`, `Rejection = 7`, `FinalResult = 8` (dinamis dapat diperluas)

### 5.3 Tabel (20) Berdasarkan ERD Drawio Terbaru

Tanda `?` berarti nullable. Kolom `CreatedAt` wajib dan `UpdatedAt` nullable di semua tabel yang memilikinya.

1. `Department`: Id, ParentDepartmentId? (FK), Code (unique), Name, Description?, CreatedAt, UpdatedAt?
2. `Role`: Id, Name (unique), Description?, IsActive (default true), CreatedAt, UpdatedAt?
3. `Employee`: Id, ManagerId? (FK), DepartmentId (FK), EmployeeNumber, FullName, Email (unique), Gender (int enum), JobTitle, IsActive (default true), CreatedAt, UpdatedAt?
4. `Account`: Id, EmployeeId (FK, unique), RoleId (FK), Username (unique), Password (hash), IsActive, LastLoginAt?, CreatedAt, UpdatedAt?
5. `StageType`: Id, Name, Description?, StageCategory (int enum), IsActive, CreatedAt, UpdatedAt?
6. `QuestionBank`: Id, DepartmentId (FK), StageTypeId (FK), CreatedBy (Guid), CreatedAt, UpdatedAt?
7. `Question`: Id, QuestionBankId (FK), QuestionText, QuestionType (int enum), Weight, SequenceNumber, IsRequired, IsActive, CreatedAt, UpdatedAt?
8. `QuestionOptions`: Id, QuestionId (FK), OptionLabel, OptionText, Score, SequenceNumber, IsActive, CreatedAt, UpdatedAt?
9. `RecruitmentBatch`: Id, DepartmentId (FK), BatchCode (unique), Name, Description, StartDate, EndDate, Status (int enum), CreatedBy (Guid), CreatedAt, UpdatedAt?
10. `BatchStage`: Id, NextStageId? (FK), RecruitmentBatchId (FK), StageTypeId (FK), StageName (unique per batch), IsActive (bool), PassingScore?, StartDate, EndDate, CreatedAt, UpdatedAt?
11. `Candidates`: Id, FullName, Email (unique), Gender (int enum), PhoneNumber (unique), BirthDate, CV, ExpectedSalary?, Portfolio?, Education, Institution, Address, GPA, CreatedAt, UpdatedAt?
12. `BatchApplications`: Id, CandidateId (FK), RecruitmentBatchId (FK), CodeApplication (unique), AppliedAt, FinalResultStatus? (int enum), Status (int enum), Note?, OfferingLetterStatus? (int enum), FileOfferingLetter?, CreatedAt, UpdatedAt?
13. `ApplicationStage`: Id, BatchApplicationId (FK), BatchStageId (FK), Status (int enum), TotalScore?, StartedAt, EndedAt, CompletedAt?, Notes?, IsAttempts (default false), DurationTest?, CreatedAt, UpdatedAt?
14. `CandidateAnswer`: Id, ApplicationStageId (FK), QuestionText, QuestionOptionText, Score, CreatedAt
15. `StageEmployee`: Id, BatchStageId (FK), EmployeeId (FK), Status (int enum), IsActive, Note?, CreatedAt, UpdatedAt?
16. `InterviewSession`: Id, ApplicationStageId (FK), SessionName, ScheduledStart, ScheduledEnd, IsOnline, Location?, MeetingUrl?, Status (int enum), FileSubmission?, EndedSubmissionAt?, CreatedBy (Guid), CreatedAt, UpdatedAt?
17. `SessionInterview`: Id, StageEmployeeId (FK), InterviewSessionId (FK), Status (int enum), CreatedAt, UpdatedAt?
18. `InterviewAssessment`: Id, SessionInterviewId (FK, unique), Notes?, Recommendation (int enum), Score?, CreatedAt, UpdatedAt?
19. `RescheduleRequest`: Id, InterviewSessionId (FK), RequesterEmail, ProposedStart, ProposedEnd, Reason, Status (int enum / varchar), ReviewedBy? (Guid), ReviewedAt?, ReviewNotes?, CreatedAt, UpdatedAt?
20. `EmailLog`: Id, SenderEmail, RecipientEmail, Type (int enum), Subject, Body, SentAt

### 5.4 Constraint dan indeks tambahan

- Unique `(RecruitmentBatchId, StageName)` pada `BatchStage`.
- Unique `(CandidateId, RecruitmentBatchId)` pada `BatchApplications` (satu kandidat satu kali per batch).
- Unique `(BatchApplicationId, BatchStageId)` pada `ApplicationStage`.
- Unique `(BatchStageId, EmployeeId)` pada `StageEmployee`.
- Unique `(StageEmployeeId, InterviewSessionId)` pada `SessionInterview`.\n- Unique `SessionInterviewId` pada `InterviewAssessment` (satu penilaian per pewawancara per sesi).
- `InterviewSession` boleh banyak per `ApplicationStage` (karena reschedule yang disetujui membatalkan sesi lama dan membuat sesi baru).
- Indeks pada FK yang sering difilter: `BatchApplications.RecruitmentBatchId`, `ApplicationStage.BatchApplicationId`, `InterviewSession.ScheduledStart`, `RescheduleRequest.Status`.

### 5.5 Seeder

Seeder dijalankan saat startup di environment Development dan bersifat idempoten (aman dijalankan berulang). Gunakan GUID tetap agar data konsisten di mesin ketiga developer.

- Department: minimal 3, termasuk satu relasi induk dan anak (contoh: Technology dengan anak Engineering; Finance; Human Resources).
- Role: HR, Interviewer.
- Employee: 1 sampai 2 HR, minimal 4 pewawancara dari departemen berbeda, dengan relasi `ManagerId` bertingkat.\n- Account: satu untuk tiap HR dan pewawancara. Kata sandi demo di-hash saat seeding.
- StageType: Psikotes (Test), Technical & Logical Test (Test), Interview HR (Interview), Interview User (Interview).
- QuestionBank dan Question: satu bank per kombinasi departemen dan tipe tes, masing-masing sekitar 10 soal pilihan ganda dengan skor tiap opsi.

## 6. Aturan bisnis yang harus dijaga handler

1. Pendaftaran kandidat: cari `Candidates` berdasarkan email atau nomor telepon; jika ada, pakai data lama, jika tidak buat baru. Tolak jika kandidat sudah terdaftar di batch yang sama. Buat `BatchApplications` dengan `CodeApplication` unik dan status InProcess.
2. Susunan tahap: `BatchStage` dihubungkan berurutan lewat `NextStageId`. Tahap berstatus `IsActive == false` dilewati saat menelusuri rantai. Tahap terakhir memiliki `NextStageId` kosong.
3. `ApplicationStage` dibuat saat HR memulai tahap seorang kandidat, bukan sekaligus saat kandidat didaftarkan.
4. Tahap bertipe Test: hanya diizinkan sekali (`IsAttempts`); kirim email berisi tautan GUID kandidat. Submit sebelum `EndedAt`: salin snapshot soal & opsi ke `CandidateAnswer`, lalu hitung `TotalScore`. Lewat `EndedAt`: tandai Expired tanpa jawaban dan skor.
5. Evaluasi HR: HR menetapkan Prioritized, Considered, atau Rejected beserta `Notes`. `PassingScore` hanya pembanding, keputusan tetap di HR. Rejected: set status aplikasi Rejected dan kirim email penolakan. Lolos: lanjut ke `NextStageId` aktif berikutnya, atau ke hasil akhir jika tidak ada.
6. Penugasan pewawancara per tahap (`StageEmployee`) dimulai berstatus Pending dan email undangan dicatat di `EmailLog`. Jika Declined (dengan `Note`), HR memilih pengganti sampai ada yang Accepted.
7. Penjadwalan: HR membuat `InterviewSession` dan memilih pewawancara yang sudah Accepted lewat `SessionInterview`. Email penugasan ke pewawancara dikirim lebih dulu; email undangan ke kandidat baru dikirim setelah pewawancara mengonfirmasi.
8. Reschedule: permintaan dari pewawancara atau kandidat dicatat di `RescheduleRequest` (status Pending) untuk ditinjau HR. Disetujui: sesi lama menjadi Cancelled/Rescheduled dan HR menjadwalkan ulang. Ditolak: jadwal tetap; jika pemohon kandidat dan tidak hadir, status menjadi Dropped.
9. Take-home test: pewawancara menetapkan `EndedSubmissionAt`; instruksi tugas dikirim lewat email (tercatat di `EmailLog`). Kandidat mengunggah file ke `FileSubmission` sebelum tenggat.
10. Penilaian: tiap pewawancara pada sesi wajib mengisi `InterviewAssessment` (Notes, Recommendation, Score). HR hanya bisa memutuskan kelulusan tahap wawancara setelah semua pewawancara pada sesi itu selesai menilai.
11. Hasil akhir: setelah semua tahap aktif selesai, HR melihat rekap lalu menetapkan `FinalResultStatus`, `Status`, `OfferingLetterStatus`, dan `Note`. Diterima: HR mengunggah offering letter (`FileOfferingLetter`) dan sistem mengirim email beserta lampiran. Ditolak: kirim email penolakan akhir.
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

- Role HR dan Interviewer lewat JWT. Claim minimal: `sub` (account id), `employeeId`, `role`. Kebijakan otorisasi: `HrOnly` dan `InterviewerOnly`.
- Pewawancara hanya boleh melihat dan mengubah data pada sesi tempat ia ditugaskan (`StageEmployee.EmployeeId == employeeId` pada token).
- HR adalah approver tertinggi untuk kelulusan tahap dan hasil akhir.
- Portal kandidat tidak memakai login, melainkan verifikasi GUID kepemilikan data. Rate limiting dipasang pada `/portal`.
- DTO soal untuk kandidat tidak boleh memuat `Score` opsi atau kunci jawaban.
- Validasi unggahan file: batasi format, ukuran, simpan lewat `IFileStorage` dengan nama acak aman.

## 9. Konfigurasi dan cara kerja tim

- Connection string SQL Server lokal ditaruh di `appsettings.Development.json` dan dikecualikan dari repositori publik.
- CORS mengizinkan origin frontend dev server.
- Git workflow: `main` selalu stabil; fitur dikerjakan di `feature/<nama>-<modul>`; merge via PR dengan review.
- Definisi selesai: handler (dengan repository), validator, endpoint berotorisasi, OpenAPI, dan unit test.

## 10. Urutan eksekusi

1. Buat enum terpusat dalam satu class dengan berbagai method bantuan (parsing, display name, validasi, transisi status).
2. Tinjau dan perbarui entitas, konfigurasi EF Core (enum disimpan sebagai integer), dan buat migrasi skema database.
3. Buat seeder data awal idempoten.
4. Buat kerangka CQRS + Repository, behaviors, middleware, JWT, and use case percontohan.
5. Pengerjaan use case per developer.

## 11. Kesepakatan Tim & Penyesuaian ERD

- **Repository**: Handler CQRS tetap menggunakan layer Repository (`IRepository<T>` dan repositori khusus) untuk akses data.
- **Primary Key**: Semua PK berjenis GUID diseragamkan memakai `Guid.CreateVersion7()`, termasuk `Candidates.Id`.
- **CreatedBy & ReviewedBy**: Kolom audit `CreatedBy` dan `ReviewedBy` disimpan sebagai `Guid` / `Guid?` mandiri tanpa foreign key ke tabel `Employee`, dan diisi langsung melalui logika backend.
- **Penyimpanan Enum**: Disimpan dalam bentuk `int` pada database, lalu disediakan method helper/ekstensi dalam satu class terpadu.
- **Perubahan Atribut ERD**: Sesuai file ERD drawio terbaru, terdapat penambahan kolom `ExpectedSalary` dan `Gender` pada `Candidates`, `OfferingLetterStatus` dan `FinalResultStatus` (enum int) pada `BatchApplications`, penyesuaian status pada `BatchStage` (menjadi `IsActive`), tipe `enum` pada `EmailLog.Type`, dsb.
