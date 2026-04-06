# GEE reference calculations for BESHStatNG unit tests
#
# Outputs:
#   TestData/gee_expected_outputs.csv
#   TestData/gee_expected_residuals.csv
#
# Requirements:
#   install.packages(c('geepack'))
#
# Notes:
# - Uses geepack::geeglm to obtain beta and working-correlation estimates.
# - Recomputes robust, naive, and bias-corrected (Mancl & DeRouen) covariance
#   using matrix formulas aligned with the VB implementation.
# - Pearson scale phi is computed as sum(Pearson^2) / N (useP=FALSE), matching
#   the default unit-test settings.

suppressPackageStartupMessages({
  library(geepack)
})

BASE_DIR <- getwd()
DATA_DIR <- file.path(BASE_DIR, "TestData")

EPS <- 1e-12

clip_var <- function(v) {
  v[v < EPS] <- EPS
  v[is.nan(v)] <- EPS
  v
}

inv_link <- function(fam, link, eta) {
  if (fam == "binomial" && link == "logit") return(1 / (1 + exp(-eta)))
  if (link == "log") return(exp(eta))
  if (link == "identity") return(eta)
  stop(paste("Unsupported link", link))
}

inv_link_deriv <- function(fam, link, mu, eta) {
  if (fam == "binomial" && link == "logit") return(mu * (1 - mu))
  if (link == "log") return(mu)
  if (link == "identity") return(rep(1, length(mu)))
  stop(paste("Unsupported link", link))
}

variance_fun <- function(fam, mu) {
  if (fam == "binomial") return(mu * (1 - mu))
  if (fam == "poisson") return(mu)
  if (fam == "gaussian") return(rep(1, length(mu)))
  stop(paste("Unsupported family", fam))
}

deviance_contrib <- function(fam, y, mu) {
  if (fam == "gaussian") return((y - mu)^2)
  if (fam == "poisson") {
    out <- rep(0, length(y))
    m <- y > 0
    out[m] <- 2 * (y[m] * log(y[m] / mu[m]) - (y[m] - mu[m]))
    out[!m] <- 2 * (0 - (0 - mu[!m]))
    return(out)
  }
  if (fam == "binomial") {
    mu <- pmin(pmax(mu, EPS), 1 - EPS)
    out <- rep(0, length(y))
    m1 <- y == 1
    m0 <- y == 0
    out[m1] <- 2 * log(1 / mu[m1])
    out[m0] <- 2 * log(1 / (1 - mu[m0]))
    m <- !(m1 | m0)
    if (any(m)) {
      out[m] <- 2 * (y[m] * log(y[m] / mu[m]) + (1 - y[m]) * log((1 - y[m]) / (1 - mu[m])))
    }
    return(out)
  }
  stop(paste("Unsupported family", fam))
}

alpha_to_unstructured <- function(alpha, q) {
  # geepack provides alpha as a vector of length q*(q-1)/2 for "unstructured"
  R <- diag(1, q)
  k <- 1
  for (i in 1:(q-1)) {
    for (j in (i+1):q) {
      R[i, j] <- alpha[k]
      R[j, i] <- alpha[k]
      k <- k + 1
    }
  }
  R
}

build_R <- function(corstr, alpha, k, waves=NULL, unique_waves=NULL) {
  if (corstr == "independence") {
    return(diag(1, k))
  }
  if (corstr == "exchangeable") {
    rho <- as.numeric(alpha[1])
    R <- matrix(rho, k, k)
    diag(R) <- 1
    return(R)
  }
  if (corstr == "ar1") {
    rho <- as.numeric(alpha[1])
    idx <- 0:(k-1)
    return(rho ^ abs(outer(idx, idx, "-")))
  }
  if (corstr == "unstructured") {
    # Use full correlation matrix dimension q from unique_waves and subset
    if (is.null(unique_waves)) stop("unique_waves required for unstructured")
    q <- length(unique_waves)
    fullR <- alpha_to_unstructured(alpha, q)
    if (is.null(waves)) {
      # no waves: assume first k positions
      return(fullR[1:k, 1:k, drop=FALSE])
    }
    # map waves to indices in unique_waves
    map <- match(waves, unique_waves)
    return(fullR[map, map, drop=FALSE])
  }
  stop(paste("Unsupported corstr", corstr))
}

calc_phi <- function(fam, mu, y, useP=FALSE, p=NULL) {
  v <- clip_var(variance_fun(fam, mu))
  pearson <- (y - mu) / sqrt(v)
  ss <- sum(pearson^2)
  n <- length(y)
  if (useP) {
    if (is.null(p)) stop("p required when useP=TRUE")
    return(ss / (n - p))
  } else {
    return(ss / n)
  }
}

