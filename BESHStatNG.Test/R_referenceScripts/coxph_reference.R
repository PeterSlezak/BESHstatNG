# coxph_reference.R
# Computes reference outputs for BESHStatNG CoxPH implementation:
# - coefficients, SEs, logLik0/logLik
# - LR / Score / Wald tests
# - residuals: martingale, deviance, schoenfeld, score, dfbeta(s), coxsnell
# - baseline hazard/survival per stratum

suppressPackageStartupMessages({
  library(survival)
})

args <- commandArgs(trailingOnly = TRUE)
file <- if (length(args) >= 1) args[1] else file.path("TestData", "coxph_dataset_strata_ties.csv")

d <- read.csv(file, stringsAsFactors = FALSE)
d$stratum <- factor(d$stratum)

S <- Surv(d$time, d$status)

fit_one <- function(ties) {
  fit <- coxph(S ~ x1 + x2 + strata(stratum), data=d, ties=ties, x=TRUE, y=TRUE)
  fit0 <- coxph(S ~ strata(stratum), data=d, ties=ties, x=TRUE, y=TRUE)

  list(fit=fit, fit0=fit0)
}

extract_all <- function(obj, ties) {
  fit <- obj$fit
  fit0 <- obj$fit0

  beta <- coef(fit)
  se <- sqrt(diag(vcov(fit)))

  ll <- fit$loglik[2]
  ll0 <- fit0$loglik[2]

  # tests
  lr_chi <- fit$logtest["test"]
  lr_df <- fit$logtest["df"]
  lr_p <- fit$logtest["pvalue"]

  score_chi <- fit$sctest["test"]
  score_df <- fit$sctest["df"]
  score_p <- fit$sctest["pvalue"]

  wald_chi <- fit$waldtest["test"]
  wald_df <- fit$waldtest["df"]
  wald_p <- fit$waldtest["pvalue"]

  # robust/sandwich
  fit_r <- coxph(S ~ x1 + x2 + strata(stratum), data=d, ties=ties, robust=TRUE, x=TRUE, y=TRUE)
  rse <- sqrt(diag(vcov(fit_r)))

  # baseline hazard/survival per stratum (uncentered)
  bh <- basehaz(fit, centered=FALSE)  # columns: hazard, time, strata
  bh$surv <- exp(-bh$hazard)

  # residuals
  mart <- residuals(fit, type="martingale")
  dev <- residuals(fit, type="deviance")
  sch <- residuals(fit, type="schoenfeld")
  schsc <- residuals(fit, type="scaledsch")
  score <- residuals(fit, type="score")
  dfb <- residuals(fit, type="dfbeta")
  dfbs <- residuals(fit, type="dfbetas")

  # Cox-Snell residual: cumulative hazard at observed time for each subject:
  # use basehaz + linear predictor; approximation:
  lp <- predict(fit, type="lp")
  # map each subject to its stratum baseline hazard at its time
  # basehaz returns step function; use max time <= t
  coxsnell <- numeric(nrow(d))
  for (i in seq_len(nrow(d))) {
    st <- as.character(d$stratum[i])
    tt <- d$time[i]
    bh_s <- bh[bh$strata == st, ]
    hz <- bh_s$hazard[max(which(bh_s$time <= tt))]
    coxsnell[i] <- hz * exp(lp[i])
  }

  list(
    ties=ties,
    beta=beta, se=se, rse=rse,
    ll0=ll0, ll=ll,
    lr=list(chisq=as.numeric(lr_chi), df=as.integer(lr_df), p=as.numeric(lr_p)),
    score=list(chisq=as.numeric(score_chi), df=as.integer(score_df), p=as.numeric(score_p)),
    wald=list(chisq=as.numeric(wald_chi), df=as.integer(wald_df), p=as.numeric(wald_p)),
    bh=bh,
    resid=list(mart=mart, dev=dev, sch=sch, schsc=schsc, score=score, dfb=dfb, dfbs=dfbs, coxsnell=coxsnell)
  )
}

print_pack <- function(out) {
  cat("\n=== CoxPH reference (ties =", out$ties, ") ===\n")
  cat("beta:\n"); print(out$beta)
  cat("se:\n"); print(out$se)
  cat("robust se:\n"); print(out$rse)
  cat(sprintf("logLik0: %.15f\nlogLik : %.15f\n", out$ll0, out$ll))
  cat("LR:", out$lr$chisq, "df", out$lr$df, "p", out$lr$p, "\n")
  cat("Score:", out$score$chisq, "df", out$score$df, "p", out$score$p, "\n")
  cat("Wald:", out$wald$chisq, "df", out$wald$df, "p", out$wald$p, "\n")

  cat("\nVB-ready arrays:\n")
  cat("expectedCoef <- c(", paste(sprintf("%.15f", out$beta), collapse=", "), ")\n", sep="")
  cat("expectedSe <- c(", paste(sprintf("%.15f", out$se), collapse=", "), ")\n", sep="")
  cat("expectedRobustSe <- c(", paste(sprintf("%.15f", out$rse), collapse=", "), ")\n", sep="")
  cat(sprintf("LL0 <- %.15f\nLL <- %.15f\n", out$ll0, out$ll))
  cat(sprintf("LR <- %.15f\nScore <- %.15f\nWald <- %.15f\n", out$lr$chisq, out$score$chisq, out$wald$chisq))

  # Print a few residuals by id for convenient spot-check
  ids <- d$id
  pick <- c(1,4,11,21)
  cat("\nSelected residual checks (id in {1,4,11,21}):\n")
  for (pid in pick) {
    i <- which(ids == pid)[1]
    cat(sprintf("id=%d mart=%.10f dev=%.10f coxsnell=%.10f\n",
                pid, out$resid$mart[i], out$resid$dev[i], out$resid$coxsnell[i]))
  }
}

# Run all three tie methods
outs <- lapply(c("breslow","efron","exact"), function(tm) extract_all(fit_one(tm), tm))
for (o in outs) print_pack(o)
