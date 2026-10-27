using System.Windows.Automation;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace LumenSnap;

internal static class Theme
{
    internal static readonly Brush Panel = Frozen("#141E2B"), Accent = Frozen("#78BEFF"), Foreground = Frozen("#E6EDF7"), Muted = Frozen("#96A9C0");
    internal static readonly string[] FontNames = Fonts.SystemFontFamilies.Select(x => x.Source).OrderBy(x => x).ToArray();
    private static Brush Frozen(string hex) { var b = Rendering.Brush(hex); b.Freeze(); return b; }
    internal static void Install(Application app, bool? animate = null)
    {
        // Template triggers act on the chrome, so local active-tool colors cannot suppress feedback.
        app.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse("""
        <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Style TargetType="Button">
            <Setter Property="Foreground" Value="#E5EFFA"/><Setter Property="Background" Value="#192739"/>
            <Setter Property="BorderBrush" Value="#354A63"/><Setter Property="BorderThickness" Value="1"/>
            <Setter Property="FontSize" Value="11"/><Setter Property="Padding" Value="6,2"/><Setter Property="Cursor" Value="Hand"/>
            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Button">
              <Grid x:Name="Motion" RenderTransformOrigin="0.5,0.5" SnapsToDevicePixels="True">
                <Grid.RenderTransform><ScaleTransform x:Name="PressScale"/></Grid.RenderTransform>
                <Border x:Name="Glow" CornerRadius="6" BorderThickness="1" BorderBrush="#8ADFFF" Opacity="0" IsHitTestVisible="False">
                  <Border.Effect><DropShadowEffect Color="#58C8FF" BlurRadius="12" ShadowDepth="0" Opacity="0.65" RenderingBias="Performance"/></Border.Effect>
                </Border>
                <Border x:Name="Chrome" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="6"/>
                <Border CornerRadius="5" Margin="1" IsHitTestVisible="False"><Border.Background><LinearGradientBrush StartPoint="0,0" EndPoint="0,1"><GradientStop Color="#16FFFFFF" Offset="0"/><GradientStop Color="#00FFFFFF" Offset="0.6"/><GradientStop Color="#08000000" Offset="1"/></LinearGradientBrush></Border.Background></Border>
                <Border x:Name="HoverFill" CornerRadius="5" Margin="1" Opacity="0" IsHitTestVisible="False"><Border.Background><LinearGradientBrush StartPoint="0,0" EndPoint="1,1"><GradientStop Color="#7ADBCF"/><GradientStop Color="#6794FF" Offset="1"/></LinearGradientBrush></Border.Background></Border>
                <Border x:Name="Focus" CornerRadius="6" BorderBrush="#D4F4FF" BorderThickness="1" Opacity="0" IsHitTestVisible="False"/>
                <ContentPresenter x:Name="Label" Margin="{TemplateBinding Padding}" HorizontalAlignment="Center" VerticalAlignment="Center" RecognizesAccessKey="True"/>
              </Grid>
              <ControlTemplate.Triggers>
                <Trigger Property="Tag" Value="primary"><Setter TargetName="Chrome" Property="Background"><Setter.Value><LinearGradientBrush StartPoint="0,0" EndPoint="0,1"><GradientStop Color="#29475F"/><GradientStop Color="#1B314A" Offset="1"/></LinearGradientBrush></Setter.Value></Setter><Setter TargetName="Chrome" Property="BorderBrush" Value="#527D9D"/></Trigger>
                <Trigger Property="Tag" Value="selected"><Setter TargetName="Chrome" Property="Background" Value="#25465C"/><Setter TargetName="Chrome" Property="BorderBrush"><Setter.Value><LinearGradientBrush StartPoint="0,0" EndPoint="1,1"><GradientStop Color="#8DE5DB"/><GradientStop Color="#8C9AFF" Offset="1"/></LinearGradientBrush></Setter.Value></Setter></Trigger>
                <Trigger Property="IsMouseOver" Value="True">
                  <Setter TargetName="Chrome" Property="Background" Value="#294357"/><Setter TargetName="Chrome" Property="BorderBrush" Value="#8CD5EF"/>
                  <Setter TargetName="Glow" Property="Opacity" Value="0.85"/><Setter TargetName="HoverFill" Property="Opacity" Value="0.13"/>
                  <Trigger.EnterActions><BeginStoryboard><Storyboard><DoubleAnimation Storyboard.TargetName="Glow" Storyboard.TargetProperty="Opacity" To="0.85" Duration="HOVER_TIME"/><DoubleAnimation Storyboard.TargetName="HoverFill" Storyboard.TargetProperty="Opacity" To="0.13" Duration="HOVER_TIME"/></Storyboard></BeginStoryboard></Trigger.EnterActions>
                  <Trigger.ExitActions><BeginStoryboard><Storyboard><DoubleAnimation Storyboard.TargetName="Glow" Storyboard.TargetProperty="Opacity" To="0" Duration="LEAVE_TIME"/><DoubleAnimation Storyboard.TargetName="HoverFill" Storyboard.TargetProperty="Opacity" To="0" Duration="LEAVE_TIME"/></Storyboard></BeginStoryboard></Trigger.ExitActions>
                </Trigger>
                <Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="Focus" Property="Opacity" Value="0.9"/></Trigger>
                <Trigger Property="IsPressed" Value="True">
                  <Setter TargetName="Chrome" Property="Background" Value="#34566D"/><Setter TargetName="Chrome" Property="BorderBrush" Value="#D2F5FF"/>
                  <Trigger.EnterActions><BeginStoryboard><Storyboard><DoubleAnimation Storyboard.TargetName="Motion" Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)" To="0.94" Duration="PRESS_TIME"/><DoubleAnimation Storyboard.TargetName="Motion" Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)" To="0.94" Duration="PRESS_TIME"/></Storyboard></BeginStoryboard></Trigger.EnterActions>
                  <Trigger.ExitActions><BeginStoryboard><Storyboard><DoubleAnimation Storyboard.TargetName="Motion" Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)" To="1" Duration="HOVER_TIME"><DoubleAnimation.EasingFunction><CubicEase EasingMode="EaseOut"/></DoubleAnimation.EasingFunction></DoubleAnimation><DoubleAnimation Storyboard.TargetName="Motion" Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)" To="1" Duration="HOVER_TIME"/></Storyboard></BeginStoryboard></Trigger.ExitActions>
                </Trigger>
                <Trigger Property="IsEnabled" Value="False"><Setter TargetName="Motion" Property="Opacity" Value="0.3"/><Setter Property="Cursor" Value="Arrow"/></Trigger>
              </ControlTemplate.Triggers>
            </ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="ToolTip"><Setter Property="Foreground" Value="#E5EFFA"/><Setter Property="FontSize" Value="11"/><Setter Property="MaxWidth" Value="250"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ToolTip"><Border Background="#F0172535" BorderBrush="#435D76" BorderThickness="1" CornerRadius="6" Padding="9,6"><ContentPresenter/></Border></ControlTemplate></Setter.Value></Setter></Style>
          <Style TargetType="TextBox">
            <Setter Property="Foreground" Value="#E6EDF7"/><Setter Property="CaretBrush" Value="#E6EDF7"/><Setter Property="SelectionBrush" Value="#35648B"/>
            <Setter Property="Background" Value="#111B28"/><Setter Property="BorderBrush" Value="#34455B"/><Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Padding" Value="6,3"/><Setter Property="FontSize" Value="11"/><Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="TextBox">
              <Border x:Name="Chrome" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="5">
                <ScrollViewer x:Name="PART_ContentHost" Margin="{TemplateBinding Padding}"/>
              </Border><ControlTemplate.Triggers><Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="Chrome" Property="BorderBrush" Value="#78BEFF"/></Trigger></ControlTemplate.Triggers>
            </ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="ComboBoxItem">
            <Setter Property="Foreground" Value="#E6EDF7"/><Setter Property="Padding" Value="6,3"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ComboBoxItem"><Border x:Name="Chrome" Background="Transparent" CornerRadius="3" Padding="{TemplateBinding Padding}"><ContentPresenter/></Border>
              <ControlTemplate.Triggers><Trigger Property="IsHighlighted" Value="True"><Setter TargetName="Chrome" Property="Background" Value="#29455F"/></Trigger><Trigger Property="IsSelected" Value="True"><Setter TargetName="Chrome" Property="Background" Value="#284C6C"/></Trigger></ControlTemplate.Triggers>
            </ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="ComboBox">
            <Setter Property="Foreground" Value="#E6EDF7"/><Setter Property="Background" Value="#111B28"/><Setter Property="BorderBrush" Value="#34455B"/>
            <Setter Property="FontSize" Value="12"/><Setter Property="MinHeight" Value="26"/><Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto"/>
            <Setter Property="VirtualizingPanel.IsVirtualizing" Value="True"/>
            <Setter Property="ItemsPanel"><Setter.Value><ItemsPanelTemplate><VirtualizingStackPanel/></ItemsPanelTemplate></Setter.Value></Setter>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ComboBox"><Grid>
              <Border x:Name="Chrome" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="1" CornerRadius="5"/>
              <ToggleButton Focusable="False" IsChecked="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}"><ToggleButton.Template><ControlTemplate TargetType="ToggleButton"><Border Background="Transparent"><Path Data="M 0,0 L 4,4 L 8,0" Stroke="#96A9C0" StrokeThickness="1.3" HorizontalAlignment="Right" VerticalAlignment="Center" Margin="0,0,10,0"/></Border></ControlTemplate></ToggleButton.Template></ToggleButton>
              <ContentPresenter x:Name="Selection" Content="{TemplateBinding SelectionBoxItem}" IsHitTestVisible="False" Margin="9,4,28,4" VerticalAlignment="Center"/>
              <TextBox x:Name="PART_EditableTextBox" Visibility="Hidden" Text="{Binding Text, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Background="Transparent" BorderThickness="0" Margin="1,1,25,1" Padding="7,4"/>
              <Popup x:Name="PART_Popup" IsOpen="{TemplateBinding IsDropDownOpen}" Placement="Bottom" AllowsTransparency="True" Focusable="False" PopupAnimation="None">
                <Border Background="#172333" BorderBrush="#3B526D" BorderThickness="1" CornerRadius="6" Padding="4" MinWidth="{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}"><ScrollViewer MaxHeight="180" CanContentScroll="True"><ItemsPresenter KeyboardNavigation.DirectionalNavigation="Contained"/></ScrollViewer></Border>
              </Popup>
            </Grid><ControlTemplate.Triggers>
              <Trigger Property="IsEditable" Value="True"><Setter TargetName="Selection" Property="Visibility" Value="Hidden"/><Setter TargetName="PART_EditableTextBox" Property="Visibility" Value="Visible"/></Trigger>
              <Trigger Property="IsKeyboardFocusWithin" Value="True"><Setter TargetName="Chrome" Property="BorderBrush" Value="#78BEFF"/></Trigger>
            </ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="CheckBox">
            <Setter Property="Foreground" Value="#DCE9F8"/><Setter Property="FontSize" Value="12"/><Setter Property="Margin" Value="0,5"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="CheckBox"><Grid><Grid.ColumnDefinitions><ColumnDefinition Width="18"/><ColumnDefinition Width="*"/></Grid.ColumnDefinitions>
              <Border x:Name="Box" Width="16" Height="16" Background="#111B28" BorderBrush="#465C74" BorderThickness="1" CornerRadius="4"><Path x:Name="Tick" Visibility="Hidden" Data="M 3,7 L 6,10 L 12,4" Stroke="#EAF5FF" StrokeThickness="1.6"/></Border>
              <ContentPresenter Grid.Column="1" Margin="7,0,0,0" VerticalAlignment="Center"/>
            </Grid><ControlTemplate.Triggers>
              <Trigger Property="IsChecked" Value="True"><Setter TargetName="Box" Property="Background" Value="#32658E"/><Setter TargetName="Tick" Property="Visibility" Value="Visible"/></Trigger>
              <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Box" Property="BorderBrush" Value="#78BEFF"/></Trigger>
              <Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="Box" Property="BorderBrush" Value="White"/></Trigger>
            </ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="Slider">
            <Setter Property="Margin" Value="0,4"/><Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Slider"><Grid Height="24" Background="Transparent">
              <Track x:Name="PART_Track" Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}" Value="{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" IsDirectionReversed="{TemplateBinding IsDirectionReversed}">
                <Track.DecreaseRepeatButton><RepeatButton Command="Slider.DecreaseLarge" Focusable="False"><RepeatButton.Template><ControlTemplate TargetType="RepeatButton"><Border Height="3" Background="#78BEFF" CornerRadius="1.5"/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton>
                <Track.Thumb><Thumb Width="13" Height="13"><Thumb.Template><ControlTemplate TargetType="Thumb"><Border x:Name="Knob" Background="#C5E5FF" BorderBrush="#78BEFF" BorderThickness="1" CornerRadius="7"/><ControlTemplate.Triggers><Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Knob" Property="Background" Value="White"/></Trigger><Trigger Property="IsDragging" Value="True"><Setter TargetName="Knob" Property="Background" Value="#78BEFF"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Thumb.Template></Thumb></Track.Thumb>
                <Track.IncreaseRepeatButton><RepeatButton Command="Slider.IncreaseLarge" Focusable="False"><RepeatButton.Template><ControlTemplate TargetType="RepeatButton"><Border Height="3" Background="#344960" CornerRadius="1.5"/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.IncreaseRepeatButton>
              </Track>
              <Border x:Name="Focus" IsHitTestVisible="False" BorderBrush="Transparent" BorderThickness="1" CornerRadius="5"/>
            </Grid><ControlTemplate.Triggers><Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="Focus" Property="BorderBrush" Value="#78BEFF"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="ScrollBar">
            <Setter Property="Width" Value="9"/><Setter Property="MinHeight" Value="24"/><Setter Property="Background" Value="Transparent"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ScrollBar"><Grid Background="Transparent">
              <Track x:Name="PART_Track" Orientation="Vertical" IsDirectionReversed="True" Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}" ViewportSize="{TemplateBinding ViewportSize}" Value="{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}">
                <Track.DecreaseRepeatButton><RepeatButton Command="ScrollBar.PageUpCommand" Focusable="False"><RepeatButton.Template><ControlTemplate TargetType="RepeatButton"><Border Background="Transparent"/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton>
                <Track.Thumb><Thumb MinHeight="20"><Thumb.Template><ControlTemplate TargetType="Thumb"><Border x:Name="Knob" Background="#43586F" CornerRadius="3" Margin="2,0"/><ControlTemplate.Triggers><Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Knob" Property="Background" Value="#78BEFF"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Thumb.Template></Thumb></Track.Thumb>
                <Track.IncreaseRepeatButton><RepeatButton Command="ScrollBar.PageDownCommand" Focusable="False"><RepeatButton.Template><ControlTemplate TargetType="RepeatButton"><Border Background="Transparent"/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.IncreaseRepeatButton>
              </Track>
            </Grid></ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="ScrollViewer"><Setter Property="HorizontalScrollBarVisibility" Value="Disabled"/></Style>
        </ResourceDictionary>
        """.Replace("HOVER_TIME", (animate ?? SystemParameters.ClientAreaAnimation) ? "0:0:0.14" : "0:0:0").Replace("LEAVE_TIME", (animate ?? SystemParameters.ClientAreaAnimation) ? "0:0:0.2" : "0:0:0").Replace("PRESS_TIME", (animate ?? SystemParameters.ClientAreaAnimation) ? "0:0:0.07" : "0:0:0")));
    }
    internal static Button Button(string text, string hint, Action click, double width = 90)
    {
        var b = new Button { Content = text, ToolTip = hint, Width = width, Height = 28, Margin = new Thickness(2), Background = Frozen("#1B293B"), BorderBrush = Frozen("#354A63") };
        AutomationProperties.SetName(b, hint); b.Click += (_, _) => click(); return b;
    }
    internal static Button Toolbar(object content, string hint, Action click, double oldWidth = 40)
    {
        var b = Button("", hint, click, oldWidth * ToolbarMetrics.Density);
        b.Content = content; b.Height = 40 * ToolbarMetrics.Density; b.Margin = new Thickness(2 * ToolbarMetrics.Density);
        b.Padding = new Thickness(2); b.FontSize = 10.5; b.Background = Brushes.Transparent; b.BorderBrush = Brushes.Transparent; return b;
    }
    internal static UIElement Icon(string name, double size = 16)
    {
        var path = new System.Windows.Shapes.Path { Width = size, Height = size, Stretch = Stretch.Uniform, StrokeThickness = 1.5, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round, StrokeLineJoin = PenLineJoin.Round,
            Data = Geometry.Parse(name switch {
                "Pencil" => "M3,13 L12,4 L16,8 L7,17 L2,18 Z M10,6 L14,10",
                "Arrow" => "M3,17 L17,3 M8,3 L17,3 L17,12", "Text" => "M3,3 L17,3 M10,3 L10,17 M6,17 L14,17",
                "Line" => "M3,17 L17,3", "Rectangle" => "M3,4 L17,4 L17,16 L3,16 Z",
                "Ellipse" => "M10,3 C19,3 19,17 10,17 C1,17 1,3 10,3 Z",
                "Undo" => "M7,4 L3,8 L7,12 M3,8 L12,8 C18,8 18,17 12,17", "Redo" => "M13,4 L17,8 L13,12 M17,8 L8,8 C2,8 2,17 8,17",
                "Print" => "M5,7 L5,2 L15,2 L15,7 M5,15 L2,15 L2,7 L18,7 L18,15 L15,15 M5,12 L15,12 L15,18 L5,18 Z",
                "Close" => "M4,4 L16,16 M16,4 L4,16", _ => "M4,4 L16,4 L16,16 L4,16 Z" }) };
        path.SetBinding(Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(Button), 1) });
        return path;
    }
    internal static FrameworkElement Range(string label, double value, double min, double max, double step, Action<double> apply)
    {
        var row = new Grid { MinHeight = 28 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) }); row.ColumnDefinitions.Add(new ColumnDefinition()); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });
        row.Children.Add(new TextBlock { Text = label, Foreground = Muted, FontSize = 11, VerticalAlignment = VerticalAlignment.Center });
        var slider = new Slider { Minimum = min, Maximum = max, Value = value, TickFrequency = step, IsSnapToTickEnabled = true, Margin = new Thickness(0, 0, 8, 0), ToolTip = label };
        var number = new TextBox { Text = value.ToString("0.##"), Padding = new Thickness(4, 3, 4, 3), VerticalContentAlignment = VerticalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, ToolTip = $"{label}: {min}–{max}" };
        Grid.SetColumn(slider, 1); Grid.SetColumn(number, 2); row.Children.Add(slider); row.Children.Add(number);
        slider.ValueChanged += (_, _) => number.Text = slider.Value.ToString("0.##");
        slider.PreviewMouseLeftButtonUp += (_, _) => apply(slider.Value); slider.KeyUp += (_, _) => apply(slider.Value);
        void Commit() { if (double.TryParse(number.Text, out double n) && double.IsFinite(n) && n >= min && n <= max) { slider.Value = n; apply(n); } else number.Text = slider.Value.ToString("0.##"); }
        number.LostKeyboardFocus += (_, _) => Commit(); number.PreviewKeyDown += (_, e) => { if (e.Key == Key.Enter) { Commit(); e.Handled = true; } };
        return row;
    }
    internal static TextBlock Caption(string text) => new() { Text = text, Foreground = Muted, FontSize = 10.5, Margin = new Thickness(0, 0, 0, 3) };
    internal static StackPanel Field(string label, UIElement input) { var p = new StackPanel { Margin = new Thickness(0, 0, 0, 5) }; p.Children.Add(Caption(label)); p.Children.Add(input); return p; }
    internal static Border Section(string title, params UIElement[] children)
    {
        var p = new StackPanel(); p.Children.Add(new TextBlock { Text = title, Foreground = Foreground, FontSize = 11, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
        foreach (var child in children) p.Children.Add(child);
        return new Border { BorderBrush = Frozen("#2A3C50"), BorderThickness = new Thickness(0, 1, 0, 0), Padding = new Thickness(0, 9, 0, 2), Margin = new Thickness(0, 5, 0, 4), Child = p };
    }
    internal static Border Surface(UIElement child) => new()
    { Background = new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#1D2D40"), (Color)ColorConverter.ConvertFromString("#101A28"), 90), BorderBrush = Frozen("#3C526C"), BorderThickness = new Thickness(ToolbarMetrics.Density), CornerRadius = new CornerRadius(8), Padding = new Thickness(3 * ToolbarMetrics.Density), Child = child };
}
