using System;

namespace Plugin.Calendar.Plugin
{
    /// <summary>
    /// Cross Calendar.Plugin
    /// </summary>
    public static class CrossCalendar
    {
	    private static readonly Lazy<ICalendar> Implementation = new Lazy<ICalendar>(CreateCalendar, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => Implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static ICalendar Current
        {
            get
            {
                ICalendar ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

	    private static ICalendar CreateCalendar()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
            return new Calendar.PluginImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");

    }
}
