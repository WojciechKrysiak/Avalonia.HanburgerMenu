using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Templates;

namespace Avalonia.HamburgerMenu
{
	class HamburgerContainerGenerator : ItemContainerGenerator<HamburgerMenuItem>
	{
	    private readonly AvaloniaProperty _iconTemplateProperty;

	    public HamburgerContainerGenerator(IControl owner,
			AvaloniaProperty contentProperty, 
			AvaloniaProperty contentTemplateProperty,
			AvaloniaProperty iconTemplateProperty
		) :
            base(owner, contentProperty, contentTemplateProperty)
	    {
	        _iconTemplateProperty = iconTemplateProperty;
	    }

        public IDataTemplate IconTemplate { get; set; }

		protected override IControl CreateContainer(object item)
		{
			if (item == null || item is HamburgerMenuItem)
				return item as HamburgerMenuItem;

			var result = base.CreateContainer(item);

		    if (result == null)
		        return null;

		    result.SetValue(_iconTemplateProperty, IconTemplate, BindingPriority.Style);

            return result;
		}
	}
}
