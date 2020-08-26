namespace LostTech.TensorFlow {
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using NUnit.Framework;
    using Python.Runtime;

    public class Tests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void EnsureCanGetTensorFlow() {
            var deploymentTarget = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "TFP", Guid.NewGuid().ToString()));
            var environment = PackagedTensorFlow.EnsureDeployed(deploymentTarget);
            Runtime.PythonDLL = environment.DynamicLibraryPath.FullName;
            PythonEngine.PythonHome = environment.Home.FullName;
            PythonEngine.Initialize();

            using var _ = Py.GIL();
            dynamic tf = Py.Import("tensorflow");
            Console.WriteLine(tf.__version__);
            tf.set_random_seed(42);
            Assert.IsTrue((bool)tf.test.is_built_with_cuda() == !RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
        }
    }
}
