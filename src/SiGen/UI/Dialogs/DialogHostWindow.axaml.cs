using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SiGen.ViewModels.Dialogs;
using System;

namespace SiGen.UI.Dialogs;

public partial class DialogHostWindow : Window
{
    private readonly double baseMinWidth;
    private readonly double baseMinHeight;
    private readonly double baseMaxWidth;
    private readonly double baseMaxHeight;

    public DialogHostWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        baseMinWidth = MinWidth;
        baseMinHeight = MinHeight;
        baseMaxWidth = MaxWidth;
        baseMaxHeight = MaxHeight;
    }

    public void SetDialogContent<TResult>(UserControl dialogControl, DialogViewModelBase<TResult> viewModel)
    {
        var contentPresenter = this.FindControl<ContentPresenter>("DialogContent")!;
        contentPresenter.Content = dialogControl;

        DataContext = viewModel;
        Title = viewModel.Title;

        ApplyInitialSize(dialogControl, viewModel);
        ScheduleDeferredInitialSize(dialogControl, viewModel);

        Closing += async (s, e) =>
        {
            if (!viewModel.CanClose())
            {
                e.Cancel = true;
                return;
            }

            await viewModel.OnClosingAsync();

            if (!IsDialogCompleted(viewModel))
                viewModel.CancelDialog();
        };

        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape && viewModel.CanClose())
                viewModel.CancelDialog();
        };
    }

    private void ApplyInitialSize<TResult>(UserControl dialogControl, DialogViewModelBase<TResult> viewModel)
    {
        dialogControl.Measure(Size.Infinity);
        var contentSize = dialogControl.DesiredSize;

        double chromeWidth = 12;
        double chromeHeight = 4; // Account for window borders

        if (viewModel.ShowTitleBar)
        {
            var titleBar = this.FindControl<Grid>("CustomTitleBar");
            titleBar?.Measure(new Size(contentSize.Width, double.PositiveInfinity));
            if (titleBar != null)
                chromeHeight += titleBar.DesiredSize.Height;
        }

        ApplyContentConstraints(dialogControl, chromeWidth, chromeHeight);
        contentSize = new Size(contentSize.Width + chromeWidth, contentSize.Height + chromeHeight);

        if (viewModel.Resizable)
        {
            Width = GetInitialLength(dialogControl.Width, contentSize.Width, MinWidth, MaxWidth);
            Height = GetInitialLength(dialogControl.Height, contentSize.Height, MinHeight, MaxHeight);
            return;
        }

        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private void ApplyContentConstraints(UserControl dialogControl, double chromeWidth, double chromeHeight)
    {
        MinWidth = Math.Max(baseMinWidth, dialogControl.MinWidth + chromeWidth);
        MinHeight = Math.Max(baseMinHeight, dialogControl.MinHeight + chromeHeight);

        MaxWidth = ResolveMaximum(dialogControl.MaxWidth, chromeWidth, baseMaxWidth);
        MaxHeight = ResolveMaximum(dialogControl.MaxHeight, chromeHeight, baseMaxHeight);
    }

    private void ScheduleDeferredInitialSize<TResult>(UserControl dialogControl, DialogViewModelBase<TResult> viewModel)
    {
        if (!viewModel.Resizable)
            return;

        bool applied = false;

        void ApplyDeferredSize()
        {
            if (applied)
                return;

            applied = true;
            Dispatcher.UIThread.Post(() => ApplyInitialSize(dialogControl, viewModel), DispatcherPriority.Loaded);
        }

        EventHandler? openedHandler = null;
        openedHandler = (_, _) =>
        {
            Opened -= openedHandler;
            ApplyDeferredSize();
        };
        Opened += openedHandler;

        if (dialogControl.DataContext != null)
        {
            ApplyDeferredSize();
            return;
        }

        EventHandler? dataContextChangedHandler = null;
        dataContextChangedHandler = (_, _) =>
        {
            dialogControl.DataContextChanged -= dataContextChangedHandler;
            ApplyDeferredSize();
        };
        dialogControl.DataContextChanged += dataContextChangedHandler;
    }

    private static double GetInitialLength(double explicitLength, double desiredLength, double minLength, double maxLength)
    {
        double length = !double.IsNaN(explicitLength) ? explicitLength : desiredLength;

        if (!double.IsNaN(minLength))
            length = Math.Max(length, minLength);
        if (!double.IsInfinity(maxLength))
            length = Math.Min(length, maxLength);

        return length;
    }

    private static double ResolveMaximum(double contentMaximum, double chromeSize, double baseMaximum)
    {
        if (double.IsNaN(contentMaximum) || double.IsInfinity(contentMaximum))
            return baseMaximum;

        double resolvedMaximum = contentMaximum + chromeSize;
        if (!double.IsInfinity(baseMaximum))
            resolvedMaximum = Math.Min(resolvedMaximum, baseMaximum);

        return resolvedMaximum;
    }

    private void CustomTitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void ResizeGrip_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanResize || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (sender is Control { Tag: string edgeName } && Enum.TryParse<WindowEdge>(edgeName, out var edge))
            BeginResizeDrag(edge, e);
    }

    private static bool IsDialogCompleted<TResult>(DialogViewModelBase<TResult> viewModel)
    {
        try
        {
            var task = viewModel.GetResultAsync();
            return task.IsCompleted;
        }
        catch
        {
            return false;
        }
    }
}