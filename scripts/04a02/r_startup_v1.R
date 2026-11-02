# Nonstatistical startup proof only. No reference algorithm, personal packages or filesystem writes.
options(warn=2)
q <- function(x) paste0('"', gsub('"', '\\"', gsub('\\', '/', x, fixed=TRUE), fixed=TRUE), '"')
arr <- function(x) paste0('[',paste(vapply(x,q,character(1)),collapse=','),']')
if (as.character(getRversion()) != '4.5.1' || as.character(utils::packageVersion('stats')) != '4.5.1') stop('Wrong runtime')
cat(paste0('{"schema":"04A02-C09-R-startup-v1","r":',q(as.character(getRversion())),
  ',"stats":',q(as.character(utils::packageVersion('stats'))),',"arch":',q(R.version$arch),
  ',"locale":',q(Sys.getlocale('LC_NUMERIC')),',"home":',q(Sys.getenv('R_USER')),
  ',"r_profile":',q(Sys.getenv('R_PROFILE')),',"r_environ":',q(Sys.getenv('R_ENVIRON')),
  ',"libraries":',arr(.libPaths()),',"namespaces":',arr(loadedNamespaces()),'}\n'))
