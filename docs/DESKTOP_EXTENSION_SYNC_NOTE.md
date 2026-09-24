# Ghi Chú Đồng Bộ Ứng Dụng Desktop & Browser Extension (WordsNote)

> **Thời điểm cập nhật:** Kể từ commit cấu hình domain mới & tái thiết kế giao diện (Tháng 9/2026)  
> **Trạng thái:** ⚠️ **TẠM THỜI CHƯA ĐỒNG BỘ BẢN BUILD MỚI TRÊN STORE**

---

## 1. Bối cảnh & Lý do chưa xuất bản bản build mới ngay

1. **Microsoft Store Review đang diễn ra:**
   - Phiên bản ứng dụng **Desktop App (.NET 8 WPF MSIX v1.1.3.0)** và **Microsoft Edge Add-ons** hiện đang trong quá trình xét duyệt của hội đồng kiểm duyệt Microsoft Store (Partner Center).
   - Việc đẩy bản build mới hoặc thay đổi mã định danh gói (Package Identity) trong thời gian này có thể gây xung đột hàng đợi chứng nhận (certification queue collision) hoặc khiến phiên bản đang đợi duyệt bị hủy (superseded/failed).
2. **Quy định triển khai:**
   - **Web App:** Đã cập nhật và hoạt động chính thức trên tên miền mới `https://words-note.thienhn.io.vn/`.
   - **Backend API:** Đã cấu hình mở rộng CORS (`AllowVue`), cho phép tên miền `https://words-note.thienhn.io.vn`, `https://words-note-five.vercel.app`, cũng như các cổng phát triển local.
   - **Desktop & Extension:** Tạm thời giữ nguyên mã nguồn và đóng gói hiện tại. Tài liệu này đóng vai trò là kim chỉ nam (migration playbook) để tiến hành đồng bộ ngay sau khi Microsoft hoàn tất phê duyệt.

---

## 2. Thông tin môi trường & Tên miền mới

| Thành phần | Địa chỉ cũ / Hiện tại | Địa chỉ mới (Kể từ commit này) | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Frontend Web** | `https://words-note-five.vercel.app` | `https://words-note.thienhn.io.vn/` | Đã cấu hình CORS & Rewrite proxy `/api` |
| **Portfolio Showcase** | `https://thienhn.io.vn/projects/words-note` | `https://thienhn.io.vn/projects/words-note` | Cập nhật link điều hướng về domain mới |
| **Backend API** | `http://words-note.runasp.net` | `http://words-note.runasp.net` | Hỗ trợ CORS cho domain mới |

---

## 3. Checklist cập nhật Desktop App (Thực hiện SAU KHI Microsoft duyệt v1.1.3.0)

Sau khi nhận email thông báo **"Certification Passed & Published"** từ Microsoft Partner Center cho bản 1.1.3.0:

- [ ] **Bước 1: Tăng số hiệu phiên bản (Version Bump):**
  - Cập nhật `src/desktop/WordsNote.Package/Package.appxmanifest`:
    ```xml
    <Identity Name="..." Publisher="..." Version="1.1.4.0" ProcessorArchitecture="x64" />
    ```
- [ ] **Bước 2: Cập nhật cấu hình URL & Showcase:**
  - File `src/desktop/WordsNote.Desktop/appsettings.json`:
    - Giữ `ApiBaseUrl`: `http://words-note.runasp.net`
  - File `src/desktop/WordsNote.Desktop/ViewModels/MainViewModel.cs`:
    - Kiểm tra các liên kết mở trình duyệt (Web Sync / Help / Privacy Policy) trỏ về `https://words-note.thienhn.io.vn/`.
- [ ] **Bước 3: Đóng gói MSIX Bundle & Kiểm định WACK:**
  - Build cấu hình `Release | x64`
  - Chạy Windows App Certification Kit (WACK) xác nhận 0 lỗi (zero failure).
  - Tải file `.msixbundle` / `.msixupload` lên Microsoft Partner Center.

---

## 4. Checklist cập nhật Browser Extension (Edge & Chrome)

- [ ] **Bước 1: Tăng version trong `package.json` và `manifest.json`:**
  - `src/extension/package.json`: Tăng lên `1.1.4`
  - `src/extension/public/manifest.json`: Tăng `version` lên `1.1.4`
- [ ] **Bước 2: Kiểm tra liên kết Web & API:**
  - File `src/extension/src/services/remoteStudy.js`:
    - Đảm bảo kết nối API an toàn.
  - File `src/extension/src/popup/components/DailyCards.vue` & `Header.vue`:
    - Đổi link "Mở ứng dụng web" sang `https://words-note.thienhn.io.vn/`.
- [ ] **Bước 3: Đóng gói và submit:**
  - Chạy `npm run build` trong `src/extension/`
  - Nén thư mục `dist/` thành file zip
  - Tải lên Microsoft Edge Add-ons Developer Portal & Chrome Web Store Developer Dashboard.

---

## 5. Tổng kết

Mọi tính năng mới trên Web Frontend (Giao diện chuẩn Taste Skill, Dark/Light mode tối ưu, sửa lỗi đồng bộ Google Login, khắc phục CORS với `words-note.thienhn.io.vn`) đã hoàn thiện và hoạt động độc lập mà không làm ảnh hưởng đến quá trình phê duyệt ứng dụng Desktop của Microsoft Store.
