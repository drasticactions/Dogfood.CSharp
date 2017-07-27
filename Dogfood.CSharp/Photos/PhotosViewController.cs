using UIKit;
using Photos;
using Foundation;
using System.Drawing;
using CoreFoundation;

namespace Dogfood.CSharp.Photos
{
	public class PhotosViewController : UICollectionViewController
	{
		static readonly NSString cellId = new NSString("ImageCell");

		PHFetchResult fetchResults;
		PHImageManager imageMgr;
		PhotoLibraryObserver observer;

        public static PhotosViewController GenerateNewController() {
		    var layout = new UICollectionViewFlowLayout
		    {
		        ItemSize = new SizeF(100, 100)
		    };
            return new PhotosViewController(layout);
		}

		public PhotosViewController(UICollectionViewLayout layout) : base(layout)
		{
			Title = "All Photos";

			imageMgr = new PHImageManager();
			fetchResults = PHAsset.FetchAssets(PHAssetMediaType.Image, new PHFetchOptions()
			{
				SortDescriptors = new NSSortDescriptor[] { new NSSortDescriptor("creationDate", false) }
			});

			observer = new PhotoLibraryObserver(this);

			PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(observer);
		}

		class PhotoLibraryObserver : PHPhotoLibraryChangeObserver
		{
			readonly PhotosViewController controller;

			public PhotoLibraryObserver(PhotosViewController controller)
			{
				this.controller = controller;
			}

			public override void PhotoLibraryDidChange(PHChange changeInstance)
			{
				DispatchQueue.MainQueue.DispatchAsync(() => {

					var changes = changeInstance.GetFetchResultChangeDetails(controller.fetchResults);
					controller.fetchResults = changes.FetchResultAfterChanges;
					controller.CollectionView.ReloadData();
				});
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			CollectionView.RegisterClassForCell(typeof(ImageCell), cellId);

			this.CollectionView.BackgroundColor = UIColor.White;
		}

		public override System.nint GetItemsCount(UICollectionView collectionView, System.nint section)
		{
			return fetchResults.Count;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var imageCell = (ImageCell)collectionView.DequeueReusableCell(cellId, indexPath);

			imageMgr.RequestImageForAsset((PHAsset)fetchResults[indexPath.Item], new SizeF(160, 160),
				PHImageContentMode.AspectFill, new PHImageRequestOptions(), (img, info) => {
					imageCell.ImageView.Image = img;
				});

			return imageCell;
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var photoController = new PhotoViewController
			{
				Asset = (PHAsset)fetchResults[indexPath.Item]
			};
			NavigationController.PushViewController(photoController, true);
		}
	}

	public class ImageCell : UICollectionViewCell
	{
		public UIImageView ImageView { get; set; }

		[Export("initWithFrame:")]
		public ImageCell(RectangleF frame) : base(frame)
		{

			ImageView = new UIImageView(new RectangleF(0, 0, 100, 100));
			//ImageView.BackgroundColor = UIColor.White;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			ContentView.AddSubview(ImageView);
		}
	}
}
