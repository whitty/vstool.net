// SPDX-License-Identifier: GPL-3.0-or-later
// (C) Copyright 2025 Greg Whiteley

using System.CommandLine;

sealed internal class Program
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1031:catch a more specific allowed exception type, or rethrow the exception", Justification = "We're just squashing it for the CLI.")]
    private static Task<int> WithEnv(string solution, Func<VsTool.Env, int> fn)
    {
        try
        {
            var env = new VsTool.Env(solution);
            return Task.FromResult(fn(env));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failure: {e.Message}");
        }
        return Task.FromResult(1);
    }

    private static Task<int> WithEnv(string solution, Action<VsTool.Env> fn)
    {
        return WithEnv(solution, (env) => {
            fn(env);
            return 0;
        });
    }

    private static int Main(string[] args)
    {
        var waitFlag = new Option<bool>(
            name: "--wait",
            description: "Wait for completion.");

        var solutionArgument = new Argument<string>(
            name: "solution",
            description: "Path to solution.");

        var rootCommand = new RootCommand("Command line helper for Visual Studio");

        ////////////////////////////////////////////////////////////////////////
        var buildCommand = new Command("build", "Start build of current configuration.")
        {
            waitFlag
        };
        buildCommand.AddArgument(solutionArgument);
        rootCommand.AddCommand(buildCommand);

        buildCommand.SetHandler((solution, wait) => {
            return WithEnv(solution, (env) => {
                env.Build(wait);
                if (wait)
                    Console.WriteLine($"Built {solution}");
                else
                    Console.WriteLine($"Sent build to {solution}");
            });
        }, solutionArgument, waitFlag);

        ////////////////////////////////////////////////////////////////////////
        var goCommand = new Command("go", "Start or continue the debugger.")
        {
            waitFlag
        };
        goCommand.AddArgument(solutionArgument);
        rootCommand.AddCommand(goCommand);

        goCommand.SetHandler((solution, wait) => {
            return WithEnv(solution, (env) => {
                env.Go(wait);
                if (wait)
                    Console.WriteLine($"Started {solution}");
                else
                    Console.WriteLine($"Sent go to {solution}");
            });
        }, solutionArgument, waitFlag);

        ////////////////////////////////////////////////////////////////////////
        var stopCommand = new Command("stop", "Stop the process running under the debugger.")
        {
            waitFlag
        };
        stopCommand.AddArgument(solutionArgument);
        rootCommand.AddCommand(stopCommand);

        stopCommand.SetHandler((solution, wait) => {
            return WithEnv(solution, (env) => {
                env.Stop(wait);
                if (wait)
                    Console.WriteLine($"Stopped {solution}");
                else
                    Console.WriteLine($"Sent stop to {solution}");
            });
        }, solutionArgument, waitFlag);

        ////////////////////////////////////////////////////////////////////////
        return rootCommand.Invoke(args);
    }
}
