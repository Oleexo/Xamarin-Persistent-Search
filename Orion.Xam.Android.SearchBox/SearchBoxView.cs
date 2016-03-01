using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location.Places;
using Android.Gms.Maps.Model;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Orion.Xam.Android.SearchBox.Events;

namespace Orion.Xam.Android.SearchBox {
	public class SearchBoxView : RelativeLayout {
		private RelativeLayout _root;
		private ListView _listResults;
		private RelativeLayout _searchArea;
		private EditText _search;
		private ImageView _mic;
		private ImageView _overflow;
		private ImageView _listButton;
		private Context _context;
		private TextView _placeholder;
		private bool _isSearching;
		private SearchAdapter _resultAdapter;
		private IList<SearchResult> _itemsSource;
		private List<SearchResult> _filteredSources;
		private DrawerArrowDrawable _drawerArrowDrawable;
		private bool _mapsResults;
		private GoogleApiClient _googleApiClient;
		private string _defaultPlacerHolder;
		private Drawable _drawableRight;
		private ImageView _clear;
		private CancellationTokenSource _cancellationTokenSearch;
		private int _progressBarStyle;
		private bool _isLoading;

		#region Properties
		public string PlaceHolder { get; set; }

		public IList<SearchResult> ItemsSource {
			get { return _itemsSource; }
			set {
				_itemsSource = value;
				_filteredSources = value?.ToList() ?? new List<SearchResult>();
			}
		}

		public SearchResult CurrentSelection { get; set; }

		public bool MapsResults {
			get { return _mapsResults; }
			set {
				_mapsResults = value;
				if (value)
					EnabledMaps();
				else
					_googleApiClient = null;
			}
		}

		public LatLngBounds LatLngBounds { get; set; }

		public Drawable DrawableRight {
			get { return _drawableRight; }
			set {
				_drawableRight = value;
				ToggleDrawableRight();
			}
		}

		public GoogleApiClient GoogleApiClient {
			get { return _googleApiClient; }
			set { _googleApiClient = value; }
		}
		public Func<string, Task<List<SearchResult>>> CustomSearchFunc { get; set; }

		public bool IsLoading {
			get { return _isLoading; }
			set {
				_isLoading = value;
				var progressBar = FindViewById<ProgressBar>(Resource.Id.sb_progressBar);
				progressBar.Visibility = value ? ViewStates.Visible : ViewStates.Invisible;
			}
		}
		#endregion

		#region Events

		public event EventHandler<SearchBoxEventArgs> DrawableRightClick;
		public event EventHandler<SearchBoxItemSelectedEventArgs> ItemSelected;
		public event EventHandler<SearchBoxItemFilteredEventArgs> ItemsFiltered;
		public event EventHandler<SearchBoxKeyBoardEventArgs> KeyboardChanged;
		#endregion

		#region Constructors
		public SearchBoxView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
		}

		public SearchBoxView(Context context) : base(context) {
			Initialize(context);
		}

		public SearchBoxView(Context context, IAttributeSet attrs) : base(context, attrs) {
			InitializeAttrs(context, attrs);
			Initialize(context);
		}

