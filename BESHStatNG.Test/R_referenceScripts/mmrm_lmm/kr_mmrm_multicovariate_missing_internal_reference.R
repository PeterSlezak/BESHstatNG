# ==============================================================================
# kr_mmrm_multicovariate_missing_internal_reference.R
# ==============================================================================
# Purpose:
#   Export component-level R mmrm Kenward-Roger reference values for the existing
#   BESHStatNG incomplete longitudinal multicovariate MMRM data set.
#
# Input:
#   TestData/mixedmodel_longitudinal_multicovariate_missing.csv
#
# Output:
#   TestData/kr_mmrm_multicovariate_missing_internal_reference.csv
#   kr_reference_outputs/kr_mmrm_multicovariate_missing_internal_reference.csv
#
# The CSV is intentionally long-form and structure-tagged so that MSTest can
# compare beta, unadjusted Var(beta), theta, Cov(theta), P/Q/R, adjusted Var(beta),
# and KR denominator-DF/scaling components for CS, CSH, AR(1), HAR(1), and UN.
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
  stop("Could not find mixedmodel_longitudinal_multicovariate_missing.csv. Run from the test project root or copy the CSV next to this script.")
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

make_formula <- function(type) {
  stats::as.formula(paste0(
    "distance_mm ~ sex_code + treatment_active + site_central + site_south + ",
    "age_centered_8 + treatment_active:age_centered_8 + ",
    type, "(visit_f | subject_id)"
  ))
}

component_or_slot <- function(object, name) {
  value <- object[[name]]
  if (!is.null(value)) {
    return(value)
  }

  component <- get("component", envir = asNamespace("mmrm"), inherits = FALSE)
  component(object, name)
}

fallback_names <- function(x, prefix) {
  nms <- names(x)
  if (is.null(nms) || length(nms) != length(x) || any(!nzchar(nms))) {
    nms <- paste0(prefix, seq_along(x))
  }
  nms
}

rows <- list()
add_row <- function(structure, kind, subkind, label, h, j, row, col, row_name, col_name, value) {
  rows[[length(rows) + 1L]] <<- data.frame(
    structure = structure,
    kind = kind,
    subkind = subkind,
    label = label,
    h = as.integer(h),
    j = as.integer(j),
    row = as.integer(row),
    col = as.integer(col),
    row_name = row_name,
    col_name = col_name,
    value = as.numeric(value),
    stringsAsFactors = FALSE
  )
}

add_vector <- function(structure, kind, values, names_vec) {
  for (r in seq_along(values)) {
    add_row(structure, kind, "value", "", -1L, -1L, r - 1L, -1L, names_vec[[r]], "", values[[r]])
  }
}

add_matrix <- function(structure, kind, subkind, label, h, j, mat, rn, cn) {
  for (r in seq_len(nrow(mat))) {
    for (c in seq_len(ncol(mat))) {
      add_row(structure, kind, subkind, label, h, j, r - 1L, c - 1L, rn[[r]], cn[[c]], mat[r, c])
    }
  }
}

tr <- function(x) sum(diag(x))
quad_form_mat <- function(l, v) l %*% v %*% t(l)

kr_df_components <- function(v0, l, w, p) {
  n_beta <- ncol(v0)
  n_theta <- ncol(w)
  n_visits <- ncol(p)
  stopifnot(nrow(v0) == n_beta, ncol(l) == n_beta)
  stopifnot(nrow(w) == n_theta, nrow(p) == n_visits * n_theta)

  slvol <- solve(quad_form_mat(l, v0))
  m_mat <- quad_form_mat(t(l), slvol)
  nl <- nrow(l)
  mv0 <- m_mat %*% v0
  pl <- lapply(seq_len(nrow(p) / ncol(p)), function(x) {
    ii <- (x - 1L) * ncol(p) + 1L
    jj <- x * ncol(p)
    p[ii:jj, , drop = FALSE]
  })
  mv0pv0 <- lapply(pl, function(x) mv0 %*% x %*% v0)

  a1 <- 0
  a2 <- 0
  for (i in seq_along(pl)) {
    for (j in seq_along(pl)) {
      a1 <- a1 + w[i, j] * tr(mv0pv0[[i]]) * tr(mv0pv0[[j]])
      a2 <- a2 + w[i, j] * tr(mv0pv0[[i]] %*% mv0pv0[[j]])
    }
  }

  b <- 1 / (2 * nl) * (a1 + 6 * a2)
  e_star <- 1 / (1 - a2 / nl)
  g <- ((nl + 1) * a1 - (nl + 4) * a2) / ((nl + 2) * a2)
  denom <- 3 * nl + 2 - 2 * g
  c1 <- g / denom
  c2 <- (nl - g) / denom
  c3 <- (nl + 2 - g) / denom
  v_star <- 2 / nl * (1 + c1 * b) / (1 - c2 * b)^2 / (1 - c3 * b)
  rho <- v_star / (2 * e_star^2)
  den_df <- 4 + (nl + 2) / (nl * rho - 1)
  lambda <- den_df / (e_star * (den_df - 2))

  c(
    num_df = nl,
    den_df = den_df,
    lambda = lambda,
    a1 = a1,
    a2 = a2,
    b = b,
    e_star = e_star,
    v_star = v_star,
    rho = rho
  )
}

