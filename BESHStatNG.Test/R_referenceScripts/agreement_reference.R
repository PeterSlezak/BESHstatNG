# Reference computations for BESHStatNG Agreement.vb
#
# This script uses only base R (no external packages) and defines inline arrays
# (no CSV IO) to mirror the unit test datasets.

cat("\n--- Shrout & Fleiss (1979) example (6 targets x 4 raters) ---\n")

X <- rbind(
  c(9,  2, 5, 8),
  c(6,  1, 3, 2),
  c(8,  4, 6, 8),
  c(7,  1, 2, 6),
  c(10, 5, 6, 9),
  c(6,  2, 4, 7)
)

# ---------- ICC(1,1) / ICC(1,k) ----------
icc11 <- function(x, alpha=0.05) {
  # x: targets (rows) as groups, columns are replicates/raters
  n <- nrow(x)
  k <- ncol(x)
  # One-way ANOVA MS
  row_means <- rowMeans(x)
  grand_mean <- mean(x)
  MSB <- k * sum((row_means - grand_mean)^2) / (n - 1)
  MSW <- sum((x - row_means)^2) / (n * (k - 1))
  Fobs <- MSB / MSW
  df1 <- n - 1
  df2 <- n * (k - 1)
  est <- (MSB - MSW) / (MSB + (k - 1) * MSW)
  # F-based CI (lower-tail quantiles)
  q1 <- qf(1 - alpha/2, df1, df2)
  q2 <- qf(1 - alpha/2, df2, df1)
  FL <- Fobs / q1
  FU <- Fobs * q2
  L <- (FL - 1) / (FL + (k - 1))
  U <- (FU - 1) / (FU + (k - 1))
  list(estimate=est, L=L, U=U)
}

icc1k <- function(x, alpha=0.05) {
  n <- nrow(x)
  k <- ncol(x)
  ci11 <- icc11(x, alpha)
  est <- (k * ci11$estimate) / (1 + (k - 1) * ci11$estimate)
  L <- (k * ci11$L) / (1 + (k - 1) * ci11$L)
  U <- (k * ci11$U) / (1 + (k - 1) * ci11$U)
  list(estimate=est, L=L, U=U)
}

res11 <- icc11(X)
res1k <- icc1k(X)
cat(sprintf("ICC(1,1) = %.10f, 95%% CI [%.10f, %.10f]\n", res11$estimate, res11$L, res11$U))
cat(sprintf("ICC(1,k) = %.10f, 95%% CI [%.10f, %.10f]\n", res1k$estimate, res1k$L, res1k$U))


# ---------- Passing–Bablok (simple perfect-line sanity dataset) ----------
cat("\n--- Passing–Bablok sanity dataset: y = 2x ---\n")
x <- c(1,2,3,4,5)
y <- c(2,4,6,8,10)

pb_slope <- function(x,y) {
  n <- length(x)
  slopes <- c()
  for (i in 1:(n-1)) {
    for (j in (i+1):n) {
      dx <- x[i] - x[j]
      dy <- y[i] - y[j]
      if (dx == 0 && dy == 0) next
      if (dx == 0 && dy != 0) {
        slopes <- c(slopes, sign(dy) * Inf)
      } else {
        s <- dy / dx
        if (s == -1) next
        slopes <- c(slopes, s)
      }
    }
  }
  if (length(slopes) == 0) stop("No valid slopes")
  slopes <- sort(slopes)
  # For this perfect-line dataset all slopes are identical
  median(slopes)
}

b <- pb_slope(x,y)
a <- median(y - b*x)
cat(sprintf("PB slope = %.10f, intercept = %.10f\n", b, a))


# ---------- Deming regression sanity dataset: y = 2x ----------
cat("\n--- Deming sanity dataset: y = 2x, lambda = 1 ---\n")
lambda <- 1
xbar <- mean(x); ybar <- mean(y)
Sxx <- var(x); Syy <- var(y); Sxy <- cov(x,y)

delta <- 1/lambda # VB analytical uses delta = sigma_y^2 / sigma_x^2 = 1/lambda
A <- Syy - delta*Sxx
disc <- A*A + 4*delta*Sxy*Sxy
root <- sqrt(disc)
sgn <- ifelse(Sxy >= 0, 1, -1)
b_deming <- (A + sgn*root) / (2*Sxy)
a_deming <- ybar - b_deming*xbar
cat(sprintf("Deming slope = %.10f, intercept = %.10f\n", b_deming, a_deming))

cat("\nDone.\n")
