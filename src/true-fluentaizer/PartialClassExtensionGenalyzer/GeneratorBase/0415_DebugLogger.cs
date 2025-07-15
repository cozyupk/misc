using System;

namespace PartialClassExtGen.GeneratorBase
{
#if DEBUG
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning disable RS1035 // Do not use banned APIs
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    /// <summary>
    /// This class is used for debugging purposes to log messages during the code generation process.
    /// </summary>
    internal class DebugLogger
    {
        /// <summary>
        /// Gets or sets the file path associated with the current operation.
        /// </summary>
        private string FilePath { get; set; }

        protected internal DebugLogger(string unixtime)
        {
            FilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\debug_{unixtime}.log.txt";
        }

        public void Log(string message)
        {
            using var file = new System.IO.StreamWriter(FilePath, true);
            // This is a placeholder for the actual logging implementation.
            // In a real-world scenario, you might log to a file, console, or any logging framework.
            file.WriteLine($"{DateTime.Now}: {message}");
        }

        private readonly static Lazy<DebugLogger> _instance
            = new(
                () => new DebugLogger(
                            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                          ),
                          System.Threading.LazyThreadSafetyMode.ExecutionAndPublication
                );

        public static DebugLogger Instance => _instance.Value;
    }
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning restore RS1035 // Do not use banned APIs
#pragma warning restore IDE0079 // Suppress unnecessary suppression
#endif
}
