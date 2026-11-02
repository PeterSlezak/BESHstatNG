# SOURCE PROPOSAL ONLY. NOT EXECUTED OR SCIENTIFICALLY APPROVED IN CHECKPOINT 02.
# No automatic test/build hook. Requires a separately reviewed owner release DCF.
# SHA fields below are preflight attestations; this script does not calculate SHA256.
# The operator must independently verify executable/runtime/script hashes before run.
main <- function() {
  args <- commandArgs(trailingOnly = TRUE)
  if (length(args) != 2L) stop("Expected approved-release.dcf and NEW candidate.json")
  approval <- read.dcf(args[1], all = TRUE)
  required <- c("RunIndependentReference", "PromoteNumericalGolden", "NativeWorkbookCapture",
                "OwnerDecision", "RVersion", "RscriptSHA256", "RRuntimeSHA256", "ScriptSHA256",
                "SHA256PreflightVerified", "TolerancePolicy", "TailPolicy", "FiniteSingularPolicy")
  if (nrow(approval) != 1L || !all(required %in% names(approval))) stop("Incomplete release")
  field <- function(name) as.character(approval[[name]][1])
  if (field("RunIndependentReference") != "YES" || field("PromoteNumericalGolden") != "NO" ||
      field("NativeWorkbookCapture") != "NO" || field("SHA256PreflightVerified") != "YES") stop("Not released")
  if (field("RVersion") != "4.5.1" || as.character(getRversion()) != field("RVersion") ||
      as.character(utils::packageVersion("stats")) != field("RVersion")) stop("Unapproved R/stats version")
  for (key in c("RscriptSHA256", "RRuntimeSHA256", "ScriptSHA256")) {
    if (!grepl("^[0-9a-f]{64}$", field(key))) stop("Invalid preflight SHA256")
  }
  for (key in c("OwnerDecision", "TolerancePolicy", "TailPolicy", "FiniteSingularPolicy")) {
    if (!grepl("^[A-Za-z0-9][A-Za-z0-9._-]*$", field(key))) stop("Invalid decision identifier")
  }
  if (field("TailPolicy") != "upper-F-2-6" || field("FiniteSingularPolicy") != "reject-unapproved-degenerate") stop("Unapproved inference policy")
  if (file.exists(args[2])) stop("Output exists; no snapshot rewriting")
  options(warn = 2, OutDec = ".")
  if (Sys.setlocale("LC_NUMERIC", "C") == "") stop("Invariant numeric culture unavailable")

  groups <- list(A = c(1, 2, 3), B = c(2, 4, 6), C = c(3, 5, 7))
  row_ids <- list(A = c(101L, 103L, 107L), B = c(109L, 113L, 127L), C = c(131L, 137L, 139L))
  if (!identical(lengths(groups), lengths(row_ids)) || any(lengths(groups) < 2L) ||
      any(!is.finite(unlist(groups)))) stop("Invalid frozen input")
  response <- unlist(groups, use.names = FALSE)
  group <- factor(rep(names(groups), lengths(groups)), levels = c("A", "B", "C"))
  primary <- stats::oneway.test(response ~ group, var.equal = TRUE, na.action = stats::na.fail)
  independent_table <- stats::anova(stats::lm(response ~ group, na.action = stats::na.fail))
  ss <- independent_table[["Sum Sq"]]
  ms <- independent_table[["Mean Sq"]]
  df <- as.integer(independent_table[["Df"]])
  if (!identical(as.integer(primary$parameter), c(2L, 6L)) || !identical(df, c(2L, 6L)) ||
      ss[2] <= 0 || any(!is.finite(c(primary$statistic, primary$p.value, ss, ms)))) stop("Undefined/unapproved result")
  if (primary$p.value <= 0 || primary$p.value >= 1) stop("Extreme/boundary tail policy is not approved")
  comparison <- list(case_id = "C30-SYN-ANOVA", kind = "independent-reference-candidate",
    status = "PENDING_REVIEW_NOT_A_GOLDEN", source_commit = "12e25924aae93f0b36ab33937176c86c19641b50",
    dataset_sha256 = "98dce529f444e6ede2f5f8e34ef072cd7407efd2a3a25597d2d1504c6b4f6cba",
    group_labels = c("A", "B", "C"), groups = groups, row_ids = row_ids, row_id_transport = "provenance-only", alpha = 0.05,
    seed = "N/A", culture = "InvariantCulture/C", method = "classical equal-variance one-way ANOVA",
    Welch = FALSE, posthoc = FALSE, FStatistic = unname(primary$statistic), PValue = primary$p.value,
    df = df, SSBetween = ss[1], SSWithin = ss[2], SSTotal = sum(ss), MSBetween = ms[1], MSWithin = ms[2])
  provenance <- list(owner_decision = field("OwnerDecision"), tolerance_policy = field("TolerancePolicy"),
    R_version = as.character(getRversion()), stats_version = as.character(utils::packageVersion("stats")),
    Rscript_sha256 = field("RscriptSHA256"), runtime_sha256 = field("RRuntimeSHA256"), script_sha256 = field("ScriptSHA256"),
    generated_at_utc = format(Sys.time(), tz = "UTC", format = "%Y-%m-%dT%H:%M:%SZ"),
    citation = "R stats oneway.test var.equal=TRUE and anova(lm); see release proposal")

  # Small base-R JSON writer for this fixed primitive/list schema; no packages.
  # Decision strings are ASCII identifiers/SHA256; other strings are fixed or R version/time metadata.
  json <- function(value) {
    if (is.null(value)) return("null")
    if (is.list(value)) {
      values <- vapply(value, json, character(1))
      if (is.null(names(value))) return(paste0("[", paste(values, collapse = ","), "]"))
      return(paste0("{", paste(paste0(encodeString(names(value), quote = '"'), ":", values), collapse = ","), "}"))
    }
    if (length(value) != 1L) return(paste0("[", paste(vapply(as.list(value), json, character(1)), collapse = ","), "]"))
    if (is.logical(value)) return(if (isTRUE(value)) "true" else "false")
    if (is.character(value)) return(encodeString(value, quote = '"'))
    if (!is.numeric(value) || !is.finite(value)) stop("Unsupported/nonfinite output")
    trimws(formatC(value, digits = 17, format = "g", decimal.mark = "."))
  }
  payload <- json(list(schema = "04A02-independent-candidate-v1", comparison = comparison, provenance = provenance))
  con <- file(args[2], open = "wx", encoding = "UTF-8")
  on.exit(close(con), add = TRUE)
  writeLines(payload, con, useBytes = TRUE)
}
main()
