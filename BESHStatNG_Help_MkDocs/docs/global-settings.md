# Global Settings

The **Global Settings** dialog contains application-wide defaults and presentation options used across supported BESHStatNG analyses.

The settings are saved between Excel sessions. An individual analysis can still provide its own alpha or random seed; when it does, that method-specific value takes precedence over the global default.

---

## Open the Global Settings dialog

In Excel:

1. Open the **BESH Stat NG** ribbon tab.
2. Click **Settings**.

## Settings at a glance

| Setting | Default | Valid values | Main effect |
|---|---:|---|---|
| **Trace Program Execution** | Selected | On or off | Controls detailed diagnostic logging |
| **Default alpha** | `0.050` | `0.001` to `0.999` | Initializes supported alpha controls and sets the p-value highlighting threshold |
| **Decimal places for p-values** | `8` | `2` to `16` | Controls the displayed precision of marked p-values |
| **Very small p-values** | `< threshold` | `< threshold`, Scientific notation, or Fixed decimal | Controls how p-values below the display threshold are shown |
| **Use > threshold for p-values close to 1** | Selected | On or off | Optionally shortens p-values very close to 1 |
| **Default Random Seed** | Blank | A supported non-zero 32-bit integer, or blank | Controls reproducibility when no method-level seed is supplied |

!!! important
    **Default alpha** affects statistical defaults and p-value highlighting. The three p-value presentation controls affect only how p-values are displayed; they do not change the calculated p-values or the significance threshold.

---

## Trace Program Execution

Select **Trace Program Execution** to include additional diagnostic information in the BESHStatNG log.

Detailed logging is useful when:

- troubleshooting numerical convergence;
- investigating an unexpected result;
- diagnosing a failed analysis;
- preparing a reproducible issue report.

Clearing this option reduces trace-level messages but does not disable ordinary warnings and errors.

