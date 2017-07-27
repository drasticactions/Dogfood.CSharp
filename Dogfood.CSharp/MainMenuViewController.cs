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
			MainMenu[] tableItems = new MainMenu[]
			{
                new MainMenu(){
                    ControllerName = "CoreML.SqueezeNetCameraViewController",
                    ControllerType = "UIViewController"
                },
				new MainMenu(){
					ControllerName = "Photos.PhotosViewController",
					ControllerType = "UICollectionViewController"
				},
			};
			this.TableView.Source = new TableSource(tableItems, this);
			this.TableView.ReloadData();
		}
    }

    public class MainMenu {

        public string ControllerType { get; set; }

        public string ControllerName { get; set; }
    }

	public class TableSource : UITableViewSource
	{

		MainMenu[] TableItems;
		string CellIdentifier = "TableCell";
		UIViewController Owner;

		public TableSource(MainMenu[] items, UIViewController owner)
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
			var item = TableItems[indexPath.Row];
            switch(item.ControllerType) {
                case "UIViewController":
					var controller = Activator.CreateInstance(Type.GetType($"Dogfood.CSharp.{item.ControllerName}")) as UIViewController;
					Owner.NavigationController.PushViewController(controller, true);
                    break;
				case "UICollectionViewController":
                    UICollectionViewController uiCollectionViewController;
                    switch(item.ControllerName) {
                        case "Photos.PhotosViewController":
                            uiCollectionViewController = Photos.PhotosViewController.GenerateNewController();
                            break;
                        default:
                            throw new Exception("No controller selected!");
                    }
                    Owner.NavigationController.PushViewController(uiCollectionViewController, true);
					break;
            }
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
			MainMenu item = TableItems[indexPath.Row];

			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }

			cell.TextLabel.Text = item.ControllerName;

			return cell;
		}
	}
}