# Isolated read-only candidate comparison; never a promoted numerical golden.
stopifnot(as.character(getRversion()) == "4.5.1")
library(survival)
i <- 0:23
d <- data.frame(time=1+((i*7) %% 11), status=ifelse(i %% 4 == 0,0,1),
                group=ifelse(i %% 3 == 0,"b","A"),
                stratum=ifelse(i %% 2 == 0,"S z","S a"),
                x1=(((i*5) %% 13)-6)/5, x2=i %% 3)
out <- data.frame(case=character(), field=character(), value=double())
add <- function(case, field, value) {
  out <<- rbind(out, data.frame(case=case,field=field,value=as.numeric(value)))
}
for (tie in c("breslow","efron","exact")) {
  for (stratified in c(FALSE,TRUE)) {
    formula <- if(stratified) Surv(time,status) ~ x1+x2+strata(stratum) else Surv(time,status) ~ x1+x2
    fit <- coxph(formula,data=d,ties=tie,control=coxph.control(iter.max=50,eps=1e-6),x=TRUE)
    key <- paste("Cox",tie,if(stratified) "strata" else "plain",sep="|")
    add(key,"x1",coef(fit)[1]); add(key,"x2",coef(fit)[2])
    add(key,"loglik",fit$loglik[2])
  }
}
for (stratified in c(FALSE,TRUE)) {
  formula <- if(stratified) Surv(time,status) ~ group+strata(stratum) else Surv(time,status) ~ group
  fit <- survdiff(formula,data=d,rho=0)
  add(paste("Logrank",if(stratified) "strata" else "groups",sep="|"),"chi-square",fit$chisq)
}
km <- survfit(Surv(time,status) ~ group,data=d,conf.type="log-log",conf.int=.95)
for (g in unique(d$group)) {
  indices <- which(rep(names(km$strata),km$strata) == paste0("group=",g))
  for(j in indices) add(paste("KM",g,sep="|"),paste0("S@",km$time[j]),km$surv[j])
}
args <- commandArgs(trailingOnly=TRUE)
stopifnot(length(args)==1, !file.exists(args[1]))
write.csv(out,args[1],row.names=FALSE)
cat("NOT_GOLDEN\n",R.version.string,"\nsurvival ",as.character(packageVersion("survival")),
    "\nstatus: 0=censored, 1=event; KM log-log 95% CI; cox ties explicit; survdiff rho=0 only.\n",sep="")
