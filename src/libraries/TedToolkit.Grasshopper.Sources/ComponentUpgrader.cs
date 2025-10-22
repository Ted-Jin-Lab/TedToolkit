using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace TedToolkit.Grasshopper;

public abstract class ComponentUpgrader<TTarget>(Guid from, DateTime time)
    : IGH_UpgradeObject where TTarget : IGH_Component, new()
{
    private Guid? _toGuid;
    public DateTime Version => time;

    public Guid UpgradeFrom { get; } = from;

    public Guid UpgradeTo => _toGuid ??= new TTarget().ComponentGuid;

    public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
    {
        var result = new TTarget
        {
            NickName = target.NickName //Shall we?
        };

        var index = document.Objects.IndexOf(target);

        if (target is IGH_Component oldComponent) SwapParameters(oldComponent, result);

        result.CreateAttributes();
        result.Attributes.Pivot = target.Attributes.Pivot;
        result.Attributes.ExpireLayout();

        document.DestroyAttributeCache();
        document.DestroyObjectTable();
        document.RemoveObject(target, false);
        document.AddObject(result, false, index);

        return result;
    }

    private static void SwapParameters(IGH_Component oldComponent, IGH_Component newComponent)
    {
        foreach (var (from, to) in ConnectParameters(oldComponent.Params.Input, newComponent.Params.Input))
            GH_UpgradeUtil.MigrateSources(from, to);
        foreach (var (from, to) in ConnectParameters(oldComponent.Params.Output, newComponent.Params.Output))
            GH_UpgradeUtil.MigrateRecipients(from, to);
    }

    private static IEnumerable<(IGH_Param From, IGH_Param To)> ConnectParameters(List<IGH_Param> oldParams,
        List<IGH_Param> newParams)
    {
        //TODO: Better way to get the best param.
        foreach (var oldParam in oldParams)
            if (newParams.OrderByDescending(p => GetScore(oldParam, p)).FirstOrDefault() is { } newParam)
                yield return (oldParam, newParam);
    }

    private static int GetScore(IGH_Param paramA, IGH_Param paramB)
    {
        var score = 0;
        if (paramA.GetType() == paramB.GetType()) score += 10;
        score += GetStringScore(paramA.Name.ToUpper(), paramB.Name.ToUpper());
        return score;
    }

    private static int GetStringScore(string a, string b)
    {
        return -LevenshteinDistance(a, b);
    }

    public static int LevenshteinDistance(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var dp = new int[len1 + 1, len2 + 1];

        for (var i = 0; i <= len1; i++)
            dp[i, 0] = i;
        for (var j = 0; j <= len2; j++)
            dp[0, j] = j;

        for (var i = 1; i <= len1; i++)
        for (var j = 1; j <= len2; j++)
        {
            var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
            dp[i, j] = Math.Min(Math.Min(
                    dp[i - 1, j] + 1, // Deletion
                    dp[i, j - 1] + 1), // Insertion
                dp[i - 1, j - 1] + cost); // Substitution
        }

        return dp[len1, len2];
    }
}