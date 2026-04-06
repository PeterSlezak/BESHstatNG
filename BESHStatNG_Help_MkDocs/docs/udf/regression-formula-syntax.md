# Regression formula syntax for UDFs

This page describes the `formula=` and `formulaAddressing=` arguments used by regression UDFs.

At the moment, this background formula facility is implemented for:

- `BESH.SURV.COX_FIT`, `BESH.REGR.ORDLOGIT_FIT`
- and `BESH.REGR.FORMULA_VALIDATE` that use the grammar described below on formula text string

The same syntax is intended to be reused by other regression UDFs in the future.

---

## Overview

The `formula=` argument lets you build the model design from the raw predictor matrix `x` inside the UDF.

Instead of treating every column of `x` as a simple continuous main effect, you can request:

- selected main effects
- polynomial terms
- continuous-variable interactions
- categorical main effects via `factor(...)`

If `formula` is omitted or blank, the default behavior is:

- use **all columns of `x`**
- treat each column as a **continuous main effect**
- use the supplied `varNames` only for labeling/output

---

## Where this is used

Current UDF:

```excel
=BESH.SURV.COX_FIT(time, status, x, [varNames], [strata], [ties], [robust], , [formula], [formulaAddressing], [maxIter], [tol])
```

Typical examples:

```excel
=BESH.SURV.COX_FIT(A2:A101, B2:B101, C2:F101, "age,bmi,stage,treat", , , FALSE, "A + A^2 + factor(C, ref=1) + B:D", "relative", 100, 1E-8)
```

```excel
=BESH.SURV.COX_FIT(A2:A101, B2:B101, C2:F101, "prison,dose,stage,treat", , , FALSE, "'prison' + 'dose' + 'dose'^2", "names", 100, 1E-8)
```

```excel
=BESH.SURV.COX_FIT(A2:A101, B2:B101, C2:F101, "age,bmi,stage,treat", , , FALSE, "C + C^2 + factor(E, ref=1) + D:F", "absolute", 100, 1E-8)
```

---

## Formula grammar

The supported right-hand-side grammar is intentionally simple.

### Additive terms

Use `+` to add terms to the model:

```text
A + B + C
```

### Polynomial terms

Use `^` followed by an integer degree:

```text
A^2
A^3
```

Currently:

- the degree must be an integer
- the degree must be at least `2`
- polynomial terms are supported only for continuous predictors

### Interaction terms

Use `:` for continuous-variable interactions:

```text
A:B
A:B:C
```

Currently:

- interactions are supported only for continuous predictors
- repeated variables inside one interaction term are not allowed
- examples such as `A:A` or `A:B:A` are rejected

### Categorical main effects

Use `factor(...)` to treat a raw predictor column as categorical:

```text
factor(C)
factor(C, ref=2)
```

`ref=` sets the reference level explicitly.

If `ref=` is not supplied, the implementation uses its default categorical reference-level behavior.

!!! note
    `factor(...)` currently applies only to **categorical main effects**.
    Interactions involving `factor(...)` are not implemented yet.

---

## Supported examples

### Continuous main effects only

```text
A + B + D
```

### Main effects and a polynomial

```text
A + A^2 + B
```

### Continuous interaction

```text
A + B + A:B
```

### Three-way interaction

```text
A + B + C + A:B:C
```

### Categorical main effect

```text
A + factor(C)
```

### Categorical main effect with explicit reference level

```text
A + factor(C, ref=1)
```

### Names mode

```text
'prison' + 'dose' + 'dose'^2
```

### Names mode with an apostrophe in the variable name

Use doubled apostrophes inside the quoted name:

```text
'Children''s dose' + 'prison'
```

---

## Unsupported formulas and current limitations

The following are **not** supported in the current implementation:

### Interactions involving categorical predictors

```text
factor(C):A
factor(C):factor(D)
```

### Polynomial subterms inside interactions

```text
A^2:B
A:B^2
```

### Polynomial factor terms

```text
factor(C)^2
```

### Repeated variables inside one interaction

```text
A:A
A:B:A
```

### Text-valued factor columns in `x`

The `x` argument is read as a **numeric** matrix.
So if you use `factor(C)`, column `C` should already contain numeric category codes.

!!! warning
    `factor(...)` does **not** currently convert text labels into factor levels inside the UDF.
    If you want a categorical predictor in `formula=`, the corresponding column in `x` should be numeric-coded already.

---

## `formulaAddressing` options

The `formulaAddressing` argument controls how variable references inside `formula` are interpreted.

Accepted values are:

- `"relative"` (default)
- `"absolute"`
- `"names"`

### `relative`

In `relative` mode:

- `A` means the **first** column of `x`
- `B` means the **second** column of `x`
- `AA` means the **27th** column of `x`

Example:

```excel
=BESH.SURV.COX_FIT(A2:A101, B2:B101, C2:F101, "age,bmi,stage,treat", , , FALSE, "A + A^2 + factor(C, ref=1) + B:D", "relative", 100, 1E-8)
```

In that example:

- `A` refers to worksheet column `C`
- `B` refers to worksheet column `D`
- `C` refers to worksheet column `E`
- `D` refers to worksheet column `F`

because those are the first, second, third, and fourth columns of the supplied `x` range.

