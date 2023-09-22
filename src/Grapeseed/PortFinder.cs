using System.Net.NetworkInformation;

namespace Grapevine
{
    /// <summary>
    /// Utility for finding open ports on the local machine
    /// </summary>
    public static class PortFinder
    {
        /// <summary>
        /// Integer that represents of the minimum possible port number
        /// </summary>
        /// <value></value>
        public const int MinPortNumber = 1;

        /// <summary>
        /// Integer that represents the maximum possible port number
        /// </summary>
        /// <value></value>
        public const int MaxPortNumber = 65535;

        /// <summary>
        /// Error message when no open port is found in the specified range.
        /// </summary>
        /// <value></value>
        public const string NoOpenPortFoundMsg = "No local open ports found in range {0} - {1}";

        /// <summary>
        /// Error message when the port number range is invalid.
        /// </summary>
        /// <value></value>
        public const string OutOfRangeMsg = "Value {0} must be an integer between {1} and {2}.";

        /// <summary>
        /// Returns the next open port number in the specified range formatted as a string.
        /// If the start index is greater than the end index, then the search will be in descending order, otherwise searches in ascending order.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <remarks>Throws ArgumentOutOfRangeException if either the startIndex or endIndex are outside of the range of valid port numbers.</remarks>
        /// <returns>A string representation of the port number.</returns>
        public static string FindNextLocalOpenPort(int startIndex = MinPortNumber, int endIndex = MaxPortNumber)
        {
            // 1. Ensure that both the startIndex and endIndex are inside the range of valid port numbers
            if (!startIndex.IsInRange())
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), string.Format(OutOfRangeMsg, startIndex, MinPortNumber, MaxPortNumber));
            }

            if (!endIndex.IsInRange())
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), string.Format(OutOfRangeMsg, endIndex, MinPortNumber, MaxPortNumber));
            }

            var useAscending = startIndex <= endIndex;

            // 2. Get a list of all ports that are currently in use
            var portsInUse = GetPortsInUse();

            // 3. Create the condition and iterator for the for loop
            Func<int, bool> condition = useAscending
                ? (int i) => i <= endIndex
                : (int i) => i >= endIndex;

            Func<int, int> iterator = useAscending
                ? (int i) => ++i
                : (int i) => --i;

            // 4. Search for next unused port
            for (var i = startIndex; condition(i); i = iterator(i))
            {
                if (portsInUse.Contains(i)) continue;
                return i.ToString();
            }

            // 5. Throw exception if not matching port was found
            throw new IndexOutOfRangeException(string.Format(
                NoOpenPortFoundMsg,
                Math.Min(startIndex, endIndex),
                Math.Max(startIndex, endIndex)
            ));
        }

        public static IEnumerable<int> GetPortsInUse()
        {
            return IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Select(l => l.Port);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the integer is in the range of valid values for a port number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsInRange(this int value) => value >= MinPortNumber && value <= MaxPortNumber;
    }
}
