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

### 3) Default Random Seed

This value defines the **global default pseudo-random seed** used by workflows that rely on randomization, resampling, or stochastic initialization **when no explicit method-level seed is supplied**.

Leave this field **blank** if you want BESHStatNG to use a time-based seed instead.

Valid input:

- any valid **32-bit integer**
- blank = **no deterministic default seed**
- value `0` should be avoided; in the settings normalization logic it is treated as **unset**

### How the seed is resolved

BESHStatNG now follows this precedence:

1. **Explicit method seed**, if the workflow exposes one and the user enters it
2. otherwise **Global Settings → Default Random Seed**
3. otherwise a **time-based seed**

This means the global seed is especially useful for workflows that support stochastic behavior but **do not show a dedicated seed box in their own dialog**.

### Where the global default seed is currently used

At the time of writing, the global default seed is used by:

- **Bland–Altman** bootstrap confidence intervals
- **Deming Regression** bootstrap confidence intervals
- **Lin’s Concordance Correlation Coefficient** bootstrap confidence intervals
- **Cohen’s / Weighted Kappa** bootstrap confidence intervals
- **K-Means Clustering** when the optional method-level seed box is left blank

### Why this setting is useful

Use a global default seed when you want:

- reproducible bootstrap confidence intervals across sessions,
- reproducible stochastic clustering starts,
- consistent results while validating or comparing analyses,
- easier debugging of results that depend on resampling.

### Important interpretation note

A fixed seed does **not** change the statistical method itself.  
It only makes the pseudo-random sequence reproducible.

For example:

- bootstrap percentile limits should be reproducible across runs with the same seed,
- random-start clustering should initialize the same way across runs with the same seed.

!!! example
    If you set **Default Random Seed = 123456789**, then supported bootstrap-based agreement methods and k-means clustering (when no explicit seed is entered there) will produce reproducible pseudo-random behavior across runs and sessions.

## Settings currently shown but not active

Some builds may display controls that are currently disabled, such as **Decimal Places for P-value Presenting**.

These disabled controls are **not active settings** in the current production build and are not used in calculations or output formatting.

## Save behavior

Click **Save** to store the current Global Settings.

The saved values are applied to future dialogs and future BESHStatNG sessions.

## Help button

The **Help** button in the dialog opens this page.

## See also
- [Resampling in BESH Stat NG](methods/resampling.md)
- [Home](index.md)