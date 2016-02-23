using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace Orion.Xam.Android.SearchBox {
	internal class SearchAdapter : ArrayAdapter<SearchResult> {
		private readonly List<SearchResult> _sources;
		private readonly EditText _search;
		private bool _animate = true;
		private int count = 0;

		public SearchAdapter(Context context, List<SearchResult> sources, EditText search) : base(context, 0, sources) {
			_sources = sources;
			_search = search;
		}

		public override View GetView(int position, View convertView, ViewGroup parent) {
			SearchResult option = GetItem(position);
			if (convertView == null) {
				convertView = LayoutInflater.From(Context).Inflate(
					Resource.Layout.SearchOption, parent, false);

				if (_animate) {
					Animation anim = AnimationUtils.LoadAnimation(Context,
						Resource.Animation.AnimDown);
					anim.Duration = 400;
					convertView.StartAnimation(anim);
					if (count == Count) {
						_animate = false;
					}
					count++;
				}
			}

			View border = convertView.FindViewById(Resource.Id.sb_border);
			if (position == 0) {
				border.Visibility = ViewStates.Visible;
			} else {
				border.Visibility = ViewStates.Gone;
			}
			TextView title = convertView.FindViewById<TextView>(Resource.Id.sb_title);
			title.Text  = option.Title;
			if (option.Icon != null) {
				ImageView icon = convertView.FindViewById<ImageView>(Resource.Id.sb_icon);
				icon.SetImageDrawable(option.Icon);
			}
			//ImageView up = (ImageView) convertView.FindViewById<ImageView>(R.id.up);
			//up.setOnClickListener(new OnClickListener() {

			//	@Override
			//	public void onClick(View v) {
			//                 mSearch.setText(title.getText().toString());
			//                 mSearch.setSelection(mSearch.getText().length());
			//	}

			//});

			return convertView;
		}
	}
}