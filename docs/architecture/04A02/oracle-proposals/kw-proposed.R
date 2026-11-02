# SOURCE PROPOSAL ONLY. NOT EXECUTED OR SCIENTIFICALLY APPROVED IN CHECKPOINT 02.
# No automatic test/build hook. Requires a separately reviewed owner release DCF.
# SHA fields are attestations, not calculations: operator must verify the exact
# executable/runtime/script SHA256 externally before permitting this invocation.
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
  if (field("TailPolicy") != "upper-chi-square-2-raw-and-corrected" ||
      field("FiniteSingularPolicy") != "reject-unapproved-degenerate") stop("Unapproved inference policy")
  if (file.exists(args[2])) stop("Output exists; no snapshot rewriting")
  options(warn = 2, OutDec = ".")
  if (Sys.setlocale("LC_NUMERIC", "C") == "") stop("Invariant numeric culture unavailable")

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
  comparison <- list(case_id = "C30-SYN-KW", kind = "independent-reference-candidate",
    status = "PENDING_REVIEW_NOT_A_GOLDEN", source_commit = "12e25924aae93f0b36ab33937176c86c19641b50",
    dataset_sha256 = "7e156caca9ded21a8e0967d5755ec40143a11542b2094cc03ebe7474400e120e",
    group_labels = c("A", "B", "C"), groups = groups, row_ids = row_ids, row_id_transport = "provenance-only", alpha = 0.05,
    seed = "N/A", culture = "InvariantCulture/C", method = "average ranks; asymptotic chi-square; no Dunn",
    Dunn = FALSE, exact_inference = FALSE, df = df, HStatistic = unname(H), PValue = raw_p,
    TieCorrectedHStatistic = unname(corrected$statistic), TieCorrectedPValue = corrected$p.value,
    tie_correction = correction, rank_sums = as.list(rank_sums),
    formula_corrected_H_for_separate_review = unname(H / correction))
  provenance <- list(owner_decision = field("OwnerDecision"), tolerance_policy = field("TolerancePolicy"),
    R_version = as.character(getRversion()), stats_version = as.character(utils::packageVersion("stats")),
    Rscript_sha256 = field("RscriptSHA256"), runtime_sha256 = field("RRuntimeSHA256"), script_sha256 = field("ScriptSHA256"),
    generated_at_utc = format(Sys.time(), tz = "UTC", format = "%Y-%m-%dT%H:%M:%SZ"),
    citation = "R stats kruskal.test; separately reviewed raw-H/rank/tie formula; see release proposal")

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
