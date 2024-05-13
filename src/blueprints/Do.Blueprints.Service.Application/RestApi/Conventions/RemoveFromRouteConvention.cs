﻿using Do.RestApi.Configuration;

namespace Do.RestApi.Conventions;

public class RemoveFromRouteConvention(IEnumerable<string> _parts,
    Func<ActionModelContext, bool>? _when = default
) : IApiModelConvention<ActionModelContext>
{
    public void Apply(ActionModelContext context)
    {
        if (_when is not null && !_when(context)) { return; }

        for (var i = 0; i < context.Action.RouteParts.Count; i++)
        {
            var routePart = RemoveParts(context.Action.RouteParts[i], _parts);
            context.Action.RouteParts[i] = routePart;
            if (string.IsNullOrWhiteSpace(routePart))
            {
                context.Action.RouteParts.RemoveAt(i);
                i--;
            }
        }

        context.Action.Name = RemoveParts(context.Action.Name, _parts);
    }

    string RemoveParts(string from, IEnumerable<string> parts)
    {
        var words = Regexes.StartsWithUpperCaseEndsWithLowerCase().Split(from);
        if (words is null) { return from; }

        return string.Join(string.Empty, words.Except(parts));
    }
}