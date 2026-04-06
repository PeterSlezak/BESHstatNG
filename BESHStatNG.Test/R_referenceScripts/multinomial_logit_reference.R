# multinomial_logit_reference.R
# Replicates BESHStatNG MultinomialLogitModel outputs for mlogit_dataset_grouped_basic.csv
# - baseline category = LAST in sorted order => for {1,2,3}, baseline = 3
# - offset added to EACH non-baseline linear predictor (same scalar offset term)
# - weights are frequency/case weights

suppressPackageStartupMessages({
  library(nnet)
})

args <- commandArgs(trailingOnly = TRUE)
file <- if (length(args) >= 1) args[1] else file.path("TestData", "mlogit_dataset_grouped_basic.csv")

df <- read.csv(file, stringsAsFactors = FALSE)

# baseline must be first level for nnet::multinom
# so put "3" first: levels=c(3,1,2) => baseline=3, coefficients reported for 1 and 2 vs 3
df$yF <- factor(df$y, levels = c(3, 1, 2))

# full model
fit <- multinom(yF ~ x1 + x2 + offset(offset), data = df, weights = w, Hess = TRUE, trace = FALSE)

# null model (intercepts only, still include offset per VB null model)
fit0 <- multinom(yF ~ 1 + offset(offset), data = df, weights = w, Hess = TRUE, trace = FALSE)

LL  <- as.numeric(logLik(fit))
LL0 <- as.numeric(logLik(fit0))

# coefficient matrix rows: "1","2" columns: (Intercept), x1, x2
coef_mat <- coef(fit)
se_mat   <- summary(fit)$standard.errors

# Flatten in VB order: cat=1 block then cat=2 block, each: Intercept, x1, x2
coef_vec <- as.vector(t(coef_mat[, c("(Intercept)", "x1", "x2")]))
se_vec   <- as.vector(t(se_mat[,  c("(Intercept)", "x1", "x2")]))


# Stats per VB formulas
K <- length(levels(df$yF))                 # 3
p <- ncol(coef_mat)                        # 3 (Intercept,x1,x2)
kFull <- (K - 1) * p                       # 6
kNull <- (K - 1)                           # 2
nEff <- sum(df$w)

AIC <- -2 * LL + 2 * kFull
BIC <- -2 * LL + kFull * log(max(1, nEff))

CoxSnell <- 1 - exp((2 / nEff) * (LL0 - LL))
denNk <- 1 - exp((2 / nEff) * LL0)
Nagelkerke <- CoxSnell / denNk
McFadden <- 1 - (LL / LL0)

LR_chi2 <- 2 * (LL - LL0)
LR_df <- kFull - kNull
LR_p <- 1 - pchisq(LR_chi2, LR_df)

# GOF deviance (covariate-pattern), same as VB: include offset in key; round to 12 digits
make_key <- function(x1, x2, off, digits = 12) {
  fmt <- paste0("%.", digits, "f")
  paste(sprintf(fmt, 1), sprintf(fmt, x1), sprintf(fmt, x2), sprintf(fmt, off), sep = "|")
}

df$key <- make_key(df$x1, df$x2, df$offset, 12)

# predicted probs for each row (all levels, returned in yF levels order: 3,1,2)
p_raw <- predict(fit, type = "probs")
if (is.null(dim(p_raw))) {
  p_raw <- matrix(p_raw, nrow = 1)
  colnames(p_raw) <- levels(df$yF)
}

# reorder to original category order 1,2,3 for GOF (VB uses categories ascending with baseline last internally for ref=Last)
# Note: levels(df$yF) is c("3","1","2")
p_123 <- p_raw[, c("1","2","3"), drop = FALSE]

# Collapse into groups
groups <- split(seq_len(nrow(df)), df$key)

dev <- 0
G <- length(groups)

for (g in groups) {
  # representative row
  r <- g[1]
  m <- sum(df$w[g])

  # counts in original order 1,2,3
  y1 <- sum(df$w[g][df$y[g] == 1])
  y2 <- sum(df$w[g][df$y[g] == 2])
  y3 <- sum(df$w[g][df$y[g] == 3])
  yv <- c(y1, y2, y3)

  pi <- as.numeric(p_123[r, ])
  mu <- m * pi

  # D = 2 * sum_k y_k log(y_k / mu_k) (ignore y_k=0)
  for (k in 1:3) {
    if (yv[k] > 0) {
      dev <- dev + 2 * yv[k] * log(yv[k] / max(1e-300, mu[k]))
    }
  }
}

GOF_df <- G * (K - 1) - kFull
GOF_p <- 1 - pchisq(dev, GOF_df)

# weighted accuracy, tie-break to smallest cat (VB does this)
# probs already in 1,2,3 order
pred_cat <- apply(p_123, 1, function(pr) {
  # tie-break to smallest => which.max already returns first max in R, so ok
  c(1,2,3)[which.max(pr)]
})
acc <- sum(df$w[pred_cat == df$y]) / sum(df$w)

cat("=== Multinomial Logit Reference (VB parity) ===\n")
cat("File:", file, "\n\n")

cat("Coefficients (VB order):\n")
print(coef_vec)

cat("\nSEs (VB order):\n")
print(se_vec)

cat("\nLogLik0:", LL0, "\nLogLik :", LL, "\n")
cat("\nAIC:", AIC, "  BIC:", BIC, "\n")

cat("\nLR chi2:", LR_chi2, " df:", LR_df, " p:", LR_p, "\n")
cat("GOF chi2:", dev, " df:", GOF_df, " p:", GOF_p, "\n")

cat("\nPseudo R2:\n")
cat("  CoxSnell  :", CoxSnell, "\n")
cat("  Nagelkerke:", Nagelkerke, "\n")
cat("  McFadden  :", McFadden, "\n")

cat("\nAccuracy:", acc, "\n")

cat("\nVB-ready arrays:\n")
cat("expectedCoef <- c(", paste(sprintf("%.15f", coef_vec), collapse = ", "), ")\n", sep = "")
cat("expectedSe   <- c(", paste(sprintf("%.15f", se_vec), collapse = ", "), ")\n", sep = "")
cat(sprintf("LL0 <- %.15f\nLL  <- %.15f\n", LL0, LL))
cat(sprintf("LR_chi2 <- %.15f; LR_df <- %d; LR_p <- %.15f\n", LR_chi2, LR_df, LR_p))
cat(sprintf("GOF_chi2 <- %.15f; GOF_df <- %d; GOF_p <- %.15f\n", dev, GOF_df, GOF_p))
cat(sprintf("AIC <- %.15f; BIC <- %.15f\n", AIC, BIC))
cat(sprintf("CoxSnell <- %.15f; Nagelkerke <- %.15f; McFadden <- %.15f\n", CoxSnell, Nagelkerke, McFadden))
cat(sprintf("Accuracy <- %.15f\n", acc))
