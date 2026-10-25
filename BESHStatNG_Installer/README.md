# BESHStatNG_Installer — Windows distribution and signing

This directory contains the **WiX-based Windows installer** for the supported Excel-DNA BESHStatNG add-in. It packages Windows XLL output and Windows-only setup components. It does **not** build or distribute the Office.js/WebAssembly application, the development Office Ribbon/manifest, a Mac add-in or a LibreOffice extension.

## Project areas

- `ExcelAddInDeploy/` — WiX MSI authoring, localization, packaged Excel add-in files and installer resources.
- `ExcelAddInBundle/` — WiX bundle/Bootstrapper resources and project definitions, maintained separately from the application code.
- `InstallerCA/` — Windows-only custom action code, including close-Excel prompting behavior.
- `Installer.sln` — installer projects/solution; separate from routine portable-library builds.

## Windows release workflow

Build/test the Windows add-in, Core/Common and the applicable x86/x64 packed XLL files first. Use `../Directory.Build.props` as the version source (`BeshVersion` four-part application version and `BeshVersion3` three-part MSI representation). Build the WiX projects with the compatible **Visual Studio 2026/WiX HeatWave** tooling, then validate file inclusion, installation, registration/activation, upgrade, removal and Excel bitness. Follow the established XLL/MSI signing procedure separately and verify signatures on the final artifacts.

Avoid broad file harvesting: the release package should include only intended XLL payloads and explicitly required installer files. Do not add development WASM assets, browser manifests, diagnostic captures or local test logs to the production MSI. Keep generated WiX harvest output deterministic: broad file globs can accidentally include artifacts from nearby build folders.

## For users

Published installers and installation instructions are available at [beshstat.eu/download](https://beshstat.eu/download/) and in [Getting started](https://beshstat.eu/beshstatng/help/latest/getting-started/). Close Excel before installation. The released Windows build, not these installer sources, determines supported features.
