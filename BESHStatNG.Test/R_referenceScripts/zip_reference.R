# zip_reference.R
# Reference implementation for BESHStatNG ZeroInflatedPoisson
# - Reads CSV test data (zip_dataset_basic_count.csv, zip_dataset_basic_zero.csv)
# - Fits ZIP with an EM algorithm matching the VB implementation (incl. over-relaxation + monotone fallback)
# - Writes R-computed outputs to *_R.csv files (so you can compare to the VB-produced expected files)

options(stringsAsFactors = FALSE)

read_csv <- function(path) {
  read.csv(path, check.names = FALSE)
}

logistic_stable <- function(x) {
  out <- numeric(length(x))
  pos <- x >= 0
  out[pos] <- 1 / (1 + exp(-x[pos]))
  ex <- exp(x[!pos])
  out[!pos] <- ex / (1 + ex)
  out
}

log_poisson_pmf <- function(y, mu) {
  y * log(mu) - mu - lgamma(y + 1)
}

log_zip_zero_term <- function(pi, mu) {
  # log( pi + (1-pi) * exp(-mu) ) via log-sum-exp
  logA <- log(pi)
  logB <- log(1 - pi) - mu
  m <- pmax(logA, logB)
  m + log(exp(logA - m) + exp(logB - m))
}

log_zip_positive_term <- function(pi, y, mu) {
  ifelse(pi >= 1, -Inf, log(1 - pi) + log_poisson_pmf(y, mu))
}

zip_loglik <- function(y, Xcount, beta, Xzero, gamma) {
  eta_c <- as.numeric(Xcount %*% beta)
  mu <- exp(eta_c)
  eta_z <- as.numeric(Xzero %*% gamma)
  pi <- logistic_stable(eta_z)

  ll <- numeric(length(y))
  is0 <- (y == 0)
  ll[is0] <- log_zip_zero_term(pi[is0], mu[is0])
  ll[!is0] <- log_zip_positive_term(pi[!is0], y[!is0], mu[!is0])
  sum(ll)
}

