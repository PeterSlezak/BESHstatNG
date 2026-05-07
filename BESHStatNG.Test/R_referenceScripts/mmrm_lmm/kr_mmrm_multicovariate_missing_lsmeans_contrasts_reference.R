# ==============================================================================
# kr_mmrm_multicovariate_missing_lsmeans_contrasts_reference.R
# ==============================================================================
# Purpose:
#   Generate hard-coded R mmrm Kenward-Roger reference-grid LS-mean and treatment
#   contrast constants for the BESHStatNG multicovariate missing-data MMRM tests.
#
# Input:
#   TestData/mixedmodel_longitudinal_multicovariate_missing.csv
#
# Output:
#   - CSV exports in kr_reference_outputs/
#   - ready-to-paste VB constants printed to the console
#
# Reference-grid definition mirrored from BESHStatNG tests:
#   By factors:
#       visit = 1, 2, 3, 4
#       treatment_active = 0, 1
#   Marginal factors, equal-cell weighting:
#       sex_code = 0, 1
#       clinic_site_code = 0, 1, 2
#   Covariate:
#       baseline_distance_centered = observed analysis-row mean
#
# Fixed-effect design mirrored from BESHStatNG tests:
#   Intercept
#   visit=2, visit=3, visit=4
#   treatment_active
#   treatment_active:visit=2, treatment_active:visit=3, treatment_active:visit=4
#   sex_code
#   site_central, site_south
#   baseline_distance_centered
#   treatment_active:baseline_distance_centered
#
# The test project does not require R at runtime.  Run this script only when
# regenerating hard-coded external references.
# ===============================================================================

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
dat <- dat[!is.na(dat$distance_mm) & dat$distance_mm != "", , drop = FALSE]

dat$subject_id <- factor(dat$subject_id)
dat$visit_num <- as.numeric(dat$visit)
dat$visit_f <- factor(dat$visit_num)
dat$distance_mm <- as.numeric(dat$distance_mm)
dat$sex_code <- as.numeric(dat$sex_code)
dat$treatment_active <- ifelse(dat$treatment_arm == "Active", 1, 0)
dat$site_central <- ifelse(dat$clinic_site == "Central", 1, 0)
dat$site_south <- ifelse(dat$clinic_site == "South", 1, 0)
dat$baseline_distance_centered <- as.numeric(dat$baseline_distance_centered)

dat$v2 <- ifelse(dat$visit_num == 2, 1, 0)
dat$v3 <- ifelse(dat$visit_num == 3, 1, 0)
dat$v4 <- ifelse(dat$visit_num == 4, 1, 0)

baseline_mean <- mean(dat$baseline_distance_centered, na.rm = TRUE)

cases <- list(
  list(name = "Compound Symmetry", mmrm_type = "cs"),
  list(name = "Heterogeneous Compound Symmetry", mmrm_type = "csh"),
  list(name = "AR(1)", mmrm_type = "ar1"),
  list(name = "Heterogeneous AR(1)", mmrm_type = "ar1h"),
  list(name = "Unstructured", mmrm_type = "us")
)

make_formula <- function(type) {
  stats::as.formula(paste0(
    "distance_mm ~ v2 + v3 + v4 + treatment_active + ",
    "treatment_active:v2 + treatment_active:v3 + treatment_active:v4 + ",
    "sex_code + site_central + site_south + baseline_distance_centered + ",
    "treatment_active:baseline_distance_centered + ",
    type, "(visit_f | subject_id)"
  ))
}

assign_coef <- function(L, beta_names, candidates, value) {
  hit <- intersect(candidates, beta_names)
  if (length(hit) == 0) {
    stop("Could not find coefficient. Tried: ", paste(candidates, collapse = ", "),
         "; available: ", paste(beta_names, collapse = ", "))
  }
  L[hit[1]] <- value
  L
}

