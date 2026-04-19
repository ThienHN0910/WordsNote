# WordsNote.Package (MSIX)

This project is a Windows Application Packaging Project for publishing `WordsNote.Desktop` to Microsoft Store.

## Before real Store submission

1. Replace placeholder assets in `Assets/` with real images for all required sizes and scale factors.
2. Update `Package.appxmanifest`:
   - `Identity Name`
   - `Publisher`
   - `DisplayName`, `PublisherDisplayName`, `Description`
3. Keep required capabilities only:
   - `internetClient` for cloud sync
   - `runFullTrust` for packaged WPF app
4. Use a temporary test certificate for local sideload tests.
5. For Store submission, Microsoft signs the package with Store certificate automatically.
