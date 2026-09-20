# Spec: Admin Quiz Sets Management & Learnspace Data Enrichment

## 1. Overview & Objectives
Enrich the WordsNote Quiz system by:
1. Scanning subject codes found in `E:\learnspace` (e.g. `PRN232`, `ITE302c`, `HCM202`) and importing comprehensive, high-quality question banks (FE/PE exam banks) into the system catalog and MongoDB question collection.
2. Expanding the Admin Portal (`AdminModal.vue` + `QuizSetsController.cs`) with full CRUD and Lock/Unlock status toggles (`isRestricted`) for Quiz Sets, restricted strictly to authenticated Admin accounts (`ADMIN_EMAIL=hnt.vn.vn@gmail.com`).
3. Providing an ingestion script/workflow to map external question sources (FuExam, Albazzz, Quizlet formats) to WordsNote's standardized question schema (`QuestionDocument`).

---

## 2. Learnspace Subject Discovery & Data Mapping
### 2.1 Subject Codes Identified in `E:\learnspace`
From `E:\learnspace`, curriculum folders include:
- `PRN232`: Lập trình .NET & Web API (.NET 8, ASP.NET Core Web API, EF Core, Microservices, Clean Architecture, PE/FE questions)
- `ITE302c`: Ethics in Information Technology (12 full FE exam sets SP24->SP26, ethics frameworks, copyright, security)
- `HCM202`: Tư tưởng Hồ Chí Minh (15 FE exam sets SU24->SU26)

### 2.2 Standard Data Schema
Each question maps to the existing WordsNote `QuestionDocument`:
```json
{
  "id": "prn232-1",
  "subjectId": "prn232",
  "questionNumber": 1,
  "question": "What is ASP .NET Core Identity?",
  "options": {
    "A": "A simple interface for generating unique IDs.",
    "B": "A membership system that provides services for user authentication...",
    "C": "...",
    "D": "..."
  },
  "answers": ["B"],
  "choose": 1,
  "explanation": "Chi tiết giải thích...",
  "source": "FuExam / albazzz / quizlet",
  "exam": "SP26-FE"
}
```

### 2.3 Subject Catalog Registration (`catalog.json` & MongoDB)
- `PRN232`: Color `#0284c7`, restricted: `false`
- `ITE302c`: Color `#e11d48`, restricted: `false`
- `HCM202`: Color `#d97706`, restricted: `false`
- Existing: `mln122` (`#2563eb`), `prm393` (`#7c3aed`), `jfe301` (`#059669`, restricted: `true`), `jit401` (`#dc2626`, restricted: `true`).

---

## 3. Backend Admin Quiz Sets API (`QuizSetsController.cs`)

All Admin endpoints require `isAdmin == true` validated strictly via `GetCurrentUserContextAsync()` (matching JWT claims with `ADMIN_EMAIL=hnt.vn.vn@gmail.com` or role `Admin`).

### 3.1 Endpoints
1. `GET /api/admin/quiz-sets`
   - Returns full list of `QuizSetDocument` with `Id`, `Code`, `Title`, `Description`, `Color`, `TotalQuestions`, `IsRestricted`, `UpdatedAt`.
2. `POST /api/admin/quiz-sets`
   - Request body: `{ id: string, code: string, title: string, description?: string, color?: string, isRestricted?: boolean }`
   - Validation: Unique `id`, non-empty `code` and `title`.
   - Action: Inserts new `QuizSetDocument` into `wordsnote_quiz_sets`.
3. `PUT /api/admin/quiz-sets/{id}`
   - Request body: `{ code?: string, title?: string, description?: string, color?: string, isRestricted?: boolean }`
   - Action: Updates matching fields and `UpdatedAt = DateTime.UtcNow`.
4. `PATCH /api/admin/quiz-sets/{id}/restriction`
   - Request body: `{ isRestricted: boolean }`
   - Action: Updates `IsRestricted` and `UpdatedAt`.
5. `DELETE /api/admin/quiz-sets/{id}`
   - Action: Deletes quiz set from `wordsnote_quiz_sets` and cascades delete of questions in `wordsnote_questions` where `SubjectId == id`.

### 3.2 Catalog Synchronization Hygiene
Ensure `SyncCatalogMetadataAsync()` does not overwrite an admin's custom `IsRestricted` or `Title` changes in MongoDB. It only inserts newly added subjects from `catalog.json` if they do not already exist in MongoDB.

---

## 4. Frontend Admin UI (`AdminModal.vue` + `QuizAPI.ts`)

### 4.1 Tab 3: "📚 Quản lý Bộ môn (Quiz Sets)"
- Tab counter showing `quizSets.length`.
- Header with:
  - "+ Thêm môn học mới" button.
  - "Làm mới" button.
- Responsive Data Table / Cards:
  - **Mã môn**: Code badge with subject color.
  - **Tên môn học**: Title + short description.
  - **Số câu hỏi**: Total questions count badge.
  - **Trạng thái**: Switch toggle / interactive pill:
    - `🔒 Khóa VIP` (isRestricted == true)
    - `🌐 Mở tự do` (isRestricted == false)
    - Clicking toggles immediate status via `PATCH /api/admin/quiz-sets/{id}/restriction`.
  - **Thao tác**:
    - Nút "Sửa" -> Opens Edit Quiz Set modal.
    - Nút "Xóa" -> Confirmation dialog -> calls `DELETE /api/admin/quiz-sets/{id}`.
- Modal Form for Create / Edit Quiz Set:
  - Fields: `ID` (read-only in edit mode), `Mã môn (Code)`, `Tên môn (Title)`, `Mô tả`, `Màu sắc` (color picker), `Khóa VIP` (toggle checkbox).

---

## 5. Quality & Verification Gates
1. Unit tests in `FeatureFusion.Tests`:
   - Admin authorization check (403 for non-admins).
   - Create, Update, Toggle restriction, and Delete Quiz Sets.
2. Frontend build verification (`npm run build`).
3. Pre-publish packaging (`dotnet publish -c Release`).
4. Single-publish deployment coordination:
   - Ask user to publish backend once after all code is ready and locally verified.
   - Run end-to-end verification via Chrome DevTools MCP.
