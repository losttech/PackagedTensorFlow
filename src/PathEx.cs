namespace LostTech.TensorFlow {
    using System;
    using System.IO;

    static class PathEx {
        public static bool IsNested(string root, string item) {
            if (root is null) throw new ArgumentNullException(nameof(root));
            if (item is null) throw new ArgumentNullException(nameof(item));

            root = Path.GetFullPath(root);
            item = Path.GetFullPath(item);

            while (true) {
                string directory = Path.GetDirectoryName(item);
                if (directory is null) return false;
                if (directory.Length >= item.Length) return false;
                if (directory == root) return true;

                item = directory;
            }
        }
    }
}