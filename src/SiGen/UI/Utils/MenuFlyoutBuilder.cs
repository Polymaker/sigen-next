using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace SiGen.UI.Utils;

public class MenuFlyoutBuilder
{
    private readonly MenuFlyout _menuFlyout = new();

    public MenuFlyoutBuilder AddHeader(string header)
    {
        _menuFlyout.Items.Add(new MenuItem
        {
            Header = header,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisible = false,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(8,2),
            Classes = { "HideMenuGesture" },
        });
        return this;
    }

    public MenuFlyoutBuilder AddSubText(string? subText)
    {
        if (string.IsNullOrWhiteSpace(subText))
            return this;

        _menuFlyout.Items.Add(new MenuItem
        {
            Header = new TextBlock
            {
                Text = subText,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                FontStyle = FontStyle.Italic,
                TextAlignment = TextAlignment.Center,
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsHitTestVisible = false,
            
            Padding = new Thickness(6,2),
            Classes = { "HideMenuGesture" },
            MaxWidth = 200,
        });

        return this;
    }

    public MenuFlyoutBuilder AddOption(string header, Action onClick, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
    {
        return AddOption(new TextBlock
        {
            Text = header,
            //HorizontalAlignment = horizontalAlignment,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextAlignment = horizontalAlignment switch
            {
                HorizontalAlignment.Left => TextAlignment.Left,
                HorizontalAlignment.Center => TextAlignment.Center,
                HorizontalAlignment.Right => TextAlignment.Right,
                _ => TextAlignment.Left
            },
             Classes = { "HideMenuGesture" },
        }, onClick);
    }

    public MenuFlyoutBuilder AddOption(object header, Action onClick)
    {
        var item = new MenuItem
        {
            Header = header,
            Classes = { "HideMenuGesture" },
        };

        item.Click += (_, _) => onClick();
        _menuFlyout.Items.Add(item);
        return this;
    }

    public MenuFlyout Build() => _menuFlyout;

}
