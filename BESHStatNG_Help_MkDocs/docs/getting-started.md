# Getting started

This page covers installation and the first things to check when **BESHStatNG** does not show up in Excel.

!!! tip
    Need to report a bug or suggest an improvement? Please use the [GitHub issue tracker](https://github.com/PeterSlezak/BESHstatNG/issues) so the problem can be tracked, reproduced, and linked to fixes.

## Install the add-in (MSI)

1. **Close Excel** (and any other Office apps).
2. Run the **BESHStatNG** `.msi` installer.
3. Start Excel and look for the **BESH Stat NG** tab on the ribbon.

!!! success "Code-signed installer"
    The BESHStatNG MSI installer is digitally signed.

    When installing, Windows should show a verified publisher instead of an *Unknown publisher* warning. If you want to check this manually, right-click the downloaded `.msi` file, choose **Properties**, and open the **Digital Signatures** tab.

    Use installers downloaded from the official BESHStat website or from the project’s GitHub releases page.

!!! note "Windows SmartScreen and organization policies"
    Code signing improves installation trust and should reduce SmartScreen / unknown-publisher warnings. However, Windows SmartScreen reputation and corporate endpoint-security policies may still show additional prompts, especially immediately after a new release or in managed environments.

    If installation is blocked by your organization, ask your IT administrator to verify the digital signature and allow the signed BESHStatNG installer.

### Verify the installer signature

To verify the installer before running it:

1. Right-click the downloaded `.msi` file.
2. Choose **Properties**.
3. Open the **Digital Signatures** tab.
4. Select the listed signature and click **Details**.
5. Confirm that Windows reports the signature as valid.

If the **Digital Signatures** tab is missing, the file may not be the signed installer. Download the MSI again from the official BESHStat website or from the GitHub releases page.

## Confirm the add-in loaded

If you don’t see the **BESH Stat NG** ribbon tab:

1. In Excel go to **File → Options → Add-ins**.
2. Check **Active Application Add-ins** for an entry that looks like BESHStatNG.
3. If you see it under **Disabled Application Add-ins**:

    - At the bottom, choose **Manage: Disabled Items → Go…**
    - Re-enable BESHStatNG.

## Excel Trust Center and managed environments

Excel or organization security policies can still block add-ins even when the installer is digitally signed.

- **Trusted Locations**: in **File → Options → Trust Center → Trust Center Settings → Trusted Locations**, add the folder where BESHStatNG is installed.
- **Protected View**: if your workbook opens in Protected View, click **Enable Editing** before running analysis.

!!! note
    BESHStatNG is an **Excel-DNA** `.xll` add-in installed by an MSI. How/where it’s installed depends on your MSI configuration.

## Updating / uninstalling

- To update, typically install the newer MSI **over** the existing version.
- To uninstall, use Windows **Apps & Features / Programs and Features**.

## Logs and error messages

When something fails, BESHStatNG writes a log file and may show an error dialog.

- The main log file is stored next to the add-in in:

    `...\ExtraFiles\Logs\all.log`

If you report an issue, include:

- what method you ran (e.g. *Kruskal‑Wallis*, *Cox regression*)
- what inputs you selected
- the relevant part of `all.log`