zip_em_fit <- function(y, Xcount, Xzero,
                       eps = 1e-9, max_em = 200, max_irls = 25,
                       s_init = 1.3, max_backtracks = 6) {

  n <- length(y)
  p_count <- ncol(Xcount)
  p_zero  <- ncol(Xzero)

  y0 <- sum(y == 0)
  if (y0 == 0) stop("No zero present in the data. ZIP cannot be fitted.")
  if (y0 == n) stop("All observations are zero. ZIP is not identifiable (no positive counts).")

  # ---- Initial estimates (match VB) ----
  nPos <- sum(y > 0)
  minPos <- max(30, 2 * p_count)

  wInit <- rep(1, n)
  if (nPos >= minPos) wInit <- ifelse(y > 0, 1, 0)

  # Poisson init
  dfc <- data.frame(y = y, Xcount[, -1, drop = FALSE])
  colnames(dfc) <- c("y", paste0("x", seq_len(ncol(dfc) - 1)))
  f_count <- as.formula("y ~ .")

  beta <- coef(glm(f_count, data = dfc, family = poisson(link = "log"),
                   weights = wInit,
                   control = glm.control(epsilon = eps, maxit = max_irls)))

  # Logistic init (swapped zeros/ones)
  y_swap <- ifelse(y == 0, 1, 0)
  dfz <- data.frame(y = y_swap, Xzero[, -1, drop = FALSE])
  colnames(dfz) <- c("y", paste0("z", seq_len(ncol(dfz) - 1)))
  f_zero <- as.formula("y ~ .")

  gamma <- coef(glm(f_zero, data = dfz, family = binomial(link = "logit"),
                    control = glm.control(epsilon = eps, maxit = max_irls)))

  # E-step
  mu <- exp(as.numeric(Xcount %*% beta))
  pi <- logistic_stable(as.numeric(Xzero %*% gamma))
  probi <- ifelse(y == 0, pi / (pi + (1 - pi) * exp(-mu)), 0)
  probi1 <- 1 - probi

  ll_new <- zip_loglik(y, Xcount, beta, Xzero, gamma)
  ll_old <- 2 * ll_new
  ll_change <- abs(ll_new - ll_old)

  it <- 0
  converged <- FALSE

  while (ll_change > eps && it <= max_em) {
    ll_old <- ll_new
    beta_old <- beta
    gamma_old <- gamma

    # M-step count: weighted Poisson
    beta_em <- coef(glm(f_count, data = dfc, family = poisson(link = "log"),
                        weights = probi1,
                        start = beta,
                        control = glm.control(epsilon = eps, maxit = max_irls)))

    # M-step zero: fractional structural zero indicator
    dfz2 <- dfz
    dfz2$y <- probi
    gamma_em <- coef(glm(f_zero, data = dfz2, family = binomial(link = "logit"),
                         start = gamma,
                         control = glm.control(epsilon = eps, maxit = max_irls)))

    ll_em <- zip_loglik(y, Xcount, beta_em, Xzero, gamma_em)

    # ---- Over-relaxation with monotone fallback (match VB) ----
    s <- s_init
    accepted <- FALSE
    beta_try <- beta_em
    gamma_try <- gamma_em
    ll_try <- -Inf

    for (bt in 0:max_backtracks) {
      beta_try <- beta_old + s * (beta_em - beta_old)
      gamma_try <- gamma_old + s * (gamma_em - gamma_old)
      ll_try <- zip_loglik(y, Xcount, beta_try, Xzero, gamma_try)

      if (ll_try >= ll_em) { accepted <- TRUE; break }
      s <- 1 + 0.5 * (s - 1)
      if (s <= 1 + 1e-6) break
    }

    if (accepted) {
      beta <- beta_try
      gamma <- gamma_try
      ll_new <- ll_try
    } else {
      beta <- beta_em
      gamma <- gamma_em
      ll_new <- ll_em
    }

    # E-step from accepted params
    mu <- exp(as.numeric(Xcount %*% beta))
    pi <- logistic_stable(as.numeric(Xzero %*% gamma))
    probi <- ifelse(y == 0, pi / (pi + (1 - pi) * exp(-mu)), 0)
    probi1 <- 1 - probi

    ll_change <- abs(ll_new - ll_old)
    it <- it + 1
    if (ll_change < eps) converged <- TRUE
  }

  list(beta = beta, gamma = gamma, loglik = ll_new, deviance = -2 * ll_new,
       mu = mu, pi = pi, iterations = it, ll_change = ll_change,
       converged = converged)
}

# Numerical Hessian (central differences), small p only
hessian_central <- function(fn, par, h = 1e-5) {
  p <- length(par)
  H <- matrix(0, p, p)
  f0 <- fn(par)

  for (i in 1:p) {
    for (j in i:p) {
      ei <- rep(0, p); ei[i] <- 1
      ej <- rep(0, p); ej[j] <- 1

      if (i == j) {
        f1 <- fn(par + h * ei)
        f2 <- fn(par - h * ei)
        H[i, i] <- (f1 - 2 * f0 + f2) / (h^2)
      } else {
        fpp <- fn(par + h * ei + h * ej)
        fpm <- fn(par + h * ei - h * ej)
        fmp <- fn(par - h * ei + h * ej)
        fmm <- fn(par - h * ei - h * ej)
        val <- (fpp - fpm - fmp + fmm) / (4 * h^2)
        H[i, j] <- val
        H[j, i] <- val
      }
    }
  }
  H
}

# ---------------------------
# Main
# ---------------------------
args <- commandArgs(trailingOnly = FALSE)
fileArg <- grep("^--file=", args, value = TRUE)
script_path <- if (length(fileArg) == 1) sub("^--file=", "", fileArg) else ""
base_dir <- if (nzchar(script_path)) dirname(normalizePath(script_path)) else getwd()

count_path <- file.path(base_dir, "zip_dataset_basic_count.csv")
zero_path  <- file.path(base_dir, "zip_dataset_basic_zero.csv")

count_df <- read_csv(count_path)
zero_df  <- read_csv(zero_path)

if (!all(count_df$id == zero_df$id)) stop("IDs do not match between count and zero datasets.")

y <- count_df$y

