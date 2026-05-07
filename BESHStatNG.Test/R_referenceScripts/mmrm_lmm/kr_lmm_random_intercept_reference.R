# ==============================================================================
# kr_lmm_random_intercept_reference.R
# ==============================================================================
# Purpose:
#   External R reference/export script for BESHStatNG LMM internal KR-backend validation.
#
# Input:
#   TestData/mixedmodel_lmm_random_intercept_data.csv
#
# Output:
#   kr_reference_outputs/r_lmm_random_intercept_kr_coefficients.csv
#   kr_reference_outputs/r_lmm_random_intercept_kr_vcov.csv
#   kr_reference_outputs/r_lmm_random_intercept_kr_linear_estimates.csv
#
# Notes:
#   Uses lme4 for fitting and pbkrtest::vcovAdj() for Kenward-Roger adjusted
#   coefficient covariance.
# ==============================================================================

required <- c("lme4", "pbkrtest")
missing <- required[!vapply(required, requireNamespace, logical(1), quietly = TRUE)]
if (length(missing) > 0) {
  stop("Install required packages first: ", paste(missing, collapse = ", "))
}

root <- normalizePath(file.path(getwd()), winslash = "/", mustWork = FALSE)
data_path <- file.path(root, "TestData", "mixedmodel_lmm_random_intercept_data.csv")
if (!file.exists(data_path)) {
  data_path <- file.path(root, "mixedmodel_lmm_random_intercept_data.csv")
}
if (!file.exists(data_path)) {
  stop("Could not find mixedmodel_lmm_random_intercept_data.csv")
}

out_dir <- file.path(root, "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

dat <- read.csv(data_path, stringsAsFactors = FALSE)
dat$subject <- factor(dat$subject)
dat$visit <- as.numeric(dat$visit)
dat$y <- as.numeric(dat$y)

fit <- lme4::lmer(y ~ visit + (1 | subject), data = dat, REML = TRUE)
vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))
beta <- lme4::fixef(fit)

coef_tab <- data.frame(
  effect = names(beta),
  beta = as.numeric(beta),
  ordinary_se = sqrt(diag(as.matrix(vcov(fit)))),
  kr_adjusted_se = sqrt(diag(vc_kr)),
  stringsAsFactors = FALSE
)
write.csv(coef_tab,
          file.path(out_dir, "r_lmm_random_intercept_kr_coefficients.csv"),
          row.names = FALSE)

vc_long <- data.frame(
  row_index = rep(seq_len(nrow(vc_kr)), each = ncol(vc_kr)),
  col_index = rep(seq_len(ncol(vc_kr)), times = nrow(vc_kr)),
  row_name = rep(rownames(vc_kr), each = ncol(vc_kr)),
  col_name = rep(colnames(vc_kr), times = nrow(vc_kr)),
  kr_adjusted_varbeta = as.vector(t(vc_kr))
)
write.csv(vc_long,
          file.path(out_dir, "r_lmm_random_intercept_kr_vcov.csv"),
          row.names = FALSE)

make_row <- function(label, l) {
  est <- sum(l * beta)
  se <- sqrt(as.numeric(t(l) %*% vc_kr %*% l))
  data.frame(label = label, estimate = est, kr_adjusted_se = se, stringsAsFactors = FALSE)
}

lin <- rbind(
  make_row("Intercept", c("(Intercept)" = 1, "visit" = 0)),
  make_row("Visit slope", c("(Intercept)" = 0, "visit" = 1)),
  make_row("Predicted visit 2", c("(Intercept)" = 1, "visit" = 2))
)

write.csv(lin,
          file.path(out_dir, "r_lmm_random_intercept_kr_linear_estimates.csv"),
          row.names = FALSE)

message("Wrote random-intercept LMM KR reference exports to: ", out_dir)
