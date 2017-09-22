namespace CDK
{
    /// <summary>
    /// Used for Discovery. These constants are used to fill in some of the values in the
    /// ScribeConnectorAttrribute declaration on the class that implements IConnector.
    /// Settings that are often the same in Connectors.
    /// </summary>
    public class StandardConnectorSettings
    {
        public const string SettingsUITypeName = "";
        public const string SettingsUIVersion = "1.0";
        public const string ConnectionUITypeName = "ScribeOnline.GenericConnectionUI";
        public const string ConnectionUIVersion = "1.0";
        public const string XapFileName = "ScribeOnline";
    }
}