add_df_components <- function(structure, label, l, var_beta, theta_vcov, p_stack) {
  vals <- kr_df_components(var_beta, l, theta_vcov, p_stack)
  for (nm in names(vals)) {
    add_row(structure, "df", nm, label, -1L, -1L, -1L, -1L, "", "", vals[[nm]])
  }
}

kr_adjustment_decomposition <- function(v, w, p, q, r) {
  n_beta <- ncol(v)
  n_theta <- ncol(w)
  theta_per_group <- nrow(q) / nrow(p)
  n_groups <- n_theta / theta_per_group
  stopifnot(nrow(v) == n_beta, nrow(w) == n_theta)
  stopifnot(nrow(p) == n_theta * n_beta, ncol(p) == n_beta)
  stopifnot(abs(theta_per_group - round(theta_per_group)) < sqrt(.Machine$double.eps))
  stopifnot(abs(n_groups - round(n_groups)) < sqrt(.Machine$double.eps))
  theta_per_group <- as.integer(round(theta_per_group))
  n_groups <- as.integer(round(n_groups))

  linear_delta <- matrix(0, nrow = n_beta, ncol = n_beta, dimnames = dimnames(v))
  second_delta <- matrix(0, nrow = n_beta, ncol = n_beta, dimnames = dimnames(v))
  linear_pair <- vector("list", n_theta * n_theta)
  second_pair <- vector("list", n_theta * n_theta)

  for (i in seq_len(n_theta)) {
    for (j in seq_len(n_theta)) {
      pair_index <- (i - 1L) * n_theta + j
      gi <- ceiling(i / theta_per_group)
      gj <- ceiling(j / theta_per_group)
      iid <- (i - 1L) * n_beta + 1L
      jid <- (j - 1L) * n_beta + 1L
      p_i <- p[iid:(iid + n_beta - 1L), , drop = FALSE]
      p_j <- p[jid:(jid + n_beta - 1L), , drop = FALSE]
      linear_middle <- -p_i %*% v %*% p_j
      second_middle <- matrix(0, nrow = n_beta, ncol = n_beta)

      if (gi == gj) {
        ii <- i - (gi - 1L) * theta_per_group
        jj <- j - (gi - 1L) * theta_per_group
        ijid <- ((ii - 1L) * theta_per_group + jj - 1L) * n_beta +
          (gi - 1L) * n_beta * theta_per_group^2 +
          1L
        q_ij <- q[ijid:(ijid + n_beta - 1L), , drop = FALSE]
        r_ij <- r[ijid:(ijid + n_beta - 1L), , drop = FALSE]
        linear_middle <- q_ij + linear_middle
        second_middle <- -0.25 * r_ij
      }

      linear_contrib <- 2 * w[i, j] * v %*% linear_middle %*% v
      second_contrib <- 2 * w[i, j] * v %*% second_middle %*% v
      dimnames(linear_contrib) <- dimnames(v)
      dimnames(second_contrib) <- dimnames(v)
      linear_pair[[pair_index]] <- linear_contrib
      second_pair[[pair_index]] <- second_contrib
      linear_delta <- linear_delta + linear_contrib
      second_delta <- second_delta + second_contrib
    }
  }

  full_delta <- linear_delta + second_delta
  list(
    linear_delta = linear_delta,
    second_delta = second_delta,
    full_delta = full_delta,
    adjusted_reconstructed = v + full_delta,
    linear_pair = linear_pair,
    second_pair = second_pair
  )
}

