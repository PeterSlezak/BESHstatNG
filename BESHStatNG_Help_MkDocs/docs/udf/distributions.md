# Distributions UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._

## BESH.DIST.F_PDF

Computes the probability density function (PDF) of the F distribution.

**Function wizard:** F distribution PDF (equivalent to F.DIST(x, df1, df2, FALSE)).

### Syntax

`=BESH.DIST.F_PDF(x, df1, df2)`

### Parameters

- **x** — Point at which to evaluate the density (must be ≥ 0).
- **df1** — Numerator degrees of freedom (must be > 0). Non-integer values are supported.
- **df2** — Denominator degrees of freedom (must be > 0). Non-integer values are supported.

### Returns

The value of the F distribution PDF at `x`.
If inputs are invalid or the density is not finite, returns `#NUM!`.

### Notes

This is the worksheet-friendly wrapper around
`distributions.Distributions.F_PDF(x, df1, df2)`.

Relation to Excel:

`F.DIST(x, df1, df2, FALSE)` ↔ `BESH.DIST.F_PDF(x, df1, df2)`

Relation to R:

`df(x, df1, df2)` ↔ `BESH.DIST.F_PDF(x, df1, df2)`

Behavior at x = 0: the F density behaves like `x^(df1/2 - 1)` as `x → 0`.
Therefore:

- If `df1 > 2`, then `f(0) = 0`.
- If `df1 = 2`, then `f(0) = 1`.
- If `df1 < 2`, the density diverges (infinite) and this UDF returns `#NUM!`.

### Example

```

=BESH.DIST.F_PDF(1.25, 5, 10)
```

## BESH.DIST.PRTRNG

Studentized range distribution CDF: returns `P(0 ≤ Q ≤ q)`.

**Function wizard:** Studentized range CDF: returns P(0 ≤ Q ≤ q) for df=v and r groups (AS190).

### Syntax

`=BESH.DIST.PRTRNG(q, v, r)`

### Parameters

- **q** — Studentized range value (must be > 0).
- **v** — Degrees of freedom (must be ≥ 1). Non-integer values are supported.
- **r** — Number of groups/samples (must be ≥ 2).

### Returns

The probability `P(0 ≤ Q ≤ q)`. If inputs are invalid or the internal routine reports failure,
returns `#NUM!`.

### Notes

This is a worksheet-friendly wrapper around the internal implementation
`distributions.Distributions.PRTRNG(q, v, r, iFault)` (Algorithm AS 190).

The Studentized range distribution is commonly used for Tukey-style multiple comparison
procedures (e.g., Tukey HSD) after ANOVA.

### Example

```

=BESH.DIST.PRTRNG(3.5, 20, 5)
```

## BESH.DIST.PRTRNG.TAIL

Studentized range distribution upper tail: returns `P(Q > q)`.

**Function wizard:** Studentized range upper-tail: returns P(Q > q) = 1 - BESH.DIST.PRTRNG(q,v,r).

### Syntax

`=BESH.DIST.PRTRNG.TAIL(q, v, r)`

### Parameters

- **q** — Studentized range value (must be > 0).
- **v** — Degrees of freedom (must be ≥ 1).
- **r** — Number of groups/samples (must be ≥ 2).

### Returns

The probability `P(Q > q)`. If inputs are invalid, returns `#NUM!`.

### Notes

This function is computed as `1 - BESH.DIST.PRTRNG(q, v, r)` with safeguards for
floating-point rounding.

### Example

```

=BESH.DIST.PRTRNG.TAIL(3.5, 20, 5)
```