		public SearchBoxView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
			InitializeAttrs(context, attrs);
			Initialize(context);
		}
		#endregion

		#region Inits
		private void InitializeAttrs(Context context, IAttributeSet attrs) {
			var attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.SearchBoxView);
			_defaultPlacerHolder = attributes.GetString(Resource.Styleable.SearchBoxView_placeholder);
			_mapsResults = attributes.GetBoolean(Resource.Styleable.SearchBoxView_mapsResults, false);
			DrawableRight = attributes.GetDrawable(Resource.Styleable.SearchBoxView_drawableRight);
		}

		private void Initialize(Context context) {
			_context = context;
			_filteredSources = new List<SearchResult>();
			Inflate(context, Resource.Layout.search_box, this);
			_root = FindViewById<RelativeLayout>(Resource.Id.sb_root);
			_listResults = FindViewById<ListView>(Resource.Id.sb_list_results);
			_searchArea = FindViewById<RelativeLayout>(Resource.Id.sb_searchArea);
			_search = FindViewById<EditText>(Resource.Id.sb_search);
			_mic = FindViewById<ImageView>(Resource.Id.sb_mic);
			_overflow = FindViewById<ImageView>(Resource.Id.sb_overflow);
			_listButton = FindViewById<ImageView>(Resource.Id.sb_list_button);
			_placeholder = FindViewById<TextView>(Resource.Id.sb_placeholder);
			_clear = FindViewById<ImageView>(Resource.Id.sb_clear);

			_drawerArrowDrawable = new DrawerArrowDrawable(Resources);
			_listButton.SetImageDrawable(_drawerArrowDrawable);
			_placeholder.Text = PlaceHolder ?? _defaultPlacerHolder;
			ToggleDrawableRight();

			_placeholder.Click += PlaceholderOnClick;
			_listButton.Click += ListButtonOnClick;
			_search.TextChanged += SearchOnTextChanged;
			_listResults.ItemClick += ListResultsOnItemClick;
			_overflow.Click += OverflowOnClick;
			_clear.Click += ClearOnClick;
		}

		#endregion

		#region Events methods
		private void ClearOnClick(object sender, EventArgs eventArgs) {
			ClearSearch();
		}
		private void OverflowOnClick(object sender, EventArgs eventArgs) {
			DrawableRightClick?.Invoke(this, new SearchBoxEventArgs());
		}
		private void ListButtonOnClick(object sender, EventArgs eventArgs) {
			ToggleSearch();
		}
		private void PlaceholderOnClick(object sender, EventArgs eventArgs) {
			OpenSearch();
		}

		private void SearchOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs) {
			Search(new string(textChangedEventArgs.Text.ToArray()));
		}
		private void ListResultsOnItemClick(object sender, AdapterView.ItemClickEventArgs args) {
			var result = _filteredSources.ElementAt(args.Position);
			SetSelectedElement(result);

		}
		#endregion

		public void ClearSearch() {
			_search.Text = string.Empty;
			Search(string.Empty);
		}
		private void ToggleDrawableRight() {
			if (_search == null || _overflow == null) {
				return;
			}
			if (DrawableRight != null) {
				_overflow.SetImageDrawable(_drawableRight);
				_overflow.Visibility = ViewStates.Visible;
			}
			else {
				_overflow.Visibility = ViewStates.Gone;
			}
		}

		protected void EnabledMaps() {
			try {
				_googleApiClient = new GoogleApiClient.Builder(_context)
					.AddApi(PlacesClass.GEO_DATA_API)
					.Build();
				_googleApiClient.Connect();
			}
			catch (Exception e) {
				System.Diagnostics.Debug.WriteLine(e);
			}
		}

		private void ToggleSearch() {
			if (_isSearching) {
				CloseSearch();
			}
			else {
				OpenSearch();
			}
		}


		private void OpenSearch() {
			if (_isSearching)
				return;
			_placeholder.Visibility = ViewStates.Gone;
			_search.Visibility = ViewStates.Visible;
			_search.RequestFocus();
			_listResults.Visibility = ViewStates.Visible;
			_clear.Visibility = string.IsNullOrEmpty(_search.Text) ? ViewStates.Invisible : ViewStates.Visible;
			_resultAdapter = new SearchAdapter(_context, _filteredSources, _search);
			_listResults.Adapter = _resultAdapter;

			_drawerArrowDrawable.setParameter(1);
			_drawerArrowDrawable.setFlip(true);
			EnableKeyboard();
			_isSearching = true;
		}

		private void CloseSearch(bool afterSelection = false) {
			if (!_isSearching)
				return;
			_placeholder.Visibility = ViewStates.Visible;
			_search.Visibility = ViewStates.Gone;
			_listResults.Visibility = ViewStates.Gone;
			_clear.Visibility = ViewStates.Gone;

			_drawerArrowDrawable.setParameter(0);
			_drawerArrowDrawable.setFlip(false);
			DisableKeyboard(afterSelection);
			_isSearching = false;
		}

		private async void Search(string text) {
			IsLoading = true;
			_clear.Visibility = string.IsNullOrEmpty(text) ? ViewStates.Invisible : ViewStates.Visible;
			_filteredSources = await SearchAsync(text);
			_listResults.Adapter = _resultAdapter = new SearchAdapter(_context, _filteredSources, _search);
			ItemsFiltered?.Invoke(this, new SearchBoxItemFilteredEventArgs(_filteredSources.ToList()));
			IsLoading = false;
		}

		private Task<List<SearchResult>> SearchAsync(string text) {
			var tcs = new TaskCompletionSource<List<SearchResult>>();

			_cancellationTokenSearch?.Cancel();
			_cancellationTokenSearch = new CancellationTokenSource();
			var token = _cancellationTokenSearch.Token;
			Task.Run(async () => {
				var results = ItemsSource?.Where(s => s.IsMatch(text))?.ToList() ?? new List<SearchResult>();
				token.ThrowIfCancellationRequested();
				if (MapsResults && !string.IsNullOrWhiteSpace(text)) {
					if (_googleApiClient == null) {
						EnabledMaps();
					}
					var predictionBuffer = await PlacesClass.GeoDataApi.GetAutocompletePredictionsAsync(_googleApiClient, text, null, null);
					foreach (var element in predictionBuffer) {
						results.Add(new MapsSearchResult(element));
					}
				}
				token.ThrowIfCancellationRequested();
				if (CustomSearchFunc != null) {
					var customSearchResults = await CustomSearchFunc.Invoke(text);
					results.AddRange(customSearchResults);
				}
				token.ThrowIfCancellationRequested();
				tcs.SetResult(results.OrderBy(o => o.Title).ToList());
			}, token);
			tcs.Task.ContinueWith((result) => {
				_cancellationTokenSearch = null;
			}, token);
			return tcs.Task;
		}

		#region Keyboards
		private void EnableKeyboard() {
			var inputMethodManager = _context.GetSystemService(Context.InputMethodService) as InputMethodManager;
			inputMethodManager?.ToggleSoftInputFromWindow(ApplicationWindowToken, ShowSoftInputFlags.Explicit, 0);
			KeyboardChanged?.Invoke(this, new SearchBoxKeyBoardEventArgs(false, SearchBoxKeyBoardEventArgs.KeyboardStatus.Opened));
		}
		private void DisableKeyboard(bool afterSelection) {
			var inputMethodManager = _context.GetSystemService(Context.InputMethodService) as InputMethodManager;
			inputMethodManager?.HideSoftInputFromWindow(ApplicationWindowToken, 0);
			KeyboardChanged?.Invoke(this, new SearchBoxKeyBoardEventArgs(afterSelection, SearchBoxKeyBoardEventArgs.KeyboardStatus.Closed));
		}

		#endregion

		private void SetSelectedElement(SearchResult result) {
			_placeholder.Text = result.Title;
			CurrentSelection = result;
			ItemSelected?.Invoke(this, new SearchBoxItemSelectedEventArgs(result));
			CloseSearch(true);
			_listResults.InvalidateViews();
		}

	}
}

