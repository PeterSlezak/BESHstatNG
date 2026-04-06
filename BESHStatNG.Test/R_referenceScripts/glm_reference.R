# glm_make_reference_outputs.R
# Generates glm_expected_outputs.csv and glm_expected_residuals.csv
# matching the current test CSV data files.

suppressPackageStartupMessages({
  library(readr)
  library(dplyr)
  library(stats)
})

invchisq_p <- function(x, df) {
  if (is.nan(x) || df <= 0) return(NaN)
  if (is.infinite(x)) return(0)
  return(1 - pchisq(x, df))
}

norm_p <- function(z) {
  if (is.nan(z)) return(NaN)
  return(2 * (1 - pnorm(abs(z))))
}

aicc <- function(loglik, n, p) {
  denom <- (n - p - 1)
  if (denom <= 0) return(NaN)
  return(-2*loglik + (2*p*n)/denom)
}

# -----------------------------
# Load CSV helper
# -----------------------------
load_design <- function(file, includeX=TRUE) {
  df <- read_csv(file, show_col_types = FALSE)
  if (includeX) {
    list(y=df$y, X=df %>% select(x1,x2))
  } else {
    list(y=df$y, X=NULL)
  }
}

# -----------------------------
# Fit model helper
# -----------------------------
fit_glm <- function(y, X, fam, link) {
  if (is.null(X)) {
    dat <- data.frame(y=y)
    f <- glm(y ~ 1, family=fam(link=link), data=dat)
  } else {
    dat <- data.frame(y=y, X)
    f <- glm(y ~ x1 + x2, family=fam(link=link), data=dat)
  }
  f
}

# -----------------------------
# Pull outputs in your VB naming
# -----------------------------
extract_outputs <- function(modelname, fit) {
  sm <- summary(fit)

  coefs <- sm$coefficients
  n <- length(fit$y)
  p <- length(coef(fit))
  dfres <- n - p

  # Pearson GOF
  mu <- fitted(fit)
  y <- fit$y

  V <- fit$family$variance(mu)
  pearson_chisq <- sum((y - mu)^2 / V)
  phi <- pearson_chisq / dfres

  # Scale rule in your VB: Binomial/Poisson => scale=1 else scale=phi
  famname <- fit$family$family
  scale <- if (famname %in% c("binomial","poisson","Negative Binomial")) 1 else phi

  final_dev <- fit$deviance
  null_dev  <- fit$null.deviance

  g2_chisq <- (null_dev - final_dev) / scale
  g2_df <- p - 1
  g2_p <- invchisq_p(g2_chisq, g2_df)

  dev_gof_chisq <- final_dev / scale
  dev_gof_p <- invchisq_p(dev_gof_chisq, dfres)

  pearson_p <- invchisq_p(pearson_chisq, dfres)

  pseudoR2 <- if (null_dev <= 0 || is.nan(null_dev) || is.infinite(null_dev)) 0 else 1 - final_dev/null_dev

  loglik_unscaled <- as.numeric(logLik(fit))  # glm() is already "unscaled" in the same sense
  AICv  <- AIC(fit)
  BICv  <- BIC(fit)
  AICcv <- aicc(loglik_unscaled, n, p)

  out <- tibble(
    model=modelname,
    key=character(),
    value=double()
  )

  # coefficients & stats
  for (rn in rownames(coefs)) {
    nm <- if (rn=="(Intercept)") "Intercept" else rn
    out <- bind_rows(out, tibble(model=modelname, key=paste0("coef_",nm), value=coefs[rn,1]))
    out <- bind_rows(out, tibble(model=modelname, key=paste0("se_",nm),   value=coefs[rn,2]))
    out <- bind_rows(out, tibble(model=modelname, key=paste0("z_",nm),    value=coefs[rn,3]))
    out <- bind_rows(out, tibble(model=modelname, key=paste0("p_",nm),    value=coefs[rn,4]))
  }

  # global statistics
  stats <- tibble(
    model=modelname,
    key=c("phi","scale","pearson_chisq","pearson_p",
          "g2_chisq","g2_p","dev_gof_chisq","dev_gof_p",
          "pseudoR2","loglik_unscaled","aic","aicc","bic",
          "final_deviance","null_deviance"),
    value=c(phi, scale, pearson_chisq, pearson_p,
            g2_chisq, g2_p, dev_gof_chisq, dev_gof_p,
            pseudoR2, loglik_unscaled, AICv, AICcv, BICv,
            final_dev, null_dev)
  )
  bind_rows(out, stats)
}

