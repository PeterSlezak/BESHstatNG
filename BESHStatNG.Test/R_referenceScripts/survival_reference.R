# survival_reference.R
# Reference generator aligned to the algorithms implemented in Survival.vb (BESHStatNG)
# Produces: KM tables, Greenwood SE, VB log-log CI, weighted logrank variants,
# fixed-time comparison, and equality-of-medians pseudo-count test.

read_data <- function(path) {
  d <- read.csv(path, stringsAsFactors = FALSE)
  d$status <- as.integer(d$status)
  d$time <- as.numeric(d$time)
  d
}

# VB sorts by: Time asc, Cenzored desc (events first if event=1), Group asc
vb_sort <- function(d) {
  o <- order(d$time, -d$status, d$group)
  d[o, ]
}

km_vb <- function(d_group) {
  d_group <- vb_sort(d_group)
  n <- nrow(d_group)
  times <- sort(unique(d_group$time))
  surv <- 1.0
  se_sum <- 0.0
  se_sum2 <- 0.0
  out <- data.frame(time=numeric(), risk=integer(), events=integer(), cens=integer(),
                    surv=numeric(), se=numeric(), lcl=numeric(), ucl=numeric())
  for (t in times) {
    at_risk <- sum(d_group$time >= t)
    ev <- sum(d_group$time == t & d_group$status == 1L)
    ce <- sum(d_group$time == t & d_group$status == 0L)
    if (ev > 0) {
      # apply ev times one-by-one (VB loops each record, but equivalent with repeated update)
      for (k in seq_len(ev)) {
        surv <- surv * (1 - 1/at_risk)
        # greenwood sum: 1/(n(n-1))
        se_sum <- se_sum + 1/(at_risk*(at_risk-1))
        se_sum2 <- se_sum2 + log((at_risk-1)/at_risk)
        at_risk <- at_risk - 1
      }
    }
    se <- sqrt(se_sum * (surv^2))
    # VB LogSE
    logse <- if (se_sum2 != 0) sqrt(se_sum) / (-se_sum2) else NA_real_
    # VB CI: Prob^(exp(±1.96*LogSE))
    if (is.finite(logse) && surv > 0) {
      lcl <- surv^(exp( 1.96*logse))
      ucl <- surv^(exp(-1.96*logse))
    } else {
      lcl <- NA_real_
      ucl <- NA_real_
    }
    out <- rbind(out, data.frame(time=t, risk=sum(d_group$time >= t),
                                 events=ev, cens=ce, surv=surv, se=se,
                                 lcl=lcl, ucl=ucl))
  }
  out
}

# Weighted logrank aligned to VB's weight methods and computations.
# VB uses strata; we compute within each stratum and sum.
weight_vec <- function(method, s_prev, n_risk) {
  method <- tolower(method)
  if (method == "logrank") return(1.0)
  if (method == "gehan-breslow") return(n_risk)
  if (method == "tarone-ware") return(sqrt(n_risk))
  if (method == "peto") return(s_prev)
  if (method == "modified peto") return(s_prev)
  stop("Unknown method")
}

weighted_logrank_vb <- function(d, method) {
  d <- vb_sort(d)
  groups <- sort(unique(d$group))
  strata <- sort(unique(d$stratum))
  k <- length(groups)
  if (k < 2) stop("Need >=2 groups")

  # Check all censored in any group
  for (g in groups) {
    if (sum(d$status[d$group==g]) == 0) return(NULL)
  }

  # VB removes "early censored at minimal time" per group/stratum (see code), but in this dataset
  # it doesn't change; implement as-is for fidelity:
  cleaned <- d
  for (s in strata) {
    for (g in groups) {
      idx <- which(cleaned$stratum==s & cleaned$group==g)
      if (length(idx)==0) next
      gdat <- cleaned[idx, ]
      tmin <- min(gdat$time)
      # if first record(s) at min time are censored, drop them
      drop <- which(gdat$time==tmin & gdat$status==0L)
      if (length(drop)>0) {
        cleaned <- cleaned[-idx[drop], ]
      }
    }
  }

  # compute WLS logrank
  # For k groups => chi-square with df=k-1 from (O-E)' V^-1 (O-E)
  OminusE <- rep(0, k)
  V <- matrix(0, k, k)

  for (s in strata) {
    ds <- cleaned[cleaned$stratum==s, ]
    if (nrow(ds)==0) next
    ds <- vb_sort(ds)
    times <- sort(unique(ds$time))
    Sprev <- 1.0
    for (t in times) {
      risk_total <- sum(ds$time >= t)
      d_total <- sum(ds$time == t & ds$status==1L)
      if (d_total == 0) next
      w <- weight_vec(method, Sprev, risk_total)
      # group-wise risk and events
      r_g <- sapply(groups, function(g) sum(ds$group==g & ds$time >= t))
      d_g <- sapply(groups, function(g) sum(ds$group==g & ds$time==t & ds$status==1L))
      # expected d
      e_g <- d_total * r_g / risk_total
      OminusE <- OminusE + w * (d_g - e_g)

      # variance contribution (multivariate hypergeometric)
      # VB uses: Var = w^2 * d*(n-d)/(n-1) * (diag(p)-p p')
      if (risk_total > 1) {
        factor <- w^2 * d_total * (risk_total - d_total) / (risk_total - 1)
        p <- r_g / risk_total
        V <- V + factor * (diag(p) - tcrossprod(p))
      }

      # update Sprev after events at t (KM step)
      Sprev <- Sprev * (1 - d_total / risk_total)
    }
  }

  # Drop last group to make V invertible in rank k-1 (common approach)
  Oe <- OminusE[1:(k-1)]
  Vsub <- V[1:(k-1), 1:(k-1)]
  chisq <- as.numeric(t(Oe) %*% solve(Vsub) %*% Oe)
  p <- 1 - pchisq(chisq, df=k-1)
  list(chisq=chisq, p=p)
}

