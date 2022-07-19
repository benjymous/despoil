using System.Text.RegularExpressions;

string text = System.IO.File.ReadAllText("issues.txt").Replace("\r","");
var entries = text.Split("\n\n\n");

var issues = new List<string>();
var threads = new Dictionary<string, List<string>>();
var threadKey = new Dictionary<string, string>();
var entityKey = new Dictionary<string, string>();
var entityAlias = new Dictionary<string, string>();
var entityAppearence = new Dictionary<string, HashSet<string>>();

var collections = new Dictionary<string, List<(string,string)>>();

var entriesHtml = new List<string>();
var colourStyles = new List<string>();

var seenDates = new HashSet<double>();

var dateMarkers = new Dictionary<long, string>();
var dateAppearence = new Dictionary<long, HashSet<string>>();

var outputHtml = new List<string>
{
    "<html>",
    "<head>",
    "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">",
    "  <link href='style.css' rel='stylesheet'>",
    "  <link href='colors.css' rel='stylesheet'>",
    "  <link href='order0.css' rel='stylesheet' id='orderStyle'>",
    "  <script src='despoil.js'></script>",
    "  <title>Despoil - The (hopefully) less spoilery Sandman Universe timeline</title>",
    "</head>",

    "<body>"
};

double currentDate = 0;
const double Increment = 0.0001;

var eventDates = new List<(int,double)>();

foreach (var entry in entries)
{

    int eventCount = 0;

    var entrylines = entry.Split("\n");
    if (entrylines.Length < 4)
    {
        Console.WriteLine("Error with " + entrylines[0]);
        return;
    }

    for (int i=5; i< entrylines.Length; i++)
    {

        if (!(entrylines[i].StartsWith('#') || entrylines[i].StartsWith("?")))
        {
            Console.WriteLine("Error with " + entrylines[0]);
            Console.WriteLine($"{entrylines[i]}' should start with #");
            return;
        }
    }
   
    var thread = entrylines[1].Trim();
    var issueTitle = $"<span class='itemissue'>{entrylines[0].Trim()}</span><span class='itemtitle'>{entrylines[2]}</span>";
    var issueTitlePlain = $"{entrylines[0].Trim()} - {entrylines[2]}";
    currentDate = despoil.Util.ParseDate(entrylines[3]);

    if (!threads.ContainsKey(thread))
    {
        threadKey[thread] = $"t{threads.Count}";
        threads[thread] = new List<string>(); 
    }

    var threadkey = threadKey[thread];
    var issueId = $"{threadkey}_{threads[thread].Count}";

    threads[thread].Add(issueTitle);
    issues.Add(issueTitlePlain);



    var issueCollections = entrylines[4].Split(",").Select(x => x.Trim());

    foreach (var issueCollection in issueCollections)
    {
        if (!collections.ContainsKey(issueCollection))
        {
            collections[issueCollection] = new List<(string,string)>();
        }

        
        collections[issueCollection].Add((issueTitle,issueId));

    }




    var entryEvents = entrylines.Skip(5).ToList();
    if (entryEvents.Count == 0)
    {
        entryEvents.Add(issueTitlePlain);
    }

    var dateStr = "&nbsp;";
    foreach (var e in entryEvents)
    {
        if (e.StartsWith("?")) continue;

        var ev = e;

        MatchCollection matches = Regex.Matches(e, "<.*?>");

        List<string> entityClasses = new List<string>();

        foreach (Match match in matches)
        {
            var tag = match.ToString();
            var bareText = match.ToString().Replace("<", "").Replace(">", "");
            var entity = "";
            var alias = "";

            if (bareText.Contains("|"))
            {
                var bits = bareText.Split("|");
                bareText = bits[0];
                alias = bits[1];
                if (!entityAlias.ContainsKey(bareText))
                {
                    entityAlias[bareText] = alias;
                }
            }
            else
            {
                if (entityAlias.ContainsKey(bareText))
                {
                    alias = entityAlias[bareText];
                }
                else
                {
                    alias = bareText;
                }
            }

            if (!entityKey.ContainsKey(alias))
            {
                entity = $"entity_{entityKey.Count}";
                entityKey[alias] = entity;
            }
            else
            {
                entity = entityKey[alias];
            }

            if (!entityAppearence.ContainsKey(entity))
            {
                entityAppearence[entity] = new HashSet<string>();
            }

            entityClasses.Add(entity);
            entityAppearence[entity].Add(issueId);

            //var resultTag = $"<span class='entity {entity}'>{bareText}</span>";
            var title = alias==bareText ? "" : $"title='{alias}'";
            var resultTag = $"<span {title} class='entity {entity}' onclick='togglePush(\"{entity}\")'>{bareText}</span>";

            ev = ev.Replace(tag, resultTag);
        }


        var evBody = "";
       

        if (ev.StartsWith("## "))
        {
            var bits = ev.Split(':');
            dateStr = bits[0].Substring(2);
            currentDate = despoil.Util.ParseDate(dateStr);

            if (dateStr.Contains("|"))
            {
                dateStr = dateStr.Substring(dateStr.IndexOf("|") + 1);
            }
            else
            {
                dateStr = dateStr.Replace("~", "approx");

                if (dateStr.Contains("."))
                {
                    dateStr = dateStr.Substring(0, (dateStr.IndexOf(".")));
                }
            

                if (dateStr.Contains("-"))
                {
                    dateStr = dateStr.Replace("-", "");
                    dateStr += " BC";
                }
                if (currentDate >= 0 && currentDate < 1000) 
                {
                    dateStr += " AD";
                }
            }

            evBody = String.Join(":", bits.Skip(1));
        }
        else if (ev.StartsWith("# "))
        {
            evBody = ev.Substring(2);

            currentDate += Increment;
        }
        else
        {
            evBody = ev;
        }

        if (evBody.Contains("++"))
        {
            evBody = evBody.Substring(0, evBody.IndexOf("++"));
        }

        while (seenDates.Contains(currentDate))
        {
            currentDate += Increment;
        }

        int centuryKey = int.MinValue;
        int decadeKey = int.MinValue;;

        if (currentDate > 0 && currentDate < 3000)
        {
            int century = 1 + ((int)currentDate / 100);
            centuryKey = (century-1)*100;

            if (!dateMarkers.ContainsKey(centuryKey))
            {
                dateMarkers.Add(centuryKey, $"{despoil.Util.NumberOrdinal(century)} century");
            }

            if (currentDate >= 1900 && currentDate < 3000)
            {
                decadeKey = ((int)currentDate / 10)*10;

                if (!dateMarkers.ContainsKey(decadeKey))
                {
                    dateMarkers.Add(decadeKey, $"{decadeKey}s");
                }
            }
        }

        if (centuryKey != int.MinValue)
        {
            if (!dateAppearence.ContainsKey(centuryKey))
            {
                dateAppearence[centuryKey] = new HashSet<string>();
            }
            dateAppearence[centuryKey].Add(issueId);
        }

        if (decadeKey != int.MinValue)
        {
            if (!dateAppearence.ContainsKey(decadeKey))
            {
                dateAppearence[decadeKey] = new HashSet<string>();
            }
            dateAppearence[decadeKey].Add(issueId);
        }


        if (eventCount == 0)
        {
            // Add dummy entry for issue
            seenDates.Add(currentDate);
            eventDates.Add((eventDates.Count+1,currentDate));

            entriesHtml.Add($"<div class='itemrow {issueId}_outer'>");
            entriesHtml.Add($"<button class='top issueToggle {threadkey} {issueId}' onclick='toggleIssue(\"{issueId}\")'>");
            
            entriesHtml.Add($"{entrylines[3]} - {issueTitle}");
            
            entriesHtml.Add("</button>");
            entriesHtml.Add("</div>");
        }

        eventCount++;


        seenDates.Add(currentDate);

        eventDates.Add((eventDates.Count+1,currentDate));

        entriesHtml.Add($"<div class='itemrow {issueId}_outer hidden'>");
        entriesHtml.Add($"<div class='itemdate' title='{currentDate}'>{dateStr}</div>");
        entriesHtml.Add($"<div class='event {threadkey} {issueId} {string.Join(" ", entityClasses)}'>");
        
        entriesHtml.Add($"<div class='itemsubtitle'>{entrylines[3]} - {issueTitle}</div>");
        entriesHtml.Add($"<span class='itembody'>{evBody}</span>");
       
        entriesHtml.Add("</div>");
        entriesHtml.Add("</div>");
    }
}


