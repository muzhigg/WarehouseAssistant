using System;
using System.Linq;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;

namespace WarehouseAssistant.WebUI.Tests;

public static class TestExtensions
{
    public static T FindComponentWithSelector<T>(this IRenderedFragment renderedFragment, string selector)
        where T : IComponent
    {
        var elements = renderedFragment.FindAll(selector);
        
        foreach (var element in elements)
        {
            var component = renderedFragment.FindComponent<T>(element);
            if (component != null)
            {
                return component.Instance;
            }
        }
        
        throw new InvalidOperationException(
            $"Component of type {typeof(T).FullName} with selector '{selector}' not found.");
    }
    
    private static IRenderedComponent<TComponent>? FindComponent<TComponent>(this IRenderedFragment renderedFragment,
        IElement                                                                                    element)
        where TComponent : IComponent
    {
        return renderedFragment.FindComponents<TComponent>()
            .FirstOrDefault(component =>
                component.Markup.Contains(element.OuterHtml, StringComparison.OrdinalIgnoreCase));
    }
}