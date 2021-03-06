//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OECD_SDMX.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("OECD.MDB")]
        public string DatabaseName {
            get {
                return ((string)(this["DatabaseName"]));
            }
            set {
                this["DatabaseName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://stats.oecd.org/restsdmx/sdmx.ashx/GetDataStructure/all")]
        public string dataStructureURL {
            get {
                return ((string)(this["dataStructureURL"]));
            }
            set {
                this["dataStructureURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\jihad\\Desktop\\Arbeit\\!OECD_Projekt\\OECD_SDMX\\bin\\Debug\\Inputs\\Identifica" +
            "tors.txt")]
        public string IdentificatorsFile {
            get {
                return ((string)(this["IdentificatorsFile"]));
            }
            set {
                this["IdentificatorsFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\jihad\\Desktop\\Arbeit\\!OECD_Projekt\\OECD_SDMX\\bin\\Debug\\Inputs")]
        public string InputsFolder {
            get {
                return ((string)(this["InputsFolder"]));
            }
            set {
                this["InputsFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://stats.oecd.org/restsdmx/sdmx.ashx/GetSchema")]
        public string schemaURL {
            get {
                return ((string)(this["schemaURL"]));
            }
            set {
                this["schemaURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\jihad\\Desktop\\Arbeit\\!OECD_Projekt\\OECD_SDMX\\bin\\Debug\\Identificators")]
        public string IdentificatorsFolder {
            get {
                return ((string)(this["IdentificatorsFolder"]));
            }
            set {
                this["IdentificatorsFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://stats.oecd.org/sdmx-json/data")]
        public string JSONdataURL {
            get {
                return ((string)(this["JSONdataURL"]));
            }
            set {
                this["JSONdataURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://stats.oecd.org/restsdmx/sdmx.ashx/GetData/\r\n")]
        public string dataURL {
            get {
                return ((string)(this["dataURL"]));
            }
            set {
                this["dataURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Connector.XML")]
        public string ConnectorFile {
            get {
                return ((string)(this["ConnectorFile"]));
            }
            set {
                this["ConnectorFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SUBJECT")]
        public string SubjectAttribute {
            get {
                return ((string)(this["SubjectAttribute"]));
            }
            set {
                this["SubjectAttribute"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MEASURE")]
        public string MeasureAttribute {
            get {
                return ((string)(this["MeasureAttribute"]));
            }
            set {
                this["MeasureAttribute"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("FREQUENCY")]
        public string FrequencyAttribute {
            get {
                return ((string)(this["FrequencyAttribute"]));
            }
            set {
                this["FrequencyAttribute"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\jihad\\Desktop\\Arbeit\\!OECD_Projekt\\OECD_SDMX\\bin\\Debug\\Outputs")]
        public string OutputsFolder {
            get {
                return ((string)(this["OutputsFolder"]));
            }
            set {
                this["OutputsFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COU,LOC,LOCATION,COUNTRY")]
        public string CountryAttrList {
            get {
                return ((string)(this["CountryAttrList"]));
            }
            set {
                this["CountryAttrList"] = value;
            }
        }
    }
}
