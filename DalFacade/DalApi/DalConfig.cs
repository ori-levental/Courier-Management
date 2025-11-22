namespace DalApi;
using System.Xml.Linq;

/// <summary>
/// Static class responsible for reading and storing the application's configuration
/// regarding the active Data Access Layer (DAL) implementation. Implements the Simple Factory pattern's configuration source.
/// </summary>
static class DalConfig
{
    /// <summary>
    /// Metadata record containing the necessary information (Package/Assembly name, Namespace, Class name)
    /// to load a specific DAL implementation via reflection.
    /// </summary>
    internal record DalImplementation
    (string Package,    // package/dll name
     string Namespace, // namespace where DAL implementation class is contained in
     string Class    // DAL implementation class name
    );

    /// <summary>
    /// Stores the short name of the currently selected DAL (e.g., "xml" or "list").
    /// </summary>
    internal static string s_dalName;

    /// <summary>
    /// Dictionary mapping short DAL names (keys) to their loading metadata (DalImplementation).
    /// </summary>
    internal static Dictionary<string, DalImplementation> s_dalPackages;

    /// <summary>
    /// Static constructor. Loads and parses the 'dal-config.xml' file once to initialize the static selection fields.
    /// </summary>
    static DalConfig()
    {
        XElement dalConfig = XElement.Load(@"..\xml\dal-config.xml") ??
            throw new DalConfigException("dal-config.xml file is not found");

        s_dalName =
            dalConfig.Element("dal")?.Value ?? throw new DalConfigException("<dal> element is missing");

        var packages = dalConfig.Element("dal-packages")?.Elements() ??
            throw new DalConfigException("<dal-packages> element is missing");

        // The LINQ query parses XML elements into the dictionary
        s_dalPackages = (from item in packages
                         let pkg = item.Value
                         let ns = item.Attribute("namespace")?.Value ?? "Dal"
                         let cls = item.Attribute("class")?.Value ?? pkg
                         select (item.Name, new DalImplementation(pkg, ns, cls))
                            ).ToDictionary(p => "" + p.Name, p => p.Item2);
    }
}

/// <summary>
/// Exception thrown when there is an error loading or parsing the DAL configuration file (dal-config.xml).
/// </summary>
[Serializable]
public class DalConfigException : Exception
{
    public DalConfigException(string msg) : base(msg) { }
    public DalConfigException(string msg, Exception ex) : base(msg, ex) { }
}