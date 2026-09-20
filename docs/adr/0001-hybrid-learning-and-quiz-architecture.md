# 0001. Hybrid Learning Hub with Quiz Bank Integration and Ponytail Minimalism

Date: 2026-09-20

## Context and Problem Statement
WordsNote was originally designed as a pure Spaced Repetition System (SRS) for vocabulary cards (`DeskDocument` and `CardDocument`). Users need to study university subject question banks (such as MLN122, PRM393, JFE301, JIT401 from MLN122_FE) with multiple-choice questions, timed practice, and explanations, while also deploying the ecosystem to Microsoft Edge Add-ons and Microsoft Store.

## Decision
1. **Hybrid Learning Hub**: Maintain clean separation in the Domain between vocabulary Flashcards (`Desk`, `Card`) and Examination Question Banks (`QuizSet`, `Question`).
2. **Ponytail Minimalism**:
   - Apply YAGNI: Avoid heavy microservices or bloated CMS architectures.
   - Use direct streaming/seeding from the structured JSON files into MongoDB.
   - Reuse existing frontend Vue 3 + Pinia stores and WPF controls rather than adding unnecessary npm/nuget dependencies.
   - Keep quiz practice sessions client-stateless, calculating score and accuracy locally with minimal server-side submission sync.
3. **Distribution**:
   - Deploy Extension to Microsoft Edge Add-ons using the current Manifest V3 standard without changes to core browser APIs.
   - Package Desktop App for Microsoft Store using the existing `WordsNote.Package` (WAP MSIX) project and standard appxmanifest configurations.
