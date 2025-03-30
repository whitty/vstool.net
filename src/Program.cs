// (C) Copyright 2025 Greg Whiteley

using System.CommandLine;

internal class Program
{
    private static Task WithEnv(string solution, Func<VsTool.Env, int> fn)
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

    private static Task WithEnv(string solution, Action<VsTool.Env> fn)
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
            var env = new VsTool.Env(solution);
            env.Stop(wait);
        }, solutionArgument, waitFlag);

        ////////////////////////////////////////////////////////////////////////
        return rootCommand.Invoke(args);
    }
}
