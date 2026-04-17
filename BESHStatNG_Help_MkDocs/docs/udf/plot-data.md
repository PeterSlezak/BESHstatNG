# Plot Data UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Histogram](../methods/histogram.md)
- [Roc Curve](../methods/roc-curve.md)

## BESH.PLOT.HIST_BINS

Returns histogram bin midpoints and frequencies for one or more numeric input columns.

**Function wizard:** Histogram bin midpoints and frequencies for one or more numeric series.

### Syntax

`=BESH.PLOT.HIST_BINS(data, binRule)`

### Parameters

- **data** — One numeric column or a multi-column numeric range. When multiple columns are supplied, each column is
treated as a separate series. The first row may contain headers, which are used as group names.
- **binRule** — Optional binning rule. Accepted values include `sturges`, `doane`, `scott`, and
`freedman-diaconis`. The default is `sturges`.

### Returns

A spill range with columns:

- `Group` — source column or series name.
- `BinMidpoint` — midpoint of the bin.
- `Frequency` — number of observations falling in the bin.

### Notes

This function is useful when you want to build a histogram with native Excel charts while keeping the binning
logic aligned with the GUI histogram feature. The returned table is already in long format, which makes it easy
to filter by group or build separate series from the same spill range.

Each input column is processed independently using the requested automatic bin-width rule.

## BESH.PLOT.HIST_NORMAL

Returns the coordinates of the normal overlay curve corresponding to the GUI histogram overlay.

**Function wizard:** Normal-overlay coordinates matched to the GUI histogram frequency scale.

### Syntax

`=BESH.PLOT.HIST_NORMAL(data, binRule)`

### Parameters

- **data** — One numeric column or a multi-column numeric range. Each column is treated as a separate series.
The first row may contain headers, which are used as group names.
- **binRule** — Optional automatic binning rule used to determine the effective bin width for scaling the overlay to histogram frequency units.
Accepted values include `sturges`, `doane`, `scott`, and `freedman-diaconis`.

### Returns

A spill range with columns:

- `Group` — source column or series name.
- `X` — x-coordinate of the overlay curve.
- `NormalFrequency` — expected histogram-scale frequency for the fitted normal curve.

### Notes

The overlay is not a probability density on a unit area scale. Instead, it is scaled to the histogram frequency axis,
matching the GUI histogram overlay so that the curve can be drawn directly on top of the histogram bars.

The fitted curve uses the same quartile-based center and spread approximation as the GUI histogram overlay.

## BESH.PLOT.KM_CURVE

Returns step-ready Kaplan–Meier survival-curve coordinates for one or more groups.

**Function wizard:** Step-ready Kaplan-Meier survival-curve coordinates with optional confidence limits.

### Syntax

`=BESH.PLOT.KM_CURVE(time, status, group, alpha)`

### Parameters

- **time** — Single-column follow-up times. Values must be non-negative and aligned row-by-row with `status`.
The first cell may be a header.
- **status** — Single-column event indicator containing 1 for event and 0 for censoring. The first cell may be a header.
- **group** — Optional single-column group labels. When omitted, all observations are treated as one group.
- **alpha** — Optional two-sided significance level used for the confidence limits. The default is 0.05.

### Returns

A spill range with columns:

- `Group` — group label.
- `PlotOrder` — plotting order within the group.
- `Time` — x-coordinate for the step curve.
- `Survival` — Kaplan–Meier survival probability.
- `LowerCI` — lower confidence limit.
- `UpperCI` — upper confidence limit.
- `AtRisk` — number at risk reported on the drop point rows; blank on horizontal connector rows.

### Notes

The function converts the tabular Kaplan–Meier output into a step-ready representation by duplicating each event/censor time:
one row continues the previous plateau and the next row moves vertically to the updated survival level.
This shape can be plotted directly as an XY scatter with straight lines.

The output is intended for plotting rather than formal tabular reporting. For a compact tabular summary at each observed time,
the existing `BESH.SURV.KM_TABLE` UDF remains the better choice.

## BESH.PLOT.ROC_POINTS

Returns chart-ready ROC curve coordinates from a marker column and a binary status column.

**Function wizard:** ROC thresholds and curve coordinates (false-positive rate vs. true-positive rate).

### Syntax

`=BESH.PLOT.ROC_POINTS(marker, status, positiveClass, direction, alpha)`

### Parameters

- **marker** — Single-column numeric marker values. Higher values are assumed to indicate the positive class unless
`direction` is set to `lower`.
- **status** — Single-column class labels aligned row-by-row with `marker`. The column may contain 0/1 values,
logical binary labels, or any two-category text labels.
- **positiveClass** — Optional label identifying the positive class. When omitted, the function automatically uses 1 for binary 0/1 inputs.
For text labels, supplying this argument is recommended to avoid ambiguity.
- **direction** — Optional direction flag:

- `higher` — larger marker values indicate the positive class (default).
- `lower` — smaller marker values indicate the positive class.
- **alpha** — Optional two-sided significance level used internally for the stored ROC confidence intervals.
The default is 0.05, corresponding to a 95% confidence interval.

### Returns

A spill range with columns:

- `Threshold` — decision threshold on the original marker scale.
- `Sensitivity` — true-positive rate.
- `Specificity` — true-negative rate.
- `FalsePositiveRate` — 1 − specificity, the usual ROC x-axis.
- `TruePositiveRate` — sensitivity, the usual ROC y-axis.

### Notes

The output includes the two ROC end points in addition to the empirical cut-offs computed by the underlying ROC engine.
This means the returned table can be plotted directly as an XY scatter with straight lines.

When smaller marker values indicate the positive class, set `direction` to `lower`.
Internally the function reverses the marker scale, fits the ROC, and then converts thresholds back to the original scale.

## BESH.PLOT.ROC_STATS

Returns a numerical ROC summary table containing AUC, standard errors, confidence intervals, and the test against AUC = 0.5.

**Function wizard:** Numerical ROC summary: AUC, standard errors, confidence intervals, and p-value.

### Syntax

`=BESH.PLOT.ROC_STATS(marker, status, positiveClass, direction, alpha)`

### Parameters

- **marker** — Single-column numeric marker values. Higher values are assumed to indicate the positive class unless
`direction` is set to `lower`.
- **status** — Single-column class labels aligned row-by-row with `marker`.
- **positiveClass** — Optional label identifying the positive class. For text labels, supplying this argument is recommended.
- **direction** — Optional direction flag: `higher` (default) or `lower`.
- **alpha** — Optional two-sided significance level used for the DeLong and Hanley–McNeil confidence intervals.

### Returns

A two-column spill range with metric names and values, mirroring the numerical summary shown by the GUI ROC output.

### Notes

The returned table includes the Wilcoxon AUC estimate, DeLong and Hanley–McNeil confidence intervals,
the corresponding standard errors, and the two-sided p-value for the null hypothesis that AUC = 0.5.

Use this function when you need the numeric ROC summary for reporting while `BESH.PLOT.ROC_POINTS`
supplies the chart coordinates for plotting.
