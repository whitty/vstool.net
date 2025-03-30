// SPDX-License-Identifier: GPL-3.0-or-later
// (C) Copyright 2025 Greg Whiteley
using System.Runtime.Versioning;

using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;

namespace VsTool
{
    #region Version Interface
    // For now these are just direct wrappers of functionality
    // the idea is to implement the pieces of larger bits of functionality
    // overloaded by supported version
    interface Solution
    {
        void Build(bool Wait = false);
        void Stop(bool Wait = false);
        void Go(bool Wait = false);
    };

    [SupportedOSPlatform("windows")]
    sealed class DTE100 : Solution
    {
        public DTE100(EnvDTE100.Solution4 sln)
        {
            m_sln = sln;
        }

        EnvDTE100.Solution4 m_sln;

        void Solution.Build(bool wait)
        {
            m_sln.SolutionBuild.Build(wait);
        }

        void Solution.Stop(bool wait)
        {
            m_sln.DTE.Debugger.Stop(wait);
        }

        void Solution.Go(bool wait)
        {
            m_sln.DTE.Debugger.Go(wait);
        }
    };

    [SupportedOSPlatform("windows")]
    sealed class DTE80 : Solution
    {
        public DTE80(EnvDTE80.DTE2 sln)
        {
            m_sln = sln;
        }

        EnvDTE80.DTE2 m_sln;

        void Solution.Build(bool wait)
        {
            m_sln.Solution.SolutionBuild.Build(wait);
        }

        void Solution.Stop(bool wait)
        {
            m_sln.DTE.Debugger.Stop(wait);
        }

        void Solution.Go(bool wait)
        {
            m_sln.DTE.Debugger.Go(wait);
        }
    };

    #endregion

    [SupportedOSPlatform("windows")]
    internal static class ROT
    {
        public sealed class COMException: System.Runtime.InteropServices.COMException
        {
            public COMException(string? message, int errorCode) : base(message, errorCode) {}
            public COMException(string? message) : base(message) {}
            public COMException(string? message, Exception inner) : base(message, inner) {}
            private COMException() : base() {}
        }

        [DllImport("ole32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        [DllImport("ole32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static extern int MkParseDisplayName(IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] String szUserName, out UInt32 pchEaten, out IMoniker ppmk);

        static IRunningObjectTable GetRot()
        {
            IRunningObjectTable? pRunningObjectTable = null;
            var hResult = GetRunningObjectTable(0, out pRunningObjectTable);
            if (hResult < 0)
            {
                Marshal.ThrowExceptionForHR(hResult);
            }
            if (pRunningObjectTable == null)
            {
                throw new COMException("Failed to get RunningObjectTable", hResult);
            }
            return pRunningObjectTable;
        }

        static IBindCtx CreateBindContext()
        {                        IBindCtx? pBindCtx = null;
            var hResult = CreateBindCtx(0, out pBindCtx);
            if (hResult < 0)
            {
                Marshal.ThrowExceptionForHR(hResult);
            }
            if (pBindCtx == null)
            {
                throw new COMException("Failed to get BindCtx", hResult);
            }

            return pBindCtx;
        }

        static IMoniker ParseDisplayName(IBindCtx pBindCtx, string mk)
        {
            UInt32 cbEaten;
            IMoniker? pMoniker = null;
            var hResult = MkParseDisplayName(pBindCtx, mk, out cbEaten, out pMoniker);
            if (hResult < 0)
            {
                Marshal.ThrowExceptionForHR(hResult);
            }
            if (cbEaten < mk.Length || pMoniker == null)
            {
                throw new COMException($"Failed to interpret '{mk}' as moniker");
            }
            return pMoniker;
        }

        public static bool IsRunning(string path)
        {
            IRunningObjectTable pRunningObjectTable = GetRot();
            IBindCtx pBindCtx = CreateBindContext();

            IMoniker pMoniker = ParseDisplayName(pBindCtx, path);

            var hResult = pRunningObjectTable.IsRunning(pMoniker);
            if (hResult < 0)
            {
                Marshal.ThrowExceptionForHR(hResult);
            }

            Console.WriteLine($"GetRunningObjectTable returned {hResult}");
            return true;
        }
    }

    public class Env
    {
    #region Ctor & Destructor
        public Env(string path)
        {
            m_path = path ?? throw new ArgumentNullException(nameof(path));

            object? p = null;
            if (OperatingSystem.IsWindows())
            {
                if (ROT.IsRunning(path))
                {
                    Console.WriteLine("IS running?");
                }
                p = System.Runtime.InteropServices.Marshal.BindToMoniker(path);

                if ((EnvDTE100.Solution4?)p is EnvDTE100.Solution4 e100)
                {
                    m_solution = new DTE100(e100);
                    return;
                }

                if ((EnvDTE80.DTE2?)p is EnvDTE80.DTE2 e80)
                {
                    m_solution = new DTE80(e80);
                    return;
                }
            }
            throw new TypeLoadException($"Unable to load matching type for solution: {p}");
        }
        #endregion

        public void Build(bool Wait = false) => m_solution.Build(Wait);
        public void Stop(bool Wait = false) => m_solution.Stop(Wait);
        public void Go(bool Wait = false) => m_solution.Go(Wait);

        private string m_path;
        private Solution m_solution;
    }

}
