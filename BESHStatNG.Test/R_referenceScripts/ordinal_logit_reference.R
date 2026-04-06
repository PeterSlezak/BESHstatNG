# Ordinal logistic regression reference for BESHStatNG OrdinalLogitModel
#
# This script computes (as closely as possible) the same outputs produced by
# BESHStatNG/src/RegModels/OrdinalLogitModel.vb for the included test datasets.
#
# Dataset(s):
#   - TestData/ordinal_logit_dataset_basic.csv
#   - TestData/ordinal_logit_dataset_intercept_only.csv
#
# Notes:
#   * The VB implementation is a proportional-odds cumulative logit model:
#       logit(P(Y <= c_k)) = alpha_{k+1} - eta_i
#       eta_i = x_i^T beta + offset_i
#   * BIC, pseudo-R2, GOF and residuals use nobs = sum(weights) when weights exist.
#   * Leverage is computed as h_i = tr(I_i %*% Cov) where I_i is per-observation
#     observed-information contribution (I_i = -w_i * H_i), and Cov = inv(Info)
#     with ridge added to the diagonal (ridge = 1e-10, matching VB default).
#
# The script:
#   1) Fits the model using ordinal::clm (supports offset + weights).
#   2) Recomputes loglik, GOF, classification, residuals, leverage using the
#      same formulas as the VB implementation.
#   3) Prints results and also emits VB-friendly arrays.

options(stringsAsFactors = FALSE)

# ---------------------------
# Utilities
# ---------------------------
logistic_stable <- function(x) {
  # Stable logistic to avoid overflow
  out <- numeric(length(x))
  pos <- x >= 0
  out[pos] <- 1 / (1 + exp(-x[pos]))
  ex <- exp(x[!pos])
  out[!pos] <- ex / (1 + ex)
  out
}

fmt_vb_vec <- function(x, digits = 12) {
  paste0(
    "{\n        ",
    paste(sprintf(paste0("%.", digits, "f"), x), collapse = ",\n        "),
    "\n    }"
  )
}

fmt_vb_mat <- function(M, digits = 12) {
  nr <- nrow(M); nc <- ncol(M)
  rows <- character(nr)
  for (i in seq_len(nr)) {
    rows[i] <- paste0("        {", paste(sprintf(paste0("%.", digits, "f"), M[i, ]), collapse = ", "), "}")
  }
  paste0("{\n", paste(rows, collapse = ",\n"), "\n    }")
}

# Extract beta/alpha from a clm-like object robustly across versions
extract_params <- function(fit) {
  b_all <- tryCatch(stats::coef(fit), error = function(e) NULL)

  beta <- NULL
  alpha <- NULL

  if (!is.null(fit$beta)) beta <- fit$beta
  if (!is.null(fit$alpha)) alpha <- fit$alpha
  if (is.null(alpha) && !is.null(fit$Theta)) alpha <- fit$Theta

  if (is.null(beta) || is.null(alpha)) {
    # Fallback: split coef() by names
    if (!is.null(b_all)) {
      nms <- names(b_all)
      thr_idx <- grepl("\\|", nms) | grepl("threshold", nms, ignore.case = TRUE)
      if (is.null(alpha)) alpha <- unname(b_all[thr_idx])
      if (is.null(beta)) beta <- unname(b_all[!thr_idx])
    }
  }

  if (is.null(beta)) beta <- numeric(0)
  if (is.null(alpha)) stop("Could not extract thresholds (alpha) from fitted model.")

  list(beta = as.numeric(beta), alpha = as.numeric(alpha))
}

