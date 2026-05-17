# Resampling in BESH Stat NG

**Includes:** overview of the resampling strategies used across BESHStatNG, including **jackknife**, **bootstrap percentile**, **bootstrap BCa**, and **permutation / exact enumeration**; guidance on when each approach is used; current user-facing coverage; reproducibility and random-seed handling; and practical advice on choosing among analytical and resampling-based procedures.  
**Purpose:** provide one cross-cutting reference page that explains **how BESHStatNG computes resampling-based confidence intervals and p-values**, instead of requiring users to discover those details only from individual method pages and changelogs.

---

## Overview

Many BESHStatNG methods offer more than one way to quantify uncertainty.

Depending on the method, BESHStatNG may use:

- **analytical** standard errors and confidence intervals,
- **jackknife** resampling,
- **bootstrap percentile** confidence intervals,
- **bootstrap BCa** confidence intervals,
- or **permutation / exact enumeration** for small-sample hypothesis testing.

This page explains those approaches in one place and shows where they appear in the product.

!!! note "Why this page exists"
    Resampling support is now a meaningful cross-cutting capability in BESHStatNG.  
    The goal of this page is to make that capability easy to understand and easy to find.

---

## What “resampling” means here

In BESHStatNG, **resampling** means that the software repeatedly rebuilds a statistic from modified versions of the observed data in order to estimate:

- a **confidence interval**,
- a **standard error**,
- or a **null distribution / p-value**.

The main resampling families currently used in BESHStatNG are:

### 1) Jackknife

The **jackknife** systematically leaves out part of the data and recomputes the statistic.

Typical patterns are:

- **leave-one-row-out**,
- **leave-one-pair-out**,
- or, for repeated-measures agreement problems, **leave-one-subject-out**.

Jackknife is deterministic once the data are fixed. It does **not** require pseudo-random sampling.

It is often used when you want:

- a resampling-based standard error,
- a confidence interval without Monte Carlo randomness,
- or an ingredient for **BCa** acceleration.

### 2) Bootstrap Percentile

The **bootstrap** samples from the observed data **with replacement**, refits the statistic in each bootstrap sample, and uses the empirical distribution of those bootstrap replicates to form an interval.

The simplest interval form currently exposed in BESHStatNG is the **percentile interval**.

Use bootstrap percentile when:

- you want a simple resampling interval,
- you do not want to rely only on asymptotic analytical formulas,
- and a percentile interval is sufficient for the analysis.

### 3) Bootstrap BCa

**BCa** stands for **bias-corrected and accelerated** bootstrap.

Compared with a simple percentile interval, BCa adjusts for:

- **bias** in the bootstrap distribution,
- and **skewness / asymmetry** through the acceleration term.

In practice, BCa is often the most refined bootstrap interval currently exposed in BESHStatNG.

Use BCa when:

- the statistic may have a skewed sampling distribution,
- you want a more refined bootstrap interval than the simple percentile form,
- or the method page explicitly recommends BCa for more reliable interval estimation.

### 4) Permutation / exact enumeration

Permutation methods generate a **null reference distribution** by rearranging labels, ranks, signs, or paired structure under the null hypothesis.

Depending on the method and sample size, BESHStatNG may use:

- **exact enumeration** of all relevant permutations / sign patterns,
- or **Monte Carlo permutation sampling** when exact enumeration would be too large.

Permutation-style procedures are especially useful when:

- sample sizes are small,
- exact or near-exact inference is desirable,
- or the method has a natural randomization / exchangeability structure.

---

## Analytical vs resampling-based uncertainty

Not every problem needs resampling.

Analytical standard errors and confidence intervals are often:

- **faster**, 
- easier to compare across software,
- and entirely deterministic.

Resampling approaches become attractive when:

- analytical approximations are weak or unavailable,
- the statistic has a complicated sampling distribution,
- the data structure is paired or clustered,
- or the method page specifically offers resampling as a more robust alternative.

A useful way to think about the options is:

