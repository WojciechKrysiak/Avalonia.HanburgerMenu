using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Avalonia.HamburgerMenu
{
	public enum MenuSide
	{
		Left,
		Right
	}

	public class HamburgerContainer : TemplatedControl, IPanel
    {
		private MenuSide menuSide;

		private double overlayVerticalOffset = 0.0;

		public static readonly AvaloniaProperty<MenuSide> MenuSideProperty =
				AvaloniaProperty.RegisterDirect<HamburgerContainer, MenuSide>(nameof(MenuSide), x => x.MenuSide, (x, v) => x.MenuSide = v);
		
		public static readonly AvaloniaProperty<double> OverlayVerticalOffsetProperty =
				AvaloniaProperty.RegisterDirect<HamburgerContainer, double>(nameof(OverlayVerticalOffset), x => x.OverlayVerticalOffset, (x, v) => x.OverlayVerticalOffset = v);

		public static readonly AvaloniaProperty<HamburgerMenuItem> MenuItemProperty =
            AvaloniaProperty.RegisterAttached<HamburgerContainer, Control, HamburgerMenuItem>("MenuItem");

		private HamburgerMenu menu;
		private IControl mainItem;
		private IControl overlayItem;

		public MenuSide MenuSide
		{
			get => menuSide;
			set => SetAndRaise(MenuSideProperty, ref menuSide, value);
		}

		public double OverlayVerticalOffset
		{
			get => overlayVerticalOffset;
			set => SetAndRaise(OverlayVerticalOffsetProperty, ref overlayVerticalOffset, value);
		}

		private AvaloniaList<HamburgerMenuItem> _menuItems = new AvaloniaList<HamburgerMenuItem>();

		/// <summary>
		/// Gets the children of the <see cref="HamburgerContainer"/>
		/// </summary>
		[Content]
		public Controls.Controls Children { get; } = new Controls.Controls();

		static HamburgerContainer()
		{
			ClipToBoundsProperty.OverrideDefaultValue<HamburgerContainer>(true);

			AffectsMeasure<HamburgerContainer>(MenuSideProperty);
		}

        public HamburgerContainer()
		{
			Children.CollectionChanged += ChildrenChanged;
		}

		/// <summary>
		/// Called when the <see cref="Children"/> collection changes.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		protected void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var newItems = e.NewItems?.OfType<Control>().ToList() ?? new List<Control>();
			var newMenuItems = BuildMenuItems(newItems).ToList();
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					_menuItems.InsertRange(e.NewStartingIndex, newMenuItems);
					break;

				case NotifyCollectionChangedAction.Move:
					_menuItems.MoveRange(e.OldStartingIndex, e.OldItems.Count, e.NewStartingIndex);
					break;

				case NotifyCollectionChangedAction.Remove:
					_menuItems.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
					break;

				case NotifyCollectionChangedAction.Replace:
					for (var i = 0; i < e.OldItems.Count; ++i)
					{
						var index = i + e.OldStartingIndex;
						_menuItems[index] = newMenuItems[i];
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					_menuItems.Clear();
					_menuItems.AddRange(BuildMenuItems(newItems));
					break;
			}

			//if (!_menuItems.Any(mi => mi.IsSelected && !mi.IsOverlay))
			//{
			//	var firstOverlay = _menuItems.FirstOrDefault(mi => mi.IsSelected && mi.IsOverlay);
			//	var firstNonOverlay = _menuItems.FirstOrDefault(mi => !mi.IsOverlay);
				
			//	if (firstNonOverlay != null)
			//		firstNonOverlay.IsSelected = true;

			//	if (firstOverlay != null)
			//		firstNonOverlay.IsSelected = false;
			//}

			InvalidateMeasure();
		}

		private IEnumerable<HamburgerMenuItem> BuildMenuItems(IEnumerable<Control> controls)
		{
			foreach (var child in controls)
			{
				var menuItem = child.GetValue(MenuItemProperty);
				if (menuItem != null)
				{
					yield return menuItem;
					continue;
				}

				if (child is IHeadered headered)
					yield return new HamburgerMenuItem { Content = headered.Header };
				else
					yield return new HamburgerMenuItem { Content = string.Empty };
			}
		}

		private const string PART_MainPresenter = "PART_MainPresenter";
		private const string PART_OverlayPresenter = "PART_OverlayPresenter";
		private const string PART_OverlayShadow = "PART_OverlayShadow";
		private const string PART_Menu = "PART_Menu";

		private ContentPresenter mainPresenter;
		private ContentPresenter overlayPresenter;
		private Control overlayShadow;

		private HamburgerMenuItem overlayMenuItem;
		private HamburgerMenuItem mainMenuItem;

		protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
		{
			base.OnTemplateApplied(e);

			if (menu != null)
			{
				menu.SelectionChanged -= MenuSelectionChanged;
				menu.PointerReleased -= DeselectOverlay;
			}

			menu = e.NameScope.Find<HamburgerMenu>(PART_Menu);

			if (menu == null)
			{
				Logger.Error("HamburgerMenu", this, "The menu container will not work without a menu");
				return;
			}

			menu.SelectionChanged += MenuSelectionChanged;

			menu.Items = _menuItems;

			mainPresenter = e.NameScope.Find<ContentPresenter>(PART_MainPresenter);

			var selected = _menuItems.FirstOrDefault(mi => mi.IsSelected && !mi.IsOverlay);
			var selectedIndex = _menuItems.IndexOf(selected);
			if (selectedIndex >= 0 && mainItem != Children[selectedIndex])
			{
				SetMainiItem(selected, Children[selectedIndex]);
			}

			overlayPresenter = e.NameScope.Find<ContentPresenter>(PART_OverlayPresenter);

			if (overlayShadow != null)
			{
				overlayShadow.PointerReleased -= DeselectOverlay;
			}

			overlayShadow = e.NameScope.Find<Control>(PART_OverlayShadow);
			if (overlayShadow != null)
			{
				overlayShadow.PointerReleased += DeselectOverlay;
			}
		}

		private void DeselectOverlay(object sender, RoutedEventArgs args)
		{
			menu.ClearOverlay();
		}

		private void ClearOverlay()
		{
			if (overlayMenuItem != null)
			{
				overlayMenuItem = null;
				LogicalChildren.Remove(overlayItem);
				overlayItem = null;
				if (overlayPresenter != null)
				{
					overlayPresenter.Content = null;
				}
			}
		}

		private void SetNewOverlay(HamburgerMenuItem newItem, IControl child)
		{
			overlayMenuItem = newItem;
			overlayItem = child;
			LogicalChildren.Add(child);

			if (overlayPresenter != null)
			{
				overlayPresenter.Content = child;
				overlayPresenter.UpdateChild();
				var availableSize = Bounds.Size;
				availableSize = availableSize.WithWidth(availableSize.Width - menu.Bounds.Width);
				overlayPresenter.Measure(availableSize);
				var size = overlayPresenter.DesiredSize;
				var transformedPoint = overlayMenuItem.TranslatePoint(new Point(), this);
				if (transformedPoint.Y + size.Height > availableSize.Height)
					OverlayVerticalOffset = Math.Max(0, availableSize.Height - size.Height);
				else
					OverlayVerticalOffset = transformedPoint.Y;
			}
		}

		private void ClearMainItem()
		{
			if (mainMenuItem != null)
			{
				mainMenuItem = null;
				LogicalChildren.Remove(mainItem);
				mainItem = null;
			}
		}

		private void SetMainiItem(HamburgerMenuItem selectedItem, IControl child)
		{
			mainMenuItem = selectedItem;
			mainItem = child;
			LogicalChildren.Add(child);
			if (mainPresenter != null)
			{
				mainPresenter.Content = child;
			}
		}

		private void MenuSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach (var removedItem in e.RemovedItems.OfType<HamburgerMenuItem>())
			{
				if (removedItem.IsOverlay)
					ClearOverlay();
				else
					ClearMainItem();
			}

			var newOverlay = e.AddedItems.OfType<HamburgerMenuItem>().FirstOrDefault(hmi => hmi.IsOverlay);
			var newMain = e.AddedItems.OfType<HamburgerMenuItem>().FirstOrDefault(hmi => !hmi.IsOverlay);

			if (newOverlay != null)
			{
				var index = _menuItems.IndexOf(newOverlay);

				var child = Children[index];

				SetNewOverlay(newOverlay, child);
			}

			if (newMain != null)
			{
				var index = _menuItems.IndexOf(newMain);

				var child = Children[index];
				SetMainiItem(newMain, child);
			}

			e.Handled = true;

			InvalidateMeasure();
		}
	}
}
