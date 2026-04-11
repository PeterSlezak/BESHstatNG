# What's New

## version 0.6.5.0 (2026-04-11)

- Excel user defined functions exposed (4th batch including multiple linear regression model, generalized linear model, GLM NB2 with dispersion parameter estimation, Zero-Inflated Poisson regression, GEE)
- Addition of new methods:
  - the Cluster analysis including K-means clustering, and Hierarchical clustering with dendrogram
  - Exploratory Factor analysis
  - Weighted Deming Regression
  - Weighted kappa
  - Lin CCC
  - Bland–Altman


## version 0.5.8.0 (2026-03-29)

- Excel user defined functions exposed (3rd batch including ordinal and multinomial logistic regression functions, multiple comparison procedures for one-way ANOVA, one-way repeated measures ANOVA, Friedman test, and Kruskal–Wallis test, a set of assumption tests (normality, symmetry, etc.))
- Existing COX fit excel user defined functions enhancement of "FORMULA=" parameter that supports categorical factors, polynomial terms, and continuous-variable interaction terms specifications.
- GLM, NB2, ordinal logistic, and multinomial logistic Build Model tabs now support categorical factors, polynomial terms, and continuous-variable interaction terms.
- GEE Build Model tab now supports categorical factors, polynomial terms, and continuous-variable interaction terms.
- Zero-Inflated Poisson Build Model tabs now support categorical factors, polynomial terms, and continuous-variable interaction terms in both the Poisson and Logistic components.
- Zero-Inflated Poisson now uses separate authored-effect lists and separate starting-value vectors for the Poisson and Logistic parts.
- GLM-family, GEE, and ZIP regression pages now document expanded-design-matrix parameter counting for user-supplied starting values.
- Bug fix in Kendall's tau-b confidence interval calculation.
- Significance-level (alpha) option added to all applicable functions that returns confidence intervals.

## version 0.5.0.0 (2026-03-18)

- Excel user defined functions exposed (2nd batch including COX proportional hazard model functions, ANOVA tables ...)
- Multiple linear regression (linear model) now allows the categorical predictor to be specified as factor. The user does not need to create dummy variables in advance, and additionally the underlying class correctly handles the degrees of freedom in the ANOVA table.
- XYZ scatter plot now allows the manual axis scaling. User interface was slightly modified based on addition of new user controls.
- Internal dependency from NLOG package removed and in-house simple logger implemented. It resulted in about 20% size reduction of final addin .XLL file.

## version 0.4.6.0 (2026-03-13)

- Excel user defined functions exposed (1st batch including some distribution CDFs, nonparametric tests, survival analysis related tests)
- Auto update feature added
- XYZ 3D scatterplot enhancements. Option added to flip each axis. Overlay 3D wire frames objects (sphere, ellipsoid).
- bug fix with histogram normal curve overlay computation where with certain input data histogram bin midpoints coming into GaussOverlayComputation were wrong (rounded too aggressively), which then makes GaussOverlayComputation compute the wrong bin width and over-scaled the curve.

## version 0.4.0.0 (2026-02-08)

- Chart exporting dialog added. It supports excel chart exporting in various image formats and image quality suitable for publications.
- 3D scatterplot chart now supports creation of animated GIF.
- Multiple linear regression (linear model) now allows specification of polynomial and interaction terms.
- Internal regression data input handling was refactored to enable future enhancements.

## version 0.3.2.0 (2026-02-01)

- New agreement related methods added including Passing-Bablok and Deming Regression; Various versions of Intraclass correlation coefficients

## version 0.3.0.0 (2026-01-25)

- (Bug fix) Fixed input table dimensions check in 2x2 table by-worksheet-input where (3 x 3 had been actually checked instead of 2 x 2 dimensions)
- In all inputs containing built-in RefEdit control, the 1st control is selected on Windows form start

