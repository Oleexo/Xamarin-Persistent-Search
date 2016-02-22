namespace Orion.Xam.Android.SearchBox.Events {
	public class SearchBoxItemSelectedEventArgs : SearchBoxEventArgs {
		public SearchResult Selection { get; }

		public SearchBoxItemSelectedEventArgs(SearchResult selection) {
			Selection = selection;
		}
	}
}