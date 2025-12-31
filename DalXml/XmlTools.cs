namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

/// <summary>
/// Static utility class for handling all XML serialization, deserialization,
/// and configuration file access (XML Config).
/// </summary>
static class XMLTools
{
    // Path to the XML files directory
    const string s_xmlDir = @"..\xml\";

    static XMLTools()
    {
        // Ensures the XML directory exists before any file operation
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    /// <summary>
    /// Saves an entire generic List of entities to an XML file using XmlSerializer.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="list">The List object to serialize.</param>
    /// <param name="xmlFileName">The name of the destination XML file.</param>
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a List of entities from an XML file using XmlSerializer.
    /// If the file does not exist, returns a new empty list.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="xmlFileName">The name of the XML source file.</param>
    /// <returns>The deserialized List of entities, or a new empty list.</returns>
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (!File.Exists(xmlFilePath)) return new();
            using FileStream file = new(xmlFilePath, FileMode.Open);
            XmlSerializer x = new(typeof(List<T>));
            return x.Deserialize(file) as List<T> ?? new();
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region SaveLoadWithXElement
    /// <summary>
    /// Saves an XElement structure (the root of an XML document) to a file.
    /// </summary>
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the root XElement from a file. If the file does not exist, creates a new file with an empty root element and returns it.
    /// </summary>
    /// <param name="xmlFileName">The name of the XML file.</param>
    /// <returns>The loaded or newly created root XElement.</returns>
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
            XElement rootElem = new(xmlFileName);
            rootElem.Save(xmlFilePath);
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region XmlConfig
    /// <summary>
    /// Reads an integer configuration value, increments it by one, saves the new value, and returns the original value. Used for running IDs.
    /// </summary>
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }

    /// <summary>
    /// Reads a configuration value and returns it as a non-nullable integer. Throws FormatException if conversion fails.
    /// </summary>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a configuration value and returns it as a non-nullable double. Throws FormatException if conversion fails.
    /// </summary>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToDoubleNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a configuration value and returns it as a nullable double (double?).
    /// </summary>
    public static double? GetConfigNullableDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return root.ToDoubleNullable(elemName);
    }

    /// <summary>
    /// Reads a configuration value and returns it as a non-nullable DateTime. Throws FormatException if conversion fails.
    /// </summary>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Reads a configuration value and returns it as a non-nullable TimeSpan. Throws FormatException if conversion fails.
    /// </summary>
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        TimeSpan dt = root.ToTimeSpanNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Reads a configuration value and returns it as a non-nullable string. Throws FormatException if value is missing/null.
    /// </summary>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string dt = root.ToStringNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Sets a new non-nullable integer value in the config file.
    /// </summary>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a new non-nullable DateTime value in the config file.
    /// </summary>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a new non-nullable TimeSpan value in the config file.
    /// </summary>
    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a new non-nullable string value in the config file.
    /// </summary>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a new non-nullable double value in the config file.
    /// </summary>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a new nullable double value (double?) in the config file. Handles null by saving an empty string.
    /// </summary>
    public static void SetConfigNullableDoubleVal(string xmlFileName, string elemName, double? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        // Use ?? to ensure a non-null string is passed to SetValue
        root.Element(elemName)?.SetValue(elemVal.ToString() ?? string.Empty);
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    #endregion


    #region ExtensionFuctions
    /// <summary>
    /// Extension method: Safely tries to parse an element's value to a nullable Enum.
    /// </summary>
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;

    /// <summary>
    /// Extension method: Safely tries to parse an element's value to a nullable DateTime.
    /// Supports both English and Hebrew date formats.
    /// </summary>
    public static DateTime? ToDateTimeNullable(this XElement element, string name)
    {
        string? dateStr = (string?)element.Element(name);
        if (dateStr == null) return null;
        
        // Try parsing with Hebrew culture first (for Hebrew dates)
        if (DateTime.TryParse(dateStr, System.Globalization.CultureInfo.GetCultureInfo("he-IL"), 
            System.Globalization.DateTimeStyles.None, out var hebrewResult))
        {
            return hebrewResult;
        }
        
        // Fall back to English/current culture
        if (DateTime.TryParse(dateStr, System.Globalization.CultureInfo.GetCultureInfo("en-US"), 
            System.Globalization.DateTimeStyles.None, out var englishResult))
        {
            return englishResult;
        }
        
        return null;
    }

    /// <summary>
    /// Extension method: Safely tries to parse an element's value to a nullable TimeSpan.
    /// </summary>
    public static TimeSpan? ToTimeSpanNullable(this XElement element, string name) =>
        TimeSpan.TryParse((string?)element.Element(name), out var result) ? (TimeSpan?)result : null;

    /// <summary>
    /// Extension method: Gets the value of a sub-element as a nullable string. Returns null if the sub-element does not exist.
    /// </summary>
    public static string? ToStringNullable(this XElement element, string name) =>
        (string?)element.Element(name);

    /// <summary>
    /// Extension method: Safely tries to parse an element's value to a nullable double.
    /// </summary>
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;

    /// <summary>
    /// Extension method: Safely tries to parse an element's value to a nullable integer.
    /// </summary>
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;
    #endregion

}