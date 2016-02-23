using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Orion.Xam.Android.SearchBox.Sample {
	[Activity(Label = "SearchBox Sample", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity {
		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
			var searchBoxView = FindViewById<SearchBoxView>(Resource.Id.searchbox);
			searchBoxView.ItemsSource = new List<SearchResult> {
				new SearchResult {
					Title = "Result 1"
				},
				new SearchResult {
					Title = "Result 2"
				},
				new SearchResult {
					Title = "Result 3"
				},
				new SearchResult {
					Title = "Result 4"
				},
				new SearchResult {
					Title = "Result 5"
				}
			};
		}
	}
}