# Fixed time-point comparison (2 groups only), aligned to VB:
compare_fix_time <- function(d) {
  groups <- sort(unique(d$group))
  stopifnot(length(groups)==2)
  g1 <- d[d$group==groups[1], ]
  g2 <- d[d$group==groups[2], ]
  km1 <- km_vb(g1)
  km2 <- km_vb(g2)
  times <- sort(unique(c(km1$time, km2$time)))
  res <- data.frame(time=times, diff=NA_real_, p=NA_real_)
  # helper: survival at time t
  s_at <- function(km, t) {
    idx <- which(km$time <= t)
    if (length(idx)==0) return(1.0)
    km$surv[max(idx)]
  }
  for (i in seq_along(times)) {
    t <- times[i]
    s1 <- s_at(km1, t)
    s2 <- s_at(km2, t)
    res$diff[i] <- s1 - s2
    # log(-log(S)) test uses delta and var approx; we reproduce VB simplified calc:
    if (s1>0 && s2>0 && s1<1 && s2<1) {
      z <- (log(-log(s1)) - log(-log(s2)))
      # Use chi-square df=1 with z^2 (VB uses CDF via ChiSquare_Inv_RT/ChiSquare)
      res$p[i] <- 1 - pchisq(z^2, df=1)
    } else {
      res$p[i] <- NA_real_
    }
  }
  res
}

