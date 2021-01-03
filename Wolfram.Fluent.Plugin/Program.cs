using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Blast.API.Processes;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;
using Genbox.WolframAlpha;
using Genbox.WolframAlpha.Objects;

/*
 * Developer's Guide: https://www.fluentsearch.net/posts/c-plugins-developer-guide
 * ISearchResult object displays search results (left side of Fluent Search)
 * ISearchOperation list (right side of Fluent Search)
 * ISearchApplication interface must be inherited by search application class 
 */

namespace Wolfram.Fluent.Plugin
{
    public class WolframOperation : SearchOperationBase
    {
        public WolframOperation() : base("Open in Wolfram Alpha", "", "")
        {
        }
    }
    
    public class WolframSearchResult : SearchResultBase
    {
        public WolframSearchResult(WolframResult wolframResult, string searchedText, string resultType,
            double score, IList<ISearchOperation> supportedOperations, ICollection<SearchTag> tags,
            ProcessInfo processInfo = null) : base("Dictionary", wolframResult.Search, searchedText, resultType, score,
            supportedOperations, tags, processInfo)
        {
            var imageIcon = new BitmapImageResult(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Fluent.Wolfram.Plugin.wolfram-alpha-colored.png"));
            PreviewImage = imageIcon;
            UseIconGlyph = false;
            //IconGlyph = "\uE82D";
            SearchObjectId = wolframResult.Search;
            InformationElements = wolframResult.Pods.Select(pod => new InformationElement(pod.Title, pod.RawText))
                .ToList();
        }

        protected override void OnSelectedSearchResultChanged()
        {
        }
    }

    // ISearchApplication
    // this is the search application
    public class WolframSearchApp : ISearchApplication
    {
        private readonly WolframClient _wolframClient;
        private SearchTag[] _searchTags;
        private IList<ISearchOperation> _supportedOperations;
        private SearchApplicationInfo _applicationInfo;

        public WolframSearchApp()
        {
            _wolframClient = new WolframClient();
            _searchTags = new[] {new SearchTag {Name = "Dictionary"}};
            _supportedOperations = new[] {new WolframOperation()};
            _applicationInfo = new SearchApplicationInfo("Wolfram Alpha",
                "Compute expert-level answers using Wolfram’s breakthrough \nalgorithms, knowledgebase and AI technology", _supportedOperations)
            {
                // add custom PreviewImage here replacing iconGlyph
                MinimumSearchLength = 1,
                IsProcessSearchEnabled = false,
                IsProcessSearchOffline = false,
                ApplicationIconGlyph = "\uE82D",
                SearchAllTime = ApplicationSearchTime.Fast
                //SearchProcessTime = ApplicationSearchTime.Fast,
            };
        }

        public ValueTask LoadSearchApplicationAsync()
        {
            return ValueTask.CompletedTask;
        }

        public SearchApplicationInfo GetApplicationInfo()
        {
            return _applicationInfo;
        }

        public async IAsyncEnumerable<ISearchResult> SearchAsync(SearchRequest searchRequest,
            CancellationToken cancellationToken)
        {
            string searchedText = searchRequest.SearchedText;
            string searchedTag = searchRequest.SearchedTag;

            if (searchedTag != "word")
                yield break;
            
            // remove whitespace from string
            // may be redundant and/or throw errors
            //searchedText = searchedText.Replace(" ", String.Empty);
            WolframResult wolframResult = await _wolframClient.SearchWolfram("word " + searchedText);
            //WolframResult wolframResult = await _wolframClient.SearchWolfram(searchedText.Replace("?", String.Empty));
            yield return new WolframSearchResult(wolframResult, searchedText, "word", 10, _supportedOperations,
                new List<SearchTag>
                {
                    new() {Name = "word"}
                    //new() {Name = "?"}
                });
        }

        public ValueTask<ISearchResult> GetSearchResultForId(string serializedSearchObjectId)
        {
            return new ValueTask<ISearchResult>();
        }

        public ValueTask<IHandleResult> HandleSearchResult(ISearchResult searchResult)
        {
            var search = searchResult;
            string url =
                $"https://www.wolframalpha.com/input/?i={HttpUtility.UrlEncode(search.SearchObjectId.ToString())}";
            ProcessUtils.GetManagerInstance().StartNewProcess(url);
            return new ValueTask<IHandleResult>(new HandleResult(true, false));
        }
    }

    public interface IWolframClient
    {
        Task<WolframResult> SearchWolfram(string search);
    }

    public class WolframClient : IWolframClient
    {
        private readonly WolframAlphaClient _client;

        private readonly IReadOnlyDictionary<string, string> _subPodsIdsToDisplayName =
            new Dictionary<string, string>
            {
                ["Input"] = "Input",
                ["Definition:WordData"] = "Result",
                ["Synonyms:WordData"] = "Synonyms"
            };

        public WolframClient()
        {
            _client = new WolframAlphaClient("2JRGWR-965KH2T2AR");
        }

        public async Task<WolframResult> SearchWolfram(string search)
        {
            var response = await _client.FullResultAsync(search);
            WolframResult wolframResult = new WolframResult
            {
                Search = search
            };
            foreach (Pod pod in response.Pods)
            {
                if (!_subPodsIdsToDisplayName.TryGetValue(pod.Id, out var displayName))
                    continue;

                foreach (SubPod subPod in pod.SubPods)
                {
                    wolframResult.Pods.Add(new PodResult
                    {
                        Title = displayName,
                        RawText = subPod.Plaintext
                    });
                }
            }

            return wolframResult;
        }
    }
    

    public class WolframResult
    {
        public string Search { get; set; }

        public IList<PodResult> Pods { get; set; } = new List<PodResult>();

        public override string ToString()
        {
            return $"For search: {Search}: {string.Join("\n", Pods)}";
        }
    }

    public class PodResult
    {
        public string Title { get; set; }

        public string RawText { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, RawText: {RawText}";
        }
    }

}