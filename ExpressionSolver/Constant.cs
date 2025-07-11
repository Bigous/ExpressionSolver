﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class Constant(string name, decimal Value) : IExpression
{
    public string Name { get; } = name;

    public decimal Compute() => Value;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