make_l <- function(beta_names, visit, treatment) {
  L <- rep(0, length(beta_names))
  names(L) <- beta_names

  v2 <- ifelse(visit == 2, 1, 0)
  v3 <- ifelse(visit == 3, 1, 0)
  v4 <- ifelse(visit == 4, 1, 0)

  L <- assign_coef(L, beta_names, c("(Intercept)", "Intercept"), 1)
  L <- assign_coef(L, beta_names, c("v2"), v2)
  L <- assign_coef(L, beta_names, c("v3"), v3)
  L <- assign_coef(L, beta_names, c("v4"), v4)
  L <- assign_coef(L, beta_names, c("treatment_active"), treatment)
  L <- assign_coef(L, beta_names, c("treatment_active:v2", "v2:treatment_active"), treatment * v2)
  L <- assign_coef(L, beta_names, c("treatment_active:v3", "v3:treatment_active"), treatment * v3)
  L <- assign_coef(L, beta_names, c("treatment_active:v4", "v4:treatment_active"), treatment * v4)
  L <- assign_coef(L, beta_names, c("sex_code"), 0.5)
  L <- assign_coef(L, beta_names, c("site_central"), 1 / 3)
  L <- assign_coef(L, beta_names, c("site_south"), 1 / 3)
  L <- assign_coef(L, beta_names, c("baseline_distance_centered"), baseline_mean)
  L <- assign_coef(L, beta_names,
                   c("treatment_active:baseline_distance_centered",
                     "baseline_distance_centered:treatment_active"),
                   treatment * baseline_mean)
  L
}

extract_df_1d <- function(fit, L) {
  raw <- mmrm::df_1d(fit, L)
  if (is.numeric(raw) && length(raw) == 1 && is.null(names(raw))) return(as.numeric(raw))
  nms <- names(raw)
  if (!is.null(nms)) {
    for (cand in c("df", "den_df", "df_den", "ddf", "denom")) {
      hit <- which(tolower(nms) == tolower(cand))
      if (length(hit) > 0) return(as.numeric(raw[[hit[1]]]))
    }
    for (cand in c("df", "den", "ddf")) {
      hit <- grep(cand, nms, ignore.case = TRUE)
      if (length(hit) > 0) return(as.numeric(raw[[hit[1]]]))
    }
  }
  stop("Could not extract denominator DF from mmrm::df_1d(); inspect str(mmrm::df_1d(fit, L)).")
}

linear_stats <- function(fit, L) {
  beta <- stats::coef(fit)
  ordinary_vcov <- as.matrix(fit$beta_vcov)
  kr_vcov <- as.matrix(stats::vcov(fit))

  estimate <- as.numeric(sum(L * beta))
  ordinary_se <- sqrt(as.numeric(t(L) %*% ordinary_vcov %*% L))
  kr_se <- sqrt(as.numeric(t(L) %*% kr_vcov %*% L))
  df <- extract_df_1d(fit, L)
  t_value <- estimate / kr_se
  p_value <- 2 * stats::pt(-abs(t_value), df = df)

  list(estimate = estimate,
       ordinary_se = ordinary_se,
       kr_se = kr_se,
       df = df,
       t_value = t_value,
       p_value = p_value)
}

ls_rows <- list()
contrast_rows <- list()