# Category derivatives (port of VB GetCategoryDerivatives)
get_cat_derivs <- function(y_idx, gk, fk, sk) {
  K <- length(gk) + 1
  Km1 <- K - 1

  if (y_idx == 0) {
    py <- gk[1]
    dp_deta <- -fk[1]
    d2p_deta2 <- fk[1] * sk[1]

    involvedA <- c(0)
    dp_da <- c(fk[1])
    d2p_da2 <- c(fk[1] * sk[1])
    d2p_deta_da <- c(-fk[1] * sk[1])

    return(list(py = py,
                dp_deta = dp_deta,
                d2p_deta2 = d2p_deta2,
                involvedA = involvedA,
                dp_da = dp_da,
                d2p_da2 = d2p_da2,
                d2p_deta_da = d2p_deta_da))
  }

  if (y_idx == (K - 1)) {
    j <- Km1 - 1  # 0-based alpha index
    py <- 1 - gk[j + 1]
    dp_deta <- fk[j + 1]
    d2p_deta2 <- -fk[j + 1] * sk[j + 1]

    involvedA <- c(j)
    dp_da <- c(-fk[j + 1])
    d2p_da2 <- c(-fk[j + 1] * sk[j + 1])
    d2p_deta_da <- c(fk[j + 1] * sk[j + 1])

    return(list(py = py,
                dp_deta = dp_deta,
                d2p_deta2 = d2p_deta2,
                involvedA = involvedA,
                dp_da = dp_da,
                d2p_da2 = d2p_da2,
                d2p_deta_da = d2p_deta_da))
  }

  # Middle categories
  k <- y_idx  # 0-based y_idx uses boundary k and k-1
  py <- gk[k + 1] - gk[k]
  dp_deta <- -(fk[k + 1] - fk[k])
  d2p_deta2 <- fk[k + 1] * sk[k + 1] - fk[k] * sk[k]

  involvedA <- c(k, k - 1)
  dp_da <- c(fk[k + 1], -fk[k])
  d2p_da2 <- c(fk[k + 1] * sk[k + 1], -fk[k] * sk[k])
  d2p_deta_da <- c(-fk[k + 1] * sk[k + 1], fk[k] * sk[k])

  list(py = py,
       dp_deta = dp_deta,
       d2p_deta2 = d2p_deta2,
       involvedA = involvedA,
       dp_da = dp_da,
       d2p_da2 = d2p_da2,
       d2p_deta_da = d2p_deta_da)
}

predict_row_probs <- function(xrow, offset_i, beta, alpha) {
  eta <- sum(xrow * beta) + offset_i
  K <- length(alpha) + 1

  probs <- numeric(K)
  prev <- 0
  for (k in 1:(K - 1)) {
    Fk <- logistic_stable(alpha[k] - eta)
    pk <- if (k == 1) Fk else (Fk - prev)
    probs[k] <- max(0, pk)
    prev <- Fk
  }
  probs[K] <- max(0, 1 - prev)

  s <- sum(probs)
  if (s > 0) probs <- probs / s
  probs
}

build_obs_hessian <- function(xrow, offset_i, y_idx, beta, alpha) {
  # Returns per-observation Hessian contribution of loglik (WITHOUT weight)
  p <- length(beta)
  q <- p + length(alpha)
  Hobs <- matrix(0, nrow = q, ncol = q)

  eta <- sum(xrow * beta) + offset_i
  if (any(diff(alpha) <= 0)) return(Hobs)

  # gk/fk/sk for K-1 boundaries
  t <- alpha - eta
  gk <- logistic_stable(t)
  fk <- gk * (1 - gk)
  sk <- 1 - 2 * gk

  d <- get_cat_derivs(y_idx, gk, fk, sk)
  py <- d$py
  if (is.na(py) || py <= 0) return(Hobs)
  pySafe <- max(py, 1e-300)

  dp_deta <- d$dp_deta
  d2p_deta2 <- d$d2p_deta2

  involvedA <- d$involvedA
  dp_da <- d$dp_da
  d2p_da2 <- d$d2p_da2
  d2p_deta_da <- d$d2p_deta_da

  d2logp_deta2 <- (d2p_deta2 / pySafe) - (dp_deta * dp_deta) / (pySafe * pySafe)

  # beta-beta
  if (p > 0) {
    for (u in 1:p) {
      for (v in 1:p) {
        Hobs[u, v] <- Hobs[u, v] + d2logp_deta2 * xrow[u] * xrow[v]
      }
    }
  }

  # beta-alpha and alpha-alpha (diag for involved)
  for (tIdx in seq_along(involvedA)) {
    aIdx <- involvedA[tIdx]  # 0-based

    d2logp_deta_da <- (d2p_deta_da[tIdx] / pySafe) - (dp_deta * dp_da[tIdx]) / (pySafe * pySafe)

    if (p > 0) {
      for (col in 1:p) {
        v <- d2logp_deta_da * xrow[col]
        Hobs[col, p + aIdx + 1] <- Hobs[col, p + aIdx + 1] + v
        Hobs[p + aIdx + 1, col] <- Hobs[p + aIdx + 1, col] + v
      }
    }

    d2logp_da2 <- (d2p_da2[tIdx] / pySafe) - (dp_da[tIdx] * dp_da[tIdx]) / (pySafe * pySafe)
    Hobs[p + aIdx + 1, p + aIdx + 1] <- Hobs[p + aIdx + 1, p + aIdx + 1] + d2logp_da2
  }

  if (length(involvedA) == 2) {
    a0 <- involvedA[1]
    a1 <- involvedA[2]
    off <- -(dp_da[1] * dp_da[2]) / (pySafe * pySafe)
    Hobs[p + a0 + 1, p + a1 + 1] <- Hobs[p + a0 + 1, p + a1 + 1] + off
    Hobs[p + a1 + 1, p + a0 + 1] <- Hobs[p + a1 + 1, p + a0 + 1] + off
  }

  Hobs
}

