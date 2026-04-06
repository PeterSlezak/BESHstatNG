# glm_nb2_reference.R
# Reference implementation aligned to BESHStatNG GLM_NB (NB2) theta_ml + outer loop.

suppressPackageStartupMessages({
  library(MASS)
})

nb_residDev <- function(y, mu, alpha) {
  # matches regression.NegativeBinomial.residDev_ in BESHStatNG
  if (y <= 0) {
    dev1 <- 0
  } else {
    mu <- max(mu, 1e-300)
    dev1 <- y * log(y / mu)
  }
  mu <- max(mu, 1e-300)
  if (alpha <= 0) return(Inf)
  denom <- 1 + alpha * mu
  numer <- 1 + alpha * max(y, 0)
  if (denom <= 0 || numer <= 0) return(Inf)
  dev2 <- (max(y,0) + 1/alpha) * log(numer / denom)
  2 * (dev1 - dev2)
}

nb_loglik <- function(y, mu, alpha) {
  if (alpha <= 0) return(-Inf)
  ll <- numeric(length(y))
  for (i in seq_along(y)) {
    yi <- y[i]; mui <- mu[i]
    if (yi < 0 || mui < 0) { ll[i] <- -Inf; next }
    if (mui == 0 && yi > 0) { ll[i] <- -Inf; next }
    tmp <- 0
    if (yi > 0) tmp <- yi * log(alpha * mui)
    denom <- 1 + alpha * mui
    tmp <- tmp - (yi + 1/alpha) * log(denom)
    tmp <- tmp + lgamma(yi + 1/alpha) - lgamma(1/alpha) - lgamma(yi + 1)
    ll[i] <- tmp
  }
  sum(ll)
}

theta_ml_alpha_vb <- function(y, mu, w=NULL, eps=1e-8, maxiter=50, alpha_fallback=1.0) {
  # Returns alpha = 1/theta, mirroring GLM_NB.theta_ml in BESHStatNG
  n <- length(y)
  if (is.null(w)) w <- rep(1, n)
  r <- (y/mu - 1)
  th_acc <- sum(w * (r*r))
  sumW <- sum(w)
  if (th_acc <= 0 || sumW <= 0) return(alpha_fallback)
  th <- sumW / th_acc
  del <- 1.0
  it <- 0
  while (it < maxiter && abs(del) > eps) {
    th <- abs(th)
    info <- 0.0
    score <- 0.0
    for (i in 1:n) {
      wi <- w[i]
      yi <- y[i]
      mui <- mu[i]
      info <- info + wi * (-trigamma(th + yi) + trigamma(th) - 1/th + 2/(mui + th) - (yi + th)/(mui + th)^2)
      score <- score + wi * (digamma(th + yi) - digamma(th) + log(th) + 1 - log(th + mui) - (yi + th)/(mui + th))
    }
    del <- score / info
    th <- th + del
    it <- it + 1
  }
  if (th < 0) return(0.0)
  1/th
}

