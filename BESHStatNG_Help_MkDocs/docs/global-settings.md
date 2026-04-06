# Global Settings

This page documents the **Global Settings** dialog in BESHStatNG.

Use this dialog to configure application-wide settings that affect multiple analyses.

## Open the Global Settings dialog

In Excel:

1. Open the **BESH Stat NG** ribbon tab.
2. Click **Settings**.

## Currently available global settings

### 1) Trace Program Execution

This option enables additional diagnostic logging during statistical computations.

When enabled, BESHStatNG writes more detailed execution information to the application log. This is mainly useful for:

- troubleshooting numerical-convergence issues,
- investigating unexpected results,
- diagnosing failures together with the log file.

!!! note
    In the current production implementation, this setting is **saved** together with the other global settings and is restored in future BESHStatNG sessions.

### 2) Default Alpha

This value defines the default two-sided significance level used to initialize alpha controls in supported analysis dialogs.

Default value:

- `0.050` → 95% confidence level

Valid range in the UI:

- `0.001` to `0.999`

This default alpha is currently used for:

- initializing the alpha control in supported forms,
- p-value highlighting in result tables,
- the Box’s M equal-covariance decision threshold,
- similar default significance-threshold decisions that follow the global alpha setting.

!!! example
    If you set **Default Alpha = 0.10**, then supported dialogs will open with a default **90% confidence interval** instead of 95%.

## Settings currently shown but not active

Some builds may display controls that are currently disabled, such as **Decimal Places for P-value Presenting**.

These disabled controls are **not active settings** in the current production build and are not used in calculations or output formatting.

## Save behavior

Click **Save** to store the current Global Settings.

The saved values are applied to future dialogs and future BESHStatNG sessions.

## Help button

The **Help** button in the dialog opens this page.
