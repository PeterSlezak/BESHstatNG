# ==============================================================================
# kr_mmrm_multicovariate_missing_type3_reference.R
# ==============================================================================
# Purpose:
#   Generate hard-coded R mmrm Kenward-Roger multi-df Type III / term-level
#   F-test reference constants for the BESHStatNG multicovariate missing-data
#   MMRM validation tests.
#
# Input:
#   TestData/mixedmodel_longitudinal_multicovariate_missing.csv
#
# Output:
#   - CSV export in kr_reference_outputs/
#   - ready-to-paste VB constants printed to the console
#
# Notes:
#   This script intentionally uses the same manual dummy-coded fixed-effect
#   design as the BESHStatNG validation test:
#       intercept + sex_code + treatment_active + site_central + site_south
#       + age_centered_8 + treatment_active:age_centered_8
#
#   Because site is represented by two dummy coefficients in BESHStatNG, this
#   script adds a true two-df clinic_site test with L rows selecting site_central
#   and site_south.  The other terms are one-row F tests.
#
#   The test project does not require R at runtime.  Run this script only when
#   regenerating hard-coded external references.
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
dat$visit_f <- factor(dat$visit)
dat$sex_code <- as.numeric(dat$sex_code)
dat$treatment_active <- ifelse(dat$treatment_arm == "Active", 1, 0)
dat$site_central <- ifelse(dat$clinic_site == "Central", 1, 0)
dat$site_south <- ifelse(dat$clinic_site == "South", 1, 0)
dat$age_centered_8 <- as.numeric(dat$age_centered_8)
dat$distance_mm <- as.numeric(dat$distance_mm)

cases <- list(
  list(name = "Compound Symmetry", mmrm_type = "cs"),
  list(name = "Heterogeneous Compound Symmetry", mmrm_type = "csh"),
  list(name = "AR(1)", mmrm_type = "ar1"),
  list(name = "Heterogeneous AR(1)", mmrm_type = "ar1h"),
  list(name = "Unstructured", mmrm_type = "us")
)

# BESHStatNG term-level restrictions for the manually dummy-coded fixed design.
# clinic_site is the only multi-row restriction in this validation set.
terms <- list(
  list(term = "sex_code", effects = c("sex_code")),
  list(term = "treatment_active", effects = c("treatment_active")),
  list(term = "clinic_site", effects = c("site_central", "site_south")),
  list(term = "age_centered_8", effects = c("age_centered_8")),
  list(term = "treatment_active:age_centered_8", effects = c("treatment_active:age_centered_8"))
)

make_formula <- function(type) {
  stats::as.formula(paste0(
    "distance_mm ~ sex_code + treatment_active + site_central + site_south + ",
    "age_centered_8 + treatment_active:age_centered_8 + ",
    type, "(visit_f | subject_id)"
  ))
}

make_L <- function(beta_names, effects) {
  missing_effects <- setdiff(effects, beta_names)
  if (length(missing_effects) > 0) {
    stop("Could not find effects in beta vector: ", paste(missing_effects, collapse = ", "))
  }
  L <- matrix(0, nrow = length(effects), ncol = length(beta_names))
  colnames(L) <- beta_names
  rownames(L) <- effects
  for (i in seq_along(effects)) {
    L[i, effects[i]] <- 1
  }
  L
}

rank_matrix <- function(L, tol = 1e-10) {
  qr(L, tol = tol)$rank
}

extract_scalar <- function(obj, candidates, default = NA_real_) {
  if (is.null(obj)) return(default)
  if (is.atomic(obj) && length(obj) == 1 && is.null(names(obj))) return(as.numeric(obj))
  nms <- names(obj)
  if (!is.null(nms)) {
    for (cand in candidates) {
      hit <- which(tolower(nms) == tolower(cand))
      if (length(hit) > 0) return(as.numeric(obj[[hit[1]]]))
    }
    for (cand in candidates) {
      hit <- grep(cand, nms, ignore.case = TRUE)
      if (length(hit) > 0) return(as.numeric(obj[[hit[1]]]))
    }
  }
  default
}

