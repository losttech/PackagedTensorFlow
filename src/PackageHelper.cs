namespace LostTech.TensorFlow {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    static class PackageHelper {
        internal static string? TryLocatePackage(string path) {
            foreach (string? probeDirectory in probeDirectories) {
                string candidate = Path.Combine(probeDirectory, path.Replace('/', Path.DirectorySeparatorChar));
                if (Directory.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        static readonly string[] probeDirectories = GetDefaultProbeDirectories();

        static string[] GetDefaultProbeDirectories() {
            if (AppDomain.CurrentDomain.GetData("PROBING_DIRECTORIES") is string listOfDirectories
                && !string.IsNullOrEmpty(listOfDirectories)) {
                return listOfDirectories.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            }

            string? packageDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (!string.IsNullOrEmpty(packageDirectory)) {
                return new string[] { packageDirectory };
            }

            string basePathVariable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME";
            string? basePath = Environment.GetEnvironmentVariable(basePathVariable);

            if (string.IsNullOrEmpty(basePath)) {
                return new string[] { string.Empty };
            }

            return new string[] { Path.Combine(basePath, ".nuget", "packages") };
        }
    }
}
