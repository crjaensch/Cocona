﻿using Cocona.Application;
using Cocona.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocona.Command.Dispatcher.Middlewares
{
    public class HelpAndVersionMiddleware : CommandDispatcherMiddleware
    {
        private readonly ICoconaHelpRenderer _helpRenderer;
        private readonly ICoconaCommandHelpProvider _commandHelpProvider;

        public HelpAndVersionMiddleware(CommandDispatchDelegate next, ICoconaHelpRenderer helpRenderer, ICoconaCommandHelpProvider commandHelpProvider) : base(next)
        {
            _helpRenderer = helpRenderer;
            _commandHelpProvider = commandHelpProvider;
        }

        public override ValueTask<int> DispatchAsync(CommandDispatchContext ctx)
        {
            var unknownOption = ctx.ParsedCommandLine.UnknownOptions.FirstOrDefault();
            if (string.Equals(unknownOption, "h", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(unknownOption, "help", StringComparison.OrdinalIgnoreCase))
            {
                var help = _commandHelpProvider.CreateCommandHelp(ctx.Command);
                Console.Write(_helpRenderer.Render(help));
                return new ValueTask<int>(129);
            }

            if (string.Equals(unknownOption, "version", StringComparison.OrdinalIgnoreCase))
            {
                var help = _commandHelpProvider.CreateVersionHelp();
                Console.Write(_helpRenderer.Render(help));
                return new ValueTask<int>(129);
            }

            return Next(ctx);
        }
    }
}