calc_cov_mats <- function(fam, link, beta, corstr, alpha, df, offset_col=NULL, useP=FALSE) {
  # Build X and per-cluster pieces
  X <- model.matrix(~ x1 + x2, data=df)
  y <- df$y
  groups <- df$cluster

  waves_all <- if ("time" %in% names(df)) df$time else NULL
  if (is.null(waves_all)) {
    # VB assigns sequential waves within cluster
    waves_all <- ave(groups, groups, FUN=function(z) seq_along(z) - 1)
  }
  unique_waves <- sort(unique(waves_all))

  eta <- as.vector(X %*% beta)
  if (!is.null(offset_col)) eta <- eta + df[[offset_col]]
  mu <- inv_link(fam, link, eta)

  # Pearson scale
  p <- length(beta)
  phi <- calc_phi(fam, mu, y, useP=useP, p=p)

  B <- matrix(0, p, p)
  C <- matrix(0, p, p)

  for (g in unique(groups)) {
    idx <- which(groups == g)
    Xi <- X[idx, , drop=FALSE]
    yi <- y[idx]
    etai <- eta[idx]
    mui <- mu[idx]
    waves_i <- waves_all[idx]

    dmu <- inv_link_deriv(fam, link, mui, etai)
    D <- Xi * as.vector(dmu)

    vmu <- clip_var(variance_fun(fam, mui))
    sdev <- sqrt(vmu)
    k <- length(idx)

    Ri <- build_R(corstr, alpha, k, waves=waves_i, unique_waves=unique_waves)
    V <- diag(sdev) %*% Ri %*% diag(sdev)

    VinvD <- solve(V, D)
    VinvR <- solve(V, (yi - mui))

    B <- B + t(D) %*% VinvD
    s <- t(D) %*% VinvR
    C <- C + s %*% t(s)
  }

  Binv <- solve(B)
  cov_naive <- Binv * phi
  cov_robust <- Binv %*% C %*% Binv

  list(phi=phi, cov_naive=cov_naive, cov_robust=cov_robust, mu=mu, eta=eta, X=X, waves=waves_all, unique_waves=unique_waves)
}

bias_reduced_cov <- function(fam, link, beta, corstr, alpha, df, cov_naive, phi, offset_col=NULL, useP=FALSE) {
  X <- model.matrix(~ x1 + x2, data=df)
  y <- df$y
  groups <- df$cluster

  waves_all <- if ("time" %in% names(df)) df$time else NULL
  if (is.null(waves_all)) {
    waves_all <- ave(groups, groups, FUN=function(z) seq_along(z) - 1)
  }
  unique_waves <- sort(unique(waves_all))

  eta <- as.vector(X %*% beta)
  if (!is.null(offset_col)) eta <- eta + df[[offset_col]]
  mu <- inv_link(fam, link, eta)

  p <- length(beta)
  bcm <- matrix(0, p, p)

  for (g in unique(groups)) {
    idx <- which(groups == g)
    Xi <- X[idx, , drop=FALSE]
    yi <- y[idx]
    etai <- eta[idx]
    mui <- mu[idx]
    waves_i <- waves_all[idx]

    resid <- yi - mui
    dmu <- inv_link_deriv(fam, link, mui, etai)
    D <- Xi * as.vector(dmu)

    vmu <- clip_var(variance_fun(fam, mui))
    sdev <- sqrt(vmu)
    k <- length(idx)

    Ri <- build_R(corstr, alpha, k, waves=waves_i, unique_waves=unique_waves)
    V <- diag(sdev) %*% Ri %*% diag(sdev)

    VinvD <- solve(V, D) / phi

    # h = t(VinvD %*% cov_naive %*% t(D))  -> k x k
    h <- t(VinvD %*% cov_naive %*% t(D))
    tmp2 <- diag(1, k) - h

    # aresid = solve(tmp2, resid)
    aresid <- solve(tmp2, resid)

    srt <- solve(V, aresid)
    srt2 <- as.vector(t(D) %*% srt) / phi

    bcm <- bcm + (srt2 %*% t(srt2))
  }

  cov_naive %*% bcm %*% cov_naive
}

