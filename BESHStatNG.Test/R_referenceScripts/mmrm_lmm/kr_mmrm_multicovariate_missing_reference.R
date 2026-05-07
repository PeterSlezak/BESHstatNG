# ==============================================================================
# kr_mmrm_multicovariate_missing_reference.R
# ==============================================================================
# Purpose:
#   Generate hard-coded R mmrm Kenward-Roger reference constants for the
#   BESHStatNG multicovariate missing-data MMRM validation tests.
#
# Input:
#   TestData/mixedmodel_longitudinal_multicovariate_missing.csv
#
# Output:
#   - CSV exports in kr_reference_outputs/
#   - a ready-to-paste VB constants block printed to the console
#
# Notes:
#   This script intentionally does not create any additional test data.  It uses
#   the existing augmented longitudinal test data file.
#
#   It uses the R package "mmrm".  The script prints sessionInfo() and the mmrm
#   package version so that the pasted VB test comment can record the exact
#   reference package version.
#
#   The mmrm package version current on CRAN at package generation time was
#   0.3.17, but final release references should be regenerated locally with the
#   exact pinned version you want to support.
# ==============================================================================

required <- c("mmrm")
missing <- required[!vapply(required, requireNamespace, logical(1), quietly = TRUE)]
if (length(missing) > 0) {
  stop("Install required packages first: ", paste(missing, collapse = ", "))
}

pkg_version <- as.character(utils::packageVersion("mmrm"))
message("mmrm version: ", pkg_version)

root <- normalizePath(getwd(), winslash = "/", mustWork = FALSE)
data_path <- file.path(root, "TestData", "mixedmodel_longitudinal_multicovariate_missing.csv")
if (!file.exists(data_path)) {
  data_path <- file.path(root, "mixedmodel_longitudinal_multicovariate_missing.csv")
}
if (!file.exists(data_path)) {
  stop("Could not find mixedmodel_longitudinal_multicovariate_missing.csv")
}

