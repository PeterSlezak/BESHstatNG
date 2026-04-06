# linear_model_reference.R
# Reference calculations for BESHStatNG LinearModel_Tests
# Uses only base R.

options(digits=17)
df <- read.csv("lm_dataset_basic.csv")
y <- df$y
x1 <- df$x1
x2 <- df$x2
w <- df$w
n <- length(y)

calc_stats <- function(wts=NULL) {
  if (is.null(wts)) {
    wts <- rep(1, n)
  }
  W <- diag(wts)
  X <- cbind(1, x1, x2)
  p <- ncol(X)
  beta <- solve(t(X)%*%W%*%X, t(X)%*%W%*%y)
  yhat <- as.vector(X %*% beta)
  resid <- y - yhat
  sse <- sum(wts * resid^2)

  ybarw <- sum(wts*y)/sum(wts)
  sst <- sum(wts*(y - ybarw)^2)
  ssr <- sst - sse

  dfModel <- p - 1
  dfResid <- n - p
  dfTotal <- n - 1

  mse <- sse/dfResid
  rmse <- sqrt(mse)

  r2 <- if (sst > 0) 1 - sse/sst else 0
  adjr2 <- 1 - (1-r2)*(dfTotal/dfResid)

  msr <- ssr/dfModel
  fstat <- msr/mse
  pstat <- 1 - pf(fstat, dfModel, dfResid)

  sigma2ML <- max(sse/n, 1e-300)
  ll <- -(n/2) * (log(2*pi*sigma2ML) + 1)
  aic <- -2*ll + 2*p
  bic <- -2*ll + log(n)*p

  invXtWX <- solve(t(X)%*%W%*%X)
  cov <- mse * invXtWX
  se <- sqrt(pmax(0, diag(cov)))
  tval <- beta/se
  pval <- 2*(1-pt(abs(tval), dfResid))

  # Diagnostics (match LinearModel.vb)
  inv_info <- cov/mse
  h <- numeric(n)
  for (i in 1:n) {
    xi <- X[i,]
    h[i] <- wts[i] * as.numeric(t(xi) %*% inv_info %*% xi)
    h[i] <- max(0, min(0.999999, h[i]))
  }
  oneMinus <- pmax(1e-12, 1-h)
  jack <- resid/oneMinus
  stdres <- resid*sqrt(wts) / sqrt(pmax(1e-300, mse*oneMinus))
  cooks <- (resid^2*wts/(p*mse)) * (h/(oneMinus^2))

  list(beta=beta, se=se, t=tval, p=pval,
       yhat=yhat, resid=resid, h=h, stdres=stdres, jack=jack, cooks=cooks,
       sse=sse, sst=sst, ssr=ssr, mse=mse, rmse=rmse,
       r2=r2, adjr2=adjr2, fstat=fstat, pstat=pstat, ll=ll, aic=aic, bic=bic,
       cov=cov)
}

term_anova <- function(wts=NULL, type=c("I","III")) {
  type <- match.arg(type)
  if (is.null(wts)) wts <- rep(1, n)
  Xfull <- cbind(1, x1, x2)
  p <- ncol(Xfull)
  W <- diag(wts)
  fit_sse <- function(cols) {
    X <- Xfull[, cols, drop=FALSE]
    beta <- solve(t(X)%*%W%*%X, t(X)%*%W%*%y)
    r <- y - as.vector(X%*%beta)
    sum(wts*r^2)
  }
  sseFull <- fit_sse(1:p)
  dfResid <- n - p
  mseFull <- sseFull/dfResid
  terms <- list(x1=2, x2=3)

  out <- list()
  if (type=="I") {
    included <- 1
    ssePrev <- fit_sse(included)
    dfPrev <- length(included)
    for (nm in names(terms)) {
      newCols <- sort(unique(c(included, terms[[nm]])))
      sseNew <- fit_sse(newCols)
      dfNew <- length(newCols)
      ss <- ssePrev - sseNew
      df <- dfNew - dfPrev
      ms <- ss/df
      f <- ms/mseFull
      pval <- 1 - pf(f, df, dfResid)
      out[[nm]] <- c(df=df, ss=ss, ms=ms, f=f, p=pval)
      included <- newCols
      ssePrev <- sseNew
      dfPrev <- dfNew
    }
  } else {
    fullCols <- 1:p
    for (nm in names(terms)) {
      drop <- terms[[nm]]
      keep <- setdiff(fullCols, drop)
      sseRed <- fit_sse(keep)
      ss <- sseRed - sseFull
      df <- length(drop)
      ms <- ss/df
      f <- ms/mseFull
      pval <- 1 - pf(f, df, dfResid)
      out[[nm]] <- c(df=df, ss=ss, ms=ms, f=f, p=pval)
    }
  }
  out
}