# Equality-of-medians test (VB algorithm; Biometrics 2012 pseudo-count chi-square over floor/ceil tables).
# Important: VB uses group KM tables (per-record updates) and SurvivalAt(t, grpTable).
# If your SurvivalAt implementation contains a bug (e.g., searching for a constant time),
# set bug_constant_time=TRUE to reproduce it; otherwise leave FALSE.
# --- Equality of medians (VB-aligned pseudo-counts + Pearson chi-square) ---
# Aligned to corrected SurvivalAt: use last survival value where time <= t.
equality_of_medians_vb <- function(d) {
  # Map groups to 0..k-1 in increasing order (VB style)
  gnames <- sort(unique(d$group))
  d$gi <- match(d$group, gnames) - 1L
  k <- length(gnames)

  # VB pooled KM median (pooled data, event-time aggregation)
  sorted_all <- d[order(d$time), ]
  surv <- 1.0
  pooledMedian <- -1
  ev_times <- sort(unique(sorted_all$time[sorted_all$status == 1L]))
  for (t in ev_times) {
    at_risk <- sum(sorted_all$time >= t)
    ev <- sum(sorted_all$time == t & sorted_all$status == 1L)
    surv <- surv * (1 - ev / at_risk)
    if (surv <= 0.5) { pooledMedian <- t; break }
  }
  if (pooledMedian < 0) {
    return(list(chisq=NaN, p=NaN, pooledMedian=pooledMedian, nhat1=rep(NaN, k), df=k-1))
  }

  # VB per-group sorting: time asc, events first, group asc
  vb_sort <- function(dd) dd[order(dd$time, -dd$status, dd$gi), ]

  # Build KM table for a group: include all record times, keep last per time
  km_table_group <- function(dg) {
    dg <- vb_sort(dg)
    at_risk <- nrow(dg)
    S <- 1.0
    out_time <- numeric()
    out_surv <- numeric()
    ut <- unique(dg$time)
    for (t in ut) {
      rows <- dg[dg$time == t, ]
      for (r in seq_len(nrow(rows))) {
        if (rows$status[r] == 1L) S <- S * (1 - 1 / at_risk)
        at_risk <- at_risk - 1
      }
      out_time <- c(out_time, t)
      out_surv <- c(out_surv, S)
    }
    data.frame(time=out_time, surv=out_surv)
  }

  tabs <- lapply(0:(k-1), function(i) km_table_group(d[d$gi == i, ]))

  # Correct SurvivalAt: last time <= t, else 1.0
  survival_at <- function(t, tab) {
    if (nrow(tab) == 0) return(1.0)
    idx <- max(which(tab$time <= t), na.rm = TRUE)
    if (is.infinite(idx)) return(1.0)
    tab$surv[idx]
  }

  # Pseudocounts nhat1
  nhat1 <- numeric(k)
  for (i in 0:(k-1)) {
    gdf <- d[d$gi == i, ]
    sumq <- 0.0
    for (r in seq_len(nrow(gdf))) {
      time <- gdf$time[r]
      status <- gdf$status[r]
      if (status == 1L) {
        q <- if (time > pooledMedian) 1.0 else 0.0
      } else {
        if (time >= pooledMedian) {
          q <- 1.0
        } else {
          S_t <- survival_at(time, tabs[[i+1]])
          S_med <- survival_at(pooledMedian, tabs[[i+1]])
          q <- if (S_t > 0) S_med / S_t else 0.0
          q <- max(0.0, min(1.0, q))
        }
      }
      sumq <- sumq + q
    }
    nhat1[i+1] <- sumq
  }

  groupNs <- as.numeric(table(d$gi))
  nTotal <- sum(groupNs)
  N_above <- sum(nhat1)
  expAbove <- groupNs * N_above / nTotal

  chi2 <- 0.0
  for (i in seq_len(k)) {
    obsA <- nhat1[i]; expA <- expAbove[i]
    obsB <- groupNs[i] - obsA; expB <- groupNs[i] - expA
    chi2 <- chi2 + (obsA - expA)^2 / expA + (obsB - expB)^2 / expB
  }

  dfree <- k - 1
  p <- 1 - pchisq(chi2, dfree)
  list(chisq=chi2, p=p, pooledMedian=pooledMedian, nhat1=nhat1, df=dfree)
}


main <- function() {
  d3 <- read_data("survival_dataset.csv")
  d2 <- read_data("survival_dataset_2group.csv")
  cat("KM Group A (2-group dataset):\n")
  print(km_vb(d2[d2$group=="A", ]))

  cat("\nWeighted logrank (3-group):\n")
  for (m in c("logrank","gehan-breslow","tarone-ware","peto","modified peto")) {
    out <- weighted_logrank_vb(d3, m)
    cat(m, "chisq=", out$chisq, "p=", out$p, "\n")
  }

  cat("\nFixed time-point comparison (2-group):\n")
  print(compare_fix_time(d2))

  cat("\nEquality of medians:\n")
  em <- equality_of_medians_vb(d3)
  print(em)
}

if (sys.nframe() == 0) main()