out_dir <- file.path(root, "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

dat <- read.csv(data_path, stringsAsFactors = FALSE)

# Match the BESHStatNG test design:
#   y = distance_mm
#   X = intercept + sex_code + treatment_active + site_central + site_south
#       + age_centered_8 + treatment_active:age_centered_8
#   Rows with missing response are excluded.
dat <- dat[!is.na(dat$distance_mm) & dat$distance_mm != "", , drop = FALSE]
dat$subject_id <- factor(dat$subject_id)
dat$visit_f <- factor(dat$visit)
dat$sex_code <- as.numeric(dat$sex_code)
dat$treatment_active <- ifelse(dat$treatment_arm == "Active", 1, 0)
dat$site_central <- ifelse(dat$clinic_site == "Central", 1, 0)
dat$site_south <- ifelse(dat$clinic_site == "South", 1, 0)
dat$age_centered_8 <- as.numeric(dat$age_centered_8)
dat$distance_mm <- as.numeric(dat$distance_mm)

# R mmrm covariance types that correspond to BESHStatNG MMRM structures:
#   cs   -> Compound Symmetry
#   csh  -> Heterogeneous Compound Symmetry
#   ar1  -> AR(1)
#   ar1h -> Heterogeneous AR(1)
#   us   -> Unstructured
#
# The public mmrm covariance-structure documentation lists us, cs/csh, ar1/ar1h,
# toeplitz, ante-dependence, and spatial structures.  If you need ID or diagonal
# heterogeneous references specifically from R mmrm, verify the exact covariance
# alias in your pinned mmrm version with mmrm::covariance_types().
cases <- list(
  list(name = "Compound Symmetry", mmrm_type = "cs"),
  list(name = "Heterogeneous Compound Symmetry", mmrm_type = "csh"),
  list(name = "AR(1)", mmrm_type = "ar1"),
  list(name = "Heterogeneous AR(1)", mmrm_type = "ar1h"),
  list(name = "Unstructured", mmrm_type = "us")
)

coef_rows <- list()
md_rows <- list()
cov_rows <- list()

make_formula <- function(type) {
  stats::as.formula(paste0(
    "distance_mm ~ sex_code + treatment_active + site_central + site_south + ",
    "age_centered_8 + treatment_active:age_centered_8 + ",
    type, "(visit_f | subject_id)"
  ))
}

extract_col <- function(tab, patterns) {
  nms <- names(tab)
  for (pat in patterns) {
    hit <- grep(pat, nms, ignore.case = TRUE, value = TRUE)
    if (length(hit) > 0) return(hit[1])
  }
  stop("Could not find coefficient-table column matching: ", paste(patterns, collapse = ", "))
}

for (case in cases) {
  message("Fitting ", case$name, " (", case$mmrm_type, ")")

  fit <- mmrm::mmrm(
    formula = make_formula(case$mmrm_type),
    data = dat,
    reml = TRUE,
    method = "Kenward-Roger",
    vcov = "Kenward-Roger"
  )

  smry <- summary(fit)
  tab <- as.data.frame(smry$coefficients)
  tab$effect <- rownames(tab)
  rownames(tab) <- NULL

  estimate_col <- extract_col(tab, c("^estimate$", "estimate"))
  se_col <- extract_col(tab, c("std.*error", "standard.*error", "^se$"))
  df_col <- extract_col(tab, c("^df$", "den.*df"))
  t_col <- extract_col(tab, c("t.*value", "t_stat", "^t$"))
  p_col <- extract_col(tab, c("pr\\(", "p.*value", "^p$"))

  for (i in seq_len(nrow(tab))) {
    effect <- tab$effect[i]
    L <- rep(0, length(coef(fit)))
    names(L) <- names(coef(fit))
    if (effect %in% names(L)) {
      L[effect] <- 1
      one <- try(mmrm::df_1d(fit, L), silent = TRUE)
    } else {
      one <- NULL
    }

    coef_rows[[length(coef_rows) + 1L]] <- data.frame(
      structure = case$name,
      effect = effect,
      estimate = as.numeric(tab[[estimate_col]][i]),
      ordinary_se = sqrt(diag(as.matrix(fit$beta_vcov)))[i],
      kr_se = as.numeric(tab[[se_col]][i]),
      df = as.numeric(tab[[df_col]][i]),
      t_value = as.numeric(tab[[t_col]][i]),
      p_value = as.numeric(tab[[p_col]][i]),
      stringsAsFactors = FALSE
    )
  }

  vc <- as.matrix(vcov(fit))
  cov_rows[[length(cov_rows) + 1L]] <- data.frame(
    structure = case$name,
    row_index = rep(seq_len(nrow(vc)), each = ncol(vc)),
    col_index = rep(seq_len(ncol(vc)), times = nrow(vc)),
    row_name = rep(rownames(vc), each = ncol(vc)),
    col_name = rep(colnames(vc), times = nrow(vc)),
    kr_adjusted_varbeta = as.vector(t(vc)),
    stringsAsFactors = FALSE
  )

  # Multi-df Type III-style tests via car::Anova are intentionally not required
  # for this script because we do not want to add another package dependency.
  # If you want a specific L-matrix test, add it here with mmrm::df_md(fit, L).
}

coef_df <- do.call(rbind, coef_rows)
cov_df <- do.call(rbind, cov_rows)

write.csv(coef_df,
          file.path(out_dir, "r_mmrm_multicovariate_missing_kr_coefficients.csv"),
          row.names = FALSE)
write.csv(cov_df,
          file.path(out_dir, "r_mmrm_multicovariate_missing_kr_vcov.csv"),
          row.names = FALSE)

cat("\n' Paste into a VB ReferenceCases() function.\n")
cat("' Generated by kr_mmrm_multicovariate_missing_reference.R\n")
cat("' R package versions: mmrm ", pkg_version, "\n", sep = "")
cat("Return New List(Of KrCoefficientReference) From {\n")
for (i in seq_len(nrow(coef_df))) {
  comma <- if (i < nrow(coef_df)) "," else ""
  cat(sprintf(
    "    New KrCoefficientReference(structureName:=\"%s\", effect:=\"%s\", estimate:=%.12g, ordinarySE:=%.12g, krSE:=%.12g, df:=%.12g, tValue:=%.12g, pValue:=%.12g)%s\n",
    coef_df$structure[i],
    coef_df$effect[i],
    coef_df$estimate[i],
    coef_df$ordinary_se[i],
    coef_df$kr_se[i],
    coef_df$df[i],
    coef_df$t_value[i],
    coef_df$p_value[i],
    comma
  ))
}
cat("}\n\n")

utils::sessionInfo()
