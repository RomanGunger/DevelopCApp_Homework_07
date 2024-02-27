using System.Reflection;
using System.Text;

[AttributeUsage(AttributeTargets.Property)]
class DontSaveAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Property)]
class CustomNameAttribute : Attribute
{
    public string Name { get; }

    public CustomNameAttribute(string name)
    {
        this.Name = name;
    }
}


class TestClass
{
    [CustomName("newName")]
    public int I { get; set; }
    [CustomName("newNam1e")]
    public string? S { get; set; }
    public decimal D { get; set; }
    public char[]? C { get; set; }

    public TestClass() { }
    public TestClass(int i)
    {
        this.I = i;
    }
    public TestClass(int i, string s, decimal d, char[] c) : this(i)
    {
        this.S = s;
        this.D = d;
        this.C = c;
    }

}

class Program
{
    public static void Main()
    {
        //TestClass type = new TestClass(1, "fdfds", 11m, new char[] { '1', 'd' });

        var str = (Activator.CreateInstance(typeof(TestClass), new object[] { 2, "fdfds", 111m, new char[] { '1', 'd' } })) as TestClass;
        Console.WriteLine(ObjectToString(str));

        Console.WriteLine();

        var obj = StringToObj(ObjectToString(str));
        Console.WriteLine(ObjectToString(obj));
    }

    static string ObjectToString(object o)
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(o.GetType().ToString());
        var properties = o.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.GetValue(o, null) == null)
                continue;

            if (property.GetCustomAttribute<DontSaveAttribute>() != null)
                continue;

            CustomNameAttribute attribute = property.GetCustomAttribute<CustomNameAttribute>();
            if (attribute != null)
            {
                string propertyName = attribute.Name;
                builder.Append($" {propertyName}:");
            }
            else
                builder.Append($" {property.Name}:");

            if (property.PropertyType == typeof(char[]))
            {
                foreach (var value in property.GetValue(o, null) as char[])
                {
                    builder.Append($"{value},");
                }
            }
            else
            {
                builder.Append($"{property.GetValue(o)}");
            }
        }

        return builder.ToString().Remove(builder.Length - 1);
    }


    static object StringToObj(string data)
    {
        var parser = data.Split(" ");

        object obj = new object();
        obj = Activator.CreateInstance(null, parser[0]).Unwrap();


        for (int i = 1; i < parser.Length; i++)
        {
            string name = string.Empty;

            int indexName = parser[i].IndexOf(":");
            if (indexName >= 0)
                name = parser[i].Substring(0, indexName);

            int indexValue = parser[i].IndexOf(":") + 1;
            if (indexValue >= 0)
                parser[i] = parser[i].Substring(indexValue);


            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
            {
                CustomNameAttribute attribute = propertyInfo.GetCustomAttribute<CustomNameAttribute>();

                if (attribute?.Name == name)
                {
                    Type propType = propertyInfo.PropertyType;

                    object parsedVal = Convert.ChangeType(parser[i], propType);
                    propertyInfo.SetValue(obj, parsedVal);

                    i++;
                    indexName = parser[i].IndexOf(":");
                    if (indexName >= 0)
                        name = parser[i].Substring(0, indexName);
                    indexValue = parser[i].IndexOf(":") + 1;
                    if (indexValue >= 0)
                        parser[i] = parser[i].Substring(indexValue);
                }
            }

            Type type = obj.GetType();
            var property = type.GetProperty(name);

            if (property.PropertyType == typeof(int))
                property.SetValue(obj, int.Parse(parser[i]));
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(obj, parser[i]);
            }
            else if (property.PropertyType == typeof(decimal))
                property.SetValue(obj, decimal.Parse(parser[i]));
            else if (property.PropertyType == typeof(char[]))
            {
                int removeIndex = parser[i].IndexOf(",");
                parser[i] = parser[i].Remove(removeIndex, removeIndex);

                char[] charArr;
                charArr = parser[i].ToCharArray();

                property.SetValue(obj, charArr);
            }
        }

        return obj;
    }

}