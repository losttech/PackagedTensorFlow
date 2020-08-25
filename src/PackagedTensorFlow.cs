namespace LostTech.TensorFlow {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using LostTech.WhichPython;
    using static System.FormattableString;
    using static System.Runtime.InteropServices.OSPlatform;
    using static System.Runtime.InteropServices.RuntimeInformation;

    public static class PackagedTensorFlow {
        static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);
        public static PythonEnvironment EnsureDeployed(DirectoryInfo target) {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (PathEx.IsNested(root: target.FullName, item: AssemblyPath))
                throw new ArgumentException("Can not deploy over this assembly");

            if (ProcessArchitecture != Architecture.X64)
                throw new PlatformNotSupportedException(ProcessArchitecture.ToString());

            string platform = IsOSPlatform(Windows) ? "win"
                : IsOSPlatform(Linux) ? "linux"
                : IsOSPlatform(OSX) ? "osx"
                : throw new PlatformNotSupportedException();

            string archivePath = Path.Combine(AssemblyDirectory,
                "runtimes", platform + "-x64", "native", "TensorFlow.tar.xz");
            if (!File.Exists(archivePath)) {
                throw new FileNotFoundException(
                    message: "Packaged TensorFlow was not found. Perhaps the platform is not supported.",
                    fileName: archivePath);
            }

            string relativeInterpreterPath = IsOSPlatform(Windows) ? "python.exe"
                : IsOSPlatform(Linux) || IsOSPlatform(OSX) ? Path.Combine("bin", "python")
                : throw new PlatformNotSupportedException();

            string interpreterPath = Path.Combine(target.FullName, relativeInterpreterPath);
            // TODO: more robust detection
            if (!File.Exists(interpreterPath)) Archive.Extract(archivePath, target.FullName);

            // TODO: detect Python version
            var version = new Version(3, 7);

            string dllName = IsOSPlatform(Windows)
                ? Invariant($"python{version.Major}{version.Minor}.dll")
                : Path.Combine("lib", Invariant($"libpython{version.Major}.{version.Minor}m{DynamicLibraryExtension}"));

            target.Refresh();

            return new PythonEnvironment(
                // TODO: support non-Windows
                interpreterPath: new FileInfo(interpreterPath),
                home: target,
                // TODO: support non-Windows
                dll: new FileInfo(Path.Combine(target.FullName, dllName)),
                languageVersion: version,
                Architecture.X64);
        }

        static readonly string DynamicLibraryExtension =
            IsOSPlatform(Windows) ? ".dll"
            : IsOSPlatform(OSX) ? ".dylib"
            : ".so";
    }
}
