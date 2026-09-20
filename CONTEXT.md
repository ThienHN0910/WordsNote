# WordsNote

An intelligent vocabulary learning and exam preparation platform combining spaced repetition (SRS) flashcards with multi-subject quiz question banks across Web, Extension, and Desktop.

## Language

### Flashcard Learning
**Flashcard Desk**:
A named collection of two-sided vocabulary cards studied via spaced repetition (SRS).
_Avoid_: Deck, category, folder

**Card**:
An individual vocabulary item within a Desk, containing front prompt, back definition, hint, tags, and SRS streak metadata.
_Avoid_: Note, item, word entry

### Quiz & Examination
**Quiz Set**:
A curated bank of multiple-choice questions belonging to a specific course code or subject (e.g. MLN122, PRM393, JFE301, JIT401).
_Avoid_: Test pack, exam paper, quiz category

**Question**:
A single multiple-choice challenge within a Quiz Set, containing question text, selectable options (A, B, C, D), correct answers, and explanation.
_Avoid_: Problem, task, test item

**Practice Session**:
An interactive quiz attempt where a user answers questions from a Quiz Set, reviews immediate explanations, and tracks accuracy.
_Avoid_: Exam run, test drill

### Multi-Client Ecosystem
**Web App**:
The Vue 3 browser client providing complete Flashcard SRS and Quiz Practice experiences.
_Avoid_: Website, portal

**Browser Extension**:
A Manifest V3 extension for Chromium-based browsers (Edge Add-ons, Chrome) enabling quick word capture and study.
_Avoid_: Plugin, addon-script

**Desktop App**:
A packaged .NET 8 WPF application distributed via Microsoft Store (MSIX) for native offline-first learning.
_Avoid_: Windows client, exe-installer
