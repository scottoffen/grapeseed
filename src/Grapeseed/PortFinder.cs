﻿using System.Net.NetworkInformation;
using Grapevine.Exceptions;

namespace Grapevine;

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

        // 2. Get a list of all ports that are currently in use
        var portsInUse = GetPortsInUse().ToList();

        // 3. Create the condition and iterator for the for loop
        var useAscending = startIndex <= endIndex;

        Func<int, bool> checkCondition = useAscending
            ? i => i <= endIndex
            : i => i >= endIndex;

        Func<int, int> incrementIterator = useAscending
            ? i => ++i
            : i => --i;

        // 4. Search for next unused port
        for (var i = startIndex; checkCondition(i); i = incrementIterator(i))
        {
            if (portsInUse.Contains(i)) continue;
            return i.ToString();
        }

        // 5. Throw exception if not matching port was found
        throw new NoOpenPortFoundException(
            Math.Min(startIndex, endIndex),
            Math.Max(startIndex, endIndex)
        );
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
    private static bool IsInRange(this int value) => value is >= MinPortNumber and <= MaxPortNumber;
}