If you report a problem, describe the analysis and inputs and include the relevant log entries. See [Getting started](getting-started.md#logs-and-error-messages) for the current log location and troubleshooting guidance.

---

## Default alpha

**Default alpha** is the application-wide significance level used by supported workflows when they need a default alpha value.

- Default: `0.050`
- Valid range: `0.001` to `0.999`
- Displayed with three decimal places in the dialog

The global alpha is used for:

- initializing alpha controls in supported analysis dialogs;
- p-value highlighting in generated result tables;
- the default confidence level in supported procedures, where confidence level is calculated as `1 - alpha`;
- selected decision rules that explicitly use the global significance level.

For example:

| Default alpha | Corresponding two-sided confidence level | Highlighted p-values |
|---:|---:|---:|
| `0.100` | 90% | `p ≤ 0.100` |
| `0.050` | 95% | `p ≤ 0.050` |
| `0.010` | 99% | `p ≤ 0.010` |

!!! note
    An alpha entered directly in an analysis dialog overrides the global default for that analysis. Changing the global value does not alter results that have already been calculated.

### P-value highlighting

In standard BESHStatNG result tables, marked p-values less than or equal to **Default alpha** are highlighted. Highlighting uses the underlying numeric p-value, not its rounded display.

This distinction matters near the threshold. For example, a p-value of `0.04996` can be displayed as `0.050` when three decimal places are selected, but it is still highlighted because the unrounded value is below `0.05`.

---

## P-value presentation

The p-value presentation controls provide one consistent display rule for marked p-values in supported result tables and UDF report outputs.

These settings do **not**:

- recalculate a statistical test;
- round the value used for significance decisions;
- change the selected alpha;
- alter confidence intervals or model estimates.

### Decimal places for p-values

This setting controls fixed-decimal precision and also defines the thresholds used for values very close to 0 or 1.

- Default: `8`
- Valid range: `2` to `16`

If the selected number of decimal places is `d`:

- the lower display threshold is `10^-d`;
- the upper display threshold is `1 - 10^-d`.

Examples:

| Decimal places | Lower threshold | Upper threshold |
|---:|---:|---:|
| `2` | `0.01` | `0.99` |
| `3` | `0.001` | `0.999` |
| `4` | `0.0001` | `0.9999` |
| `8` | `0.00000001` | `0.99999999` |

Ordinary p-values are shown with exactly the selected number of decimal places. For example, with `d = 3`, `p = 0.04273` is displayed as `0.043`.

### Very small p-values

Use **Very small p-values** to choose how values below the lower threshold are presented.

#### `< threshold`

Displays the threshold as an inequality.

With three decimal places:

- `p = 0.0004` is displayed as `<0.001`;
- `p = 0.00001` is also displayed as `<0.001`.

This is usually the clearest choice for compact statistical reports.

!!! important
    `<0.001` does not mean that the p-value equals zero. It means that the calculated value is smaller than the selected display threshold.

#### Scientific notation

Displays values below the threshold in scientific notation.

With three decimal places, `p = 0.0004` is displayed as `4.000E-04` in standard result tables.

Some spilled UDF report tables may pad the exponent with an additional zero, for example `4.000E-004`. The numerical meaning is identical.

Choose this option when the approximate magnitude of a very small p-value is important.

#### Fixed decimal

Uses fixed-decimal formatting for all p-values, including values below the threshold.

With three decimal places, `p = 0.0004` is displayed as `0.000`.

This produces a uniform column but can hide the magnitude of very small values. A displayed value of `0.000` must not be interpreted as an exact zero.

### Use > threshold for p-values close to 1

When this option is selected, p-values above the upper threshold are displayed using `>` notation.

For example, with three decimal places:

- the upper threshold is `0.999`;
- `p = 0.9996` is displayed as `>0.999`.

When the option is cleared, the same value is shown using the selected fixed-decimal precision.

### Combined examples

The following table shows the effect of the controls when **Decimal places for p-values = 3**:

| Calculated p-value | `< threshold` | Scientific notation | Fixed decimal |
|---:|---:|---:|---:|
| `0.04273` | `0.043` | `0.043` | `0.043` |
| `0.0004` | `<0.001` | `4.000E-04`* | `0.000` |
| `0.00001` | `<0.001` | `1.000E-05`* | `0.000` |
| `0.9996` with upper bound selected | `>0.999` | `>0.999` | `>0.999` |

\* Spilled UDF report text may use a three-digit exponent, such as `4.000E-004`.

!!! note
    The examples use a decimal point for clarity. Excel and UDF report text may use your regional decimal separator.

### Numeric values and UDF output

BESHStatNG applies the same presentation rules through two output paths:

- In standard analysis result tables, Excel number formatting is used. The worksheet cells remain numeric and retain full precision.
- In spilled UDF report tables, marked p-value fields may be returned as display text so that values such as `<0.001` can be shown. Standalone UDFs that return a single p-value remain numeric.

If a downstream worksheet calculation needs a numeric p-value, use the numeric result cell from a standard analysis table or the relevant scalar p-value UDF rather than parsing formatted report text.

!!! note
    Only output fields identified as p-values use these rules. Other probabilities, estimates, confidence limits, and ordinary numeric fields are unaffected.

### When changes take effect

New result tables use the saved presentation settings. Existing standard result tables are not reformatted retroactively. If an existing UDF spill still shows the previous format, recalculate the formula after saving the new settings.

---

## Default Random Seed

**Default Random Seed** defines the application-wide pseudo-random seed used by supported stochastic workflows when no explicit method-level seed is supplied.

Leave the field blank to use a time-based seed. This allows the pseudo-random sequence to vary between runs.

For a deterministic global seed, enter a signed 32-bit integer from:

- `-2147483647` to `-1`; or
- `1` to `2147483647`.

The value `0` and the reserved value `-2147483648` represent an unset seed and should not be used as deterministic seeds.

### Seed precedence

BESHStatNG resolves a seed in this order:

1. an explicit seed entered for the current analysis;
2. otherwise **Global Settings → Default Random Seed**;
3. otherwise a time-based seed.

The global seed is therefore most useful when a workflow supports randomization, resampling, or stochastic initialization but no method-specific seed has been supplied.

Current examples include:

- bootstrap confidence intervals in agreement and method-comparison analyses;
- random-start clustering;
- randomized validation or initialization in supported multivariate workflows;
- other procedures that use the shared resampling infrastructure.

### Why use a fixed seed?

A fixed seed helps when you want:

- reproducible bootstrap confidence intervals;
- repeatable random starts;
- consistent results during validation or software comparison;
- easier investigation of a stochastic result.

!!! note
    A seed does not change the statistical method. It controls the pseudo-random sequence used by that method. Reproducibility also requires the same data, options, observation order, and software version.

!!! example
    If **Default Random Seed = 123456789**, supported stochastic workflows use the same pseudo-random sequence whenever no explicit method-level seed overrides it.

For a broader explanation of bootstrap, jackknife, and permutation procedures, see [Resampling in BESH Stat NG](methods/resampling.md).

---

## Save and persistence

Click **Save** to apply the selected values to the current BESHStatNG session and store them for future sessions.

The settings are loaded automatically when the add-in starts. They are stored in `BESHStatNG.settings.xml` in the add-in directory.

If the settings file is missing, BESHStatNG creates it using defaults. If it cannot be read, BESHStatNG records a warning, restores valid defaults, and attempts to recreate the file.

Closing the dialog without clicking **Save** leaves the current settings unchanged.

!!! note
    A non-numeric or unsupported random-seed value cannot be saved. Enter a valid seed or leave the field blank.

## Help button

The **Help** button opens this documentation page.

## See also

- [Getting started](getting-started.md)
- [Resampling in BESH Stat NG](methods/resampling.md)
- [User Defined Functions](udf/index.md)
- [Home](index.md)
