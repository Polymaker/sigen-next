using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System.ComponentModel;

namespace SiGen.UI.Controls;

public class CustomFormField : TemplatedControl, INotifyPropertyChanged
{
    // Styled properties for the control
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<CustomFormField, object?>(nameof(Label));

    public static readonly StyledProperty<HorizontalAlignment> LabelHorizontalAlignmentProperty =
        AvaloniaProperty.Register<CustomFormField, HorizontalAlignment>(
            nameof(LabelHorizontalAlignment), HorizontalAlignment.Left);

    public static readonly StyledProperty<Control?> InputProperty =
        AvaloniaProperty.Register<CustomFormField, Control?>(nameof(Input));

    public static readonly StyledProperty<HorizontalAlignment> InputHorizontalAlignmentProperty =
        AvaloniaProperty.Register<CustomFormField, HorizontalAlignment>(
            nameof(InputHorizontalAlignment), HorizontalAlignment.Stretch);

    public static readonly StyledProperty<object?> HelpProperty =
        AvaloniaProperty.Register<CustomFormField, object?>(nameof(Help));

    public static readonly StyledProperty<object?> InfoProperty =
        AvaloniaProperty.Register<CustomFormField, object?>(nameof(Info));

    //public static readonly StyledProperty<bool> ShowHelpProperty =
    //    AvaloniaProperty.Register<CustomFormField, bool>(nameof(ShowHelp), true);

    public static readonly StyledProperty<double> LabelWidthProperty =
        AvaloniaProperty.Register<CustomFormField, double>(nameof(LabelWidth), double.NaN);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<CustomFormField>(new StyledPropertyMetadata<Orientation>(Orientation.Horizontal));

    // Properties accessible from XAML and code
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }

    public Control? Input
    {
        get => GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public HorizontalAlignment InputHorizontalAlignment
    {
        get => GetValue(InputHorizontalAlignmentProperty);
        set => SetValue(InputHorizontalAlignmentProperty, value);
    }

    public object? Help
    {
        get => GetValue(HelpProperty);
        set => SetValue(HelpProperty, value);
    }

    public object? Info
    {
        get => GetValue(InfoProperty);
        set => SetValue(InfoProperty, value);
    }

    //public bool ShowHelp
    //{
    //    get => GetValue(ShowHelpProperty);
    //    set => SetValue(ShowHelpProperty, value);
    //}

    public double LabelWidth
    {
        get => GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private Button? helpButton;
    private ContentPresenter? labelContent;
    private Button? infoButton;
    private Control? labelContainer;
    private Control? inputContainer;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Find template parts
        labelContent = e.NameScope.Find<ContentPresenter>("PART_LabelPresenter");
        labelContainer = e.NameScope.Find<Control>("PART_LabelContainer");
        inputContainer = e.NameScope.Find<Control>("PART_InputContainer");

        //var inputContentPresenter = e.NameScope.Find<ContentPresenter>("PART_InputPresenter");
        //var helpContentPresenter = e.NameScope.Find<ContentPresenter>("PART_HelpPresenter");
        helpButton = e.NameScope.Find<Button>("PART_HelpButton");
        if (helpButton != null) {
            helpButton.Click += HelpButton_Click;
            //helpButton.IsVisible = Help != null;
        }
        infoButton = e.NameScope.Find<Button>("PART_InfoButton");
        if (infoButton != null)
            infoButton.Click += InfoButton_Click;

        ConfigureHelpFlyout();
        ConfigureInfoFlyout();
        SetContainersDock();

    }


    //protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    //{
    //    base.OnAttachedToVisualTree(e);
        
    //}

    //protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    //{
    //    base.OnDetachedFromVisualTree(e);
    //}

    private void SetContainersDock()
    {
        if (labelContainer != null && inputContainer != null)
        {
            DockPanel.SetDock(labelContainer, Orientation == Orientation.Horizontal ? Dock.Left : Dock.Top);
            DockPanel.SetDock(inputContainer, Orientation == Orientation.Horizontal ? Dock.Right : Dock.Bottom);
        }
    }

    private void HelpButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (labelContent != null)
             FlyoutBase.ShowAttachedFlyout(labelContent);
    }

    private void InfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (infoButton != null)
            FlyoutBase.ShowAttachedFlyout(infoButton);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HelpProperty) ConfigureHelpFlyout();
        if (change.Property == InfoProperty) ConfigureInfoFlyout();

        if (change.Property == OrientationProperty && labelContainer != null && inputContainer != null) SetContainersDock();
    }

    private void ConfigureHelpFlyout()
    {
        if (labelContent != null && Help != null)
        {

            //double yOffset = 5;
            //if (Help is Control control)
            //{
            //    control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //    yOffset += control.DesiredSize.Height / 2;
            //}
            FlyoutBase.SetAttachedFlyout(labelContent, new Flyout
            {
                Content = Help,
                ShowMode = FlyoutShowMode.Standard,
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                VerticalOffset = 5,
            });
        }
        else if (labelContent != null)
        {
            FlyoutBase.SetAttachedFlyout(labelContent, null);
        }
    }

    private void ConfigureInfoFlyout()
    {
        if (infoButton != null && Info != null)
        {
            FlyoutBase.SetAttachedFlyout(infoButton, new Flyout
            {
                Content = Info,
                ShowMode = FlyoutShowMode.Standard,
                Placement = PlacementMode.RightEdgeAlignedTop,
                HorizontalOffset = 5,
                VerticalOffset = -10
            });
        }
        else if (infoButton != null)
        {
            FlyoutBase.SetAttachedFlyout(infoButton, null);
        }
    }

    public double GetLabelWidth()
    {
        if (labelContainer != null)
        {
            if (labelContainer.Width != 0)
                return labelContainer.Width;

            labelContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return labelContainer.DesiredSize.Width;
        }
        return 0;
    }

    public void CloseHelp()
    {
        if (labelContent != null)
        {
            var flyout = FlyoutBase.GetAttachedFlyout(labelContent);
            flyout?.Hide();
        }
    }

    public void CloseInfo()
    {
        if (infoButton != null)
        {
            var flyout = FlyoutBase.GetAttachedFlyout(infoButton);
            flyout?.Hide();
        }
    }
}