add_expected_rows <- function(out_rows, model, beta, cov, phi, qic=NULL, qicu=NULL, dep=NULL, corstr=NULL) {
  nm <- c("Intercept", "x1", "x2")
  se <- sqrt(diag(cov))
  z <- beta / se
  p <- 2 * (1 - pnorm(abs(z)))

  for (i in seq_along(beta)) {
    out_rows[[length(out_rows)+1]] <- data.frame(model=model, key=paste0("coef_", nm[i]), value=beta[i])
    out_rows[[length(out_rows)+1]] <- data.frame(model=model, key=paste0("se_", nm[i]), value=se[i])
    out_rows[[length(out_rows)+1]] <- data.frame(model=model, key=paste0("z_", nm[i]), value=z[i])
    out_rows[[length(out_rows)+1]] <- data.frame(model=model, key=paste0("p_", nm[i]), value=p[i])
  }

  out_rows[[length(out_rows)+1]] <- data.frame(model=model, key="scale_phi", value=phi)
  if (!is.null(qic)) out_rows[[length(out_rows)+1]] <- data.frame(model=model, key="qic", value=qic)
  if (!is.null(qicu)) out_rows[[length(out_rows)+1]] <- data.frame(model=model, key="qicu", value=qicu)

  if (!is.null(dep) && !is.null(corstr)) {
    if (corstr %in% c("exchangeable", "ar1")) {
      out_rows[[length(out_rows)+1]] <- data.frame(model=model, key="dep_rho", value=as.numeric(dep[1]))
    } else if (corstr == "unstructured") {
      q <- nrow(dep)
      for (i in 1:q) {
        for (j in 1:q) {
          out_rows[[length(out_rows)+1]] <- data.frame(model=model, key=paste0("dep_", i-1, "_", j-1), value=dep[i,j])
        }
      }
    }
  }

  out_rows
}

add_residual_rows <- function(out_rows, model, df, fam, link, beta, phi, offset_col=NULL) {
  X <- model.matrix(~ x1 + x2, data=df)
  eta <- as.vector(X %*% beta)
  if (!is.null(offset_col)) eta <- eta + df[[offset_col]]
  mu <- inv_link(fam, link, eta)

  raw <- df$y - mu
  v <- clip_var(variance_fun(fam, mu))
  pearson <- raw / sqrt(v)
  dev <- sign(raw) * sqrt(deviance_contrib(fam, df$y, mu))
  dmu <- inv_link_deriv(fam, link, mu, eta)
  work <- raw / dmu

  # VB: for binomial/poisson with pScaleType=0 -> scaled residuals use phi_resid=1
  phi_resid <- if (fam %in% c("binomial", "poisson")) 1.0 else phi
  std_dev <- dev / sqrt(phi_resid)
  std_pearson <- pearson / sqrt(phi_resid)

  tmp <- data.frame(
    model=model,
    id=df$id,
    `Raw Resid.`=raw,
    `Deviance Resid.`=dev,
    `Pearson Resid.`=pearson,
    `Std Deviance Resid.`=std_dev,
    `Std Pearson Resid.`=std_pearson,
    `Working Resid.`=work
  )

  out_rows[[length(out_rows)+1]] <- tmp
  out_rows
}

# ----------------- model grid -----------------
MODELS <- list(
  # Binomial/logit full dataset
  list(name="GEE_Binomial_Logit_Independence_Robust", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="independence", se="Robust"),
  list(name="GEE_Binomial_Logit_Independence_Naive", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="independence", se="Naive"),
  list(name="GEE_Binomial_Logit_Independence_BiasReduced", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="independence", se="BiasReduced"),

  list(name="GEE_Binomial_Logit_Exchangeable_Robust", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="exchangeable", se="Robust"),
  list(name="GEE_Binomial_Logit_Exchangeable_Naive", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="exchangeable", se="Naive"),
  list(name="GEE_Binomial_Logit_Exchangeable_BiasReduced", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="exchangeable", se="BiasReduced"),

  list(name="GEE_Binomial_Logit_Autoregressive_Robust", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="ar1", se="Robust"),
  list(name="GEE_Binomial_Logit_Autoregressive_Naive", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="ar1", se="Naive"),
  list(name="GEE_Binomial_Logit_Autoregressive_BiasReduced", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="ar1", se="BiasReduced"),

  list(name="GEE_Binomial_Logit_Unstructured_Robust", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="unstructured", se="Robust"),
  list(name="GEE_Binomial_Logit_Unstructured_Naive", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="unstructured", se="Naive"),
  list(name="GEE_Binomial_Logit_Unstructured_BiasReduced", file="gee_binomial_logit_full.csv", fam="binomial", link="logit", corstr="unstructured", se="BiasReduced"),

  # Missing time variant
  list(name="GEE_Binomial_Logit_Exchangeable_Robust_MissingTime", file="gee_binomial_logit_missing_time.csv", fam="binomial", link="logit", corstr="exchangeable", se="Robust"),

  # Poisson/log with offset
  list(name="GEE_Poisson_Log_Independence_Robust_Offset", file="gee_poisson_log_offset_full.csv", fam="poisson", link="log", corstr="independence", se="Robust", offset="offset"),
  list(name="GEE_Poisson_Log_Independence_Naive_Offset", file="gee_poisson_log_offset_full.csv", fam="poisson", link="log", corstr="independence", se="Naive", offset="offset"),
  list(name="GEE_Poisson_Log_Exchangeable_Robust_Offset", file="gee_poisson_log_offset_full.csv", fam="poisson", link="log", corstr="exchangeable", se="Robust", offset="offset"),
  list(name="GEE_Poisson_Log_Exchangeable_Naive_Offset", file="gee_poisson_log_offset_full.csv", fam="poisson", link="log", corstr="exchangeable", se="Naive", offset="offset"),

  # Gaussian/identity
  list(name="GEE_Gaussian_Identity_Unstructured_Robust", file="gee_gaussian_identity_full.csv", fam="gaussian", link="identity", corstr="unstructured", se="Robust"),
  list(name="GEE_Gaussian_Identity_Unstructured_Naive", file="gee_gaussian_identity_full.csv", fam="gaussian", link="identity", corstr="unstructured", se="Naive")
)

