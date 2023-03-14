//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: StringExpression – Расширение операций со строками.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Text.RegularExpressions;
    #endregion Using

    /// <summary> Расширение операций со строками.</summary>
    public static class StringExpression
    {
        public static string[] SplitArguments(this string s) =>
            Regex.Matches(s, @"#(?=\d)|(?<=#)\d+|("".*?"")|[=<>]+\s*|(?<=^|\s|[=<>]).*?(?=\s|[=<>]|$)").Cast<Match>()
                .Select(m => m.Value.Trim()).Where(v => v != string.Empty).ToArray();
    }
}