# ==============================================================================
# kr_lmm_sleepstudy_random_slope_reference.R
# ==============================================================================
# Generates/prints the R reference values used by:
#
#   LMMRandomSlopeSleepstudyKRAgainstPbkrtestReferenceTests.vb
#
# Required packages:
#
#   install.packages(c("lme4", "pbkrtest"))
# ==============================================================================

required <- c("lme4", "pbkrtest")
missing <- required[!vapply(required, requireNamespace, logical(1), quietly = TRUE)]
if (length(missing) > 0) {
  stop("Install required packages first: ", paste(missing, collapse = ", "))
}

dat <- lme4::sleepstudy

fit <- lme4::lmer(Reaction ~ Days + (Days | Subject),
                  data = dat,
                  REML = TRUE)

vc0 <- as.matrix(vcov(fit))
vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))

coef_tab <- data.frame(
  effect = names(lme4::fixef(fit)),
  beta = as.numeric(lme4::fixef(fit)),
  ordinary_se = sqrt(diag(vc0)),
  kr_adjusted_se = sqrt(diag(vc_kr)),
  vcovAdj_over_vcov_diag = diag(vc_kr) / diag(vc0),
  stringsAsFactors = FALSE
)

print(coef_tab, digits = 12)
cat("\nvcovAdj / vcov:\n")
print(vc_kr / vc0, digits = 12)

out_dir <- file.path(getwd(), "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

write.csv(dat,
          file.path(out_dir, "mixedmodel_lmm_sleepstudy_random_slope_data.csv"),
          row.names = FALSE)

write.csv(coef_tab,
          file.path(out_dir, "r_lmm_sleepstudy_random_slope_kr_coefficients.csv"),
          row.names = FALSE)

cat("\nWrote:\n")
cat(file.path(out_dir, "mixedmodel_lmm_sleepstudy_random_slope_data.csv"), "\n")
cat(file.path(out_dir, "r_lmm_sleepstudy_random_slope_kr_coefficients.csv"), "\n")
