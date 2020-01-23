namespace LostTech.TensorFlow {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using LostTech.WhichPython;
    using static System.Runtime.InteropServices.RuntimeInformation;

    public static class PackagedTensorFlow {
        static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);
        public static PythonEnvironment EnsureDeployed(DirectoryInfo target) {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (ProcessArchitecture != Architecture.X64)
                throw new PlatformNotSupportedException(ProcessArchitecture.ToString());

            if (target.Exists && target.EnumerateFileSystemInfos().Any())
                throw new InvalidOperationException("Target directory must be empty");

            string platform = IsOSPlatform(OSPlatform.Windows) ? "win"
                : IsOSPlatform(OSPlatform.Linux) ? "linux"
                : IsOSPlatform(OSPlatform.OSX) ? "osx"
                : throw new PlatformNotSupportedException();

            string archivePath = Path.Combine(target.FullName,
                "runtimes", platform + "-x64", "native", "TensorFlow.tar.xz");
            if (!File.Exists(archivePath))
                throw new FileNotFoundException(
                    message: "Packaged TensorFlow was not found. Perhaps the platform is not supported.",
                    fileName: archivePath);


        }
    }
}
