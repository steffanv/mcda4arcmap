//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MCDA {
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.ArcMapUI;
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.Desktop.AddIns;
    
    
    /// <summary>
    /// A class for looking up declarative information in the associated configuration xml file (.esriaddinx).
    /// </summary>
    internal static class ThisAddIn {
        
        internal static string Name {
            get {
                return "MCDA Add-in";
            }
        }
        
        internal static string AddInID {
            get {
                return "{f098ac0f-6cae-4e1b-81d0-d67452d3cf57}";
            }
        }
        
        internal static string Company {
            get {
                return "Steffan Voss";
            }
        }
        
        internal static string Version {
            get {
                return "1.1";
            }
        }
        
        internal static string Description {
            get {
                return "Add-in for MCDA analysis";
            }
        }
        
        internal static string Author {
            get {
                return "Steffan Voss";
            }
        }
        
        internal static string Date {
            get {
                return "01/10/2014";
            }
        }
        
        internal static ESRI.ArcGIS.esriSystem.UID ToUID(this System.String id) {
            ESRI.ArcGIS.esriSystem.UID uid = new ESRI.ArcGIS.esriSystem.UIDClass();
            uid.Value = id;
            return uid;
        }
        
        /// <summary>
        /// A class for looking up Add-in id strings declared in the associated configuration xml file (.esriaddinx).
        /// </summary>
        internal class IDs {
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_AddDataBtn', the id declared for Add-in Button class 'AddDataBtn'
            /// </summary>
            internal static string AddDataBtn {
                get {
                    return "Ryerson_University_MCDA_AddDataBtn";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_OWAToolBtn', the id declared for Add-in Button class 'OWAToolBtn'
            /// </summary>
            internal static string OWAToolBtn {
                get {
                    return "Ryerson_University_MCDA_OWAToolBtn";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_WLCToolBtn', the id declared for Add-in Button class 'WLCToolBtn'
            /// </summary>
            internal static string WLCToolBtn {
                get {
                    return "Ryerson_University_MCDA_WLCToolBtn";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_LWLCToolBtn', the id declared for Add-in Button class 'LWLCToolBtn'
            /// </summary>
            internal static string LWLCToolBtn {
                get {
                    return "Ryerson_University_MCDA_LWLCToolBtn";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_ConfigBtn', the id declared for Add-in Button class 'ConfigBtn'
            /// </summary>
            internal static string ConfigBtn {
                get {
                    return "Ryerson_University_MCDA_ConfigBtn";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_VisualizationBtn', the id declared for Add-in Button class 'VisualizationBtn'
            /// </summary>
            internal static string VisualizationBtn {
                get {
                    return "Ryerson_University_MCDA_VisualizationBtn";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_MCDAExtension', the id declared for Add-in Extension class 'MCDAExtension'
            /// </summary>
            internal static string MCDAExtension {
                get {
                    return "Ryerson_University_MCDA_MCDAExtension";
                }
            }
            
            /// <summary>
            /// Returns 'Ryerson_University_MCDA_AddDataView', the id declared for Add-in DockableWindow class 'AddDataView+AddinImpl'
            /// </summary>
            internal static string AddDataView {
                get {
                    return "Ryerson_University_MCDA_AddDataView";
                }
            }
        }
    }
    
internal static class ArcMap
{
  private static IApplication s_app = null;
  private static IDocumentEvents_Event s_docEvent;

  public static IApplication Application
  {
    get
    {
      if (s_app == null)
        s_app = Internal.AddInStartupObject.GetHook<IMxApplication>() as IApplication;

      return s_app;
    }
  }

  public static IMxDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document as IMxDocument;

      return null;
    }
  }
  public static IMxApplication ThisApplication
  {
    get { return Application as IMxApplication; }
  }
  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }
  public static IDocumentEvents_Event Events
  {
    get
    {
      s_docEvent = Document as IDocumentEvents_Event;
      return s_docEvent;
    }
  }
}

namespace Internal
{
  [StartupObjectAttribute()]
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
  public sealed partial class AddInStartupObject : AddInEntryPoint
  {
    private static AddInStartupObject _sAddInHostManager;
    private List<object> m_addinHooks = null;

    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public AddInStartupObject()
    {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool Initialize(object hook)
    {
      bool createSingleton = _sAddInHostManager == null;
      if (createSingleton)
      {
        _sAddInHostManager = this;
        m_addinHooks = new List<object>();
        m_addinHooks.Add(hook);
      }
      else if (!_sAddInHostManager.m_addinHooks.Contains(hook))
        _sAddInHostManager.m_addinHooks.Add(hook);

      return createSingleton;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void Shutdown()
    {
      _sAddInHostManager = null;
      m_addinHooks = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static T GetHook<T>() where T : class
    {
      if (_sAddInHostManager != null)
      {
        foreach (object o in _sAddInHostManager.m_addinHooks)
        {
          if (o is T)
            return o as T;
        }
      }

      return null;
    }

    // Expose this instance of Add-in class externally
    public static AddInStartupObject GetThis()
    {
      return _sAddInHostManager;
    }
  }
}
}