# GOF profile deviance (port of VB ComputeProfileDevianceGOF)
profile_gof <- function(y_idx, X, offset, w, beta, alpha, keyDigits = 12) {
  fmt <- paste0("%.", max(0, keyDigits), "f")
  key <- apply(X, 1, function(r) paste(sprintf(fmt, r), collapse = "|"))
  if (!is.null(offset)) key <- paste0(key, "|", sprintf(fmt, offset))

  K <- length(alpha) + 1
  q <- ncol(X) + (K - 1)

  # Aggregate counts per pattern
  groups <- split(seq_along(y_idx), key)
  G <- length(groups)
  if (G <= 0) return(list(dev = NA_real_, df = 0, p = NA_real_))

  llSat <- 0
  llModel <- 0

  for (g in groups) {
    m <- sum(w[g])
    if (m <= 0) next

    repRow <- g[1]
    pi <- predict_row_probs(X[repRow, ], offset[repRow], beta, alpha)

    counts <- numeric(K)
    for (k in 0:(K - 1)) {
      counts[k + 1] <- sum(w[g][y_idx[g] == k])
    }

    for (k in 1:K) {
      yk <- counts[k]
      if (yk > 0) {
        llModel <- llModel + yk * log(max(pi[k], 1e-300))
        llSat <- llSat + yk * log(max(yk / m, 1e-300))
      }
    }
  }

  dev <- 2 * (llSat - llModel)
  df <- max(1, G * (K - 1) - q)
  p <- 1 - pchisq(dev, df)
  list(dev = dev, df = df, p = p)
}