# -----------------------------
# Residual table (only for models your VB test checks)
# -----------------------------
extract_residuals <- function(modelname, fit) {
  infl <- influence(fit, do.coef=FALSE)
  df <- tibble(
    model=modelname,
    id = 1:length(fit$y),
    `Raw Resid.` = residuals(fit, type="response"),
    `Deviance Resid.` = residuals(fit, type="deviance"),
    `Pearson Resid.` = residuals(fit, type="pearson"),
    `Laverage` = hatvalues(fit),
    `Std Deviance Resid.` = residuals(fit, type="deviance") / sqrt(1-hatvalues(fit)),
    `Std Pearson Resid.`  = residuals(fit, type="pearson")  / sqrt(1-hatvalues(fit)),
    `Cook Distance` = cooks.distance(fit)
  )
  df
}

# -----------------------------
# Model grid (must match VB test names)
# -----------------------------
MODELS <- list(
  list(name="Binomial_Logit_Full", file="glm_binomial_full.csv", includeX=TRUE, fam=binomial, link="logit"),
  list(name="Binomial_Probit_Full", file="glm_binomial_full.csv", includeX=TRUE, fam=binomial, link="probit"),
  list(name="Binomial_Log_Full", file="glm_binomial_full.csv", includeX=TRUE, fam=binomial, link="log"),
  list(name="Binomial_Identity_Full", file="glm_binomial_full.csv", includeX=TRUE, fam=binomial, link="identity"),

  list(name="Poisson_Log_Full", file="glm_poisson_full.csv", includeX=TRUE, fam=poisson, link="log"),
  list(name="Poisson_Identity_Full", file="glm_poisson_full.csv", includeX=TRUE, fam=poisson, link="identity"),
  list(name="Poisson_Sqrt_Full", file="glm_poisson_full.csv", includeX=TRUE, fam=poisson, link="sqrt"),

  list(name="Gaussian_Identity_Full", file="glm_gaussian_full.csv", includeX=TRUE, fam=gaussian, link="identity"),
  list(name="Gaussian_Log_Full", file="glm_gaussian_full.csv", includeX=TRUE, fam=gaussian, link="log"),
  list(name="Gaussian_Inverse_Full", file="glm_gaussian_full.csv", includeX=TRUE, fam=gaussian, link="inverse"),

  list(name="Gamma_Log_Full", file="glm_gamma_full.csv", includeX=TRUE, fam=Gamma, link="log"),
  list(name="Gamma_Identity_Full", file="glm_gamma_full.csv", includeX=TRUE, fam=Gamma, link="identity"),
  list(name="Gamma_Inverse_Full", file="glm_gamma_full.csv", includeX=TRUE, fam=Gamma, link="inverse"),
  list(name="Gamma_Sqrt_Full", file="glm_gamma_full.csv", includeX=TRUE, fam=Gamma, link="sqrt")
)

expected <- tibble()
resids <- tibble()

for (m in MODELS) {
  d <- load_design(m$file, includeX=m$includeX)
  fit <- fit_glm(d$y, d$X, m$fam, m$link)
  expected <- bind_rows(expected, extract_outputs(m$name, fit))

  # Residual reference only for those VB tests that check it
  if (m$name %in% c("Binomial_Logit_Full","Poisson_Log_Full","Gaussian_Identity_Full","Gamma_Log_Full")) {
    resids <- bind_rows(resids, extract_residuals(m$name, fit))
  }
}

write_csv(expected, "glm_expected_outputs.csv")
write_csv(resids, "glm_expected_residuals.csv")

cat("Wrote glm_expected_outputs.csv and glm_expected_residuals.csv\n")
