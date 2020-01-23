namespace LostTech.TensorFlow {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using LostTech.WhichPython;
    using SharpCompress.Common;
    using SharpCompress.Readers;
    using static System.FormattableString;
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

            string archivePath = Path.Combine(AssemblyDirectory,
                "runtimes", platform + "-x64", "native", "TensorFlow.tar.xz");
            if (!File.Exists(archivePath))
                throw new FileNotFoundException(
                    message: "Packaged TensorFlow was not found. Perhaps the platform is not supported.",
                    fileName: archivePath);

            string interpreterPath = Path.Combine(target.FullName, "python.exe");
            // TODO: more robust detection
            if (!File.Exists(interpreterPath))
                Extract(archivePath, target.FullName);

            // TODO: detect Python version
            var version = new Version(3, 7);
            return new PythonEnvironment(
                // TODO: support non-Windows
                interpreterPath: interpreterPath,
                home: target.FullName,
                // TODO: support non-Windows
                dll: Path.Combine(target.FullName, Invariant($"python{version.Major}{version.Minor}.dll")),
                languageVersion: version,
                Architecture.X64);
        }

        static void Extract(string archive, string target) {
            var options = new ExtractionOptions {
                ExtractFullPath = true,
                Overwrite = true,
            };
            using var stream = File.OpenRead(archive);
            using var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry()) {
                if (reader.Entry.IsDirectory) continue;

                reader.WriteEntryToDirectory(target, options);
            }
        }
    }
}
