# BESHStatNG mixed-model reference generator
#
# Run from the BESHStatNG.Test project root, or from R_referenceScripts/.
# The script reads CSV data from TestData/ and writes reference CSVs back to TestData/.
#
# Dependencies:
#   - stats: base/recommended R, used for the MMRM identity == LM check
#   - nlme: recommended R package, used for the random-intercept LMM reference
#
# Optional usage:
#   Rscript R_referenceScripts/mixed_model_reference.R

get_script_dir <- function() {
  cmd <- commandArgs(trailingOnly = FALSE)
  file_arg <- grep("^--file=", cmd, value = TRUE)
  if (length(file_arg) > 0) {
    return(dirname(normalizePath(sub("^--file=", "", file_arg[1]))))
  }
  if (!is.null(sys.frames()[[1]]$ofile)) {
    return(dirname(normalizePath(sys.frames()[[1]]$ofile)))
  }
  return(getwd())
}

script_dir <- get_script_dir()
project_root <- normalizePath(file.path(script_dir, ".."), mustWork = FALSE)
if (!dir.exists(file.path(project_root, "TestData"))) {
  project_root <- getwd()
}

testdata_dir <- file.path(project_root, "TestData")
if (!dir.exists(testdata_dir)) {
  stop("TestData directory not found. Run this script from BESHStatNG.Test root or R_referenceScripts/.")
}

write_metric_csv <- function(path, metrics) {
  out <- data.frame(metric = names(metrics), value = as.numeric(metrics), row.names = NULL)
  write.csv(out, path, row.names = FALSE, quote = FALSE)
  message("Wrote ", normalizePath(path, mustWork = FALSE))
}

# -------------------------------------------------------------------------
# 1) MMRM identity residual covariance reference
#    With R_i = sigma^2 I and ML fitting, this should match ordinary LM.
# -------------------------------------------------------------------------
mmrm_path <- file.path(testdata_dir, "mixedmodel_mmrm_identity_data.csv")
mmrm <- read.csv(mmrm_path, stringsAsFactors = FALSE)
mmrm$visit <- as.numeric(mmrm$visit)
fit_lm <- lm(y ~ visit, data = mmrm)
X <- model.matrix(fit_lm)
res <- residuals(fit_lm)
n <- nrow(mmrm)
rss <- sum(res^2)
sigma2_ml <- rss / n
var_beta_ml <- sigma2_ml * solve(crossprod(X))
loglik_ml <- -0.5 * (n * log(2 * pi) + n * log(sigma2_ml) + rss / sigma2_ml)
metrics_mmrm <- c(
  beta_intercept = unname(coef(fit_lm)["(Intercept)"]),
  beta_visit = unname(coef(fit_lm)["visit"]),
  sigma2_ml = sigma2_ml,
  logLik_ml = loglik_ml,
  objective_ml = -2 * loglik_ml,
  se_intercept = sqrt(var_beta_ml[1, 1]),
  se_visit = sqrt(var_beta_ml[2, 2]),
  rss = rss
)
write_metric_csv(file.path(testdata_dir, "mixedmodel_mmrm_identity_reference.csv"), metrics_mmrm)

# -------------------------------------------------------------------------
# 2) Random-intercept LMM reference using nlme::lme
#    y ~ visit + (1 | subject), REML.
# -------------------------------------------------------------------------
if (!requireNamespace("nlme", quietly = TRUE)) {
  stop("The 'nlme' package is required for the random-intercept LMM reference.")
}

lmm_path <- file.path(testdata_dir, "mixedmodel_lmm_random_intercept_data.csv")
lmm <- read.csv(lmm_path, stringsAsFactors = FALSE)
lmm$visit <- as.numeric(lmm$visit)
lmm$subject <- factor(lmm$subject)

fit_lme <- nlme::lme(
  y ~ visit,
  random = ~ 1 | subject,
  data = lmm,
  method = "REML",
  control = nlme::lmeControl(msMaxIter = 200, niterEM = 50, tolerance = 1e-12)
)

vc <- nlme::VarCorr(fit_lme)
# VarCorr.lme returns a character matrix; rows are usually "(Intercept)" and "Residual".
var_random_intercept <- as.numeric(vc["(Intercept)", "Variance"])
var_residual <- as.numeric(vc["Residual", "Variance"])
loglik_reml <- as.numeric(logLik(fit_lme))

metrics_lmm <- c(
  beta_intercept = unname(nlme::fixef(fit_lme)["(Intercept)"]),
  beta_visit = unname(nlme::fixef(fit_lme)["visit"]),
  var_random_intercept = var_random_intercept,
  var_residual = var_residual,
  logLik_reml = loglik_reml,
  objective_reml = -2 * loglik_reml
)
write_metric_csv(file.path(testdata_dir, "mixedmodel_lmm_random_intercept_reference.csv"), metrics_lmm)

message("Mixed-model reference generation complete.")