# ---------------------------
# Main runner
# ---------------------------
run_basic <- function(csv_path) {
  d <- read.csv(csv_path)

  # Determine category levels
  cats <- sort(unique(d$y))
  d$y_ord <- ordered(d$y, levels = cats)

  # predictors
  xnames <- c("x1", "x2")
  X <- as.matrix(d[, xnames, drop = FALSE])
  offset <- if ("offset" %in% names(d)) d$offset else rep(0, nrow(d))
  w <- if ("w" %in% names(d)) d$w else rep(1, nrow(d))

  # Fit via ordinal::clm (supports offset + weights)
  if (!requireNamespace("ordinal", quietly = TRUE)) {
    stop("Package 'ordinal' is required. Install it with install.packages('ordinal')")
  }
  library(ordinal)

  fit <- ordinal::clm(y_ord ~ x1 + x2 + offset(offset), data = d, weights = w, link = "logit", Hess = TRUE)
  fit0 <- ordinal::clm(y_ord ~ 1 + offset(offset), data = d, weights = w, link = "logit", Hess = TRUE)

  pfit <- extract_params(fit)
  beta <- pfit$beta
  alpha <- pfit$alpha
  pfit0 <- extract_params(fit0)
  alpha0 <- pfit0$alpha

  b <- c(beta, alpha)

  # --- log-likelihoods (weighted)
  y_idx <- match(as.integer(d$y), cats) - 1  # 0-based
  K <- length(cats)

  ll <- 0
  for (i in seq_len(nrow(d))) {
    pi <- predict_row_probs(X[i, ], offset[i], beta, alpha)
    ll <- ll + w[i] * log(max(pi[y_idx[i] + 1], 1e-300))
  }

  # null ll
  ll0 <- 0
  for (i in seq_len(nrow(d))) {
    pi0 <- predict_row_probs(rep(0, 0), offset[i], numeric(0), alpha0)
    ll0 <- ll0 + w[i] * log(max(pi0[y_idx[i] + 1], 1e-300))
  }

  nobs <- max(1, sum(w[w > 0]))
  kFull <- length(b)
  kNull <- K - 1

  aic <- -2 * ll + 2 * kFull
  bic <- -2 * ll + log(nobs) * kFull

  coxsnell <- 1 - exp((2 / nobs) * (ll0 - ll))
  denomNk <- 1 - exp((2 / nobs) * ll0)
  nagelkerke <- if (abs(denomNk) > 1e-14) coxsnell / denomNk else NA_real_
  mcfadden <- if (abs(ll0) > 1e-14) 1 - (ll / ll0) else NA_real_

  chi2 <- 2 * (ll - ll0)
  df_model <- kFull - kNull
  p_model <- if (df_model > 0) 1 - pchisq(chi2, df_model) else NA_real_

  # GOF deviance (pattern-based)
  gof <- profile_gof(y_idx, X, offset, w, beta, alpha, keyDigits = 12)

  # probabilities matrix
  probs <- matrix(0, nrow = nrow(d), ncol = K)
  for (i in seq_len(nrow(d))) probs[i, ] <- predict_row_probs(X[i, ], offset[i], beta, alpha)

  # classification confusion matrix
  pred <- apply(probs, 1, which.max) - 1
  conf <- matrix(0, nrow = K, ncol = K)
  for (i in seq_len(nrow(d))) {
    conf[y_idx[i] + 1, pred[i] + 1] <- conf[y_idx[i] + 1, pred[i] + 1] + w[i]
  }
  overall_acc <- sum(diag(conf)) / sum(conf)

  # residuals (deviance)
  dev_res <- numeric(nrow(d))
  for (i in seq_len(nrow(d))) {
    mu <- w[i] * probs[i, y_idx[i] + 1]
    dev <- 2 * w[i] * log(w[i] / max(mu, 1e-300))
    dev_res[i] <- sqrt(max(0, dev))
  }

  # leverage: compute Cov like VB (Info = -H + ridge I)
  ridge <- 1e-10
  q <- length(b)
  H_total <- matrix(0, nrow = q, ncol = q)
  for (i in seq_len(nrow(d))) {
    Hobs <- build_obs_hessian(X[i, ], offset[i], y_idx[i], beta, alpha)
    H_total <- H_total + w[i] * Hobs
  }
  Info <- -H_total + diag(ridge, q)
  Cov <- solve(Info)

  lev <- numeric(nrow(d))
  for (i in seq_len(nrow(d))) {
    Hobs <- build_obs_hessian(X[i, ], offset[i], y_idx[i], beta, alpha)
    I_i <- -w[i] * Hobs
    lev[i] <- sum(diag(I_i %*% Cov))
  }

  std_dev_res <- ifelse(lev < 1 & lev >= 0, dev_res / sqrt(pmax(1e-12, 1 - lev)), NA_real_)

  # standard errors (VB order) from Cov diagonal
  se <- sqrt(pmax(0, diag(Cov)))

  # Print summary
  cat("\n=== Ordinal logit reference: ", basename(csv_path), " ===\n", sep = "")
  cat("Coefficients (beta, alpha):\n"); print(b)
  cat("Std Errors (from Cov):\n"); print(se)
  cat(sprintf("LogLik: %.12f  NullLogLik: %.12f\n", ll, ll0))
  cat(sprintf("AIC: %.12f  BIC: %.12f\n", aic, bic))
  cat(sprintf("CoxSnellR2: %.12f  NagelkerkeR2: %.12f  McFaddenR2: %.12f\n", coxsnell, nagelkerke, mcfadden))
  cat(sprintf("Model Chi2: %.12f  df: %d  p: %.12g\n", chi2, df_model, p_model))
  cat(sprintf("GOF deviance: %.12f  df: %d  p: %.12g\n", gof$dev, gof$df, gof$p))
  cat(sprintf("Overall accuracy: %.12f\n", overall_acc))

  # Emit VB-friendly arrays for tests
  cat("\n--- VB arrays (paste into tests) ---\n")
  cat("expCoeffs = ", fmt_vb_vec(b, 16), "\n", sep="")
  cat("expSE = ", fmt_vb_vec(se, 16), "\n", sep="")
  cat("expProbs = ", fmt_vb_mat(probs, 12), "\n", sep="")
  cat("expDevRes = ", fmt_vb_vec(dev_res, 12), "\n", sep="")
  cat("expLev = ", fmt_vb_vec(lev, 12), "\n", sep="")
  cat("expStdDevRes = ", fmt_vb_vec(std_dev_res, 12), "\n", sep="")
  cat("expConf = ", fmt_vb_mat(conf, 1), "\n", sep="")

  invisible(list(
    coeffs = b,
    se = se,
    ll = ll,
    ll0 = ll0,
    aic = aic,
    bic = bic,
    pseudo = c(coxsnell = coxsnell, nagelkerke = nagelkerke, mcfadden = mcfadden),
    model_test = c(chi2 = chi2, df = df_model, p = p_model),
    gof = gof,
    overall_acc = overall_acc,
    conf = conf,
    probs = probs,
    dev_res = dev_res,
    lev = lev,
    std_dev_res = std_dev_res
  ))
}

