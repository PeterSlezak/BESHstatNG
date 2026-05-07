# ==============================================================================
# kr_lmm_sleepstudy_postestimation_reference.R
# ==============================================================================
# Produces R reference values for selected post-estimation L rows used by:
#
#   MixedModelKRPostEstimationModelReferenceTests.vb
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

beta <- lme4::fixef(fit)
vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))

linear_rows <- list(
  "LSMean days=0" = c(1, 0),
  "LSMean days=9" = c(1, 9),
  "Change days 9 - 0" = c(0, 9),
  "Change days 3 - 0" = c(0, 3)
)

out <- do.call(rbind, lapply(names(linear_rows), function(label) {
  L <- matrix(linear_rows[[label]], nrow = 1)
  est <- as.numeric(L %*% beta)
  se <- sqrt(as.numeric(L %*% vc_kr %*% t(L)))
  data.frame(label = label,
             L_intercept = L[1, 1],
             L_days = L[1, 2],
             estimate = est,
             kr_adjusted_se = se,
             stringsAsFactors = FALSE)
}))

print(out, digits = 12)

cat("\nReady-to-paste VB constants:\n\n")
cat("Return New List(Of LinearReferenceRow) From {\n")
for (i in seq_len(nrow(out))) {
  comma <- if (i < nrow(out)) "," else ""
  cat(sprintf(
    "    New LinearReferenceRow(\"%s\", New Double() {%.12g, %.12g}, %.12g, %.12g)%s\n",
    out$label[i],
    out$L_intercept[i],
    out$L_days[i],
    out$estimate[i],
    out$kr_adjusted_se[i],
    comma
  ))
}
cat("}\n")

out_dir <- file.path(getwd(), "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

write.csv(out,
          file.path(out_dir, "r_lmm_sleepstudy_postestimation_kr_linear_rows.csv"),
          row.names = FALSE)

cat("\nWrote:\n")
cat(file.path(out_dir, "r_lmm_sleepstudy_postestimation_kr_linear_rows.csv"), "\n")
