﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.WindowsAppSDK.Analyzers {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.WindowsAppSDK.Analyzers.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property&apos;s identifier must end with the suffix &apos;Property&apos;..
        /// </summary>
        internal static string DependencyPropertyNameEndsWithPropertyDescription {
            get {
                return ResourceManager.GetString("DependencyPropertyNameEndsWithPropertyDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property identifier &apos;{0}&apos; must end with &apos;Property&apos;.
        /// </summary>
        internal static string DependencyPropertyNameEndsWithPropertyMessageFormat {
            get {
                return ResourceManager.GetString("DependencyPropertyNameEndsWithPropertyMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property&apos;s identifier must end with the suffix &apos;Property&apos;.
        /// </summary>
        internal static string DependencyPropertyNameEndsWithPropertyTitle {
            get {
                return ResourceManager.GetString("DependencyPropertyNameEndsWithPropertyTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reduce magic strings by using nameof and tying directly to the dependency property&apos;s name..
        /// </summary>
        internal static string DependencyPropertyNameOfDescription {
            get {
                return ResourceManager.GetString("DependencyPropertyNameOfDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property registration for &apos;{0}&apos; can be made more robust using nameof().
        /// </summary>
        internal static string DependencyPropertyNameOfMessageFormat {
            get {
                return ResourceManager.GetString("DependencyPropertyNameOfMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can use nameof() method to aid in future refactoring and reduce magic strings.
        /// </summary>
        internal static string DependencyPropertyNameOfTitle {
            get {
                return ResourceManager.GetString("DependencyPropertyNameOfTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property&apos;s owner type parameter needs to match the containing class&apos;s name..
        /// </summary>
        internal static string DependencyPropertyOwnerTypeDescription {
            get {
                return ResourceManager.GetString("DependencyPropertyOwnerTypeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property registration with ownerType parameter &apos;{0}&apos; should match the actual owning type of &apos;{1}&apos;.
        /// </summary>
        internal static string DependencyPropertyOwnerTypeMessageFormat {
            get {
                return ResourceManager.GetString("DependencyPropertyOwnerTypeMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dependency Property OwnerType must match containing Type.
        /// </summary>
        internal static string DependencyPropertyOwnerTypeTitle {
            get {
                return ResourceManager.GetString("DependencyPropertyOwnerTypeTitle", resourceCulture);
            }
        }
    }
}
