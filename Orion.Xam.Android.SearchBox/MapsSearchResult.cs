using Android.Gms.Location.Places;

namespace Orion.Xam.Android.SearchBox {
	public class MapsSearchResult : SearchResult {
		public IAutocompletePrediction Element { get; }

		public MapsSearchResult(IAutocompletePrediction element) {
			Element = element;
			Type = SearchType.Maps;
			Title = element.Description;
		}
	}
}