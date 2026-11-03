// Reproducible real-Core before/after diagnostic probe. No estimator or reference promotion.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BESHStatNG;
using BESHStatNG.regression;
using BESHStatNG.ExcelCommon;

internal static class Diagnostic04A11Probe
{
    private static string D(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    private static LinearModel Fit(LinearBinomialFitIntent intent)
    {
        var model = new LinearModel { Alpha = intent.EffectiveFitAlpha, bReturnCov = true, bComputeResiduals = true };
        model.Data(intent.Request.Primary.Data, intent.Request.Primary.Names, intent.Request.Primary.RowIds, null);
        model.Fit(intent.IncludeIntercept, intent.TermGroups, intent.SumOfSquares);
        return model;
    }
    public static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var csv = File.ReadAllLines(Path.Combine(args[0], "BESHStat.Core.Tests", "TestData", "glm_gaussian_full.csv"));
        var header = csv[0].Split(',');
        var names = header.Where(x => x != "id" && x != "offset" && x != "w").ToArray();
        var data = new double[csv.Length - 1, names.Length];
        for (int i = 1; i < csv.Length; i++)
        {
            var cells = csv[i].Split(',');
            for (int j = 0; j < names.Length; j++) data[i - 1, j] = double.Parse(cells[Array.IndexOf(header, names[j])], CultureInfo.InvariantCulture);
        }
        var rows = Enumerable.Range(0, data.GetLength(0)).Select(i => 7 + 3 * i).ToArray();
        var effects = RegressionEffectModel.FromProjection(names.Skip(1), new Dictionary<string, TermSpec>(), true);
        var request = new RegressionRequest(RegressionAnalysisKind.LinearModel, names[0],
            new RegressionComponent(data, names, rows, effects, names), new RegressionOptions { Alpha = .10, Covariance = true, Residuals = true });
        var before = Fit(LinearBinomialRegressionPolicy.Project(request, TermSumOfSquaresType.TypeIII, null, null, LinearFitAlphaMode.NativeLegacyDefault));
        var after = Fit(LinearBinomialRegressionPolicy.Project(request, TermSumOfSquaresType.TypeIII, null, null));
        if (!before.results.Coeffs_est.SequenceEqual(after.results.Coeffs_est) || !before.results.Coeffs_SEsT.SequenceEqual(after.results.Coeffs_SEsT) || !before.Fitted.SequenceEqual(after.Fitted)) throw new Exception("Unintended fit delta.");
        var oldTables = before.wrapResults(); var newTables = after.wrapResults();
        for (int t = 1; t < oldTables.Count; t++)
        {
            var oldNumbers = oldTables[t].ToOutputModel().Values.Cast<object>().Where(v => v is double || v is int).ToArray();
            var newNumbers = newTables[t].ToOutputModel().Values.Cast<object>().Where(v => v is double || v is int).ToArray();
            if (!oldNumbers.SequenceEqual(newNumbers)) throw new Exception("Unintended model/ANOVA/df/VIF/covariance numeric delta.");
        }
        using (var stream = new FileStream(args[1], FileMode.CreateNew))
        using (var output = new StreamWriter(stream, new UTF8Encoding(false)) { NewLine = "\n" })
        {
            output.WriteLine("04A11 real-Core diagnostic deltas; NOT_GOLDEN; runtime=" + Environment.Version);
            output.WriteLine("requested_alpha=.10; legacy_effective=" + D(before.Alpha) + "; corrected_effective=" + D(after.Alpha));
            output.WriteLine("coefficients_SEs_fitted_equal=True; model_ANOVA_df_VIF_covariance_numeric_cells_equal=True");
            for (int i = 0; i < after.results.Coeffs_est.Length; i++)
                output.WriteLine("coefficient=" + i + "; estimate=" + D(after.results.Coeffs_est[i]) + "; SE=" + D(after.results.Coeffs_SEsT[i]) +
                    "; CI95=[" + D(before.results.Coeffs_CIlowT[i]) + "," + D(before.results.Coeffs_CIhighT[i]) + "]; CI90=[" + D(after.results.Coeffs_CIlowT[i]) + "," + D(after.results.Coeffs_CIhighT[i]) + "]");
            var glm = new GLM(new Gaussian(), new Identity()) { bReturnCov = true, bIterationDetails = true };
            var originalNames = (string[])names.Clone();
            glm.data(data, rows); glm.setVarNames(names); glm.settingInputs(.05, 100, .000001); glm.Fit(1, false);
            glm.wrapResults(null, null); glm.wrapResults(null, null);
            if (!names.SequenceEqual(originalNames)) throw new Exception("Name mutation.");
            output.WriteLine("covariance repeated_wraps=2; before_names=" + string.Join("|", originalNames) + "; after_names=" + string.Join("|", names));
            foreach (var response in new[] { 0.0, 1.0 })
            {
                var constant = new double[24, 2];
                for (int i = 0; i < 24; i++) { constant[i, 0] = response; constant[i, 1] = i - 11.5; }
                var failed = new GLM(new Binomial(), new Logit()) { bReturnCov = true, bIterationDetails = true, bComputeResiduals = true, bHosmerLemeshow = true };
                failed.data(constant); failed.setVarNames(new[] { "y", "x" }); failed.settingInputs(.05, 1, 1e-14); failed.Fit(1, false);
                var tables = failed.wrapResults(null, null);
                output.WriteLine("constant=" + D(response) + "; has_report_data=" + failed.HasReportData + "; converged=" + failed.Converged + "; tables=" + tables.Count + "; predictions_absent=" + (failed.PredictedResponses == null) + "; residuals_absent=" + (failed.AllResiduals == null));
            }
        }
    }
}
