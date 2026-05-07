# ==============================================================================
# kr_lmm_pbkrtest_scalar_f_reference.R
# ==============================================================================
# Purpose:
#   Generate hard-coded R lme4 + pbkrtest reference constants for BESHStatNG
#   LMM Kenward-Roger scalar inference and KR F-test validation.
#
# Input data files:
#   TestData/mixedmodel_lmm_random_intercept_data.csv
#   TestData/mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv
#
# Output:
#   - CSV files in kr_reference_outputs/
#   - ready-to-paste VB constants printed to console
# ==============================================================================

required <- c("lme4", "pbkrtest")
missing <- required[!vapply(required, requireNamespace, logical(1), quietly = TRUE)]
if (length(missing) > 0) {
  stop("Install required packages first: ", paste(missing, collapse = ", "))
}

pkg_lme4 <- as.character(utils::packageVersion("lme4"))
pkg_pbkrtest <- as.character(utils::packageVersion("pbkrtest"))
message("lme4 version: ", pkg_lme4)
message("pbkrtest version: ", pkg_pbkrtest)

root <- "C:\\Users\\peter\\Dropbox\\dotNET\\BESHStatNG\\BESHStatNG.Test"
data_dir <- file.path(root, "TestData")
out_dir <- file.path(root, "kr_reference_outputs")
dir.create(out_dir, showWarnings = FALSE, recursive = TRUE)

read_test_data <- function(file_name) {
  path <- file.path(data_dir, file_name)
  if (!file.exists(path)) path <- file.path(root, file_name)
  if (!file.exists(path)) stop("Could not find input data: ", file_name)
  read.csv(path, stringsAsFactors = FALSE)
}

extract_kr_table <- function(kr) {
  st <- kr$stats
  if (is.null(st)) st <- kr[["stats"]]
  if (is.null(st)) stop("Could not find $stats in KRmodcomp result")
  as.data.frame(st)
}

extract_named_value <- function(df, candidates) {
  nms <- names(df)
  for (cand in candidates) {
    hit <- which(tolower(nms) == tolower(cand))
    if (length(hit) > 0) return(as.numeric(df[1, hit[1]]))
  }
  for (cand in candidates) {
    hit <- grep(cand, nms, ignore.case = TRUE)
    if (length(hit) > 0) return(as.numeric(df[1, hit[1]]))
  }
  stop("Could not extract any of: ", paste(candidates, collapse = ", "), " from columns: ", paste(nms, collapse = ", "))
}

kr_f_stats <- function(fit, L) {
  kr <- pbkrtest::KRmodcomp(fit, L)
  st <- extract_kr_table(kr)
  list(
    num_df = extract_named_value(st, c("ndf", "numdf", "num.df")),
    den_df = extract_named_value(st, c("ddf", "dendf", "den.df")),
    scaled_f = extract_named_value(st, c("Fstat", "F", "F.value")),
    scaling = extract_named_value(st, c("F.scaling", "scaling")),
    p_value = extract_named_value(st, c("p.value", "Pr(>F)", "p"))
  )
}

scalar_reference_rows <- function(fit, model_name, effect_names) {
  beta <- lme4::fixef(fit)
  ordinary_vcov <- as.matrix(stats::vcov(fit))
  kr_vcov <- as.matrix(pbkrtest::vcovAdj(fit))
  beta_names <- names(beta)
  rows <- list()

  for (effect in effect_names) {
    idx <- match(effect, beta_names)
    if (is.na(idx)) stop("Could not find fixed effect: ", effect, "; beta names: ", paste(beta_names, collapse = ", "))
    L <- matrix(0, nrow = 1, ncol = length(beta))
    colnames(L) <- beta_names
    L[1, idx] <- 1
    f <- kr_f_stats(fit, L)
    estimate <- unname(beta[idx])
    ordinary_se <- sqrt(ordinary_vcov[idx, idx])
    kr_se <- sqrt(kr_vcov[idx, idx])
    t_value <- sign(estimate) * sqrt(f$scaled_f)
    rows[[length(rows) + 1L]] <- data.frame(
      model = model_name,
      effect = effect,
      coefficient_index = idx - 1L,
      estimate = estimate,
      ordinary_se = ordinary_se,
      kr_se = kr_se,
      df = f$den_df,
      t_value = t_value,
      p_value = f$p_value,
      stringsAsFactors = FALSE
    )
  }
  do.call(rbind, rows)
}

