using System;
using Android.Gms.Location.Places;
using Android.Graphics.Drawables;

namespace Orion.Xam.Android.SearchBox {
	public class SearchResult {
		public enum SearchType {
			Text,
			Maps,
			Custom
		}

		#region Constructors
		public SearchResult() {
			Type = SearchType.Text;
		}
		#endregion

		public string Title { get; set; }
		public SearchType Type { get; set; }
		public Drawable Icon { get; set; }

		public virtual bool IsMatch(string text) {
			return Title.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}