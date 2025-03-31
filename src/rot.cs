// SPDX-License-Identifier: GPL-3.0-or-later
// (C) Copyright 2025 Greg Whiteley

using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace VsTool;

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

        return hResult == 0;
    }

    public static System.Collections.Generic.IEnumerable<string> EachRunning()
    {
        IBindCtx pBindCtx = CreateBindContext();

        IRunningObjectTable pRunningObjectTable = GetRot();

        IEnumMoniker? pEnumMoniker = null;
        pRunningObjectTable.EnumRunning(out pEnumMoniker);
        if (pEnumMoniker is null)
        {
            throw new COMException("Failed to get EnumRunning");
        }

        pEnumMoniker.Reset();

        IMoniker[] monikers = new IMoniker[1];
        uint fetched = 0;
        var hResult = pEnumMoniker.Next((uint)monikers.Length, monikers, out fetched);

        while (hResult == 0)
        {
            string s = "";
            monikers[0].GetDisplayName(pBindCtx, monikers[0], out s);

            // Confirm that DisplayName is parseable so could be matched
            if (s.Length > 0)
            {
                try
                {
                    if (ParseDisplayName(pBindCtx, s) == null)
                    {
                        s = "";
                    }
                }
                catch (COMException)
                {
                    s = "";
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    s = "";
                }
            }
            if (s.Length > 0)
            {
                yield return s;
            }
            hResult = pEnumMoniker.Next((uint)monikers.Length, monikers, out fetched);
        }
    }

} // ROT
