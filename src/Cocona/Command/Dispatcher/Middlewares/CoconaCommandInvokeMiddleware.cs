﻿using Cocona.Command.Binder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cocona.Command.Dispatcher.Middlewares
{
    public class CoconaCommandInvokeMiddleware : CommandDispatcherMiddleware
    {
        private readonly ICoconaParameterBinder _parameterBinder;

        public CoconaCommandInvokeMiddleware(CommandDispatchDelegate next, ICoconaParameterBinder parameterBinder)
            : base(next)
        {
            _parameterBinder = parameterBinder;
        }

        public override async ValueTask<int> DispatchAsync(CommandDispatchContext ctx)
        {
            var invokeArgs = _parameterBinder.Bind(ctx.Command!, ctx.ParsedCommandLine!.Options, ctx.ParsedCommandLine!.Arguments);

            try
            {
                var result = ctx.Command!.Method.Invoke(ctx.CommandTarget, invokeArgs);
                if (result is Task<int> taskOfInt)
                {
                    var exitCode = await taskOfInt.ConfigureAwait(false);
                    return exitCode;
                }
                else if (result is ValueTask<int> valueTaskOfInt)
                {
                    var exitCode = await valueTaskOfInt.ConfigureAwait(false);
                    return exitCode;
                }
                else if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                }
                else if (result is ValueTask valueTask)
                {
                    await valueTask.ConfigureAwait(false);
                }
                else if (result is int exitCode)
                {
                    return exitCode;
                }

                return 0;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                if (ctx.CommandTarget is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
