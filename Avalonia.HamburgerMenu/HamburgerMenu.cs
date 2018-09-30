using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Avalonia.Controls.Mixins;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace Avalonia.HamburgerMenu
{
    public class HamburgerMenu : HeaderedSelectingItemsControl
    {
		private MenuSide menuSide;

		/// <summary>
		/// Defines the <see cref="IconWidth"/> property.
		/// </summary>
		public static readonly AvaloniaProperty<double> IconWidthProperty = AvaloniaProperty.Register<HamburgerMenu, double>(nameof(IconWidth), 45, false, Avalonia.Data.BindingMode.TwoWay);

        /// <summary>
        /// Defines the <see cref="LabelWidth"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<double> LabelWidthProperty = AvaloniaProperty.Register<HamburgerMenu, double>(nameof(LabelWidth), 155, false, Avalonia.Data.BindingMode.TwoWay);

        /// <summary>
        /// Defines the <see cref="CurrentLabelWidth"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<double> CurrentLabelWidthProperty = AvaloniaProperty.Register<HamburgerMenu, double>(nameof(CurrentLabelWidth), 300);

        /// <summary>
        /// Defines the <see cref="RowHeight"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<double> RowHeightProperty = AvaloniaProperty.Register<HamburgerMenu, double>(nameof(RowHeight), 45, false);

        /// <summary>
        /// Defines the <see cref="IsExpanded"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<HamburgerMenu, bool>(nameof(IsExpanded), true);

        /// <summary>
        /// Defines the <see cref="ItemIconTemplate"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<IDataTemplate> ItemIconTemplateProperty = AvaloniaProperty.Register<HamburgerMenu, IDataTemplate>(nameof(ItemIconTemplate));

        /// <summary>
        /// Defines the <see cref="HeaderIcon"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<object> HeaderIconProperty = AvaloniaProperty.Register<HamburgerMenu, object>(nameof(HeaderIcon));

		public static readonly AvaloniaProperty<MenuSide> MenuSideProperty =
				AvaloniaProperty.RegisterDirect<HamburgerMenu, MenuSide>(nameof(MenuSide), x => x.MenuSide, (x, v) => x.MenuSide = v);

		public MenuSide MenuSide
		{
			get => menuSide;
			set => SetAndRaise(MenuSideProperty, ref menuSide, value);
		}

		/// <summary>
		/// Common icon column width for all menu items.
		/// Can be calculated dynamically based on the maximum value for contained rows. 
		/// </summary>
		public double IconWidth
		{
			get => GetValue(IconWidthProperty);
			set => SetValue(IconWidthProperty, value);
		}

        /// <summary>
        /// Common label column width for all menu items.
        /// Can be calculated dynamically based on the maximum value for contained rows. 
        /// </summary>
        public double LabelWidth
		{
			get => GetValue(LabelWidthProperty);
			set => SetValue(LabelWidthProperty, value);
		}

        /// <summary>
        /// Current (animated) label column width.
        /// </summary>
        public double CurrentLabelWidth
		{
			get => GetValue(CurrentLabelWidthProperty);
			set => SetValue(CurrentLabelWidthProperty, value);
		}

        /// <summary>
        /// Common row height for all menu items.
        /// Can be calculated dynamically based on the maximum value for contained rows. 
        /// </summary>
        public double RowHeight
		{
			get => GetValue(RowHeightProperty);
			set => SetValue(RowHeightProperty, value);
		}

        /// <summary>
        /// Flag indicating the menu is expanded.
        /// </summary>
        public bool IsExpanded
		{
			get => GetValue(IsExpandedProperty);
			set => SetValue(IsExpandedProperty, value);
		}

        /// <summary>
        /// Data template for the menu item icons.
        /// </summary>
        public IDataTemplate ItemIconTemplate
        {
            get => GetValue(ItemIconTemplateProperty);
            set => SetValue(ItemIconTemplateProperty, value);
        }

        /// <summary>
        /// Icon to display alongside the menu header.
        /// </summary>
        public object HeaderIcon
        {
            get => GetValue(HeaderIconProperty);
            set => SetValue(HeaderIconProperty, value);
        }

        static HamburgerMenu()
        {
            ContentControlMixin.Attach<HamburgerMenu>(HeaderIconProperty, x => x.LogicalChildren, "PART_IconPresenter");
			SelectionModeProperty.OverrideDefaultValue<HamburgerMenu>(SelectionMode.Multiple | SelectionMode.AlwaysSelected);
        }

        public HamburgerMenu()
        {
            ItemIconTemplateProperty.Changed.AddClassHandler<HamburgerMenu>(x => x.ItemIconTemplateChanged);
        }

		public void ClearOverlay()
		{
			SelectedItems.Remove(SelectedItems.OfType<HamburgerMenuItem>().FirstOrDefault(hmi => hmi.IsOverlay));
		}

        private const string PART_IconPresenter = "PART_IconPresenter";
        private const string PART_HeaderPresenter = "PART_HeaderPresenter";

        private bool _measureOutOfDate = false;

        private Size _desiredIconSize = new Size(double.NegativeInfinity, double.NegativeInfinity);
        private Size _desiredHeaderSize = new Size(double.NegativeInfinity, double.NegativeInfinity);

        private IControl _headerIconBlock;
        private IControl _headerBlock;

        protected override IItemContainerGenerator CreateItemContainerGenerator()
		{
		    return new HamburgerContainerGenerator(this, ContentControl.ContentProperty,
		        ContentControl.ContentTemplateProperty, HamburgerMenuItem.IconTemplateProperty)
		    {
		        IconTemplate = ItemIconTemplate
		    };
		}

		protected override Size MeasureOverride(Size availableSize)
		{
		    if (_measureOutOfDate)
		    {
			    UpdateMeasure();
                _measureOutOfDate = false;
		    }

			var size = base.MeasureOverride(availableSize);
		    if (!_measureOutOfDate)
		        return size;

		    UpdateMeasure();
		    _measureOutOfDate = false;
            return base.MeasureOverride(availableSize);
        }


        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

			if (_headerIconBlock != null)
				_headerIconBlock.PointerReleased -= OpenCloseHamburgerMenu;

            _headerIconBlock = e.NameScope.Get<IControl>(PART_IconPresenter);
            _headerBlock = e.NameScope.Get<IControl>(PART_HeaderPresenter);

            var noConstraints = new Size(double.MaxValue, double.MaxValue);

            if (_headerIconBlock != null)
            {
                _headerIconBlock.Measure(noConstraints);
                _desiredIconSize = _headerIconBlock.DesiredSize;
                _headerIconBlock.InvalidateMeasure();
                _measureOutOfDate = true;

				_headerIconBlock.PointerReleased += OpenCloseHamburgerMenu;
            }

            if (_headerBlock != null)
            {
                _headerBlock.Measure(noConstraints);
                _desiredHeaderSize = _headerBlock.DesiredSize;
                _headerBlock.InvalidateMeasure();
                _measureOutOfDate = true;
            }
			CorrectSelection();
        }

        private void OpenCloseHamburgerMenu(object sender, PointerReleasedEventArgs e)
		{
			IsExpanded = !IsExpanded;
		}

		private void CorrectSelection()
		{
			if (SelectedItems.OfType<HamburgerMenuItem>().All(hmi => hmi.IsOverlay))
			{
				var firstNonOverlay = Items.OfType<HamburgerMenuItem>().FirstOrDefault(mi => !mi.IsOverlay);

				if (firstNonOverlay != null)
				{
					SelectedItems[0] = firstNonOverlay;
				}
			}
		}

		protected override void OnContainersMaterialized(ItemContainerEventArgs e)
		{
			base.OnContainersMaterialized(e);

            foreach (var menuItem in e.Containers.Select(c => c.ContainerControl).OfType<HamburgerMenuItem>())
			{
				menuItem.PropertyChanged += MenuItemPropertyChanged;
			}
		    _measureOutOfDate = true;

        }

        protected override void OnContainersDematerialized(ItemContainerEventArgs e)
		{
			foreach (var menuItem in e.Containers.Select(c => c.ContainerControl).OfType<HamburgerMenuItem>())
            {
				menuItem.PropertyChanged -= MenuItemPropertyChanged;
			}

			base.OnContainersDematerialized(e);
		    _measureOutOfDate = true;
		}

        private void MenuItemPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property == HamburgerMenuItem.DesiredIconSizeProperty ||
				e.Property == HamburgerMenuItem.DesiredContentSizeProperty)
		        _measureOutOfDate = true;
		}

		private void UpdateMeasure()
		{
			var iconSize = ItemContainerGenerator.Containers.Select(c => c.ContainerControl).OfType<HamburgerMenuItem>()
							                     .Aggregate(new Size(), (s, hmi) => 
															new Size(
																	Math.Max(s.Width, hmi.DesiredIconSize.Width), 
																	Math.Max(s.Height, hmi.DesiredIconSize.Height)));
			var contentSize = ItemContainerGenerator.Containers.Select(c => c.ContainerControl).OfType<HamburgerMenuItem>()
													.Aggregate(new Size(), (s, hmi) =>
															   new Size(
																	Math.Max(s.Width, hmi.DesiredContentSize.Width),
																	Math.Max(s.Height, hmi.DesiredContentSize.Height)));

			IconWidth = Math.Max(iconSize.Width, _desiredIconSize.Width);
			LabelWidth = Math.Max(contentSize.Width, _desiredHeaderSize.Width);
			RowHeight = new[] { iconSize.Height, contentSize.Height, _desiredHeaderSize.Height, _desiredIconSize.Height }.Max();
		}

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
			CorrectSelection();
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			base.ItemsCollectionChanged(sender, e);
            
			CorrectSelection();
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var noConstraints = new Size(double.MaxValue, double.MaxValue);

            if (e.Property == HeaderIconProperty)
            {
                if (_headerIconBlock != null)
                {
                    _headerIconBlock.Measure(noConstraints);
                    _desiredIconSize = _headerIconBlock.DesiredSize;
                    _headerIconBlock.InvalidateMeasure();
                    _measureOutOfDate = true;
                }
            }

            if (e.Property == HeaderProperty)
            {
                if (_headerBlock != null)
                {
                    _headerBlock.Measure(noConstraints);
                    _desiredHeaderSize = _headerBlock.DesiredSize;
                    _headerBlock.InvalidateMeasure();
                    _measureOutOfDate = true;
                }
            }
        }

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
			if (e.MouseButton == MouseButton.Left)
			{
				var container = GetContainerFromEventSource(e.Source);
				var selectedOverlay = SelectedItems.OfType<HamburgerMenuItem>().FirstOrDefault(hmi => hmi.IsOverlay);
				if (container is HamburgerMenuItem menuItem
					// check whether we're not clicking on the same item
					&& !SelectedItems.Contains(menuItem))
				{
					e.Handled = true;
					var selectedOverlayIndex = SelectedItems.IndexOf(selectedOverlay);
					if (menuItem.IsOverlay)
					{
						if (selectedOverlayIndex < 0)
							SelectedItems.Add(menuItem);
						else
							SelectedItems[selectedOverlayIndex] = menuItem;

						// don't remove overlay
						selectedOverlay = null;
					}
					else
					{
						var selectedMainItem = SelectedItems.OfType<HamburgerMenuItem>().FirstOrDefault(hmi => !hmi.IsOverlay);
						var selectedMainIndex = SelectedItems.IndexOf(selectedMainItem);
						SelectedItems[selectedMainIndex] = menuItem;
					}
				}

				if (selectedOverlay != null)
				{
					SelectedItems.Remove(selectedOverlay);
					e.Handled = true;
				}
			}

			base.OnPointerPressed(e);
		}

		private void ItemIconTemplateChanged(AvaloniaPropertyChangedEventArgs obj)
        {
            if (ItemContainerGenerator is HamburgerContainerGenerator container)
                container.IconTemplate = obj.NewValue as IDataTemplate;
        }
    }
}
