using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class Variable(string name, decimal value) : IExpression
{
    public string Name => name;

    public decimal Value { get; set; } = value;

    public decimal Compute() => Value;

    public override string ToString() => Name;
}