foreach (var marker in dateMarkers)
{
    Console.WriteLine($"{marker.Key} => {marker.Value}");
    eventDates.Add((eventDates.Count+1,marker.Key));
    entriesHtml.Add($"<div class='dateMarker hidden {String.Join(" ", dateAppearence[marker.Key].Select(x => "entityissue_"+x))}'>");
    entriesHtml.Add($"<div>");
    
    entriesHtml.Add(marker.Value);
    
    entriesHtml.Add("</div>");
    entriesHtml.Add("</div>");
}

int threadIdx = 0;

outputHtml.Add("<div class='row'>");

outputHtml.Add("<div class='col-menu'>");

outputHtml.Add("<span>");
outputHtml.Add("Publication date ");
outputHtml.Add("<label class='switch'>");
outputHtml.Add("<input type='checkbox' id='order' onclick='changeOrder()'>");
outputHtml.Add("<span class='slider round'></span>&nbsp;");
outputHtml.Add("</label>");
outputHtml.Add(" Chronological");
outputHtml.Add("</span>");

outputHtml.Add("<details open><summary>I have read...</summary>");

outputHtml.Add("<button onclick='showNone()'>Nothing</button> - <button onclick='showAll()'>Everything</button><br>");

outputHtml.Add("<details><summary>Collections</summary>");

var colnames = collections.Keys.OrderBy(x => x, new despoil.CollectionComparer()).ToArray();

