# PROPOSAL REVISION 2. SOURCE ONLY; NOT EXECUTED OR RUNTIME QUALIFIED.
# Supersedes publication-unsafe v1; v1 bytes remain available unchanged.
# This script NEVER opens a candidate output path. Only the approved Python
# operator may validate stdout/stderr and publish through its no-clobber boundary.
main <- function() {
  options(warn = 2, OutDec = ".")
  args <- commandArgs(trailingOnly = TRUE)
  if (length(args) != 1L) stop("Expected runner-owned approved-input.dcf only")
  required <- c("DecisionId", "CaseId", "RVersion", "StatsVersion", "ScriptSHA256",
                "RscriptSHA256", "RuntimeSetSHA256", "MethodId", "TailPolicy", "FinitePolicy")
  lines <- readLines(args[1], warn = FALSE, encoding = "UTF-8")
  if (length(lines) != length(required) || anyNA(lines) ||
      any(!grepl("^[A-Za-z][A-Za-z0-9]*: [A-Za-z0-9][A-Za-z0-9._-]*$", lines))) stop("Noncanonical DCF")
  fields <- sub(": .*", "", lines)
  if (anyDuplicated(fields) || !identical(fields, required)) stop("Duplicate/missing/extra DCF field")
  approval <- read.dcf(args[1], fields = required, all = FALSE, keep.white = required)
  if (nrow(approval) != 1L || ncol(approval) != length(required) ||
      anyNA(approval) || any(!nzchar(approval))) stop("Not exactly one complete record")
  field <- function(name) unname(approval[1, name])
  if (field("RVersion") != "4.5.1" || field("StatsVersion") != "4.5.1" ||
      as.character(getRversion()) != field("RVersion") ||
      as.character(utils::packageVersion("stats")) != field("StatsVersion")) stop("Unapproved R/stats version")
  for (key in c("ScriptSHA256", "RscriptSHA256", "RuntimeSetSHA256")) {
    if (!grepl("^[0-9a-f]{64}$", field(key))) stop("Invalid verified hash binding")
  }
  if (Sys.setlocale("LC_NUMERIC", "C") == "") stop("Invariant numeric locale unavailable")
  if (field("CaseId") != "C30-SYN-KW" || field("MethodId") != "kruskal-wallis-asymptotic" ||
      field("TailPolicy") != "upper-chi-square-2-raw-and-corrected" || field("FinitePolicy") != "reject-unapproved-degenerate") stop("Wrong case/policy")
  groups <- list(A = c(1, 2, 2), B = c(2, 4, 6), C = c(3, 5, 7))
  row_ids <- list(A = c(101L, 103L, 107L), B = c(109L, 113L, 127L), C = c(131L, 137L, 139L))
  if (!identical(lengths(groups), lengths(row_ids)) || any(lengths(groups) < 2L) ||
      any(!is.finite(unlist(groups)))) stop("Invalid frozen input")
  pooled <- unlist(groups, use.names = FALSE)
  group <- factor(rep(names(groups), lengths(groups)), levels = c("A", "B", "C"))
  N <- length(pooled)
  ranks <- rank(pooled, ties.method = "average", na.last = "keep")
  rank_sums <- vapply(split(ranks, group, drop = FALSE), sum, numeric(1))
  counts <- lengths(groups)
  ties <- as.numeric(table(pooled))
  correction <- 1 - sum(ties^3 - ties) / (N^3 - N)
  H <- 12 / (N * (N + 1)) * sum(rank_sums^2 / counts) - 3 * (N + 1)
  df <- length(groups) - 1L
  if (correction <= 0 || df != 2L || any(!is.finite(c(H, correction, ranks)))) stop("Undefined/unapproved rank inference")
  raw_p <- stats::pchisq(H, df = df, lower.tail = FALSE)
  corrected <- stats::kruskal.test(groups)
  if (as.integer(corrected$parameter) != 2L ||
      any(!is.finite(c(raw_p, corrected$statistic, corrected$p.value)))) stop("Undefined/unapproved result")
  if (raw_p <= 0 || raw_p >= 1 || corrected$p.value <= 0 || corrected$p.value >= 1) stop("Extreme/boundary tail policy is not approved")
  comparison <- list(case_id = "C30-SYN-KW", status = "PENDING_REVIEW_NOT_A_GOLDEN",
    source_commit = "12e25924aae93f0b36ab33937176c86c19641b50",
    dataset_sha256 = "7e156caca9ded21a8e0967d5755ec40143a11542b2094cc03ebe7474400e120e",
    dataset = list(group_labels = c("A", "B", "C"), values = unname(groups), row_ids = unname(row_ids)),
    row_id_transport = "provenance-only", alpha = 0.05, seed = "N/A", culture = "InvariantCulture/C",
    method_id = "kruskal-wallis-asymptotic", tail_policy = "upper-chi-square-2-raw-and-corrected",
    options = list(IncludeDunnComparisons = FALSE),
    metrics = list(HStatistic = unname(H), PValue = unname(raw_p), TieCorrectedHStatistic = unname(corrected$statistic),
      TieCorrectedPValue = unname(corrected$p.value), tie_correction = unname(correction),
      formula_corrected_H_for_separate_review = unname(H / correction), rank_sums = unname(rank_sums), DF = 2L))

  # Explicit JSON escapes, not R encodeString escape syntax. Fixed supported
  # primitive/list schema; NA/NaN/Infinity and duplicate/empty keys are refused.
  quote_json <- function(value) {
    if (length(value) != 1L || is.na(value)) stop("Missing/string shape")
    points <- utf8ToInt(enc2utf8(value))
    if (anyNA(points)) stop("Invalid Unicode")
    escaped <- vapply(points, function(point) {
      if (point == 34L) return(paste0(intToUtf8(92L), intToUtf8(34L)))
      if (point == 92L) return(paste0(intToUtf8(92L), intToUtf8(92L)))
      if (point < 32L) return(paste0(intToUtf8(92L), "u", sprintf("%04x", point)))
      intToUtf8(point)
    }, character(1))
    paste0('"', paste(escaped, collapse = ""), '"')
  }
  json <- function(value) {
    if (is.null(value)) return("null")
    if (is.list(value)) {
      values <- vapply(value, json, character(1))
      if (is.null(names(value))) return(paste0("[", paste(values, collapse = ","), "]"))
      if (anyNA(names(value)) || any(!nzchar(names(value))) || anyDuplicated(names(value))) stop("Invalid object keys")
      keys <- vapply(names(value), quote_json, character(1))
      return(paste0("{", paste(paste0(keys, ":", values), collapse = ","), "}"))
    }
    if (length(value) != 1L) return(paste0("[", paste(vapply(as.list(value), json, character(1)), collapse = ","), "]"))
    if (is.na(value)) stop("Missing scalar output")
    if (is.logical(value)) return(if (value) "true" else "false")
    if (is.character(value)) return(quote_json(value))
    if (!is.numeric(value) || !is.finite(value)) stop("Unsupported/nonfinite output")
    if (is.integer(value)) return(as.character(value))
    trimws(formatC(value, digits = 17, format = "g", decimal.mark = "."))
  }
  provenance <- list(decision_id = field("DecisionId"), r_version = as.character(getRversion()),
    stats_version = as.character(utils::packageVersion("stats")), script_version = "proposal-2",
    script_sha256 = field("ScriptSHA256"), rscript_sha256 = field("RscriptSHA256"),
    runtime_set_sha256 = field("RuntimeSetSHA256"))
  payload <- json(list(schema = "04A02-independent-candidate-v2", comparison = comparison, provenance = provenance))
  # One complete object line only; warnings/errors go to retained stderr and
  # cause Python publication refusal even if stdout happens to be valid JSON.
  cat(payload, "\n", sep = "")
}
main()
