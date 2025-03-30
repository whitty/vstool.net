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
    class DTE100 : Solution
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
    #endregion

    public class Env
    {
    #region Ctor & Destructor
        public Env(string path)
        {
            m_path = path;

            object? p = null;
            if (OperatingSystem.IsWindows())
            {
                p = System.Runtime.InteropServices.Marshal.BindToMoniker(path);

                if ((EnvDTE100.Solution4?)p is EnvDTE100.Solution4 e100)
                {
                    m_solution = new DTE100(e100);
                    return;
                }
            }
            throw new TypeLoadException($"Unable to load matching type for solution: {p}");
        }

        ~Env()
        {
        }
        #endregion

        public void Build(bool Wait = false) => m_solution.Build(Wait);
        public void Stop(bool Wait = false) => m_solution.Stop(Wait);
        public void Go(bool Wait = false) => m_solution.Go(Wait);

        private string m_path;
        private Solution m_solution;
    }

}
