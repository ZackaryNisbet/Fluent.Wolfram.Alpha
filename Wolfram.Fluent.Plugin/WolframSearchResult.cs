using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;
using Genbox.WolframAlpha.Extensions;
using Genbox.WolframAlpha.Objects;
using Genbox.WolframAlpha.Requests;
using Genbox.WolframAlpha.Responses;

namespace Wolfram.Fluent.Plugin
{
    public class WolframSearchResult : SearchResultBase
    {
        
        private static BitmapImageResult IconTest { get; } = new BitmapImageResult(Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Wolfram.Fluent.Plugin.wolfram-alpha-colored.png"));

        private readonly IReadOnlyDictionary<string, string> _subPodsIdsToDisplayName =
            new Dictionary<string, string>
            {
                ["Input"] = "Input Interpretation", //define
                ["Definition:WordData"] = "Result", //define
                ["Pronunciation:WordData"] = "Pronunciation", //word
                ["Hyphenation:WordData"] = "Hyphenation", //word
                ["Synonyms:WordData"] = "Synonyms", //word
                ["Hypernym:WordData"] = "Hypernyms (Broader Terms)" //word
            };

        public WolframSearchResult(FullResultResponse wolframResponse, string searchedText, string resultType,
            double score, IList<ISearchOperation> supportedOperations, ICollection<SearchTag> tags,
            ProcessInfo processInfo = null) : base("Dictionary", resultType, searchedText,
            resultType, score,
            supportedOperations, tags, processInfo)
        {
            PreviewImage = IconTest;
            UseIconGlyph = false;
            SearchObjectId = searchedText;
            InformationElements = new List<InformationElement>();
            foreach (Pod pod in wolframResponse.Pods)
            {
                if (_subPodsIdsToDisplayName.ContainsKey(pod.Id))
                {
                    var name = _subPodsIdsToDisplayName[pod.Id];
                    foreach (SubPod subPod in pod.SubPods)
                    {
                        var value = subPod.Plaintext;
                        InformationElements.Add(new InformationElement(name, value));
                    }
                }
                else
                {
                    if (!pod.Id.Contains(":")) continue;
                    var NameInt = pod.Id.IndexOf(":");
                    var name = pod.Id.Substring(0, NameInt);
                    foreach (SubPod subPod in pod.SubPods)
                    {
                        var value = subPod.Plaintext;
                        InformationElements.Add(new InformationElement(name, value));
                    }
                }
            }
        }

        protected override void OnSelectedSearchResultChanged()
        {
        }
    }
}