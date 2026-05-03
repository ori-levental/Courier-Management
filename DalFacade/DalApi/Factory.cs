namespace DalApi;
/// <summary>
/// Implements the Simple Factory pattern to dynamically load the selected Data Access Layer (DAL) implementation at runtime.
/// This mechanism forces the target class to adhere to the Singleton pattern.
/// </summary>
public static class Factory
{
    /// <summary>
    /// Retrieves the Singleton instance of the active DAL implementation (DalXml or DalList) based on the configuration file (dal-config.xml).
    /// </summary>
    /// <exception cref="DalConfigException">Thrown if the configuration file is missing, invalid, or the specified DAL implementation cannot be loaded or is not a Singleton.</exception>
    /// <returns>The Singleton instance of the selected class implementing IDal.</returns>
    public static IDal Get
    {
        get
        {
            string dalType = DalApi.DalConfig.s_dalName ?? throw new DalConfigException($"DAL name is not extracted from the configuration");
            DalApi.DalConfig.DalImplementation dal = DalApi.DalConfig.s_dalPackages[dalType] ?? throw new DalConfigException($"Package for {dalType} is not found in packages list in dal-config.xml");

            try { System.Reflection.Assembly.Load(dal.Package ?? throw new DalConfigException($"Package {dal.Package} is null")); }
            catch (Exception ex) { throw new DalConfigException($"Failed to load {dal.Package}.dll package", ex); }

            Type type = Type.GetType($"{dal.Namespace}.{dal.Class}, {dal.Package}") ??
                throw new DalConfigException($"Class {dal.Namespace}.{dal.Class} was not found in {dal.Package}.dll");

            return type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null) as IDal ??
                throw new DalConfigException($"Class {dal.Class} is not a singleton or wrong property name for Instance");
        }
    }
}