f_reference_rows <- function(fit, model_name, terms) {
  beta <- lme4::fixef(fit)
  beta_names <- names(beta)
  rows <- list()

  for (term in terms) {
    idx <- match(term$effects, beta_names)
    if (any(is.na(idx))) stop("Could not find fixed effects for ", term$name, ": ", paste(term$effects, collapse = ", "))
    L <- matrix(0, nrow = length(idx), ncol = length(beta))
    colnames(L) <- beta_names
    for (i in seq_along(idx)) L[i, idx[i]] <- 1
    f <- kr_f_stats(fit, L)
    rows[[length(rows) + 1L]] <- data.frame(
      model = model_name,
      term = term$name,
      coefficient_indexes = paste(idx - 1L, collapse = ";"),
      num_df = f$num_df,
      den_df = f$den_df,
      unscaled_f = f$scaled_f / f$scaling,
      scaling = f$scaling,
      scaled_f = f$scaled_f,
      p_value = f$p_value,
      stringsAsFactors = FALSE
    )
  }
  do.call(rbind, rows)
}

# Random intercept validation data.
ri <- read_test_data("mixedmodel_lmm_random_intercept_data.csv")
ri$subject <- factor(ri$subject)
ri_fit <- lme4::lmer(y ~ visit + (1 | subject), data = ri, REML = TRUE)

ri_scalar <- scalar_reference_rows(ri_fit, "RandomIntercept", c("(Intercept)", "visit"))
ri_f <- f_reference_rows(
  ri_fit,
  "RandomIntercept",
  list(
    list(name = "visit", effects = c("visit")),
    list(name = "all_fixed", effects = c("(Intercept)", "visit"))
  )
)

# Unbalanced sleepstudy random-slope validation data.
ss <- read_test_data("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
ss$subject <- factor(ss$subject)
ss_fit <- lme4::lmer(reaction ~ days + (days | subject), data = ss, REML = TRUE)

ss_scalar <- scalar_reference_rows(ss_fit, "UnbalancedSleepstudy", c("(Intercept)", "days"))
ss_f <- f_reference_rows(
  ss_fit,
  "UnbalancedSleepstudy",
  list(
    list(name = "days", effects = c("days")),
    list(name = "all_fixed", effects = c("(Intercept)", "days"))
  )
)

write.csv(rbind(ri_scalar, ss_scalar), file.path(out_dir, "r_lmm_pbkrtest_scalar_references.csv"), row.names = FALSE)
write.csv(rbind(ri_f, ss_f), file.path(out_dir, "r_lmm_pbkrtest_f_references.csv"), row.names = FALSE)

print_scalar_block <- function(label, rows) {
  cat("\n' Paste into ", label, "ScalarReferences().\n", sep = "")
  cat("' Generated by kr_lmm_pbkrtest_scalar_f_reference.R\n")
  cat("' R package versions: lme4 ", pkg_lme4, ", pbkrtest ", pkg_pbkrtest, "\n", sep = "")
  cat("Return New List(Of LmmScalarReference) From {\n")
  for (i in seq_len(nrow(rows))) {
    comma <- if (i < nrow(rows)) "," else ""
    cat(sprintf(
      "    New LmmScalarReference(effect:=\"%s\", coefficientIndex:=%d, estimate:=%.12g, ordinarySE:=%.12g, krSE:=%.12g, df:=%.12g, tValue:=%.12g, pValue:=%.12g)%s\n",
      rows$effect[i], rows$coefficient_index[i], rows$estimate[i], rows$ordinary_se[i], rows$kr_se[i], rows$df[i], rows$t_value[i], rows$p_value[i], comma
    ))
  }
  cat("}\n")
}

print_f_block <- function(label, rows) {
  cat("\n' Paste into ", label, "FReferences().\n", sep = "")
  cat("' Generated by kr_lmm_pbkrtest_scalar_f_reference.R\n")
  cat("' R package versions: lme4 ", pkg_lme4, ", pbkrtest ", pkg_pbkrtest, "\n", sep = "")
  cat("Return New List(Of LmmFReference) From {\n")
  for (i in seq_len(nrow(rows))) {
    comma <- if (i < nrow(rows)) "," else ""
    idx <- strsplit(rows$coefficient_indexes[i], ";", fixed = TRUE)[[1]]
    idx_literal <- paste(idx, collapse = ", ")
    cat(sprintf(
      "    New LmmFReference(term:=\"%s\", coefficientIndexes:=New Integer() {%s}, numDF:=%.12g, denDF:=%.12g, unscaledF:=%.12g, scaling:=%.12g, scaledF:=%.12g, pValue:=%.12g)%s\n",
      rows$term[i], idx_literal, rows$num_df[i], rows$den_df[i], rows$unscaled_f[i], rows$scaling[i], rows$scaled_f[i], rows$p_value[i], comma
    ))
  }
  cat("}\n")
}

print_scalar_block("RandomIntercept", ri_scalar)
print_f_block("RandomIntercept", ri_f)
print_scalar_block("UnbalancedSleepstudy", ss_scalar)
print_f_block("UnbalancedSleepstudy", ss_f)

utils::sessionInfo()
