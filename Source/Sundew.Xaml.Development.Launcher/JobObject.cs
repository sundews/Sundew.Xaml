// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobObject.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// A Windows job that kills everything in it as soon as the job is closed.
/// </summary>
/// <remarks>
/// A tester is started through the dotnet CLI, so the window that appears is a grandchild rather than the process
/// that was started. Killing the process that was started would leave it behind. Windows closes a job when the last
/// handle to it goes, including when the process holding it dies for any reason, so putting each tester in one of
/// these is what makes "when the launcher closes, the testers close" true even if the launcher is killed outright.
/// </remarks>
public sealed class JobObject : IDisposable
{
    private const int ExtendedLimitInformation = 9;
    private const uint LimitKillOnJobClose = 0x2000;
    private const int ProcessIdList = 3;

    private IntPtr handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobObject"/> class.
    /// </summary>
    public JobObject()
    {
        this.handle = CreateJobObject(IntPtr.Zero, null);
        if (this.handle == IntPtr.Zero)
        {
            return;
        }

        var information = default(JobObjectExtendedLimitInformation);
        information.BasicLimitInformation.LimitFlags = LimitKillOnJobClose;
        var size = Marshal.SizeOf<JobObjectExtendedLimitInformation>();
        var block = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(information, block, false);
            SetInformationJobObject(this.handle, ExtendedLimitInformation, block, (uint)size);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Puts a process, and everything it goes on to start, in the job.
    /// </summary>
    /// <param name="process">The process.</param>
    public void Add(Process process)
    {
        if (this.handle != IntPtr.Zero)
        {
            AssignProcessToJobObject(this.handle, process.Handle);
        }
    }

    /// <summary>
    /// Lists what is in the job now.
    /// </summary>
    /// <remarks>
    /// A browser started in app mode hands over to a process of its own and exits, so the process that was
    /// started says nothing about which window appeared. Job membership is inherited, so whatever it handed
    /// over to is in here, which is how that window is found again.
    /// </remarks>
    /// <returns>The process ids.</returns>
    public IReadOnlyList<int> GetProcessIds()
    {
        if (this.handle == IntPtr.Zero)
        {
            return [];
        }

        const int capacity = 512;
        var size = (Marshal.SizeOf<uint>() * 2) + (IntPtr.Size * capacity);
        var block = Marshal.AllocHGlobal(size);
        try
        {
            if (!QueryInformationJobObject(this.handle, ProcessIdList, block, (uint)size, IntPtr.Zero))
            {
                return [];
            }

            var count = (int)(uint)Marshal.ReadInt32(block, sizeof(uint));
            var ids = new List<int>(count);
            for (var index = 0; index < Math.Min(count, capacity); index++)
            {
                ids.Add((int)Marshal.ReadIntPtr(block, (sizeof(uint) * 2) + (IntPtr.Size * index)));
            }

            return ids;
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Closes the job, which kills everything still in it.
    /// </summary>
    public void Dispose()
    {
        if (this.handle != IntPtr.Zero)
        {
            CloseHandle(this.handle);
            this.handle = IntPtr.Zero;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr securityAttributes, string? name);

    [DllImport("kernel32.dll")]
    private static extern bool SetInformationJobObject(IntPtr job, int informationClass, IntPtr information, uint informationLength);

    [DllImport("kernel32.dll")]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll")]
    private static extern bool QueryInformationJobObject(IntPtr job, int informationClass, IntPtr information, uint informationLength, IntPtr returnLength);

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectBasicLimitInformation
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectExtendedLimitInformation
    {
        public JobObjectBasicLimitInformation BasicLimitInformation;
        public IoCounters IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }
}
