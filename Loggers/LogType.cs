namespace Concept.Loggers
{
    public enum LogType
    {
        /// <summary>
        /// Sensitivy app data.
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Things can help in development.
        /// </summary>
        Verbose = 1,

        /// <summary>
        /// General information into your app.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Represents when something is strange.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Represents a error in the app.
        /// </summary>
        Error = 4,

        /// <summary>
        /// An error thats need full attention.
        /// </summary>
        Critical = 5,
    }
}