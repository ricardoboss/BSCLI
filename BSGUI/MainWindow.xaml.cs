using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace BSGUI
{
    [JsonObject]
    public class Song
    {
        [JsonProperty("coverUrl")] public string CoverUri { get; set; }
        [JsonIgnore] public string CoverUrl => "https://beatsaver.com" + CoverUri;
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }

    [JsonObject]
    public class SongList
    {
        [JsonProperty("docs")] public IEnumerable<Song> Songs { get; set; }
        [JsonProperty("lastPage")] public int LastPage { get; set; }
        [JsonProperty("nextPage")] public int? NextPage { get; set; }
        [JsonProperty("prevPage")] public int? PrevPage { get; set; }
        [JsonProperty("totalDocs")] public int TotalDocs { get; set; }
    }
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<Song> Songs { get; } = new ObservableCollection<Song>();

        private int nextPage;
        
        public MainWindow()
        {
            DataContext = this;
            
            InitializeComponent();

            Task.Run(LoadList);
        }

        private void LoadList()
        {
            using var client = new WebClient();
            var jsonList = client.DownloadString("https://beatsaver.com/api/maps/hot/" + nextPage);
            var songList = JsonConvert.DeserializeObject<SongList>(jsonList);

            nextPage = songList.NextPage ?? 0;
            
            foreach (var song in songList.Songs)
                Dispatcher.Invoke(() => Songs.Add(song));
        }

        private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn))
                return;

            if (!(btn.Tag is string key))
                return;

            Task.Run(() =>
                BSCLI.Commands.CommandSave(key)
            );
        }

        private void ButtonLoadMore_OnClick(object sender, RoutedEventArgs e)
        {
            Task.Run(LoadList);
        }
    }
}