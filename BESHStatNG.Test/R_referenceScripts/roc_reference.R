# ROC reference implementation for BESHStatNG.graphics.ROC (ROC.vb)
#
# Purpose
# - Provide an R-side reference that matches ROC.vb's algorithm and statistics.
# - Uses Wilcoxon/Mann–Whitney AUC with 0.5 credit for ties.
# - Uses Hanley & McNeil (1982) AUC SE as implemented in ROC.vb.
# - Uses ROC.vb's *separate* normal-approximation SE for the p-value:
#     SE_p = sqrt((0.25 + (n1+n2-2)*0.083333) / (n1*n2))
#   and p = 2 * pnorm(-abs(AUC-0.5)/SE_p)
# - Confidence interval: AUC ± z_{1-α/2} * SE_auc
#
# NOTE
# ROC.vb constructs ROC plotting arrays in *descending* threshold order and
# forces endpoints (1,1) and (0,0) by setting sensitivity/FPR endpoints.

roc_vb <- function(patients, controls, alpha = 0.05) {
  n1 <- length(patients)
  n2 <- length(controls)
  n  <- n1 + n2

  # Concatenate and sort (like Array.Sort(Data12, arIDs))
  data12 <- c(patients, controls)
  arIDs  <- c(rep(1L, n1), rep(2L, n2))  # 1=patients, 2=controls
  o <- order(data12)
  data12 <- data12[o]
  arIDs  <- arIDs[o]

  arUnique <- sort(unique(data12))
  NoUniqueVals <- length(arUnique)

  # Cutoffs: midpoints between successive unique values, last = max + 1
  parCutOff <- numeric(NoUniqueVals)
  if (NoUniqueVals >= 2) {
    parCutOff[1:(NoUniqueVals - 1)] <- (arUnique[1:(NoUniqueVals - 1)] + arUnique[2:NoUniqueVals]) / 2
  }
  parCutOff[NoUniqueVals] <- arUnique[NoUniqueVals] + 1

  # Arrays (match ROC.vb dimensions)
  parSensitivity <- numeric(NoUniqueVals + 1)  # includes endpoints
  par1minusSpec  <- numeric(NoUniqueVals + 1)  # includes endpoints
  parSpecificity <- numeric(NoUniqueVals)
  arPatientsGroupNo <- numeric(NoUniqueVals)
  arControlsGroupNo <- numeric(NoUniqueVals)
  arPatientsCum <- numeric(NoUniqueVals)
  arContCum     <- numeric(NoUniqueVals)

  # Counters (a,b,c,d in ROC.vb)
  c_cnt <- 0L
  d_cnt <- 0L
  j <- 1L  # R is 1-based

  # Wilcoxon AUC + SE intermediate sums
  pAUC <- 0
  dQ1SE <- 0
  dQ2SE <- 0
  arPatientsCum[1] <- n1

  for (i in 1:NoUniqueVals) {
    while (j <= n && data12[j] < parCutOff[i]) {
      if (arIDs[j] == 1L) {
        c_cnt <- c_cnt + 1L
        arPatientsGroupNo[i] <- arPatientsGroupNo[i] + 1
      } else {
        d_cnt <- d_cnt + 1L
        arControlsGroupNo[i] <- arControlsGroupNo[i] + 1
      }
      j <- j + 1L
    }

    a <- n1 - c_cnt
    b <- n2 - d_cnt

    # ROC points
    parSensitivity[i + 1] <- a / (a + c_cnt)
    parSpecificity[i]     <- d_cnt / (b + d_cnt)
    par1minusSpec[i + 1]  <- 1 - parSpecificity[i]

    # Wilcoxon AUC and SE components
    if (i == 1) {
      arPatientsCum[i] <- arPatientsCum[i] - arPatientsGroupNo[i]
    } else {
      arPatientsCum[i] <- arPatientsCum[i - 1] - arPatientsGroupNo[i]
      arContCum[i]     <- arContCum[i - 1] + arControlsGroupNo[i - 1]
    }

    pAUC  <- pAUC + (arControlsGroupNo[i] * arPatientsCum[i] + 0.5 * arControlsGroupNo[i] * arPatientsGroupNo[i])
    dQ2SE <- dQ2SE + (arPatientsGroupNo[i] * (arContCum[i]^2 + arContCum[i] * arControlsGroupNo[i] + (1/3) * arControlsGroupNo[i]^2))
    dQ1SE <- dQ1SE + (arControlsGroupNo[i] * (arPatientsCum[i]^2 + arPatientsCum[i] * arPatientsGroupNo[i] + (1/3) * arPatientsGroupNo[i]^2))
  }

  # Endpoints for plotting (exactly as ROC.vb)
  parSensitivity[1] <- 1
  par1minusSpec[1]  <- 1
  parSensitivity[NoUniqueVals + 1] <- 0
  par1minusSpec[NoUniqueVals + 1]  <- 0

  # Finalize AUC and SE (Hanley–McNeil as implemented)
  pAUC  <- pAUC / (n1 * n2)
  dQ2SE <- dQ2SE / (n1 * (n2^2))
  dQ1SE <- dQ1SE / (n2 * (n1^2))
  se_auc <- sqrt((pAUC * (1 - pAUC) + (n1 - 1) * (dQ1SE - pAUC^2) + (n2 - 1) * (dQ2SE - pAUC^2)) / (n1 * n2))

  # p-value (note: ROC.vb uses a special SE for the test)
  se_p <- sqrt((0.25 + (n1 + n2 - 2) * 0.083333) / (n1 * n2))
  p_value <- 2 * pnorm(-abs(pAUC - 0.5) / se_p)

  # Normal-approx CI
  z <- qnorm(1 - alpha / 2)
  ci <- c(lower = pAUC - z * se_auc, upper = pAUC + z * se_auc)

  list(
    auc = pAUC,
    se_auc = se_auc,
    se_p = se_p,
    p_value = p_value,
    ci = ci,
    cutoffs = parCutOff,
    sensitivity = parSensitivity,
    specificity = parSpecificity,
    fpr = par1minusSpec
  )
}


### Examples (mirror the unit tests) ------------------------------------

cat("\nExample 1: Perfect separation (same as ROC_PerfectSeparation test)\n")
ex1 <- roc_vb(patients = c(2,3,4,5), controls = c(0,0.5,1), alpha = 0.05)
print(ex1[c("auc","se_auc","se_p","p_value","ci")])

cat("\nExample 2: No discrimination with ties\n")
ex2 <- roc_vb(patients = c(0,1,2,3), controls = c(0,1,2,3), alpha = 0.05)
print(ex2[c("auc","p_value")])

cat("\nExample 3: With ties (same as ROC_WithTies test)\n")
ex3 <- roc_vb(patients = c(1,2,2), controls = c(0,2,3), alpha = 0.05)
print(ex3[c("auc","p_value")])
cat("Cutoffs:\n")
print(ex3$cutoffs)
cat("Sensitivity (ROC points):\n")
print(ex3$sensitivity)
cat("Specificity (per cutoff):\n")
print(ex3$specificity)
