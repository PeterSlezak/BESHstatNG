# BESHStatNG_Installer — Windows MSI packaging

This project contains the **WiX-based installer packaging** for the supported **Windows Excel-DNA** edition of [BESHStatNG](../README.md). The installer project is maintained in the [GitHub repository](https://github.com/PeterSlezak/BESHstatNG/tree/main/BESHStatNG_Installer), but was **not included in the source ZIP used to prepare these README files**; project-specific WiX file names, target definitions and command-line arguments should therefore be checked against the current online source before publication.

## Role in a Windows release

The installer distributes the built Windows Excel-DNA add-in, including the correct packaged `.xll` payload for the intended Excel architecture. It is not used to ship the experimental Office.js/WebAssembly harness or a native Mac add-in.

A typical release sequence is:

1. Build and validate the Windows add-in and its Core/Common dependencies in the intended configuration(s).
2. Produce and verify the packed Excel-DNA XLL payload(s), including any required DLL packing.
3. Set and reconcile the application and MSI version using the repository's release process and `../Directory.Build.props`.
4. Build the WiX/MSI project using the **WiX toolset and Visual Studio integration required by its actual project version**.
5. Verify installation, upgrade/uninstall behavior, Excel registration/activation and both supported Excel bitness variants as applicable.
6. Apply the established signing and release checks before distributing the MSI.

**Do not assume** that the main `BESHStatNG.sln` builds this separate installer automatically. Check the installer project and the repository's `release_process.txt` for the applicable workflow.

## User installation

Users should download the published **signed MSI** from [beshstat.eu/download](https://beshstat.eu/download/) or [GitHub Releases](https://github.com/PeterSlezak/BESHstatNG/releases), close Excel before installation, and follow the [getting started guide](https://beshstat.eu/beshstatng/help/latest/getting-started/).

**Status:** release packaging for the stable Windows host. This README intentionally does not prescribe an unverified WiX version, source-file list or signing command.
