using System.Collections.Generic;

namespace Orion.Xam.Android.SearchBox.Events {
	public class SearchBoxItemFilteredEventArgs : SearchBoxEventArgs {
		public List<SearchResult> Items { get; }

		public SearchBoxItemFilteredEventArgs(List<SearchResult> items) {
			Items = items;
		}
	}
}