foreach (var collection in colnames)
{
    

    var coldata = collections[collection];
    var colidx = Array.IndexOf(colnames, collection);

    var key = $"col_{colidx}";

    outputHtml.Add("<details>");

    outputHtml.Add("<summary>");

    var childkeys = string.Join(",", coldata.Select(x => x.Item2));

    outputHtml.Add($"<input type='checkbox' class='checkroot' id='check_{key}' onclick='setChildren(\"{key}\")' data-children='{childkeys}'/>");
    outputHtml.Add($"<label for='check_{key}'>{collection}</label><br>");
    outputHtml.Add("</summary>");

    foreach (var issue in coldata)
    {
        var checkid = issue.Item2;
        outputHtml.Add($"<input type='checkbox' class='check_{checkid}' id='check_coll_{colidx}_{checkid}' onclick='setChecked(\"check_coll_{colidx}_\", \"{checkid}\")' />");
        outputHtml.Add($"<label for='check_coll_{colidx}_{checkid}'>{issue.Item1}</label><br>");
    }


    outputHtml.Add("</details>");
}

outputHtml.Add("</details>");


outputHtml.Add("<details><summary>Story Threads</summary>");

foreach (var thread in threads)
{
    var key = threadKey[thread.Key];
    outputHtml.Add("<details>");
    outputHtml.Add("<summary>");

    var childkeys = string.Join(",", Enumerable.Range(0, thread.Value.Count).Select(i => $"{key}_{i}"));

    outputHtml.Add($"<input type='checkbox' class='checkroot' id='check_{key}' onclick='setChildren(\"{key}\")' data-children='{childkeys}'/>");
    outputHtml.Add($"<label for='check_{key}'>{thread.Key}</label><br>");
    outputHtml.Add("</summary>");

    int idx = 0;
    foreach (var issue in thread.Value)
    {
        var checkid = $"{key}_{idx++}";
        outputHtml.Add($"<input type='checkbox' class='check_{checkid}' id='check_thread_{checkid}' onclick='setChecked(\"check_thread_\", \"{checkid}\")' />");
        outputHtml.Add($"<label for='check_thread_{checkid}'>{issue}</label><br>");
    }
    outputHtml.Add("</details>");

    colourStyles.Add($".{key}");
    colourStyles.Add("{");
    colourStyles.Add($"  border: 2px solid {despoil.Util.Rainbow(threads.Count+5, threadIdx++)};");
    colourStyles.Add("}");
    // colourStyles.Add($".{key}_outer");
    // colourStyles.Add("{");
    // colourStyles.Add($"  display: none;");
    // colourStyles.Add($"  opacity: 0;");
    // colourStyles.Add("}");
}
outputHtml.Add("</details>");

outputHtml.Add("<details><summary>Individual issues</summary>");

foreach (var thread in threads)
{
    var key = threadKey[thread.Key];

    int idx = 0;
    foreach (var issue in thread.Value)
    {
        var checkid = $"{key}_{idx++}";
        outputHtml.Add($"<input type='checkbox' class='check_issue check_{checkid}' id='check_issue_{checkid}' onclick='setChecked(\"check_issue_\", \"{checkid}\")' />");
        outputHtml.Add($"<label for='check_issue_{checkid}'>{issue}</label><br>");
    }

}

outputHtml.Add("</details>");

outputHtml.Add("</details>");


outputHtml.Add("<details open><summary>Highlight...</summary>");

outputHtml.Add("<button onclick='highlightNone()'>Clear Highlights</button>");

outputHtml.Add("<details><summary>Characters / Places / Entities</summary>");

foreach (var entity in entityKey.OrderBy(x => x.Key))
{
    outputHtml.Add($"<div class='entity_hidden {string.Join(" ", entityAppearence[entity.Value].Select(x => "entityissue_"+x))}'>");
    outputHtml.Add($"<input type='checkbox' class='check_entity' id='check_{entity.Value}' onclick='setPushed(\"{entity.Value}\")'>");
    outputHtml.Add($"<label for='check_c2_{entity.Value}'>{entity.Key}</label><br>");
    outputHtml.Add($"</div>");

}

outputHtml.Add("</details>");

outputHtml.Add("</details>");


outputHtml.Add("</div>");

outputHtml.Add("<div class='box'>");

outputHtml.AddRange(entriesHtml);

outputHtml.Add("</div>");

outputHtml.Add("</div>");

outputHtml.Add("</body></html>");

File.WriteAllLines("index.html", outputHtml);
File.WriteAllLines("colors.css", colourStyles);

var sortedDates = eventDates.Select(x => x.Item2).OrderBy(i => i).ToArray();

File.WriteAllLines("order0.css", eventDates.Select(x => $".box :nth-child({x.Item1}) {{ order: {x.Item1} }}"));
File.WriteAllLines("order1.css", eventDates.Select(x => $".box :nth-child({x.Item1}) {{ order: {Array.IndexOf(sortedDates, x.Item2)+1} }}"));