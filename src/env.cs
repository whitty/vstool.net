// SPDX-License-Identifier: GPL-3.0-or-later
// (C) Copyright 2025 Greg Whiteley
using System.Runtime.Versioning;

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

    public sealed class NotRunningException : Exception
    {
        public NotRunningException(string path) : base($"'{path}' is not running") {}
        public NotRunningException(string path, Exception inner) : base($"'{path}' is not running", inner) {}
        private NotRunningException() : base() {}
    }

    public class Env
    {
    #region Ctor & Destructor
        public Env(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            m_path = Path.GetFullPath(path);

            object? p = null;
            if (OperatingSystem.IsWindows())
            {
                if (!ROT.IsRunning(m_path))
                {
                    // TODO in admin mode this sometimes fails

                    bool running = false;
                    foreach (var v in ROT.EachRunning())
                    {
                        if (v == m_path)
                        {
                            running = true;
                            break;
                        }
                    }

                    if (!running)
                        throw new NotRunningException(m_path);
                }

                p = System.Runtime.InteropServices.Marshal.BindToMoniker(m_path);

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