# Intercepts enabled (match VB tests)
Xcount <- cbind(1, as.matrix(count_df[, c("x1","x2")]))
Xzero  <- cbind(1, as.matrix(zero_df[, c("z1","z2")]))

fit <- zip_em_fit(y, Xcount, Xzero, eps = 1e-9, max_em = 200, max_irls = 25)

beta <- fit$beta
gamma <- fit$gamma

# Covariance (approx) from observed information = -H(logLik)
par_all <- c(beta, gamma)
p_count <- length(beta)
p_zero <- length(gamma)
n <- length(y)

loglik_wrap <- function(par) {
  b <- par[1:p_count]
  g <- par[(p_count + 1):(p_count + p_zero)]
  zip_loglik(y, Xcount, b, Xzero, g)
}

H <- hessian_central(loglik_wrap, par_all, h = 1e-5)
cov_mat <- tryCatch(solve(-H), error = function(e) NULL)

se_all <- rep(NA_real_, length(par_all))
if (!is.null(cov_mat)) se_all <- sqrt(pmax(diag(cov_mat), 0))

z_all <- par_all / se_all
p_all <- 2 * pnorm(-abs(z_all))

k <- p_count + p_zero + 2
AIC  <- -2 * (fit$loglik - k)
BIC  <- fit$deviance + log(n) * k
AICc <- fit$deviance + 2 * k * (n / (n - k - 1))

# Write expected outputs (R-computed)
rows <- data.frame(model = character(), key = character(), value = numeric(), stringsAsFactors = FALSE)

add_row <- function(model, key, value) {
  rows <<- rbind(rows, data.frame(model = model, key = key, value = value, stringsAsFactors = FALSE))
}

model <- "Basic"
names_count <- c("(Intercept)", "x1", "x2")
for (i in seq_along(names_count)) {
  nm <- names_count[i]
  add_row(model, paste0("count_coef_", nm), beta[i])
  add_row(model, paste0("count_se_", nm), se_all[i])
  add_row(model, paste0("count_z_", nm), z_all[i])
  add_row(model, paste0("count_p_", nm), p_all[i])
}

names_zero <- c("(Intercept)", "z1", "z2")
for (i in seq_along(names_zero)) {
  nm <- names_zero[i]
  idx <- p_count + i
  add_row(model, paste0("zero_coef_", nm), gamma[i])
  add_row(model, paste0("zero_se_", nm), se_all[idx])
  add_row(model, paste0("zero_z_", nm), z_all[idx])
  add_row(model, paste0("zero_p_", nm), p_all[idx])
}

add_row(model, "n", n)
add_row(model, "y0count", sum(y == 0))
add_row(model, "loglik", fit$loglik)
add_row(model, "deviance", fit$deviance)
add_row(model, "AIC", AIC)
add_row(model, "AICc", AICc)
add_row(model, "BIC", BIC)
add_row(model, "EMiterations", fit$iterations)
add_row(model, "lastIterLLchange", fit$ll_change)
add_row(model, "converged", as.numeric(fit$converged))

write.csv(rows, file.path(base_dir, "zip_expected_outputs_R.csv"), row.names = FALSE)

# Predictions
pred_mean <- (1 - fit$pi) * fit$mu
pred_df <- data.frame(id = count_df$id, mu = fit$mu, pi = fit$pi, predicted_mean = pred_mean, check.names = FALSE)
write.csv(pred_df, file.path(base_dir, "zip_expected_predictions_R.csv"), row.names = FALSE)

# Residuals (match VB formulas)
raw_res <- y - pred_mean
pearson_res <- raw_res / sqrt((1 - fit$pi) * fit$mu + (1 + fit$pi * fit$mu))
res_df <- data.frame(id = count_df$id, `Raw Resid.` = raw_res, `Pearson Resid.` = pearson_res, check.names = FALSE)
write.csv(res_df, file.path(base_dir, "zip_expected_residuals_full_R.csv"), row.names = FALSE)

cat("Done. Wrote:\n",
    " - zip_expected_outputs_R.csv\n",
    " - zip_expected_predictions_R.csv\n",
    " - zip_expected_residuals_full_R.csv\n")
