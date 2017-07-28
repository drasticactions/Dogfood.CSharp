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
					ControllerName = "LavaViewController",
					ControllerType = "UIViewController",
					FromStoryboard = true
				},
                new MainMenu(){
                    ControllerName = "ShipViewController",
                    ControllerType = "UIViewController",
                    FromStoryboard = true
				},
                new MainMenu(){
                    ControllerName = "CoreML.SqueezeNetCameraViewController",
                    ControllerType = "UIViewController"
                },
				new MainMenu(){
					ControllerName = "Vision.FaceCameraViewController",
					ControllerType = "UIViewController"
				},
				new MainMenu(){
					ControllerName = "Vision.SquareFaceCameraViewController",
					ControllerType = "UIViewController"
				},
				new MainMenu(){
					ControllerName = "Vision.TextCameraViewController",
					ControllerType = "UIViewController"
				},
				new MainMenu(){
					ControllerName = "Photos.PhotosViewController",
					ControllerType = "UICollectionViewController"
				},
			};
			this.TableView.Source = new MainMenuTableSource(tableItems, ((UINavigationController)SplitViewController.ViewControllers[1]).TopViewController, SplitViewController);
			this.TableView.ReloadData();
		}

		public override void ViewWillAppear(bool animated)
		{
			ClearsSelectionOnViewWillAppear = SplitViewController.Collapsed;
			base.ViewWillAppear(animated);
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showDetail")
			{
				var controller = ((UINavigationController)segue.DestinationViewController);
				controller.NavigationItem.LeftBarButtonItem = SplitViewController.DisplayModeButtonItem;
				controller.NavigationItem.LeftItemsSupplementBackButton = true;
			}
		}
    }

    public class MainMenu {

        public string ControllerType { get; set; }

        public string ControllerName { get; set; }

        public bool FromStoryboard { get; set; }
    }

	public class MainMenuTableSource : UITableViewSource
	{

		MainMenu[] TableItems;
		string CellIdentifier = "TableCell";
		UIViewController Owner;
        UISplitViewController SplitView;

		public MainMenuTableSource(MainMenu[] items, UIViewController owner, UISplitViewController split)
		{
			Owner = owner;
            SplitView = split;
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
                    if (item.FromStoryboard) {
						var storyBoard = UIStoryboard.FromName("Main", null);
                        var storyboardController = storyBoard.InstantiateViewController(item.ControllerName);
						Owner.NavigationController.PushViewController(storyboardController, true);
						SplitView.ShowDetailViewController(storyboardController.NavigationController, Owner);
					}
                    else {
						var controller = Activator.CreateInstance(Type.GetType($"Dogfood.CSharp.{item.ControllerName}")) as UIViewController;
						Owner.NavigationController.PushViewController(controller, true);
						SplitView.ShowDetailViewController(controller.NavigationController, Owner);
						break;
                    }
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
                    SplitView.ShowDetailViewController(uiCollectionViewController.NavigationController, Owner);
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