expected_rows <- list()
resid_rows <- list()

for (spec in MODELS) {
  df <- read.csv(file.path(DATA_DIR, spec$file))

  # geeglm family object
  fam_obj <- switch(spec$fam,
    binomial = binomial("logit"),
    poisson = poisson("log"),
    gaussian = gaussian("identity")
  )

  # waves/time (geepack uses waves for ar1/unstructured)
  waves <- if ("time" %in% names(df)) df$time else NULL

  # build formula
  if (!is.null(spec$offset)) {
    fmla <- as.formula(paste0("y ~ x1 + x2 + offset(", spec$offset, ")"))
  } else {
    fmla <- y ~ x1 + x2
  }

  fit <- geeglm(
    fmla,
    id = cluster,
    waves = waves,
    data = df,
    family = fam_obj,
    corstr = spec$corstr
  )

  beta <- as.numeric(coef(fit))
  alpha <- fit$geese$alpha

  # For unstructured, build full dep matrix for reporting
  dep_mat <- NULL
  if (spec$corstr == "unstructured") {
    if (is.null(waves)) {
      q <- max(ave(df$cluster, df$cluster, FUN=function(z) seq_along(z)))
      dep_mat <- alpha_to_unstructured(alpha, q)
    } else {
      q <- length(sort(unique(waves)))
      dep_mat <- alpha_to_unstructured(alpha, q)
    }
  }

  # Covariance calculations aligned to VB
  covs <- calc_cov_mats(spec$fam, spec$link, beta, spec$corstr, alpha, df, offset_col=spec$offset, useP=FALSE)
  phi <- covs$phi
  cov_naive <- covs$cov_naive
  cov_robust <- covs$cov_robust

  cov_use <- switch(spec$se,
    Robust = cov_robust,
    Naive = cov_naive,
    BiasReduced = bias_reduced_cov(spec$fam, spec$link, beta, spec$corstr, alpha, df, cov_naive, phi, offset_col=spec$offset, useP=FALSE)
  )

  # QIC (optional)
  qic <- NA
  qicu <- NA
  try({
    q <- geepack::QIC(fit)
    # QIC() returns a matrix-like object with columns QIC and QICu
    if (is.matrix(q) || is.data.frame(q)) {
      if ("QIC" %in% colnames(q)) qic <- q[1, "QIC"]
      if ("QICu" %in% colnames(q)) qicu <- q[1, "QICu"]
    }
  }, silent=TRUE)

  expected_rows <- add_expected_rows(
    expected_rows,
    spec$name,
    beta,
    cov_use,
    phi,
    qic=qic,
    qicu=qicu,
    dep=if (spec$corstr %in% c("exchangeable", "ar1")) alpha else dep_mat,
    corstr=spec$corstr
  )

  # Residual references (subset only, same as python generator)
  if (spec$name %in% c(
    "GEE_Binomial_Logit_Exchangeable_Robust",
    "GEE_Poisson_Log_Exchangeable_Robust_Offset",
    "GEE_Gaussian_Identity_Unstructured_Robust"
  )) {
    resid_rows <- add_residual_rows(resid_rows, spec$name, df, spec$fam, spec$link, beta, phi, offset_col=spec$offset)
  }
}

expected_df <- do.call(rbind, expected_rows)
resid_df <- do.call(rbind, resid_rows)

write.csv(expected_df, file.path(DATA_DIR, "gee_expected_outputs.csv"), row.names=FALSE)
write.csv(resid_df, file.path(DATA_DIR, "gee_expected_residuals.csv"), row.names=FALSE)

cat("Wrote", file.path(DATA_DIR, "gee_expected_outputs.csv"), "\n")
cat("Wrote", file.path(DATA_DIR, "gee_expected_residuals.csv"), "\n")