| Approach | Main idea | Strengths | Typical trade-offs |
|---|---|---|---|
| Analytical | closed-form approximation | fast, deterministic, standard | may rely more heavily on asymptotic assumptions |
| Jackknife | systematic leave-one-out recomputation | deterministic, simple, useful for SE/CI and BCa acceleration | may be less flexible than full bootstrap |
| Bootstrap Percentile | empirical CI from resampled datasets | intuitive, widely used | Monte Carlo variability; percentile interval may be less refined |
| Bootstrap BCa | bias-corrected and accelerated bootstrap CI | often the strongest general-purpose bootstrap CI currently exposed | slower, more computationally demanding |
| Permutation / exact | null distribution from rearrangements under the null | powerful for small-sample inference, often very interpretable | may become expensive or need Monte Carlo sampling |

---

## Current user-facing coverage in BESHStatNG

The most visible resampling support in the current public method surface is in **agreement / method-comparison analysis**.

### Agreement and method-comparison methods

| Method | Analytical | Jackknife | Bootstrap Percentile | Bootstrap BCa | Notes |
|---|---:|---:|---:|---:|---|
| [Bland–Altman Analysis](bland-altman.md) | Yes | Yes | Yes | Yes | Repeated-measures Bland–Altman supports subject-aware resampling. |
| [Lin's CCC](lins-ccc.md) | Yes | Used internally for BCa acceleration | Yes | Yes | Analytical CI uses Fisher-z style approximation. |
| [Cohen's / Weighted Kappa](cohens-kappa.md) | Yes | Yes | Yes | Yes | Supports unweighted and weighted forms. |
| [Deming Regression](deming-regression.md) | Yes | Yes | Yes | Yes | Also includes the Linnet pseudo-value analytical option. |

!!! tip "Agreement methods are the clearest public examples"
    If you want to see how BESHStatNG surfaces resampling in the GUI and output notes today, start with:

    - [Bland–Altman Analysis](bland-altman.md)
    - [Lin's CCC](lins-ccc.md)
    - [Cohen's / Weighted Kappa](cohens-kappa.md)
    - [Deming Regression](deming-regression.md)

### Selected exact / permutation-style procedures

Permutation-style inference is also part of BESHStatNG, although it is currently surfaced less prominently than the agreement bootstrap/jackknife workflows.

Examples include:

- exact or small-sample procedures in selected **contingency-table** workflows,
- exact p-values in selected **rank-based tests**,
- exact permutation logic in selected **correlation / nonparametric UDFs** for very small samples.

Relevant places to explore include:

- [2×2 Table](2x2-table.md)
- [Proportions](proportions.md)
- [User Defined Functions (UDFs)](../udf/index.md)
- [UDF Nonparametric](../udf/nonparametric.md)
- [UDF Contingency Tables](../udf/contingency-tables.md)

!!! note "Visibility today"
    In the current user-facing product, **bootstrap and jackknife support are much more visible than permutation support**.  
    The permutation infrastructure is broader internally than the current public UI would suggest and will be extended in future BESH Stat NG releases.

---

## Clustered and repeated-measures resampling

Some BESHStatNG methods operate on data where rows are **not independent** in the ordinary row-by-row sense.

In those settings, resampling may need to preserve the grouping structure.

For example, repeated-measures Bland–Altman analysis treats the subject / sample identifier as the resampling unit when the method requires **leave-one-subject-out** or subject-aware bootstrap logic.

This is important because naïvely resampling individual rows can break the dependency structure that the analysis is trying to respect.

As the mixed-model and longitudinal toolset expands, this shared resampling infrastructure provides a foundation for more consistent handling of:

- paired data,
- repeated measures,
- clustered observations,
- and future resampling-based inference layers.

---

## Reproducibility and random seeds

Randomized resampling methods should be reproducible when the same data and settings are used.

BESHStatNG supports this through:

- a **Default Random Seed** in [Global Settings](../global-settings.md),
- explicit method-level seed fields where exposed,
- and output notes that report the **actual seed used** for supported bootstrap runs.

This matters because two bootstrap analyses can differ slightly when they use:

- different random seeds,
- different numbers of replicates,
- different percentile interpolation conventions,
- or different handling of unsuccessful replicates.

### Practical reproducibility guidance

If you want resampling-based output to be reproducible across sessions:

1. set a **Default Random Seed** in [Global Settings](../global-settings.md),
2. keep the number of bootstrap / Monte Carlo replicates fixed,
3. preserve the same data ordering and method options,
4. keep the same BESHStatNG version when comparing archived results.

!!! tip "What to record"
    When documenting a resampling-based analysis, record at least:

    - the CI / resampling type,
    - the number of replicates,
    - the random seed,
    - the BESHStatNG version,
    - and whether the data were ordinary paired rows or clustered / repeated measures.

---

## Choosing among the available options

A practical rule-of-thumb is:

### Prefer analytical intervals when:

- the method page states that the analytical approximation is standard and appropriate,
- the sample size is moderate or large,
- you want fast and deterministic output,
- you are comparing results against software that also uses analytical formulas.

### Prefer jackknife when:

- you want a deterministic resampling-based interval,
- the statistic has a natural leave-one-out interpretation,
- or the method page presents jackknife as a supported middle ground between analytical and full bootstrap.

### Prefer bootstrap percentile when:

- you want a simple resampling CI,
- the method page offers it directly,
- and you do not need the additional BCa adjustment.

### Prefer bootstrap BCa when:

- you want the most refined bootstrap CI currently exposed,
- the statistic may be skewed,
- or the method page specifically recommends BCa for better interval behavior.

### Prefer exact / permutation-based inference when:

- the sample size is small enough that exact inference is feasible,
- the null hypothesis has a natural randomization interpretation,
- or the method/UDF explicitly exposes an exact permutation or exact small-sample p-value.

---

## Computational cost and interpretation notes

Resampling adds computation.

In general:

- **analytical** < **jackknife** < **bootstrap percentile** < **bootstrap BCa**

in terms of time and computational cost.

Permutation tests can range from very fast to very expensive depending on whether:

- exact enumeration is possible,
- ties must be deduplicated,
- or Monte Carlo sampling is needed instead.

Important interpretation points:

- A bootstrap CI is still a CI for the same parameter; the method changes **how uncertainty is estimated**, not what parameter is being estimated.
- Different CI types may produce slightly different limits even on the same data.
- Exact / permutation p-values may differ from asymptotic p-values in small samples, sometimes materially.

---

## What the shared resampling infrastructure gives BESHStatNG

Internally, BESHStatNG now has a shared resampling layer rather than method-by-method ad hoc implementations.

That is important because it allows method code to reuse the same core ideas for:

- bootstrap sampling,
- clustered / subject-level bootstrap,
- jackknife metadata and reporting,
- permutation and exact-enumeration infrastructure,
- standardized run notes and seed handling.

From a user perspective, the benefit is that resampling support can become:

- more consistent across methods,
- easier to validate,
- easier to document,
- and easier to extend to future features.

---

## Validation and transparency

Resampling is part of the broader BESHStatNG emphasis on transparent, inspectable computation.

For users who want more detail on validation and public evidence, see:

- the website [Validation](https://beshstat.eu/validation/) page
- [Implementation Notes](../implementation-notes.md)
- [What's New](../whats-new.md)

!!! note "Method-specific details still matter"
    This page explains the shared capability.  
    The exact details for a given analysis — including formulas, default settings, and output tables — are still documented on the individual method pages.

---

## See also

### Method pages

- [Bland–Altman Analysis](bland-altman.md)
- [Lin's CCC](lins-ccc.md)
- [Cohen's / Weighted Kappa](cohens-kappa.md)
- [Deming Regression](deming-regression.md)
- [2×2 Table](2x2-table.md)
- [Proportions](proportions.md)

### Supporting reference pages

- [Global Settings](../global-settings.md)
- [Implementation Notes](../implementation-notes.md)
- [What's New](../whats-new.md)
- [User Defined Functions (UDFs)](../udf/index.md)
- [UDF Nonparametric](../udf/nonparametric.md)
- [UDF Agreement](../udf/agreement.md)