### `absolute`

In `absolute` mode, bare letters refer to the **actual worksheet columns** used by `x`.

Example:

```excel
=BESH.SURV.COX_FIT(A2:A101, B2:B101, C2:F101, "age,bmi,stage,treat", , , FALSE, "C + C^2 + factor(E, ref=1) + D:F", "absolute", 100, 1E-8)
```

In that example:

- `C` refers to worksheet column `C`
- `D` refers to worksheet column `D`
- `E` refers to worksheet column `E`
- `F` refers to worksheet column `F`

!!! important
    In `absolute` mode, `x` should be passed as a **direct worksheet range reference**.
    That is how the UDF determines the actual worksheet column letters.

If `x` is supplied as an already-materialized array rather than a direct worksheet reference, absolute addressing may fail.

### `names`

In `names` mode, bare column letters are disabled and variables should be referenced using **single-quoted names**.

Example:

```text
'prison' + 'dose' + 'dose'^2
```

Name matching is case-insensitive.

The names come from `varNames`.

!!! note
    Single-quoted names are also accepted in `relative` and `absolute` modes.
    So you can still write formulas using names even when the addressing mode is not `"names"`.

---

## How names are resolved

When `formulaAddressing="names"` is used:

- the parser looks up names from `varNames`
- matching is case-insensitive
- surrounding spaces are ignored
- names must be enclosed in single quotes

Examples:

```text
'Age'
'age'
' AGE '
```

all resolve to the same variable if `varNames` contains that predictor.

If a name contains an apostrophe, escape it with doubled apostrophes:

```text
'Children''s dose'
```

If the same name appears more than once in `varNames`, the reference is ambiguous and the formula is rejected.

---

## How `varNames` and `formula` work together

### If `formula` is blank

All columns of `x` are used as continuous main effects.

Example:

```excel
=BESH.SURV.COX_FIT(time, status, x, "prison,dose,stage")
```

This behaves like:

```text
A + B + C
```

but output labels use the supplied names:

- `prison`
- `dose`
- `stage`

### If `formula` is provided

Only the requested terms are included in the model.

Example:

```text
'prison' + 'dose' + 'dose'^2
```

This includes:

- the `prison` main effect
- the `dose` main effect
- the `dose^2` polynomial term

and excludes any other columns from `x` that are not referenced.

---

## Term ordering and duplicate handling

Terms are parsed from left to right.

Compatible duplicate terms are ignored.
For example:

```text
A + A + B
```

behaves like:

```text
A + B
```

But conflicting duplicates are rejected.
For example:

```text
factor(C, ref=1) + factor(C, ref=2)
```

is invalid because the same categorical main effect is requested with different reference levels.

---

## Assumptions

The current UDF formula implementation assumes:

- `time` is numeric
- `status` is binary with `1 = event` and `0 = censored`
- `x` is numeric
- rows of `time`, `status`, `x`, and optional `strata` line up
- if `factor(...)` is used, that `x` column contains numeric category codes

When whole worksheet columns are supplied for `time`, `status`, `x`, or `strata`, the UDF can skip a first-row header when appropriate and trims unused blank rows at the bottom.

---

## Predictions after a formula-based fit

When a model is fitted with `formula=`, the design expansion is stored with the fit.

That means `BESH.SURV.COX_PRED` should still receive the **raw predictor columns** in the same order as the original `x` input.
The prediction path rebuilds the required design matrix internally.

You do **not** need to pre-expand the predictors manually for:

- polynomial terms
- interaction terms
- `factor(...)`

---

## Performance note while editing formulas in Excel

Typing a long formula text directly inside `COX_FIT(...)` can feel slow because Excel may repeatedly re-evaluate the UDF while you edit.

A practical pattern is to keep the RHS formula text in a helper cell:

```excel
H1: 'prison' + 'dose' + 'dose'^2
=BESH.SURV.COX_FIT(time, status, x, varNames, strata, ties, robust, maxIter, tol, H1, "names")
```

This usually gives a smoother editing experience than typing the full formula string directly inside the UDF call.

---

## Troubleshooting

### `#VALUE!` when using `formulaAddressing="absolute"`

Check that:

- `x` is a direct worksheet range
- the formula uses worksheet column letters that match the `x` range

### Unknown variable name

Check that:

- `varNames` was supplied
- the name exists exactly once
- the name is enclosed in single quotes

### `factor(...)` does not work as expected

Check that:

- the referenced predictor column is numeric-coded
- you are using `factor(C)` or `factor(C, ref=...)`
- you are not trying to interact `factor(...)` with another term

### Interaction term rejected

Check that:

- you are not using `factor(...)` inside the interaction
- you are not using a polynomial subterm like `A^2:B`
- you are not repeating the same variable in one interaction term

---

## Summary

The `formula=` argument in regression UDFs lets you define the model structure directly in Excel.

Current capabilities include:

- additive continuous terms
- polynomial terms
- continuous-variable interactions
- categorical main effects via `factor(...)`
- three addressing styles: `relative`, `absolute`, and `names`
- use `BESH.REGR.FORMULA_VALIDATE` to validate formula text string using the grammar described above

Current limitations mainly affect:

- interactions with categorical predictors
- polynomial subterms inside interactions
- non-numeric factor columns in `x`
