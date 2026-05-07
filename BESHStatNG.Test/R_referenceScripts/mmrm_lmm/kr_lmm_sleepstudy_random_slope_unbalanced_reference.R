# ==============================================================================
# kr_lmm_sleepstudy_random_slope_unbalanced_reference.R
# ==============================================================================
# Generates R pbkrtest reference values and a ready-to-paste VB constants block for:
#
#   LMMRandomSlopeSleepstudyUnbalancedKRValidationTests.vb
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

data_path <- file.path(getwd(), "TestData", "mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
if (!file.exists(data_path)) {
  data_path <- file.path(getwd(), "mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
}
if (!file.exists(data_path)) {
  stop("Could not find mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
}

dat <- read.csv(data_path, stringsAsFactors = FALSE)
dat$subject <- factor(dat$subject)
dat$days <- as.numeric(dat$days)
dat$reaction <- as.numeric(dat$reaction)

fit <- lme4::lmer(reaction ~ days + (days | subject),
                  data = dat,
                  REML = TRUE)

vc0 <- as.matrix(vcov(fit))
vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))
beta <- lme4::fixef(fit)

coef_tab <- data.frame(
  effect = names(beta),
  beta = as.numeric(beta),
  ordinary_se = sqrt(diag(vc0)),
  kr_adjusted_se = sqrt(diag(vc_kr)),
  kr_minus_ordinary_se = sqrt(diag(vc_kr)) - sqrt(diag(vc0)),
  stringsAsFactors = FALSE
)

print(coef_tab, digits = 12)

cat("\nPaste into GetHardCodedRReferenceRows():\n\n")
cat("Return New List(Of RReferenceRow) From {\n")
for (i in seq_len(nrow(coef_tab))) {
  comma <- if (i < nrow(coef_tab)) "," else ""
  cat(sprintf(
    "    New RReferenceRow(effect:=\"%s\", beta:=%.12g, ordinarySE:=%.12g, krAdjustedSE:=%.12g)%s\n",
    coef_tab$effect[i],
    coef_tab$beta[i],
    coef_tab$ordinary_se[i],
    coef_tab$kr_adjusted_se[i],
    comma
  ))
}
cat("}\n")

out_dir <- file.path(getwd(), "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

write.csv(coef_tab,
          file.path(out_dir, "r_lmm_sleepstudy_random_slope_unbalanced_kr_coefficients.csv"),
          row.names = FALSE)

cat("\nWrote:\n")
cat(file.path(out_dir, "r_lmm_sleepstudy_random_slope_unbalanced_kr_coefficients.csv"), "\n")
