using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CYM.Excel
{
    public interface IConverter
    {
        object Convert(string input);

        Type Type { get; }
    }
}