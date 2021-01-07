using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Blast.API.Processes;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;
using Genbox.WolframAlpha;
using Genbox.WolframAlpha.Enums;
using Genbox.WolframAlpha.Requests;
using Genbox.WolframAlpha.Responses;

namespace Wolfram.Fluent.Plugin
{
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
                "Compute expert-level answers using Wolfram’s breakthrough \nalgorithms, knowledgebase and AI technology",
                _supportedOperations)
            {
                MinimumSearchLength = 1,
                IsProcessSearchEnabled = false,
                IsProcessSearchOffline = false,
                ApplicationIconGlyph = "\uE82D",
                SearchAllTime = ApplicationSearchTime.Fast
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
            
            if (!Enum.TryParse<WolframTag>(searchedTag, true, out WolframTag wolframTag))
                yield break;
            FullResultResponse response = await _wolframClient.SearchWolfram(searchedText, wolframTag);
            yield return new WolframSearchResult(response, searchedText, searchedTag, 10, _supportedOperations, null, null);
            
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
}