fit_nb2_vb <- function(df, formula, eps=1e-8, maxiter=50) {
  # Step 1: Poisson start
  pois <- glm(formula, data=df, family=poisson(link="log"))
  beta_pois <- coef(pois)

  mu_pois <- fitted(pois)
  y <- model.response(model.frame(pois))

  # initial alpha from theta_ml
  alpha <- theta_ml_alpha_vb(y, mu_pois, eps=eps, maxiter=maxiter, alpha_fallback=1.0)

  d1 <- sqrt(2 * max(1, df.residual(pois)))
  d2 <- 1.0
  lm <- nb_loglik(y, mu_pois, alpha)
  lm0 <- lm + 2*d1

  fit <- NULL

  for (iter in 1:maxiter) {
    # Step 2: NB with fixed alpha, start at Poisson params
    theta <- ifelse(alpha > 0, 1/alpha, 1e12)
    fam <- MASS::negative.binomial(theta=theta, link="log")
    fit <- glm(formula, data=df, family=fam, start=beta_pois)

    mu <- fitted(fit)
    lm_new <- nb_loglik(y, mu, alpha)
    alpha_new <- theta_ml_alpha_vb(y, mu, eps=eps, maxiter=maxiter, alpha_fallback=alpha)

    del <- abs(lm_new - lm0)/d1 + abs(alpha_new - alpha)/d2
    if (del < eps) {
      alpha <- alpha_new
      break
    }
    lm0 <- lm_new
    alpha <- alpha_new
  }

  # outputs
  mu <- fitted(fit)
  theta <- ifelse(alpha > 0, 1/alpha, 1e12)
  X <- model.matrix(fit)
  p <- ncol(X)
  n <- nrow(X)

  ll <- nb_loglik(y, mu, alpha)
  dev <- sum(mapply(nb_residDev, y, mu, MoreArgs=list(alpha=alpha)))
  y_mean <- mean(y)
  null_mu <- rep(y_mean, length(y))
  null_dev <- sum(mapply(nb_residDev, y, null_mu, MoreArgs=list(alpha=alpha)))

  g2 <- null_dev - dev
  g2df <- p - 1
  g2p <- if (g2df > 0) 1 - pchisq(g2, g2df) else NA_real_

  pear <- sum((y-mu)^2 / (mu + alpha*mu^2))
  df_resid <- n - p
  phi <- pear / df_resid
  pear_p <- 1 - pchisq(pear, df_resid)
  dev_p <- 1 - pchisq(dev, df_resid)
  pseudoR2 <- 1 - dev/null_dev

  # AIC/BIC/AICc (note +1 parameter for alpha, matching GLM_NB overrides)
  AIC  <- -2*ll + 2*(p+1)
  BIC  <- -2*ll + log(n)*(p+1)
  AICc <- -2*ll + (2*(p+1)*n/(n-p))

  # residual table matching GLM.AllResiduals: raw, deviance, pearson, leverage, std dev, std pearson, cook
  raw <- y - mu
  dev_res <- sign(raw) * sqrt(mapply(nb_residDev, y, mu, MoreArgs=list(alpha=alpha)))
  pear_res <- raw / sqrt(mu + alpha*mu^2)
  h <- hatvalues(fit)  # should match BESHStatNG leverage
  st_pear <- pear_res / sqrt(1 - h)
  st_dev  <- dev_res / sqrt(1 - h)
  cook <- (1/p) * (h/(1-h)) * (st_pear^2)

  list(
    alpha=alpha,
    coef=coef(fit),
    se=sqrt(diag(vcov(fit) * phi)),
    ll=ll,
    final_deviance=dev,
    null_deviance=null_dev,
    g2=g2, g2df=g2df, g2p=g2p,
    pearson=pear, pearson_p=pear_p,
    dev_p=dev_p,
    phi=phi, pseudoR2=pseudoR2,
    AIC=AIC, AICc=AICc, BIC=BIC,
    residuals=data.frame(
      Raw.Resid.=raw,
      Deviance.Resid.=dev_res,
      Pearson.Resid.=pear_res,
      Laverage=h,
      Std.Deviance.Resid.=st_dev,
      Std.Pearson.Resid.=st_pear,
      Cook.Distance=cook
    )
  )
}

write_expected_csvs <- function(data_csv, out_prefix) {
  df <- read.csv(data_csv)
  if (!("y" %in% names(df))) stop("CSV must have column y")
  if (all(c("x1","x2") %in% names(df))) {
    res <- fit_nb2_vb(df, y ~ x1 + x2)
  } else {
    res <- fit_nb2_vb(df, y ~ 1)
  }

  metrics <- data.frame(
    model=out_prefix,
    key=c("alpha","ll","final_deviance","null_deviance","g2chisq","g2df","g2p",
          "pearson_chisq","pearson_p","deviance_p","phi","pseudoR2","AIC","AICc","BIC","df_resid","n","p"),
    value=c(res$alpha,res$ll,res$final_deviance,res$null_deviance,res$g2,res$g2df,res$g2p,
            res$pearson,res$pearson_p,res$dev_p,res$phi,res$pseudoR2,res$AIC,res$AICc,res$BIC,
            length(res$residuals$Raw.Resid.)-length(res$coef), length(res$residuals$Raw.Resid.), length(res$coef))
  )
  write.csv(metrics, paste0(out_prefix,"_metrics.csv"), row.names=FALSE)
  coefdf <- data.frame(param=names(res$coef), coef=as.numeric(res$coef), se=as.numeric(res$se))
  write.csv(coefdf, paste0(out_prefix,"_coef.csv"), row.names=FALSE)
  write.csv(res$residuals, paste0(out_prefix,"_residuals.csv"), row.names=FALSE)
  res
}

# Example:
# full <- write_expected_csvs("glm_nb2_full.csv","Full")
# int  <- write_expected_csvs("glm_nb2_interceptonly.csv","InterceptOnly")
