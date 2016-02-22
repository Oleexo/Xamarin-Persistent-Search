using Android.Gms.Location.Places;

namespace Orion.Xam.Android.SearchBox {
	public class MapsSearchResult : SearchResult {
		private readonly IAutocompletePrediction _element;

		public MapsSearchResult(IAutocompletePrediction element) {
			_element = element;
			Type = SearchType.Maps;
			Title = element.Description;
		}
	}
}