using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genbox.WolframAlpha;
using Genbox.WolframAlpha.Objects;
using Genbox.WolframAlpha.Requests;
using Genbox.WolframAlpha.Responses;

namespace Wolfram.Fluent.Plugin
{
    public class WolframClient
    {
        private readonly WolframAlphaClient _client;

        //private WolframAlphaClient _request;

        private readonly IReadOnlyDictionary<string, string> _subPodsIdsToDisplayName =
            new Dictionary<string, string>
            {
                ["Input"] = "Input", //define
                ["Definition:WordData"] = "Result", //define
                ["Pronunciation:WordData"] = "Pronunciation", //word
                ["Hyphenation:WordData"] = "Hyphenation", //word
                ["Synonyms:WordData"] = "Synonyms", //word
                ["Hypernym:WordData"] = "Hypernyms (Broader Terms)" //word
            };
        
        private readonly IReadOnlyDictionary<WolframTag, List<string>> _tagsPodsId =
            new Dictionary<WolframTag, List<string>>
            {
                [WolframTag.Word] = new List<string>()
                {
                    "Input", "Definition:WordData", "Synonyms:WordData"
                },
                [WolframTag.Define] = new List<string>()
                {
                    "Input", "Definition:WordData"
                }
            };

        public WolframClient()
        {
            _client = new WolframAlphaClient("2JRGWR-965KH2T2AR");
        }
        
        public async Task<FullResultResponse> SearchWolfram(string wolframSearch, WolframTag wolframTag)
        {
            FullResultRequest request = new FullResultRequest(wolframSearch){IncludePodIds = _tagsPodsId[wolframTag]};
            FullResultResponse results = await _client.FullResultAsync(request).ConfigureAwait(false);
            return results;
        }
    }
}