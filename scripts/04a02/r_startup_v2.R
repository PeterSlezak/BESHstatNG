# C11 nonstatistical startup proof. No reference calculations or package installs.
options(warn=2)
q <- function(x) paste0('"', gsub('"', '\\"', gsub('\\', '/', x, fixed=TRUE), fixed=TRUE), '"')
arr <- function(x) paste0('[',paste(vapply(x,q,character(1)),collapse=','),']')
if (as.character(getRversion()) != '4.5.1' || as.character(utils::packageVersion('stats')) != '4.5.1') stop('Wrong runtime')
# Deterministic metadata inspection from the bundled library only. The operator
# holds every compiler file pinned and deny-write locked before this process.
# If an already-loaded namespace came from elsewhere, its measured path still
# fails admission; lib.loc never turns a personal namespace into bundled proof.
compiler_ns <- loadNamespace('compiler',lib.loc=file.path(R.home(),'library'))
ns <- loadedNamespaces()
compiler <- 'null'
if ('compiler' %in% ns) {
  compiler <- paste0('{"namespace_path":',q(getNamespaceInfo(compiler_ns,'path')),
    ',"namespace_version":',q(as.character(getNamespaceVersion(compiler_ns))),
    ',"package_path":',q(find.package('compiler',lib.loc=.libPaths())),
    ',"package_version":',q(as.character(utils::packageVersion('compiler',lib.loc=file.path(R.home(),'library')))),'}')
}
envkeys <- c('HOME','R_HOME','R_PROFILE_USER','R_ENVIRON_USER','R_LIBS','R_LIBS_USER','R_LIBS_SITE','R_DEFAULT_PACKAGES')
environment <- paste0('{',paste(vapply(envkeys,function(key) paste0(q(key),':',q(Sys.getenv(key))),character(1)),collapse=','),'}')
cat(paste0('{"schema":"04A02-C11-R-startup-v2","r":',q(as.character(getRversion())),
  ',"stats":',q(as.character(utils::packageVersion('stats'))),',"arch":',q(R.version$arch),
  ',"locale":',q(Sys.getlocale('LC_NUMERIC')),',"home":',q(Sys.getenv('R_USER')),
  ',"r_profile":',q(Sys.getenv('R_PROFILE')),',"r_environ":',q(Sys.getenv('R_ENVIRON')),
  ',"libraries":',arr(.libPaths()),',"namespaces":',arr(loadedNamespaces()),
  ',"compiler":',compiler,',"environment":',environment,'}\n'))
