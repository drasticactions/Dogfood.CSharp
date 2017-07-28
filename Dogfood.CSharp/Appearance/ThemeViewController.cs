using Foundation;
using System;
using UIKit;
using Dogfood.CSharp.Settings;

namespace Dogfood.CSharp.Appearance
{
    public class ThemeViewController : UITableViewController
    {
        public ThemeViewController()
        {
        }

        public override void ViewDidLoad() {
            Themes[] tableItems = new Themes[]{
                new Themes() {
                    ThemeName = "Dark",
                    Choice = ThemeChoice.Dark
                }
            };
			this.TableView.Source = new ThemeTableSource(tableItems);
			this.TableView.ReloadData();
        }
    }

	public class Themes
	{
        public ThemeChoice Choice { get; set; }  

		public string ThemeName { get; set; }
	}

    public class ThemeTableSource : UITableViewSource {

        Themes[] TableItems;
        string CellIdentifier = "TableCell";
        public ThemeTableSource(Themes[] items)
        {
            TableItems = items;
        }

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return TableItems.Length;
		}

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var item = TableItems[indexPath.Row];
            switch(item.Choice) {
                case ThemeChoice.Dark:
                    Theme.SetDarkTheme();
                    break;
            }
        }

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
			Themes item = TableItems[indexPath.Row];

			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }

			cell.TextLabel.Text = item.ThemeName;

			return cell;
		}
    }
}