for (case in cases) {
  message("Fitting ", case$name, " (", case$mmrm_type, ")")

  fit <- mmrm::mmrm(
    formula = make_formula(case$mmrm_type),
    data = dat,
    reml = TRUE,
    method = "Kenward-Roger",
    vcov = "Kenward-Roger"
  )

  beta_names <- names(stats::coef(fit))

  l_by_profile <- list()

  for (visit in 1:4) {
    for (treatment in c(0, 1)) {
      L <- make_l(beta_names, visit, treatment)
      key <- paste(visit, treatment, sep = ":")
      l_by_profile[[key]] <- L
      s <- linear_stats(fit, L)
      label <- sprintf("visit=%s, treatment_active=%s", visit, treatment)

      ls_rows[[length(ls_rows) + 1L]] <- data.frame(
        structure = case$name,
        label = label,
        visit = visit,
        treatment = treatment,
        estimate = s$estimate,
        ordinary_se = s$ordinary_se,
        kr_se = s$kr_se,
        df = s$df,
        t_value = s$t_value,
        p_value = s$p_value,
        stringsAsFactors = FALSE
      )
    }

    Ldiff <- l_by_profile[[paste(visit, 1, sep = ":")]] - l_by_profile[[paste(visit, 0, sep = ":")]]
    s <- linear_stats(fit, Ldiff)
    label <- sprintf("treatment_active=1 - treatment_active=0 | visit=%s", visit)

    contrast_rows[[length(contrast_rows) + 1L]] <- data.frame(
      structure = case$name,
      label = label,
      visit = visit,
      estimate = s$estimate,
      ordinary_se = s$ordinary_se,
      kr_se = s$kr_se,
      df = s$df,
      t_value = s$t_value,
      p_value = s$p_value,
      stringsAsFactors = FALSE
    )
  }
}

ls_ref <- do.call(rbind, ls_rows)
contrast_ref <- do.call(rbind, contrast_rows)

write.csv(ls_ref,
          file.path(out_dir, "r_mmrm_multicovariate_missing_kr_reference_grid_lsmeans.csv"),
          row.names = FALSE)
write.csv(contrast_ref,
          file.path(out_dir, "r_mmrm_multicovariate_missing_kr_reference_grid_treatment_contrasts.csv"),
          row.names = FALSE)

cat("\n' Paste into RmmrmReferenceGridLSMeanReferences().\n")
cat("' Generated by kr_mmrm_multicovariate_missing_lsmeans_contrasts_reference.R\n")
cat("' R package versions: mmrm ", pkg_version, "\n", sep = "")
cat("Return New List(Of KrReferenceGridLSMeanReference) From {\n")
for (i in seq_len(nrow(ls_ref))) {
  comma <- if (i < nrow(ls_ref)) "," else ""
  cat(sprintf(
    "    New KrReferenceGridLSMeanReference(structureName:=\"%s\", label:=\"%s\", visit:=%.12g, treatment:=%.12g, estimate:=%.12g, ordinarySE:=%.12g, krSE:=%.12g, df:=%.12g, tValue:=%.12g, pValue:=%.12g)%s\n",
    ls_ref$structure[i],
    ls_ref$label[i],
    ls_ref$visit[i],
    ls_ref$treatment[i],
    ls_ref$estimate[i],
    ls_ref$ordinary_se[i],
    ls_ref$kr_se[i],
    ls_ref$df[i],
    ls_ref$t_value[i],
    ls_ref$p_value[i],
    comma
  ))
}
cat("}\n\n")

cat("\n' Paste into RmmrmReferenceGridContrastReferences().\n")
cat("' Generated by kr_mmrm_multicovariate_missing_lsmeans_contrasts_reference.R\n")
cat("' R package versions: mmrm ", pkg_version, "\n", sep = "")
cat("Return New List(Of KrReferenceGridContrastReference) From {\n")
for (i in seq_len(nrow(contrast_ref))) {
  comma <- if (i < nrow(contrast_ref)) "," else ""
  cat(sprintf(
    "    New KrReferenceGridContrastReference(structureName:=\"%s\", label:=\"%s\", visit:=%.12g, estimate:=%.12g, ordinarySE:=%.12g, krSE:=%.12g, df:=%.12g, tValue:=%.12g, pValue:=%.12g)%s\n",
    contrast_ref$structure[i],
    contrast_ref$label[i],
    contrast_ref$visit[i],
    contrast_ref$estimate[i],
    contrast_ref$ordinary_se[i],
    contrast_ref$kr_se[i],
    contrast_ref$df[i],
    contrast_ref$t_value[i],
    contrast_ref$p_value[i],
    comma
  ))
}
cat("}\n\n")

utils::sessionInfo()