make_joint <- function(beta_names, cols) {
  l <- matrix(0, nrow = length(cols), ncol = length(beta_names), dimnames = list(NULL, beta_names))
  for (r in seq_along(cols)) {
    l[r, cols[[r]]] <- 1
  }
  l
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

  beta <- stats::coef(fit)
  var_beta <- as.matrix(component_or_slot(fit, "beta_vcov"))
  theta <- component_or_slot(fit, "theta_est")
  theta_vcov <- as.matrix(component_or_slot(fit, "theta_vcov"))
  var_beta_adj <- as.matrix(component_or_slot(fit, "beta_vcov_adj"))
  kr_comp <- component_or_slot(fit, "kr_comp")

  if (is.null(kr_comp$P) || is.null(kr_comp$Q) || is.null(kr_comp$R)) {
    stop("R mmrm fit did not expose kr_comp$P, kr_comp$Q, and kr_comp$R for ", case$name, ".")
  }

  beta_names <- fallback_names(beta, "beta_")
  theta_names <- fallback_names(theta, "theta_")

  add_vector(case$name, "beta", beta, beta_names)
  add_vector(case$name, "theta", theta, theta_names)

  add_matrix(case$name, "varbeta_unadjusted", "matrix", "", -1L, -1L, var_beta, beta_names, beta_names)
  add_matrix(case$name, "theta_vcov", "matrix", "", -1L, -1L, theta_vcov, theta_names, theta_names)
  add_matrix(case$name, "varbeta_adjusted", "matrix", "", -1L, -1L, var_beta_adj, beta_names, beta_names)
  add_matrix(case$name, "varbeta_kr_delta", "matrix", "", -1L, -1L, var_beta_adj - var_beta, beta_names, beta_names)

  add_vector(case$name, "se_ordinary", sqrt(diag(var_beta)), beta_names)
  add_vector(case$name, "se_kr", sqrt(diag(var_beta_adj)), beta_names)
  add_vector(case$name, "se_kr_delta", sqrt(diag(var_beta_adj)) - sqrt(diag(var_beta)), beta_names)

  p_stack <- as.matrix(kr_comp$P)
  q_stack <- as.matrix(kr_comp$Q)
  r_stack <- as.matrix(kr_comp$R)

  n_beta <- ncol(p_stack)
  n_theta <- nrow(p_stack) / n_beta

  kr_decomp <- kr_adjustment_decomposition(var_beta, theta_vcov, p_stack, q_stack, r_stack)
  add_matrix(case$name, "varbeta_kr_delta_linear", "matrix", "", -1L, -1L, kr_decomp$linear_delta, beta_names, beta_names)
  add_matrix(case$name, "varbeta_kr_delta_second", "matrix", "", -1L, -1L, kr_decomp$second_delta, beta_names, beta_names)
  add_matrix(case$name, "varbeta_kr_delta_reconstructed", "matrix", "", -1L, -1L, kr_decomp$full_delta, beta_names, beta_names)
  add_matrix(case$name, "varbeta_adjusted_reconstructed", "matrix", "", -1L, -1L, kr_decomp$adjusted_reconstructed, beta_names, beta_names)
  add_matrix(case$name, "varbeta_adjusted_reconstruction_error", "matrix", "", -1L, -1L, kr_decomp$adjusted_reconstructed - var_beta_adj, beta_names, beta_names)

  pair_index <- 1L
  for (h in seq_len(n_theta)) {
    for (jj in seq_len(n_theta)) {
      add_matrix(case$name, "varbeta_kr_delta_linear_pair", "matrix", "", h - 1L, jj - 1L, kr_decomp$linear_pair[[pair_index]], beta_names, beta_names)
      add_matrix(case$name, "varbeta_kr_delta_second_pair", "matrix", "", h - 1L, jj - 1L, kr_decomp$second_pair[[pair_index]], beta_names, beta_names)
      pair_index <- pair_index + 1L
    }
  }
  if (n_theta != length(theta)) {
    stop("Unexpected P-stack dimensions for ", case$name, ": nrow(P) / n_beta does not equal length(theta).")
  }

  theta_per_group <- nrow(q_stack) / nrow(p_stack)
  if (abs(theta_per_group - round(theta_per_group)) > sqrt(.Machine$double.eps)) {
    stop("Unexpected Q-stack dimensions for ", case$name, ": theta_per_group is not an integer.")
  }
  theta_per_group <- as.integer(round(theta_per_group))

  for (h in seq_len(n_theta)) {
    p_start <- (h - 1L) * n_beta + 1L
    block <- p_stack[p_start:(p_start + n_beta - 1L), , drop = FALSE]
    add_matrix(case$name, "P", "matrix", "", h - 1L, -1L, block, beta_names, beta_names)
  }

  zero_block <- matrix(0, nrow = n_beta, ncol = n_beta)
  for (h in seq_len(n_theta)) {
    for (jj in seq_len(n_theta)) {
      gi <- ceiling(h / theta_per_group)
      gj <- ceiling(jj / theta_per_group)

      if (gi != gj) {
        q_block <- zero_block
        r_block <- zero_block
      } else {
        ii <- h - (gi - 1L) * theta_per_group
        j_local <- jj - (gi - 1L) * theta_per_group
        qr_start <- ((ii - 1L) * theta_per_group + j_local - 1L) * n_beta +
          (gi - 1L) * n_beta * theta_per_group^2 +
          1L
        q_block <- q_stack[qr_start:(qr_start + n_beta - 1L), , drop = FALSE]
        r_block <- r_stack[qr_start:(qr_start + n_beta - 1L), , drop = FALSE]
      }

      add_matrix(case$name, "Q", "matrix", "", h - 1L, jj - 1L, q_block, beta_names, beta_names)
      add_matrix(case$name, "R", "matrix", "", h - 1L, jj - 1L, r_block, beta_names, beta_names)
    }
  }

  for (idx in seq_along(beta_names)) {
    l <- matrix(0, nrow = 1L, ncol = length(beta_names), dimnames = list(NULL, beta_names))
    l[1L, idx] <- 1
    add_df_components(case$name, paste0("coef:", beta_names[[idx]]), l, var_beta, theta_vcov, p_stack)
  }

  site_central <- match("site_central", beta_names)
  site_south <- match("site_south", beta_names)
  trt <- match("treatment_active", beta_names)
  trt_age <- match("treatment_active:age_centered_8", beta_names)

  if (!is.na(site_central) && !is.na(site_south)) {
    add_df_components(case$name, "term:clinic_site", make_joint(beta_names, c(site_central, site_south)), var_beta, theta_vcov, p_stack)
  }
  if (!is.na(trt) && !is.na(trt_age)) {
    add_df_components(case$name, "joint:treatment_active+treatment_active:age_centered_8", make_joint(beta_names, c(trt, trt_age)), var_beta, theta_vcov, p_stack)
  }
  non_intercept <- setdiff(seq_along(beta_names), match("(Intercept)", beta_names))
  if (length(non_intercept) > 1L) {
    add_df_components(case$name, "joint:all_non_intercept", make_joint(beta_names, non_intercept), var_beta, theta_vcov, p_stack)
  }
}

out <- do.call(rbind, rows)
out <- out[order(out$structure, out$kind, out$subkind, out$label, out$h, out$j, out$row, out$col), ]

csv_out <- "C:\\Users\\peter\\Desktop\\ddd\\kr_mmrm_multicovariate_missing_internal_reference.csv"
write.csv(out, csv_out, row.names = FALSE)

test_data_out <-  "C:\\Users\\peter\\Desktop\\ddd\\kr_mmrm_multicovariate_missing_internal_reference_test_data_out.csv"
if (dir.exists(dirname(test_data_out))) {
  write.csv(out, test_data_out, row.names = FALSE)
}

message("Wrote MMRM multicovariate missing internal KR reference: ", csv_out)
if (file.exists(test_data_out)) {
  message("Also wrote MSTest reference copy: ", test_data_out)
}
message("Note: theta/theta_vcov/P/Q/R are exported in raw R mmrm internal parameterization. ",
        "The MSTest assertion compares invariant/final KR ingredients by default and keeps raw rows for manual diagnostics.")

utils::sessionInfo()