kr_md <- function(fit, L) {
  # mmrm::df_md() has been the public helper used by the package's KR machinery.
  # This extraction wrapper is intentionally tolerant to small result-object name
  # differences between mmrm versions.
  raw <- mmrm::df_md(fit, L)
  den_df <- extract_scalar(raw, c("den_df", "df_den", "ddf", "denom", "df"), NA_real_)
  scaling <- extract_scalar(raw, c("lambda", "scaling", "f_scaling", "F.scaling"), NA_real_)
  if (!is.finite(scaling)) scaling <- 1.0
  list(raw = raw, den_df = den_df, scaling = scaling)
}

solve_symmetric <- function(a) {
  out <- try(solve(a), silent = TRUE)
  if (!inherits(out, "try-error")) return(out)
  # Fallback for very small numerical singularities in diagnostic generation.
  ev <- eigen((a + t(a)) / 2, symmetric = TRUE)
  tol <- max(dim(a)) * max(abs(ev$values)) * .Machine$double.eps * 100
  vals <- ifelse(abs(ev$values) > tol, 1 / ev$values, 0)
  ev$vectors %*% diag(vals, nrow = length(vals)) %*% t(ev$vectors)
}

rows <- list()

for (case in cases) {
  message("Fitting ", case$name, " (", case$mmrm_type, ")")

  fit <- mmrm::mmrm(
    formula = make_formula(case$mmrm_type),
    data = dat,
    reml = TRUE,
    method = "Kenward-Roger",
    vcov = "Kenward-Roger"
  )

  beta <- stats::coef(fit)
  beta_names <- names(beta)
  kr_vcov <- as.matrix(stats::vcov(fit))

  for (term in terms) {
    L <- make_L(beta_names, term$effects)
    q_eff <- rank_matrix(L)
    if (q_eff != nrow(L)) {
      stop("Reference L matrix unexpectedly rank deficient for ", case$name, " / ", term$term)
    }

    md <- kr_md(fit, L)
    if (!is.finite(md$den_df)) {
      stop("Could not extract denominator DF from mmrm::df_md() for ", case$name, " / ", term$term,
           ". Inspect str(mmrm::df_md(fit, L)) in your pinned mmrm version.")
    }

    estimate <- as.vector(L %*% beta)
    cov_l <- L %*% kr_vcov %*% t(L)
    qform <- as.numeric(t(estimate) %*% solve_symmetric(cov_l) %*% estimate)
    unscaled_f <- qform / q_eff
    scaled_f <- md$scaling * unscaled_f
    p_value <- stats::pf(scaled_f, df1 = q_eff, df2 = md$den_df, lower.tail = FALSE)

    rows[[length(rows) + 1L]] <- data.frame(
      structure = case$name,
      term = term$term,
      effects = paste(term$effects, collapse = ";"),
      num_df = q_eff,
      den_df = md$den_df,
      unscaled_f = unscaled_f,
      scaling = md$scaling,
      scaled_f = scaled_f,
      p_value = p_value,
      stringsAsFactors = FALSE
    )
  }
}

ref <- do.call(rbind, rows)
write.csv(ref,
          file.path(out_dir, "r_mmrm_multicovariate_missing_kr_type3.csv"),
          row.names = FALSE)

cat("\n' Paste into RmmrmType3References().\n")
cat("' Generated by kr_mmrm_multicovariate_missing_type3_reference.R\n")
cat("' R package versions: mmrm ", pkg_version, "\n", sep = "")
cat("Return New List(Of KrType3Reference) From {\n")
for (i in seq_len(nrow(ref))) {
  comma <- if (i < nrow(ref)) "," else ""
  effect_parts <- strsplit(ref$effects[i], ";", fixed = TRUE)[[1]]
  effect_literal <- paste(sprintf('"%s"', effect_parts), collapse = ", ")
  cat(sprintf(
    "    New KrType3Reference(structureName:=\"%s\", termName:=\"%s\", effects:=New String() {%s}, numDF:=%.12g, denDF:=%.12g, unscaledF:=%.12g, scaling:=%.12g, scaledF:=%.12g, pValue:=%.12g)%s\n",
    ref$structure[i],
    ref$term[i],
    effect_literal,
    ref$num_df[i],
    ref$den_df[i],
    ref$unscaled_f[i],
    ref$scaling[i],
    ref$scaled_f[i],
    ref$p_value[i],
    comma
  ))
}
cat("}\n\n")

utils::sessionInfo()
