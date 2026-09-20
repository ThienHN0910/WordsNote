# WordsNote Microsoft Edge Add-ons Submission Guide

Date: 2026-09-20
Target: Microsoft Edge Add-ons Store
Manifest Version: Manifest V3

---

## 1. Extension Package

- **Build & Package Command**:
  ```powershell
  cd src/extension
  npm run package:edge
  ```
- **Output Artifact**:
  `release/microsoft-edge-addon/WordsNote-Edge-Addon-v1.1.2.zip`

This zip archive includes:
- `manifest.json` (V3)
- `background.js` (Service Worker)
- `content.js` (Highlight word grabber)
- `popup.html` & compiled assets (`assets/popup.js`, `assets/popup.css`)
- Icons: 16x16, 48x48, 128x128 PNGs

---

## 2. Microsoft Partner Center Submission Checklist

1. **Sign in to Partner Center**:
   - Access: [Microsoft Partner Center](https://partner.microsoft.com/dashboard/microsoftedge)
   - Register or sign in with your developer account.
2. **Create New Extension**:
   - Click **Create new extension**.
   - Upload the generated zip file: `release/microsoft-edge-addon/WordsNote-Edge-Addon-v1.1.2.zip`.
3. **Store Listing Details**:
   - **Extension Name**: `WordsNote - Vocabulary & Exam Study Assistant`
   - **Short Description**: `Effortlessly save vocabulary while browsing and practice university exam question banks (MLN122, PRM393, JFE301, JIT401).`
   - **Detailed Description**: Highlight key capabilities:
     - Select and save words from any webpage directly to your Flashcard Desks.
     - Spaced repetition (SRS) memory review.
     - Quick practice quizzes and exam questions.
     - Works offline and syncs with cloud.
   - **Category**: Education / Productivity
   - **Languages**: English, Vietnamese
4. **Privacy & Support URLs**:
   - **Privacy Policy URL**: `https://wordsnote.app/privacy-policy` (or your deployed URL)
   - **Support Contact / URL**: `https://github.com/ThienHN0910/WordsNote/issues`
5. **Visual Assets**:
   - Store Icon: 128x128 PNG (from `src/extension/public/icons/icon128.png`).
   - Screenshots: 1280x800 or 640x400 PNG showing popup study and highlight capture.
6. **Submit for Review**:
   - Typical review duration is 24 to 72 hours.
