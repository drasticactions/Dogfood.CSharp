using Foundation;
using System;
using UIKit;

namespace Dogfood.CSharp
{
    public partial class MainMenuViewController : UITableViewController
    {
        public MainMenuViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad()
		{
			string[] tableItems = new string[]
			{
				"CoreML.SqueezeNetCameraViewController"
			};
			this.TableView.Source = new TableSource(tableItems, this);
			this.TableView.ReloadData();
		}
    }

	public class TableSource : UITableViewSource
	{

		string[] TableItems;
		string CellIdentifier = "TableCell";
		UIViewController Owner;

		public TableSource(string[] items, UIViewController owner)
		{
			Owner = owner;
			TableItems = items;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return TableItems.Length;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var controllerName = TableItems[indexPath.Row];
			var controller = Activator.CreateInstance(Type.GetType($"Dogfood.CSharp.{controllerName}")) as UIViewController;
			Owner.NavigationController.PushViewController(controller, true);
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
			string item = TableItems[indexPath.Row];

			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }

			cell.TextLabel.Text = item;

			return cell;
		}
	}
}