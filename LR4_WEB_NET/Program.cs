
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

ConstructorInfo? factoryConstructor = typeof(Factory).GetConstructor(
    BindingFlags.Instance | BindingFlags.Public,
    new Type[] { typeof(string), typeof(ulong), typeof(FactoryType) }
    );
Factory? factory1 = (Factory?)factoryConstructor?.Invoke(new object[] { "Intel", (UInt64)1500, FactoryType.Electronics });
Type? factory1InstanceType = factory1?.GetType();
Console.WriteLine("IsClass: " + factory1InstanceType?.IsClass);
Console.WriteLine("Name: " + factory1InstanceType?.Name);
Console.WriteLine("GUID: " + Type.GetType("factory", false, true)?.GUID);
Console.WriteLine("Base type: " + factory1InstanceType?.BaseType);
Console.WriteLine("FactoryType enum names: " + String.Join(", ", factory1?.GetFactoryType().GetType()?.GetEnumNames() ?? new object[] { }));

TypeInfo factoryInstanceTypeInfo = typeof(Factory).GetTypeInfo();

Console.WriteLine("\nDeclared constructors:");
foreach (var constructor in factoryInstanceTypeInfo.DeclaredConstructors)
{
    Console.Write("(");
    Console.Write(String.Join(", ", constructor.GetParameters().Select(parameter => parameter.ParameterType + " " + parameter.Name)));
    Console.Write(")");
    Console.WriteLine();
}

Console.WriteLine("\nDeclared static and instance, non-public and public members:");
foreach (var member in factoryInstanceTypeInfo.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
{
    Console.WriteLine($"{member.MemberType} {member.Name} Declared in {member.DeclaringType.Name}");
    Console.WriteLine();
}

Console.WriteLine("\nDeclared static and instance, non-public and public field-specific data:");
foreach (var field in factoryInstanceTypeInfo.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
{
    Console.WriteLine($"{(field.IsPublic ? "public" : field.IsPrivate ? "private" : field.IsFamily ? "protected" : String.Empty)} {field.MemberType} {field.FieldType} {field.Name}");
    if (field.Name == "EmployeeCount")
    {
        Console.WriteLine($"Increasing employee count of {factory1.Name} by 100");
        var previousEmployeeCount = field.GetValue(factory1);
        Console.WriteLine($"Previous count: {previousEmployeeCount}");
        var newValue = (ulong?)Convert.ChangeType(previousEmployeeCount, factory1?.EmployeeCount.GetType()) ?? 0ul;
        field.SetValue(factory1, newValue + 100);
        Console.WriteLine($"Current count: {field.GetValue(factory1)}");

    }
    Console.WriteLine();
}

Console.WriteLine("\nDeclared static and instance, non-public and public method-specific data:");
foreach (var method in factoryInstanceTypeInfo.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
{
    Console.WriteLine($"Name: {method.Name}");
    Console.WriteLine($"Is generic: {method.IsGenericMethod}");
    Console.WriteLine($"Parameters: ({String.Join(", ", method.GetParameters().Select(param => $"{param.Position} {param.ParameterType.Name} {param.Name} {param.DefaultValue}"))})");
    Console.WriteLine($"Return type: {method.ReturnType}");
    if (method.Name == "Reprofile")
    {
        Console.WriteLine($"Reprofiling {factory1.Name} (was {factory1.GetFactoryType()})");
        var result = method.Invoke(factory1, new object[] { FactoryType.Chemical });
        Console.WriteLine($"Execution result: {result}");
    }
    else if (method.Name == "DisplayStats")
    {
        Console.WriteLine("Displaying stats via a delegate");
        var methodDelegate = method.CreateDelegate<Action>(factory1);
        methodDelegate.DynamicInvoke();
    }
}

internal class Factory
{
    public string Name { protected set; get; } = String.Empty;
    public UInt64 EmployeeCount;

    static protected UInt32 FactoriesCount { set; get; }

    protected FactoryType Type = FactoryType.Unknown;

    private UInt64 Id;

    public Factory(string name, ulong employeeCount, FactoryType type)
    {
        Name = name;
        EmployeeCount = employeeCount;
        Reprofile(type);
        Id = Factory.FactoriesCount++;

    }

    public static UInt32 GetTotalFactoriesCount()
    {
        return Factory.FactoriesCount;
    }

    public void DisplayStats()
    {
        Console.WriteLine($"'{Name}' stats ============");
        Console.WriteLine($"Id in DB: {Enum.GetName<FactoryType>(Type)}");
        Console.WriteLine($"Type: {Enum.GetName<FactoryType>(Type)}");
        Console.WriteLine($"Employee count: {EmployeeCount}");
        Console.WriteLine($"=========================");
    }

    public FactoryType GetFactoryType() => Type;

    private FactoryType Reprofile(FactoryType newType)
    {
        FactoryType oldType = Type;
        Type = newType;
        return oldType;
    }
}

enum FactoryType { Unknown, Chemical, Automotive, Electronics };

interface AAA
{
    string s { get; set; }
}