# --- Write VB-aligned weighted logrank reference CSV (method,chisq,p) ---
write_weighted_logrank_reference <- function(data_csv="survival_dataset.csv", out_csv="survival_weightedlogrank_reference.csv") {
  d <- read.csv(data_csv, stringsAsFactors=FALSE)
  d$time <- as.numeric(d$time)
  d$status <- as.integer(d$status)
  d$stratum <- as.character(d$stratum)
  d$group <- as.character(d$group)

  # Ensure groups are mapped to 0..k-1 in increasing order (VB expects 0-based group IDs)
  gs <- sort(unique(d$group))
  gmap <- setNames(seq_along(gs)-1L, gs)
  d$g <- as.integer(gmap[d$group])

  vb_weighted_logrank <- function(method) {
    small <- 1e-13
    # build recs ordered by time
    rec <- d[order(d$time), ]
    # omit early censored at smallest time block (VB loop)
    i <- 1L; del <- integer(0); j <- 0L
    while (i < nrow(rec) && (rec$time[i] == rec$time[i+1L] || rec$status[i] == 0L)) {
      if (rec$status[i] == 0L) { j <- j + 1L; del <- c(del, i) }
      i <- i + 1L
    }
    if (length(del) > 0) rec <- rec[-del, ]

    strata <- unique(rec$stratum)
    k <- length(gs)
    Z <- rep(0, k)
    var <- matrix(0, k, k)
    Var2 <- matrix(0, k-1, k-1)

    for (st in strata) {
      InRisk <- rep(0, k)
      n <- nrow(rec)
      time <- rep(0, n)
      Events <- matrix(0, n, k)
      Censor <- matrix(0, n, k)

      # first event in stratum
      idx <- which(rec$status==1L & rec$stratum==st)
      if (length(idx)==0) next
      i0 <- idx[1]
      time[1] <- rec$time[i0]
      Events[1, rec$g[i0]+1] <- Events[1, rec$g[i0]+1] + 1
      InRisk[rec$g[i0]+1] <- InRisk[rec$g[i0]+1] + 1

      ii <- 2L
      for (i in (i0+1L):n) {
        if (i > n) break
        if (rec$status[i]==1L && rec$stratum[i]==st) {
          if (rec$time[i] == time[ii-1L]) {
            Events[ii-1L, rec$g[i]+1] <- Events[ii-1L, rec$g[i]+1] + 1
            InRisk[rec$g[i]+1] <- InRisk[rec$g[i]+1] + 1
          } else if (rec$time[i] > time[ii-1L]) {
            time[ii] <- rec$time[i]
            Events[ii, rec$g[i]+1] <- Events[ii, rec$g[i]+1] + 1
            InRisk[rec$g[i]+1] <- InRisk[rec$g[i]+1] + 1
            ii <- ii + 1L
          }
        }
      }
      NoTimes <- ii-2L
      if (NoTimes < 0) next

      # censored values
      for (i in seq_len(n)) {
        if (rec$status[i]==0L && rec$time[i] >= time[1] && rec$stratum[i]==st) {
          ii2 <- NoTimes + 1L
          while (rec$time[i] < time[ii2]) {
            if (ii2==1L) break
            ii2 <- ii2 - 1L
          }
          if (rec$time[i] == time[1]) ii2 <- 1L
          Censor[ii2, rec$g[i]+1] <- Censor[ii2, rec$g[i]+1] + 1
          InRisk[rec$g[i]+1] <- InRisk[rec$g[i]+1] + 1
        }
      }

      w <- 0; Sest <- 0
      for (j in 1:(NoTimes+1L)) {
        Yi <- sum(InRisk)
        Di <- sum(Events[j, ])
        ml <- tolower(method)
        if (ml=="logrank") w <- 1
        else if (ml=="gehan-breslow") w <- Yi
        else if (ml=="tarone-ware") w <- sqrt(Yi)
        else if (ml=="peto") {
          if (j==1L) w <- (1 - Di/(Yi+1))
          else {
            if (Yi==1) Yi <- Yi + small
            w <- w * (1 - Di/(Yi+1))
          }
        } else if (ml=="modified peto") {
          if (j==1L) { Sest <- (1 - Di/(Yi+1)); w <- Sest * (Yi/(Yi+1)) }
          else {
            if (Yi==1) Yi <- Yi + small
            Sest <- Sest * (1 - Di/(Yi+1))
            w <- Sest * (Yi/(Yi+1))
          }
        }

        for (g in 1:k) {
          Z[g] <- Z[g] + (w * (Events[j,g] - InRisk[g] * Di / Yi))
          if (Yi==1) Yi <- Yi + small
          var[g,g] <- var[g,g] + (w^2 * (InRisk[g]/Yi) * (1 - (InRisk[g]/Yi)) * ((Yi-Di)/(Yi-1)) * Di)
          if (g <= k-1) Var2[g,g] <- var[g,g]
        }

        for (g in 1:k) for (h in 1:k) {
          if (g != h) {
            if (Yi != 1) Yi <- Yi + small
            var[g,h] <- var[g,h] + (w^2 * InRisk[g]/Yi * InRisk[h]/Yi * ((Yi-Di)/(Yi-1)) * Di) * -1
          }
          if (g <= k-1 && h <= k-1) Var2[g,h] <- var[g,h]
        }

        InRisk <- InRisk - Events[j,] - Censor[j,]
      }
    }

    Z2 <- matrix(Z[1:(k-1)], ncol=1)
    chisq <- as.numeric(t(Z2) %*% solve(Var2) %*% Z2)
    p <- 1 - pchisq(chisq, df=k-1)
    c(chisq=chisq, p=p)
  }

  methods <- c("logrank","gehan-breslow","tarone-ware","peto","modified peto")
  out <- data.frame(method=methods, chisq=NA_real_, p=NA_real_)
  for (i in seq_along(methods)) {
    v <- vb_weighted_logrank(methods[i])
    out$chisq[i] <- v["chisq"]
    out$p[i] <- v["p"]
  }
  write.csv(out, out_csv, row.names=FALSE)
  out
}

# Auto-generate reference CSV in working directory
try(write_weighted_logrank_reference(), silent=TRUE)
