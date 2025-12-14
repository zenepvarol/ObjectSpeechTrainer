using UnityEngine;

public static class PronunciationEvaluator
{
    public static int SimilarityScore(string target, string said)
    {
        target = target.ToLower().Trim();
        said = said.ToLower().Trim();

        int distance = Levenshtein(target, said);
        int maxLen = Mathf.Max(target.Length, said.Length);

        float similarity = 1f - (float)distance / maxLen;

        return Mathf.RoundToInt(similarity * 100);
    }

    private static int Levenshtein(string a, string b)
    {
        int[,] dp = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            dp[i, 0] = i;

        for (int j = 0; j <= b.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i < dp.GetLength(0); i++)
        {
            for (int j = 1; j < dp.GetLength(1); j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;

                dp[i, j] = Mathf.Min(
                    dp[i - 1, j] + 1,
                    dp[i, j - 1] + 1,
                    dp[i - 1, j - 1] + cost
                );
            }
        }

        return dp[a.Length, b.Length];
    }
}
