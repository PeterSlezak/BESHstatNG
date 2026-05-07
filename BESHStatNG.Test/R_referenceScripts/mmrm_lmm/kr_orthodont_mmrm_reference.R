# ==============================================================================
# kr_orthodont_mmrm_reference.R
# ==============================================================================
# Purpose:
#   External R reference/export script for BESHStatNG internal KR-backend validation.
#
# Input:
#   TestData/mmrm_orthodont_potthoffroy_long.csv
#
# Output:
#   kr_reference_outputs/r_orthodont_mmrm_kr_coefficients.csv
#   kr_reference_outputs/r_orthodont_mmrm_kr_vcov.csv
#   kr_reference_outputs/r_orthodont_mmrm_kr_linear_estimates.csv
#
# Notes:
#   Requires the openpharma mmrm package. The mmrm package uses formula syntax like
#   y ~ covariates + us(visit | subject) and supports method = "Kenward-Roger".
# ==============================================================================

required <- c("mmrm")
missing <- required[!vapply(required, requireNamespace, logical(1), quietly = TRUE)]
if (length(missing) > 0) {
  stop("Install required packages first: ", paste(missing, collapse = ", "))
}

root <- normalizePath(file.path(getwd()), winslash = "/", mustWork = FALSE)
data_path <- file.path(root, "TestData", "mmrm_orthodont_potthoffroy_long.csv")
if (!file.exists(data_path)) {
  data_path <- file.path(root, "mmrm_orthodont_potthoffroy_long.csv")
}
if (!file.exists(data_path)) {
  stop("Could not find mmrm_orthodont_potthoffroy_long.csv")
}

out_dir <- file.path(root, "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

dat <- read.csv(data_path, stringsAsFactors = FALSE)
dat$Subject <- factor(dat$Subject)
dat$visit <- factor(dat$visit)
dat$SexCode <- as.numeric(dat$SexCode)
dat$age <- as.numeric(dat$age)
dat$distance <- as.numeric(dat$distance)

fit <- mmrm::mmrm(
  formula = distance ~ SexCode * age + mmrm::us(visit | Subject),
  data = dat,
  reml = TRUE,
  method = "Kenward-Roger",
  vcov = "Kenward-Roger"
)

smry <- summary(fit)
coef_tab <- as.data.frame(smry$coefficients)
coef_tab$effect <- rownames(coef_tab)
rownames(coef_tab) <- NULL
write.csv(coef_tab,
          file.path(out_dir, "r_orthodont_mmrm_kr_coefficients.csv"),
          row.names = FALSE)

vc <- as.matrix(vcov(fit))
vc_long <- data.frame(
  row_index = rep(seq_len(nrow(vc)), each = ncol(vc)),
  col_index = rep(seq_len(ncol(vc)), times = nrow(vc)),
  row_name = rep(rownames(vc), each = ncol(vc)),
  col_name = rep(colnames(vc), times = nrow(vc)),
  kr_adjusted_varbeta = as.vector(t(vc))
)
write.csv(vc_long,
          file.path(out_dir, "r_orthodont_mmrm_kr_vcov.csv"),
          row.names = FALSE)

# Linear estimates corresponding to BESHStatNG export rows:
# Visit k: Female - Male = beta_SexCode + age_k * beta_SexCode:age.
beta <- coef(fit)
beta_names <- names(beta)
make_l <- function(age_value) {
  l <- rep(0, length(beta))
  names(l) <- beta_names
  l["SexCode"] <- 1
  l["SexCode:age"] <- age_value
  l
}

ages_by_visit <- aggregate(age ~ visit, dat, unique)
lin <- do.call(rbind, lapply(seq_len(nrow(ages_by_visit)), function(i) {
  l <- make_l(ages_by_visit$age[i])
  est <- sum(l * beta)
  se <- sqrt(as.numeric(t(l) %*% vc %*% l))
  data.frame(
    label = paste0("Visit ", ages_by_visit$visit[i], ": Female - Male"),
    estimate = est,
    kr_adjusted_se = se,
    stringsAsFactors = FALSE
  )
}))

write.csv(lin,
          file.path(out_dir, "r_orthodont_mmrm_kr_linear_estimates.csv"),
          row.names = FALSE)

message("Wrote Orthodont MMRM KR reference exports to: ", out_dir)
