using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace Avalonia.HamburgerMenu
{
    public class HamburgerMenuItem : ContentControl, ISelectable
	{
		/// <summary>
		/// Defines the <see cref="Icon"/> property.
		/// </summary>
		public static readonly AvaloniaProperty<object> IconProperty = AvaloniaProperty.Register<HamburgerMenuItem, object>(nameof(Icon));

		/// <summary>
		/// Defines the <see cref="IconTemplate"/> property.
		/// </summary>
		public static readonly StyledProperty<IDataTemplate> IconTemplateProperty =
			AvaloniaProperty.Register<HamburgerMenuItem, IDataTemplate>(nameof(IconTemplate));

		/// <summary>
		/// Defines the <see cref="IsSelected"/> property.
		/// </summary>
		public static readonly StyledProperty<bool> IsSelectedProperty =
			AvaloniaProperty.Register<HamburgerMenuItem, bool>(nameof(IsSelected));


		/// <summary>
		/// Defines the <see cref="DesiredIconSize"/> property.
		/// </summary>
		public static readonly AvaloniaProperty<Size> DesiredIconSizeProperty =
			AvaloniaProperty.RegisterDirect<HamburgerMenuItem, Size>(nameof(DesiredIconSize), hmi => hmi.DesiredIconSize);

		/// <summary>
		/// Defines the <see cref="DesiredContentSize"/> property.
		/// </summary>
		public static readonly AvaloniaProperty<Size> DesiredContentSizeProperty =
			AvaloniaProperty.RegisterDirect<HamburgerMenuItem, Size>(nameof(DesiredContentSize), hmi => hmi.DesiredIconSize);

		public static readonly AvaloniaProperty<bool> IsOverlayProperty =
			AvaloniaProperty.RegisterDirect<HamburgerMenuItem,  bool>(nameof(IsOverlay), x => x.IsOverlay, (x,v) => x.IsOverlay = v);

		/// <summary>
		/// Initializes static members of the <see cref="ListBoxItem"/> class.
		/// </summary>
		static HamburgerMenuItem()
		{
			SelectableMixin.Attach<HamburgerMenuItem>(IsSelectedProperty);
			FocusableProperty.OverrideDefaultValue<HamburgerMenuItem>(true);
			AffectsMeasure<HamburgerMenuItem>(IconProperty, 
											  IconTemplateProperty,
											  DesiredIconSizeProperty,
											  DesiredContentSizeProperty
											  );

			ContentControlMixin.Attach<HamburgerMenuItem>(IconProperty, x => x.LogicalChildren, "PART_IconPresenter");
		}

		/// <summary>
		/// Gets or sets the selection state of the item.
		/// </summary>
		public bool IsSelected
		{
			get => GetValue(IsSelectedProperty); 
			set => SetValue(IsSelectedProperty, value); 
		}

		/// <summary>
		/// Gets or sets the data template used to display the menu item's icon.
		/// </summary>
		public IDataTemplate IconTemplate
		{
			get => GetValue(IconTemplateProperty); 
			set => SetValue(IconTemplateProperty, value); 
		}

		/// <summary>
		/// Icon to display alongside the menu content.
		/// </summary>
		[DependsOn(nameof(IconTemplate))]
		public object Icon
		{
			get => GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}
	    
		public Size DesiredIconSize
		{
			get => _desiredIconSize;
			private set => SetAndRaise(DesiredIconSizeProperty, ref _desiredIconSize, value);
		}

		public Size DesiredContentSize
		{
			get => _desiredContentSize;
			private set => SetAndRaise(DesiredContentSizeProperty, ref _desiredContentSize, value);
		}

		private bool isOverlay;

		public bool IsOverlay
		{
			get => isOverlay;
			set => SetAndRaise(IsOverlayProperty, ref isOverlay, value);
		}

	    IControl _contentBlock;
	    IControl _iconBlock;
	    private Size _desiredIconSize = new Size(double.NegativeInfinity, double.NegativeInfinity);
	    private Size _desiredContentSize = new Size(double.NegativeInfinity, double.NegativeInfinity);

        const string PART_IconBlock = "PART_IconBlock";
	    const string PART_ContentBlock = "PART_ContentBlock";

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
		{
			base.OnTemplateApplied(e);

			_contentBlock = e.NameScope.Get<IControl>(PART_ContentBlock);
			_iconBlock = e.NameScope.Get<IControl>(PART_IconBlock);
			var noConstraints = new Size(double.MaxValue, double.MaxValue);

		    if (_contentBlock != null)
		    {
		        _contentBlock.Measure(noConstraints);
		        DesiredContentSize = _contentBlock.DesiredSize;
		        _contentBlock.InvalidateMeasure();
		    }

		    if (_iconBlock != null)
		    {
		        _iconBlock.Measure(noConstraints);
		        DesiredIconSize = _iconBlock.DesiredSize;
		        _iconBlock.InvalidateMeasure();
		    }
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			var noConstraints = new Size(double.MaxValue, double.MaxValue);

			if (e.Property == IconProperty ||
				e.Property == IconTemplateProperty)
			{
				if (_iconBlock != null)
				{
					_iconBlock.Measure(noConstraints);
					DesiredIconSize = _iconBlock.DesiredSize;
					_iconBlock.InvalidateMeasure();
				}
			}

			if (e.Property == ContentProperty ||
				e.Property == ContentTemplateProperty)
			{
				if (_contentBlock != null)
				{   
					_contentBlock.Measure(noConstraints);
					DesiredContentSize = _contentBlock.DesiredSize;
					_contentBlock.InvalidateMeasure();
				}
			}
		}
	}
}