vif_vals <- function(wts=NULL) {
  if (is.null(wts)) wts <- rep(1, n)
  Z <- cbind(x1, x2)
  wsum <- sum(wts)
  mu <- colSums(Z*wts)/wsum
  cov <- matrix(0, ncol(Z), ncol(Z))
  for (a in 1:ncol(Z)) {
    for (b in 1:ncol(Z)) {
      cov[a,b] <- sum(wts*(Z[,a]-mu[a])*(Z[,b]-mu[b]))/(wsum-1)
    }
  }
  sd <- sqrt(diag(cov))
  R <- cov/(sd%o%sd)
  diag(solve(R))
}

ols <- calc_stats(NULL)
wls <- calc_stats(w)

cat("OLS beta\n"); print(ols$beta)
cat("OLS se\n"); print(ols$se)
cat("OLS fitted\n"); print(ols$yhat)
cat("OLS resid\n"); print(ols$resid)
cat("OLS leverage\n"); print(ols$h)
cat("OLS stdres\n"); print(ols$stdres)
cat("OLS cooks\n"); print(ols$cooks)
cat("OLS jack\n"); print(ols$jack)
cat("OLS scalars\n"); print(c(r2=ols$r2, adjr2=ols$adjr2, f=ols$fstat, p=ols$pstat))

cat("\nWLS beta\n"); print(wls$beta)
cat("WLS se\n"); print(wls$se)
cat("WLS scalars\n"); print(c(r2=wls$r2, adjr2=wls$adjr2, f=wls$fstat, p=wls$pstat))

cat("\nType I ANOVA (OLS)\n"); print(term_anova(NULL,"I"))
cat("Type III ANOVA (OLS)\n"); print(term_anova(NULL,"III"))
cat("VIF (OLS)\n"); print(vif_vals(NULL))

cat("\nType I ANOVA (WLS)\n"); print(term_anova(w,"I"))
cat("Type III ANOVA (WLS)\n"); print(term_anova(w,"III"))
cat("VIF (WLS)\n"); print(vif_vals(w))


# No-intercept OLS (to match includeIntercept:=False)
X0 <- cbind(x1, x2)
beta0 <- solve(t(X0)%*%X0, t(X0)%*%y)
yhat0 <- as.vector(X0 %*% beta0)
resid0 <- y - yhat0
sse0 <- sum(resid0^2)
sst0 <- sum(y^2)          # uncentered
r2_0 <- 1 - sse0/sst0
dfModel0 <- ncol(X0)
dfResid0 <- n - ncol(X0)
dfTotal0 <- n
adjr2_0 <- 1 - (1-r2_0)*(dfTotal0/dfResid0)
msr0 <- (sst0 - sse0)/dfModel0
mse0 <- sse0/dfResid0
f0 <- msr0/mse0
p0 <- 1 - pf(f0, dfModel0, dfResid0)

cat("\nNo-intercept OLS beta\n"); print(beta0)
cat("No-intercept OLS scalars\n"); print(c(r2=r2_0, adjr2=adjr2_0, f=f0, p=p0))