run_intercept_only <- function(csv_path) {
  d <- read.csv(csv_path)
  cats <- sort(unique(d$y))
  d$y_ord <- ordered(d$y, levels = cats)
  offset <- if ("offset" %in% names(d)) d$offset else rep(0, nrow(d))
  w <- if ("w" %in% names(d)) d$w else rep(1, nrow(d))

  if (!requireNamespace("ordinal", quietly = TRUE)) {
    stop("Package 'ordinal' is required. Install it with install.packages('ordinal')")
  }
  library(ordinal)

  fit <- ordinal::clm(y_ord ~ 1 + offset(offset), data = d, weights = w, link = "logit", Hess = TRUE)
  pfit <- extract_params(fit)
  alpha <- pfit$alpha

  # VB start for alpha-only equals logit of weighted cumulative proportions
  total <- sum(w)
  cum1 <- w[1] / total
  cum12 <- sum(w[1:2]) / total
  a1 <- log(cum1 / (1 - cum1))
  a2 <- log(cum12 / (1 - cum12))

  cat("\n=== Intercept-only reference: ", basename(csv_path), " ===\n", sep="")
  cat("Thresholds from clm:\n"); print(alpha)
  cat("Analytic alpha (weighted cumulative logits):\n"); print(c(a1, a2))
}

# ---- Run (relative paths work when executed from the test project root) ----
if (interactive()) {
  run_basic(file.path("TestData", "ordinal_logit_dataset_basic.csv"))
  run_intercept_only(file.path("TestData", "ordinal_logit_dataset_intercept_only.csv"))
} else {
  # when called with Rscript, run basic by default
  args <- commandArgs(trailingOnly = TRUE)
  if (length(args) == 0) {
    run_basic(file.path("TestData", "ordinal_logit_dataset_basic.csv"))
  } else if (length(args) == 1) {
    run_basic(args[1])
  } else {
    run_basic(args[1])
    run_intercept_only(args[2])
  }
}
